using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Retry;

public class RetryCommandHandler : IRequestHandler<RetryCommand, string>
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStore _fileStore;

    public RetryCommandHandler(IUnitOfWork uow, IFileStore fileStore)
    {
        _uow = uow;
        _fileStore = fileStore;
    }

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

        // 1. Delete files from blob storage for documents in WaitingForReview stage
        var docs = await _uow.Documents.GetByUserAndStageAsync(user.Id, RegistrationStage.WaitingForReview, ct);
        foreach (var doc in docs)
        {
            await _fileStore.DeleteAsync(doc.Path, ct);
        }
        // 2. Remove document records and extracted fields from the database
        await _uow.Documents.RemoveRangeByUserStageAsync(user.Id, RegistrationStage.WaitingForReview, ct);
        await _uow.ExtractedFields.RemoveByUserAsync(user.Id, ct);

        // 3. Reset counters & stage.
        user.Stage = RegistrationStage.WaitingForPassport;   // start from the top
        user.UploadAttempts = 0;

        await _uow.SaveChangesAsync(ct);

        return "🔄 Let's try again. Please upload your *passport* photo.";
    }
} 