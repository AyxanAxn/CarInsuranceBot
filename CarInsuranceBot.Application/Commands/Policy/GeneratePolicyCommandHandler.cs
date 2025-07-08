using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Application.Commands.Policy;
using CarInsuranceBot.Application.Utils;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using MediatR;
using CarInsuranceBot.Domain.Entities.Builders;
using CarInsuranceBot.Application.AI;

public class GeneratePolicyCommandHandler : IRequestHandler<GeneratePolicyCommand, string>
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStore _store;
    private readonly ITelegramBotClient _bot;
    private readonly IGeminiService _geminiService;

    public GeneratePolicyCommandHandler(
        IUnitOfWork uow, IFileStore store, ITelegramBotClient bot, IGeminiService geminiService)
    {
        _uow = uow; _store = store; _bot = bot; _geminiService = geminiService;
    }

    public async Task<string> Handle(GeneratePolicyCommand cmd, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(cmd.UserId, ct);
        var vin = await _uow.ExtractedFields.FirstVinAsync(user.Id, ct);

        // Generate policy number and expiry first
        var policyNumber = Guid.NewGuid().ToString("N")[..10].ToUpper();
        var expiry = DateTime.UtcNow.AddDays(7);

        // Build the prompt for Gemini
        var prompt = $@"
            You are an assistant helping generate friendly and professional car insurance policy messages.

            Create a personalized message addressed to the customer named {user.FullName}. Their new car insurance policy number is {policyNumber}, and it is valid until {expiry:yyyy-MM-dd}.

            The tone should be warm, reassuring, and trustworthy. Include the following:
            - A thank-you for choosing our service.
            - A summary that their policy is now active.
            - A reminder of the expiry date.
            - A note that the policy document (PDF) is attached.
            - A call to action if they have questions or need support.

            Keep it under 150 words and clearly structured for easy reading in a Telegram message.
        ";
        // Get personalized text from Gemini
        string personalizedText = await _geminiService.AskAsync(user.TelegramUserId, prompt, ct);

        // Build the PDF
        var pdfBytes = new PolicyPdfBuilder()
            .WithFullName(user.FullName)
            .WithVin(vin)
            .WithExpiry(expiry)
            .AddSection(personalizedText)
            .Build();

        // Save PDF to file system
        var path = await _store.SavePdf(pdfBytes, $"{user.TelegramUserId}_policy.pdf");

        // Build and save the policy entity
        var policy = new PolicyBuilder()
            .WithUserId(user.Id)
            .WithPolicyNumber(policyNumber)
            .WithStatus(PolicyStatus.Issued)
            .WithPdfPath(path)
            .WithIssuedUtc(DateTime.UtcNow)
            .WithExpiresUtc(expiry)
            .WithUser(user)
            .Build();

        _uow.Policies.Add(policy);
        user.Stage = RegistrationStage.Finished;
        await _uow.SaveChangesAsync(ct);

        // Send PDF to user
        await using var ms = new MemoryStream(pdfBytes);
        await _bot.SendDocument(user.TelegramUserId, new InputFileStream(ms, "policy.pdf"),
                                     caption: "📄 Your policy is ready!", cancellationToken: ct);

        return "✅ Policy generated and sent!";
    }
}