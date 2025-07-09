namespace CarInsuranceBot.Tests.Queries;
public class AdminStatsQueryTests(InMemoryFixture fx) : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db = fx.Db;

    [Fact]
    public async Task Stats_query_returns_correct_counts()
    {
        _db.Users.Add(new User { Id = Guid.NewGuid(), TelegramUserId = 1 });
        _db.Policies.Add(new Policy
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PolicyNumber = "PN",
            Status = PolicyStatus.Issued,
            PdfPath = "x",
            IssuedUtc = DateTime.UtcNow,
            ExpiresUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var handler = new StatsQueryHandler(new UnitOfWork(_db));
        var txt = await handler.Handle(new StatsQuery(ChatId: 12345), default);

        txt.Should().Contain("Issued: *1*");
        txt.Should().Contain("Total registered: *1*");
    }
}