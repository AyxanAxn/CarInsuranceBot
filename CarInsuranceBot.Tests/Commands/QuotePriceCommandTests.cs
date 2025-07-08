namespace CarInsuranceBot.Tests.Commands;
public class QuotePriceCommandTests(InMemoryFixture fx) : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db = fx.Db;

    [Fact]
    public async Task Returns_fixed_price_message()
    {
        var user = new User { Id = Guid.NewGuid(), TelegramUserId = 22 };
        _db.Users.Add(user); await _db.SaveChangesAsync();

        var handler = new QuotePriceCommandHandler(new UnitOfWork(_db));
        var msg = await handler.Handle(new QuotePriceCommand(user.Id), default);

        msg.Should().Contain("100 USD");
    }
}