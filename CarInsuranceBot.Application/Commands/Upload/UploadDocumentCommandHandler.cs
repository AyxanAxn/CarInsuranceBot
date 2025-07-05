using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Application.Commands.Review;
using CarInsuranceBot.Application.Commands.Upload;
using CarInsuranceBot.Application.OCR;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;
using System.Text;
using MediatR;

public class UploadDocumentCommandHandler :
    IRequestHandler<UploadDocumentCommand, string>
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStore _store;
    private readonly IMediator _mediator;
    private readonly IMindeeService _ocr;

    public UploadDocumentCommandHandler(
        IUnitOfWork uow,
        IFileStore store,
        IMediator mediator,
        IMindeeService ocr)
    {
        _uow = uow;
        _store = store;
        _mediator = mediator;
        _ocr = ocr;
    }

    public async Task<string> Handle(UploadDocumentCommand cmd, CancellationToken ct)
    {
        // --------------------------------------------------------------
        // 1. Persist the Telegram file on disk
        // --------------------------------------------------------------
        var path = await _store.SaveAsync(cmd.TelegramFile, ct);

        var user = await _uow.Users.GetAsync(cmd.ChatId, ct)
                   ?? throw new InvalidOperationException("User not found");

        var docType = cmd.IsPassport ? DocumentType.Passport
                                     : DocumentType.VehicleRegistration;

        var doc = new Document
        {
            UserId = user.Id,
            Type = docType,
            Path = path,
            UploadedUtc = DateTime.UtcNow
        };
        _uow.Documents.Add(doc);
        await _uow.SaveChangesAsync(ct);           // need the DocumentId

        // --------------------------------------------------------------
        // 2. Run OCR immediately (Mindee or simulation)
        // --------------------------------------------------------------
        await using var fs = File.OpenRead(path);
        var extracted = await _ocr.ExtractAsync(fs, docType, ct);

        foreach (var (k, v) in extracted.Values)
            _uow.ExtractedFields.Add(new ExtractedField
            {
                DocumentId = doc.Id,
                FieldName = k,
                FieldValue = v,
                Confidence = 0.9f
            });

        // --------------------------------------------------------------
        // 3. Update the user's stage
        // --------------------------------------------------------------
        if (cmd.IsPassport)
            user.Stage = RegistrationStage.WaitingForVehicle;
        else
            user.Stage = RegistrationStage.WaitingForReview;

        await _uow.SaveChangesAsync(ct);

        // --------------------------------------------------------------
        // 4. Decide what to send back to Telegram
        // --------------------------------------------------------------


        if (cmd.IsPassport)
        {
            var sb = new StringBuilder("🔍 *Please review the extracted data:*");
            foreach (var (n, v) in extracted.Values)
                sb.Append($"\n• *{n}*: `{v}`");
            sb.Append($"\n✅ Passport received! Now please send a photo of the vehicle registration certificate.");
            return sb.ToString();
        }
        else
        {
            // second document received → build review summary
            var summary = await _mediator.Send(
                new ExtractAndReviewCommand(doc.Id), ct);
            return summary;
        }
    }
}