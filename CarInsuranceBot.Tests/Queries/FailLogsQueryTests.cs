namespace CarInsuranceBot.Tests.Queries;

public class FailLogsQueryTests : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db;
    public FailLogsQueryTests(InMemoryFixture fx) => _db = fx.Db;

    [Fact]
    public async Task Returns_error_logs_for_admin()
    {
        // Arrange - Clear any existing error logs first
        _db.Errors.RemoveRange(_db.Errors);
        await _db.SaveChangesAsync();

        var errorLog1 = TestDataBuilder.CreateErrorLog("OCR processing failed");
        var errorLog2 = TestDataBuilder.CreateErrorLog("Policy generation failed");

        _db.Errors.Add(errorLog1);
        _db.Errors.Add(errorLog2);
        await _db.SaveChangesAsync();

        var handler = new FailLogsQueryHandler(new UnitOfWork(_db));

        // Act
        var result = await handler.Handle(new FailLogsQuery(ChatId: 12345), default);

        // Assert
        result.Should().ContainEquivalentOf("Last 2 Errors");
        result.Should().ContainEquivalentOf("OCR processing failed");
        result.Should().ContainEquivalentOf("Policy generation failed");
    }

    [Fact]
    public async Task Returns_empty_message_when_no_errors()
    {
        // Arrange - Clear any existing error logs
        _db.Errors.RemoveRange(_db.Errors);
        await _db.SaveChangesAsync();
        
        var handler = new FailLogsQueryHandler(new UnitOfWork(_db));

        // Act
        var result = await handler.Handle(new FailLogsQuery(ChatId: 12345), default);

        // Assert
        result.Should().ContainEquivalentOf("no errors");
    }
} 