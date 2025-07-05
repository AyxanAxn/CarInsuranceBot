namespace CarInsuranceBot.Infrastructure.Persistence.Repositories;

public sealed class ConversationRepository(ApplicationDbContext db) : IConversationRepository
{
    private readonly ApplicationDbContext _db = db;

    public void Add(Conversation convo) => _db.Conversations.Add(convo);

    public Task<int> CountAsync(CancellationToken ct = default)
        => _db.Conversations.CountAsync(ct);
}