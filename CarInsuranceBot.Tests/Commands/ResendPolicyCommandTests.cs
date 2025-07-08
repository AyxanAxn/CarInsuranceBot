namespace CarInsuranceBot.Tests.Commands;

public class ResendPolicyCommandTests : IClassFixture<InMemoryFixture>, IClassFixture<FileTestFixture>
{
    private readonly ApplicationDbContext _db;
    private readonly FileTestFixture _fileFixture;
    
    public ResendPolicyCommandTests(InMemoryFixture fx, FileTestFixture fileFixture)
    {
        _db = fx.Db;
        _fileFixture = fileFixture;
    }

    [Fact]
    public async Task Resends_existing_policy_to_user()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(123);
        var policy = TestDataBuilder.CreatePolicy(user.Id, "POL-123");
        
        // Create test file using FileTestFixture
        var policyPath = _fileFixture.CreateTestFileInDirectory("policies", "policy_123.pdf", "dummy pdf content");
        policy.PdfPath = policyPath;

        _db.Users.Add(user);
        _db.Policies.Add(policy);
        await _db.SaveChangesAsync();

        var botMock = new Mock<ITelegramBotClient>();
        var handler = new ResendPolicyCommandHandler(new UnitOfWork(_db), botMock.Object);

        // Act
        var result = await handler.Handle(new ResendPolicyCommand(user.TelegramUserId), default);

        // Assert
        result.ShouldBePolicyResent();
    }

    [Fact]
    public async Task Returns_error_when_no_policy_found()
    {
        // Arrange
        var handler = new ResendPolicyCommandHandler(new UnitOfWork(_db), Mock.Of<ITelegramBotClient>());

        // Act
        var result = await handler.Handle(new ResendPolicyCommand(999999), default);

        // Assert
        result.ShouldBeNoPolicyFound();
    }

    [Fact]
    public async Task Sends_policy_with_correct_caption()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(456);
        var policy = TestDataBuilder.CreatePolicy(user.Id, "POL-456");
        
        // Create test file using FileTestFixture
        var policyPath = _fileFixture.CreateTestFileInDirectory("policies", "policy_456.pdf", "dummy pdf content");
        policy.PdfPath = policyPath;

        _db.Users.Add(user);
        _db.Policies.Add(policy);
        await _db.SaveChangesAsync();

        var botMock = new Mock<ITelegramBotClient>();
        var handler = new ResendPolicyCommandHandler(new UnitOfWork(_db), botMock.Object);

        // Act
        var result = await handler.Handle(new ResendPolicyCommand(user.TelegramUserId), default);

        // Assert
        result.ShouldBePolicyResent();
    }
}