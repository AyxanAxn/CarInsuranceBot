namespace CarInsuranceBot.Infrastructure.Persistence.Repositories;

public class DocumentRepository(ApplicationDbContext db) : IDocumentRepository
{
    private readonly ApplicationDbContext _db = db;

    public void Add(Document doc) => _db.Documents.Add(doc);
    public Task<Document?> GetAsync(Guid id, CancellationToken ct) =>
        _db.Documents.FindAsync([id], ct).AsTask();

    public async Task<bool> ExistsHashAsync(Guid userId, string hash, CancellationToken ct) =>
        await _db.Documents.AnyAsync(d => d.UserId == userId && d.ContentHash == hash, ct);

    public async Task RemoveRangeByUserStageAsync(Guid userId,
                                              RegistrationStage stage,
                                              CancellationToken ct)
    {
        var docs = await _db.Documents
                            .Where(d => d.UserId == userId &&
                                        d.User.Stage == stage)
                            .ToListAsync(ct);

        _db.Documents.RemoveRange(docs);
    }

    public async Task<List<Document>> GetByUserAsync(Guid userId, CancellationToken ct)
    {
        return await _db.Documents
                        .Where(d => d.UserId == userId)
                        .ToListAsync(ct);
    }

    public async Task<List<Document>> GetByUserAndStageAsync(Guid userId, RegistrationStage stage, CancellationToken ct)
    {
        return await _db.Documents
            .Where(d => d.UserId == userId && d.User.Stage == stage)
            .ToListAsync(ct);
    }
}