namespace CarInsuranceBot.Infrastructure.Persistence.Repositories;

public class ExtractedFieldRepository(ApplicationDbContext db) : IExtractedFieldRepository
{
    private readonly ApplicationDbContext _db = db;

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

    public async Task RemoveByUserAsync(Guid userId, CancellationToken ct)
    {
        var fields = await _db.ExtractedFields
                              .Where(f => f.Document.UserId == userId)
                              .ToListAsync(ct);
        _db.ExtractedFields.RemoveRange(fields);
    }
}
