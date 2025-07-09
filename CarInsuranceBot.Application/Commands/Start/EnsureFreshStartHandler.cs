using CarInsuranceBot.Application.Common;
using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Start;

public class EnsureFreshStartHandler : IRequestHandler<EnsureFreshStartCommand, string>
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _auditService;

    public EnsureFreshStartHandler(IUnitOfWork uow, IAuditService auditService)
    {
        _uow = uow;
        _auditService = auditService;
    }

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
            
            // Audit log the new user creation
            await _auditService.LogCreateAsync(user, ct);
            await _auditService.LogActionAsync("User", user.Id, "START_PROCESS", 
                $"New user started insurance process. ChatId: {cmd.ChatId}, Name: {user.FirstName}", ct);
            
            return Messages.Intro();
        }

        // RUNNING session?
        if (user.Stage is RegistrationStage.WaitingForReview
                     or RegistrationStage.WaitingForPayment)
        {
            await _auditService.LogActionAsync("User", user.Id, "RESTART_ATTEMPT", 
                $"User attempted to restart while in {user.Stage} stage", ct);
            return Messages.AlreadyInProgress();
        }

        // inconsistent?  e.g. WaitingForVehicle but no passport hash
        if (user.IsInconsistent())
        {
            // Clear incomplete data
            await _uow.Documents.RemoveRangeByUserStageAsync(user.Id, user.Stage, ct);
            await _uow.ExtractedFields.RemoveByUserAsync(user.Id, ct);
            
            var originalStage = user.Stage;
            user.Reset();
            await _uow.SaveChangesAsync(ct);
            
            // Audit log the reset
            await _auditService.LogActionAsync("User", user.Id, "RESET_INCONSISTENT", 
                $"Reset user from inconsistent state: {originalStage} -> {user.Stage}", ct);
            
            return Messages.ResetDone();
        }

        // otherwise greet based on where they left off
        await _auditService.LogActionAsync("User", user.Id, "CONTINUE_PROCESS", 
            $"User continued from stage: {user.Stage}", ct);
        
        return Messages.GreetByStage(user.Stage);
    }
} 