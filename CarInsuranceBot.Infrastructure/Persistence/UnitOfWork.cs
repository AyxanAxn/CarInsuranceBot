namespace CarInsuranceBot.Infrastructure.Persistence
{
    public class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
    {
        private readonly ApplicationDbContext _db = db;

        public IExtractedFieldRepository ExtractedFields => new ExtractedFieldRepository(_db);
        public IConversationRepository Conversations => new ConversationRepository(db);
        public IDocumentRepository Documents => new DocumentRepository(_db);
        public IPolicyRepository Policies => new PolicyRepository(_db);
        public IUserRepository Users => new UserRepository(_db);
        public IQueryable<Policy> PoliciesQuery => _db.Policies;
        public IQueryable<ErrorLog> Errors => _db.Errors;
        public IQueryable<User> UsersQuery => _db.Users;

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}