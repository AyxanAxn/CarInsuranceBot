using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Application.Common.Interfaces;

public interface IDocumentRepository
{
    void Add(Document doc);
    Task<Document?> GetAsync(Guid id, CancellationToken ct);
}