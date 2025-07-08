using CarInsuranceBot.Application.Common;
using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Start;

public class EnsureFreshStartHandler : IRequestHandler<EnsureFreshStartCommand, string>
{
    private readonly IUnitOfWork _uow;

    public EnsureFreshStartHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<string> Handle(EnsureFreshStartCommand cmd, CancellationToken ct)
    {
        var user = await _uow.Users.GetAsync(cmd.ChatId, ct);

        if (user is null)
        {
            // brand-new user
            user = new User
            {
                TelegramUserId = cmd.ChatId,
                FirstName = cmd.FirstName ?? "Friend",
                Stage = RegistrationStage.WaitingForPassport
            };
            _uow.Users.Add(user);
            await _uow.SaveChangesAsync(ct);
            return Messages.Intro();
        }

        // RUNNING session?
        if (user.Stage is RegistrationStage.WaitingForReview
                     or RegistrationStage.WaitingForPayment)
            return Messages.AlreadyInProgress();

        // inconsistent?  e.g. WaitingForVehicle but no passport hash
        if (user.IsInconsistent())
        {
            // Clear incomplete data
            await _uow.Documents.RemoveRangeByUserStageAsync(user.Id, user.Stage, ct);
            await _uow.ExtractedFields.RemoveByUserAsync(user.Id, ct);
            
            user.Reset();
            await _uow.SaveChangesAsync(ct);
            return Messages.ResetDone();
        }

        // otherwise greet based on where they left off
        return Messages.GreetByStage(user.Stage);
    }
} 