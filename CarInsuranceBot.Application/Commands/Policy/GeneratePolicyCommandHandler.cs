using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Application.Utils;
using CarInsuranceBot.Domain.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using MediatR;
using CarInsuranceBot.Domain.Entities.Builders;
using CarInsuranceBot.Application.AI;

namespace CarInsuranceBot.Application.Commands.Policy;
public class GeneratePolicyCommandHandler(
    IUnitOfWork uow, 
    IPolicyFileStore policyFileStore, // use the specific interface
    ITelegramBotClient bot, 
    IGeminiService geminiService,
    IAuditService auditService) : IRequestHandler<GeneratePolicyCommand, string>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPolicyFileStore _policyFileStore = policyFileStore;
    private readonly ITelegramBotClient _bot = bot;
    private readonly IGeminiService _geminiService = geminiService;
    private readonly IAuditService _auditService = auditService;

    public async Task<string> Handle(GeneratePolicyCommand cmd, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(cmd.UserId, ct);
        string vin = await _uow.ExtractedFields.FirstVinAsync(user!.Id, ct);

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
            My company name is Aykhan Inshurance. Email is Inshurance@aykhanInshurance.comeWithCar.
            Location is somewhere in the east 
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
        var path = await _policyFileStore.SavePdf(pdfBytes, $"{user.TelegramUserId}_policy.pdf");

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

        // Audit log the policy creation and user completion
        await _auditService.LogCreateAsync(policy, ct);
        await _auditService.LogActionAsync("User", user.Id, "POLICY_GENERATED", 
            $"Policy {policyNumber} generated for user {user.FullName}. VIN: {vin}, Expires: {expiry:yyyy-MM-dd}", ct);

        // Send PDF to user
        await using var ms = new MemoryStream(pdfBytes);
        await _bot.SendDocument(user.TelegramUserId, new InputFileStream(ms, "policy.pdf"),
                                     caption: "📄 Your policy is ready!", cancellationToken: ct);

        return "✅ Policy generated and sent!";
    }
}