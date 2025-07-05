using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Application.Commands.Policy;
using CarInsuranceBot.Application.Utils;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using MediatR;

public class GeneratePolicyCommandHandler : IRequestHandler<GeneratePolicyCommand, string>
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStore _store;
    private readonly ITelegramBotClient _bot;

    public GeneratePolicyCommandHandler(
        IUnitOfWork uow, IFileStore store, ITelegramBotClient bot)
    {
        _uow = uow; _store = store; _bot = bot;
    }

    public async Task<string> Handle(GeneratePolicyCommand cmd, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(cmd.UserId, ct);
        var vin = await _uow.ExtractedFields.FirstVinAsync(user.Id, ct); // helper method

        var pdfBytes = PolicyPdfBuilder.Build(user.FullName, vin, DateTime.UtcNow.AddDays(7));
        var path = await _store.SavePdf(pdfBytes, $"{user.TelegramUserId}_policy.pdf");

        _uow.Policies.Add(new Policy
        {
            UserId = user.Id,
            PolicyNumber = Guid.NewGuid().ToString("N")[..10].ToUpper(),
            Status = PolicyStatus.Issued,
            PdfPath = path,
            IssuedUtc = DateTime.UtcNow,
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        });
        user.Stage = RegistrationStage.Finished;
        await _uow.SaveChangesAsync(ct);

        // send to Telegram
        await using var ms = new MemoryStream(pdfBytes);
        await _bot.SendDocument(user.TelegramUserId, new InputFileStream(ms, "policy.pdf"),
                                     caption: "📄 Your policy is ready!", cancellationToken: ct);

        return "✅ Policy generated and sent!";
    }
}
