using CarInsuranceBot.Application.Commands.Flow;
using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Enums;
using MediatR;

public class CancelCommandHandler : IRequestHandler<CancelCommand, string>
{
    private readonly IUnitOfWork _uow;
    public CancelCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<string> Handle(CancelCommand cmd, CancellationToken ct)
    {
        var user = await _uow.Users.GetAsync(cmd.ChatId, ct);
        if (user != null)
        {
            user.Stage = RegistrationStage.None;
            user.UploadAttempts = 0;

            // Remove documents for all in-progress stages
            await _uow.Documents.RemoveRangeByUserStageAsync(user.Id, RegistrationStage.WaitingForPassport, ct);
            await _uow.Documents.RemoveRangeByUserStageAsync(user.Id, RegistrationStage.WaitingForVehicle, ct);
            await _uow.Documents.RemoveRangeByUserStageAsync(user.Id, RegistrationStage.WaitingForReview, ct);
            await _uow.Documents.RemoveRangeByUserStageAsync(user.Id, RegistrationStage.ReadyToPay, ct);
            await _uow.Documents.RemoveRangeByUserStageAsync(user.Id, RegistrationStage.WaitingForPayment, ct);

            // Remove extracted fields (if you have a similar method)
            await _uow.ExtractedFields.RemoveByUserAsync(user.Id, ct);

            await _uow.SaveChangesAsync(ct);
        }
        return "Your session has been cancelled. To start over, type /start or upload your passport.";
    }
}