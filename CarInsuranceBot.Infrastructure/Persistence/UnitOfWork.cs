namespace CarInsuranceBot.Infrastructure.Persistence
{
    public class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
    {
        private readonly ApplicationDbContext _db = db;

        public IUserRepository Users => new UserRepository(_db);
        public IDocumentRepository Documents => new DocumentRepository(_db);
        public IExtractedFieldRepository ExtractedFields => new ExtractedFieldRepository(_db);
        public IPolicyRepository Policies => new PolicyRepository(_db);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}