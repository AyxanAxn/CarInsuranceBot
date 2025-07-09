using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Cancel;
public class CancelCommandHandler : IRequestHandler<CancelCommand, string>
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStore _fileStore;
    public CancelCommandHandler(IUnitOfWork uow, IFileStore fileStore)
    {
        _uow = uow;
        _fileStore = fileStore;
    }

    public async Task<string> Handle(CancelCommand cmd, CancellationToken ct)
    {
        var user = await _uow.Users.GetAsync(cmd.ChatId, ct);
        if (user != null)
        {
            user.Stage = RegistrationStage.None;
            user.UploadAttempts = 0;

            var stages = new[]
            {
                RegistrationStage.WaitingForPassport,
                RegistrationStage.WaitingForVehicle,
                RegistrationStage.WaitingForReview,
                RegistrationStage.ReadyToPay,
                RegistrationStage.WaitingForPayment
            };

            foreach (var stage in stages)
            {
                var docsForStage = await _uow.Documents.GetByUserAndStageAsync(user.Id, stage, ct);
                foreach (var doc in docsForStage)
                {
                    await _fileStore.DeleteAsync(doc.Path, ct);
                }
                await _uow.Documents.RemoveRangeByUserStageAsync(user.Id, stage, ct);
            }

            // Remove extracted fields (if you have a similar method)
            await _uow.ExtractedFields.RemoveByUserAsync(user.Id, ct);

            await _uow.SaveChangesAsync(ct);
        }
        return "Your session has been cancelled. To start over, type /start or upload your passport.";
    }
}