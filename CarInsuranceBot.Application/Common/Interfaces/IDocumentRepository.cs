using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;

namespace CarInsuranceBot.Application.Common.Interfaces;

public interface IDocumentRepository
{
    void Add(Document doc);
    Task<Document?> GetAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsHashAsync(Guid userId, string hash, CancellationToken ct);
    Task RemoveRangeByUserStageAsync(Guid userId,
                                 RegistrationStage stage, CancellationToken ct);
    Task<List<Document>> GetByUserAsync(Guid userId, CancellationToken ct);
}