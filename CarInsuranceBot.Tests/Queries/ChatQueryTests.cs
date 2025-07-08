using CarInsuranceBot.Application.Queries.Chat;

namespace CarInsuranceBot.Tests.Queries;

public class ChatQueryTests : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db;
    public ChatQueryTests(InMemoryFixture fx) => _db = fx.Db;

    [Fact]
    public async Task Returns_user_chat_history()
    {
        // Arrange
        var user = new User 
        { 
            Id = Guid.NewGuid(), 
            TelegramUserId = 123
        };
        var conversation1 = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Prompt = "Hello",
            Response = "Hello",
            CreatedUtc = DateTime.UtcNow.AddMinutes(-10)
        };
        var conversation2 = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Prompt = "Welcome to our insurance bot",
            Response = "Welcome to our insurance bot!",
            CreatedUtc = DateTime.UtcNow.AddMinutes(-9)
        };

        _db.Users.Add(user);
        _db.Conversations.Add(conversation1);
        _db.Conversations.Add(conversation2);
        await _db.SaveChangesAsync();

        var mockGemini = new Mock<IGeminiService>();
        mockGemini.Setup(g => g.AskAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync("Chat history for user 123: Hello, Welcome to our insurance bot");

        var handler = new ChatQueryHandler(mockGemini.Object, new UnitOfWork(_db));

        // Act
        var result = await handler.Handle(new ChatQuery(user.TelegramUserId, "Show my chat history"), default);

        // Assert
        result.Should().ContainEquivalentOf("Hello");
        result.Should().ContainEquivalentOf("Welcome to our insurance bot");
        result.Should().ContainEquivalentOf("Chat history");
    }

    [Fact]
    public async Task Returns_empty_chat_for_user_with_no_history()
    {
        // Arrange
        var user = new User 
        { 
            Id = Guid.NewGuid(), 
            TelegramUserId = 456
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var mockGemini = new Mock<IGeminiService>();
        mockGemini.Setup(g => g.AskAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync("No chat history found for user 456");

        var handler = new ChatQueryHandler(mockGemini.Object, new UnitOfWork(_db));

        // Act
        var result = await handler.Handle(new ChatQuery(user.TelegramUserId, "Show my chat history"), default);

        // Assert
        result.Should().ContainEquivalentOf("no chat history");
    }

    [Fact]
    public async Task Returns_error_for_nonexistent_user()
    {
        // Arrange
        var mockGemini = new Mock<IGeminiService>();
        mockGemini.Setup(g => g.AskAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync("User not found");

        var handler = new ChatQueryHandler(mockGemini.Object, new UnitOfWork(_db));

        // Act
        var result = await handler.Handle(new ChatQuery(999999, "Show my chat history"), default);

        // Assert
        result.Should().ContainEquivalentOf("user not found");
    }
} 