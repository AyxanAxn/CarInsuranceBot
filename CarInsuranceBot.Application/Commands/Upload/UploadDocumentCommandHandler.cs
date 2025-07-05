using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Application.Commands.Review;
using CarInsuranceBot.Application.Commands.Upload;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Application.OCR;
using CarInsuranceBot.Domain.Enums;
using System.Security.Cryptography;
using Telegram.Bot;
using System.Text;
using MediatR;

public class UploadDocumentCommandHandler :
    IRequestHandler<UploadDocumentCommand, string>
{
    private const int maxAttempts = 5;

    private readonly IUnitOfWork _uow;
    private readonly IFileStore _store;
    private readonly IMediator _mediator;
    private readonly IMindeeService _ocr;
    private readonly ITelegramBotClient _bot;

    public UploadDocumentCommandHandler(
        IUnitOfWork uow,
        IFileStore store,
        IMediator mediator,
        IMindeeService ocr,
        ITelegramBotClient bot)
    {
        _uow = uow;
        _store = store;
        _mediator = mediator;
        _ocr = ocr;
        _bot = bot;
    }

    // --------------------------------------------------------------
    public async Task<string> Handle(UploadDocumentCommand cmd, CancellationToken ct)
    {
        // ------------------------------------------------------------------
        // 0.  Load user, manage attempts
        // ------------------------------------------------------------------
        var user = await _uow.Users.GetAsync(cmd.ChatId, ct)
                   ?? throw new InvalidOperationException("User not found");

        user.UploadAttempts++;
        if (user.UploadAttempts > maxAttempts)
        {
            await _uow.SaveChangesAsync(ct);
            return $"❌ You reached {maxAttempts} upload attempts. Type /cancel to restart.";
        }

        // ------------------------------------------------------------------
        // 1.  Download tg file to memory to compute hash
        // ------------------------------------------------------------------
        await using var ms = new MemoryStream();
        await _bot.DownloadFile(cmd.TelegramFile.FilePath, ms, ct);
        var hash = Convert.ToHexString(SHA256.HashData(ms.ToArray()));

        if (await _uow.Documents.ExistsHashAsync(user.Id, hash, ct))
        {
            await _uow.SaveChangesAsync(ct); // persist attempts increment
            return "⚠️ That looks like a duplicate of an earlier photo. Please send a different image.";
        }

        // ------------------------------------------------------------------
        // 2.  Persist file to disk / blob
        // ------------------------------------------------------------------
        var path = await _store.SaveAsync(cmd.TelegramFile, ct);   // original signature
        
        // ------------------------------------------------------------------
        // 3.  Create Document row
        // ------------------------------------------------------------------
        var docType = cmd.IsPassport ? DocumentType.Passport
                                     : DocumentType.VehicleRegistration;

        var doc = new Document
        {
            UserId = user.Id,
            Type = docType,
            Path = path,
            UploadedUtc = DateTime.UtcNow,
            ContentHash = hash
        };
        _uow.Documents.Add(doc);
        await _uow.SaveChangesAsync(ct); // need doc.Id

        // ------------------------------------------------------------------
        // 4.  OCR
        // ------------------------------------------------------------------
        ms.Position = 0;
        var extracted = await _ocr.ExtractAsync(ms, docType, ct);

        foreach (var (k, v) in extracted.Values)
            _uow.ExtractedFields.Add(new ExtractedField
            {
                DocumentId = doc.Id,
                FieldName = k,
                FieldValue = v,
                Confidence = 0.9f
            });

        // ------------------------------------------------------------------
        // 5.  Advance stage & maybe reset attempts
        // ------------------------------------------------------------------
        if (cmd.IsPassport)
            user.Stage = RegistrationStage.WaitingForVehicle;
        else
        {
            user.Stage = RegistrationStage.WaitingForReview;
            user.UploadAttempts = 0;           // reset now that we have both docs
        }

        await _uow.SaveChangesAsync(ct);

        // ------------------------------------------------------------------
        // 6.  Build reply
        // ------------------------------------------------------------------
        if (cmd.IsPassport)
        {
            var sb = new StringBuilder("✅ Passport received! Now please send a photo of the vehicle registration certificate.");
            return sb.ToString();
        }
        else
        {
            // second document received → build review summary
            var summary = await _mediator.Send(new ExtractAndReviewCommand(doc.Id), ct);
            return summary;
        }
    }
}