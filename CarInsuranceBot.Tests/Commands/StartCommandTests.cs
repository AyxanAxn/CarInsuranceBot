namespace CarInsuranceBot.Tests.Commands;
public class StartCommandTests : IClassFixture<InMemoryFixture>
{
    private readonly InMemoryFixture _fx;
    public StartCommandTests(InMemoryFixture fx) => _fx = fx;

    [Fact]
    public async Task Creates_new_user_and_returns_greeting()
    {
        var uow = new UnitOfWork(_fx.Db);

        var handler = new StartCommandHandler(uow);
        var reply = await handler.Handle(new StartCommand(99, "Bob"), default);

        reply.Should().ContainEquivalentOf("welcome");
        _fx.Db.Users.Should().ContainSingle(u => u.TelegramUserId == 99);
    }
}