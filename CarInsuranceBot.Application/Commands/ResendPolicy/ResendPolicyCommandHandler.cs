using CarInsuranceBot.Application.Common.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Policy;

public class ResendPolicyCommandHandler(IUnitOfWork uow, IPolicyFileStore policyFileStore, ITelegramBotClient bot) : IRequestHandler<ResendPolicyCommand, string>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPolicyFileStore _policyFileStore = policyFileStore;
    private readonly ITelegramBotClient _bot = bot;

    public async Task<string> Handle(ResendPolicyCommand cmd, CancellationToken ct)
    {
        var policy = await _uow.Policies.GetLatestByUserAsync(cmd.ChatId, ct);
        if (policy is null)
            return "❌ No policy found for this chat. Complete the flow first.";

        // Open the PDF from blob storage
        await using var fs = await _policyFileStore.OpenReadAsync(policy.PdfPath, ct);
        await _bot.SendDocument(cmd.ChatId,
            InputFile.FromStream(fs, "policy.pdf"),
            caption: "📄 Here is your policy again.",
            cancellationToken: ct);

        return "✅ Policy resent.";
    }
}