using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Telegram.Start;

public class StartCommandHandler : IRequestHandler<StartCommand, string>
{
    private readonly IUnitOfWork _uow;

    public StartCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<string> Handle(StartCommand cmd, CancellationToken ct)
    {
        // Check if user already exists
        var user = await _uow.Users.GetAsync(cmd.ChatId, ct);

        if (user is null)
        {
            user = new User
            {
                TelegramUserId = cmd.ChatId,
                FirstName = cmd.FirstName ?? string.Empty,
                Stage = RegistrationStage.WaitingForPassport
            };
            _uow.Users.Add(user);
        }
        else
        {
            user.Stage = RegistrationStage.WaitingForPassport;
        }
        await _uow.SaveChangesAsync(ct);

        // Greeting text
        return
            "👋 *Welcome to FastCar Insurance Bot!*\n\n" +
            "Here’s how it works:\n" +
            "1️⃣  Send a photo of your *passport*\n" +
            "2️⃣  Send a photo of your *vehicle registration*\n" +
            "3️⃣  Review the extracted data\n" +
            "4️⃣  Pay the *fixed price* **100 USD**\n" +
            "5️⃣  Receive your digital policy in seconds 🚗💨";
    }
}
