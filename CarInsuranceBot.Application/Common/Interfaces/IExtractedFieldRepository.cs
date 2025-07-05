using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Application.Common.Interfaces;
public interface IExtractedFieldRepository
{
    void Add(ExtractedField field);
    Task<string> FirstVinAsync(Guid userId, CancellationToken ct);
}
