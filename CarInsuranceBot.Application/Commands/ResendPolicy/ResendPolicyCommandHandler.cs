using CarInsuranceBot.Application.Common.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Policy;

public class ResendPolicyCommandHandler : IRequestHandler<ResendPolicyCommand, string>
{
    private readonly IUnitOfWork _uow;
    private readonly ITelegramBotClient _bot;

    public ResendPolicyCommandHandler(IUnitOfWork uow, ITelegramBotClient bot)
    {
        _uow = uow;
        _bot = bot;
    }

    public async Task<string> Handle(ResendPolicyCommand cmd, CancellationToken ct)
    {
        var policy = await _uow.Policies.GetLatestByUserAsync(cmd.ChatId, ct);
        if (policy is null)
            return "❌ No policy found for this chat. Complete the flow first.";

        await using var fs = File.OpenRead(policy.PdfPath);
        await _bot.SendDocument(cmd.ChatId,
            InputFile.FromStream(fs, "policy.pdf"),
            caption: "📄 Here is your policy again.",
            cancellationToken: ct);

        return "✅ Policy resent.";
    }
}
