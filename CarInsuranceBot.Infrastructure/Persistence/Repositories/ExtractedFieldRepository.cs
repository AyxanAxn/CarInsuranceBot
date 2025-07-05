namespace CarInsuranceBot.Infrastructure.Persistence.Repositories;

public class ExtractedFieldRepository : IExtractedFieldRepository
{
    private readonly ApplicationDbContext _db;
    public ExtractedFieldRepository(ApplicationDbContext db) => _db = db;

    public void Add(ExtractedField field) => _db.ExtractedFields.Add(field);

    /// <summary>
    /// Returns the first VIN we stored for the user, or null if none exists.
    /// </summary>
    public async Task<string?> FirstVinAsync(Guid userId, CancellationToken ct) =>
        await _db.ExtractedFields
                 .Where(f => f.Document.UserId == userId &&
                             f.FieldName == "VIN")
                 .OrderBy(f => f.Id)                    // oldest first → deterministic
                 .Select(f => f.FieldValue)
                 .FirstOrDefaultAsync(ct);
}
