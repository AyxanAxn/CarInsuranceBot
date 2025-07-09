namespace CarInsuranceBot.Tests.Commands;

using CarInsuranceBot.Application.Commands.ResendPolicy;
using CarInsuranceBot.Application.Common.Interfaces;
using System.IO;

public class ResendPolicyCommandTests : IClassFixture<InMemoryFixture>, IClassFixture<FileTestFixture>
{
    private readonly ApplicationDbContext _db;
    private readonly FileTestFixture _fileFixture;
    
    public ResendPolicyCommandTests(InMemoryFixture fx, FileTestFixture fileFixture)
    {
        _db = fx.Db;
        _fileFixture = fileFixture;
    }

    private class DummyPolicyFileStore : IPolicyFileStore
    {
        public Task<string> SaveAsync(Telegram.Bot.Types.TGFile telegramFile, CancellationToken ct) => Task.FromResult("");
        public Task<string> SavePdf(byte[] pdfBytes, string fileName, CancellationToken ct = default) => Task.FromResult("");
        public Task<Stream> OpenReadAsync(string path, CancellationToken ct = default) => Task.FromResult<Stream>(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("dummy pdf content")));
        public Task DeleteAsync(string path, CancellationToken ct = default) => Task.CompletedTask;
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
        var handler = new ResendPolicyCommandHandler(new UnitOfWork(_db), new DummyPolicyFileStore(), botMock.Object);

        // Act
        var result = await handler.Handle(new ResendPolicyCommand(user.TelegramUserId), default);

        // Assert
        result.ShouldBePolicyResent();
    }

    [Fact]
    public async Task Returns_error_when_no_policy_found()
    {
        // Arrange
        var handler = new ResendPolicyCommandHandler(new UnitOfWork(_db), new DummyPolicyFileStore(), Mock.Of<ITelegramBotClient>());

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
        var handler = new ResendPolicyCommandHandler(new UnitOfWork(_db), new DummyPolicyFileStore(), botMock.Object);

        // Act
        var result = await handler.Handle(new ResendPolicyCommand(user.TelegramUserId), default);

        // Assert
        result.ShouldBePolicyResent();
    }
}