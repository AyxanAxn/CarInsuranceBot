using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Retry;

public class RetryCommandHandler : IRequestHandler<RetryCommand, string>
{
    private readonly IUnitOfWork _uow;

    public RetryCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<string> Handle(RetryCommand cmd, CancellationToken ct)
    {
        var user = await _uow.Users.GetAsync(cmd.ChatId, ct);
        
        if (user is null)
        {
            return "❌ User not found. Please start with /start";
        }

        if (user.Stage != RegistrationStage.WaitingForReview)
        {
            return "❌ You can only retry when reviewing extracted data.";
        }

        // 1. Delete previous docs for this user (in review stage).
        await _uow.Documents.RemoveRangeByUserStageAsync(user.Id, RegistrationStage.WaitingForReview, ct);
        await _uow.ExtractedFields.RemoveByUserAsync(user.Id, ct);

        // 2. Reset counters & stage.
        user.Stage = RegistrationStage.WaitingForPassport;   // start from the top
        user.UploadAttempts = 0;

        await _uow.SaveChangesAsync(ct);

        return "🔄 Let's try again. Please upload your *passport* photo.";
    }
} 