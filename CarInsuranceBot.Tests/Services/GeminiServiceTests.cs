namespace CarInsuranceBot.Tests.Services;

public class GeminiServiceTests
{
    [Fact]
    public async Task Generates_personalized_policy_text()
    {
        // Arrange
        var mockUow = new Mock<IUnitOfWork>();
        var mockUsers = new Mock<IUserRepository>();
        var mockConversations = new Mock<IConversationRepository>();

        // Setup user repository to return a user
        mockUsers.Setup(u => u.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new User { Id = Guid.NewGuid(), TelegramUserId = 12345 });

        // Setup conversations repository
        mockConversations.Setup(c => c.Add(It.IsAny<Conversation>()));

        // Setup unit of work
        mockUow.Setup(u => u.Users).Returns(mockUsers.Object);
        mockUow.Setup(u => u.Conversations).Returns(mockConversations.Object);
        mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Setup Gemini options
        var geminiOptions = new GeminiOptions
        {
            ApiKey = "test-api-key",
            Model = "gemini-2.0-flash"
        };
        var optionsMock = new Mock<IOptions<GeminiOptions>>();
        optionsMock.Setup(o => o.Value).Returns(geminiOptions);

        // Setup HTTP client with mock response
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"{
            ""candidates"": [{
                ""content"": {
                    ""parts"": [{
                        ""text"": ""This is a test response from Gemini""
                    }]
                }
            }]
        }", Encoding.UTF8, "application/json")
        };

        mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient("gemini")).Returns(httpClient);

        // Setup logger
        var logger = new Mock<ILogger<GeminiService>>();

        var service = new GeminiService(
            optionsMock.Object,
            httpClientFactory.Object,
            mockUow.Object,
            logger.Object);

        // Act
        var result = await service.AskAsync(12345, "Test prompt", default);

        // Assert
        result.Should().Be("This is a test response from Gemini");

        // Verify that the conversation was added
        mockConversations.Verify(c => c.Add(It.IsAny<Conversation>()), Times.Once);
        mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handles_api_error_gracefully()
    {
        // Arrange
        var mockUow = new Mock<IUnitOfWork>();
        var mockUsers = new Mock<IUserRepository>();
        var mockConversations = new Mock<IConversationRepository>();

        // Setup user repository to return a user
        mockUsers.Setup(u => u.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new User { Id = Guid.NewGuid(), TelegramUserId = 12345 });

        // Setup conversations repository
        mockConversations.Setup(c => c.Add(It.IsAny<Conversation>()));

        // Setup unit of work
        mockUow.Setup(u => u.Users).Returns(mockUsers.Object);
        mockUow.Setup(u => u.Conversations).Returns(mockConversations.Object);
        mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Setup Gemini options
        var geminiOptions = new GeminiOptions
        {
            ApiKey = "test-api-key",
            Model = "gemini-2.0-flash"
        };
        var optionsMock = new Mock<IOptions<GeminiOptions>>();
        optionsMock.Setup(o => o.Value).Returns(geminiOptions);

        // Setup HTTP client with error response
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request", Encoding.UTF8, "text/plain")
        };

        mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient("gemini")).Returns(httpClient);

        // Setup logger
        var logger = new Mock<ILogger<GeminiService>>();

        var service = new GeminiService(
            optionsMock.Object,
            httpClientFactory.Object,
            mockUow.Object,
            logger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.AskAsync(12345, "Test prompt", default));

        // Verify that no conversation was added due to the error
        mockConversations.Verify(c => c.Add(It.IsAny<Conversation>()), Times.Never);
        mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
} 