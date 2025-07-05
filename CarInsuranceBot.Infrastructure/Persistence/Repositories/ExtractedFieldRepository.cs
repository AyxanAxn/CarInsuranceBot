using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Infrastructure.Persistence.Repositories;

public class ExtractedFieldRepository : IExtractedFieldRepository
{
    private readonly ApplicationDbContext _db;
    public ExtractedFieldRepository(ApplicationDbContext db) => _db = db;
    public void Add(ExtractedField field) => _db.ExtractedFields.Add(field);
    public Task<string> FirstVinAsync(Guid userId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
