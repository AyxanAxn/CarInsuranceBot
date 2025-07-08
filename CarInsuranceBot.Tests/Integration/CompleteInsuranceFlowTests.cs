namespace CarInsuranceBot.Tests.Integration;

public class CompleteInsuranceFlowTests : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db;
    public CompleteInsuranceFlowTests(InMemoryFixture fx) => _db = fx.Db;

    [Fact]
    public async Task Complete_insurance_flow_from_start_to_policy()
    {
        // Arrange - Setup all mocks
        var realUow = new UnitOfWork(_db);
        var store = new Mock<IFileStore>();
        var mediator = new Mock<IMediator>();
        var ocr = new Mock<IMindeeService>();
        var bot = new Mock<ITelegramBotClient>();
        var geminiService = new Mock<IGeminiService>();

        // Setup file storage
        store.Setup(s => s.SaveAsync(It.IsAny<TelegramFile>(), It.IsAny<System.Threading.CancellationToken>()))
             .ReturnsAsync("stored_path");
        store.Setup(s => s.SavePdf(It.IsAny<byte[]>(), It.IsAny<string>()))
             .ReturnsAsync("policies/test_policy.pdf");

        // Setup bot responses
        bot.Setup(b => b.DownloadFile(It.IsAny<string>(), It.IsAny<Stream>(), default))
           .Returns(Task.CompletedTask);

        // Setup OCR responses
        ocr.Setup(o => o.ExtractAsync(It.IsAny<Stream>(), DocumentType.Passport, default))
           .ReturnsAsync(new ExtractedDocument(DocumentType.Passport)
               .Add("FullName", "John Doe")
               .Add("PassportNumber", "P1234567"));
        ocr.Setup(o => o.ExtractAsync(It.IsAny<Stream>(), DocumentType.VehicleRegistration, default))
           .ReturnsAsync(new ExtractedDocument(DocumentType.VehicleRegistration)
               .Add("VIN", "1HGBH41JXMN109186")
               .Add("Make", "Honda")
               .Add("Model", "Civic"));

        // Setup mediator responses
        mediator.Setup(m => m.Send(It.IsAny<ExtractAndReviewCommand>(), default))
                .ReturnsAsync("Review message");

        // Setup Gemini service
        geminiService.Setup(g => g.AskAsync(
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<System.Threading.CancellationToken>()
        )).ReturnsAsync("Personalized policy text for John Doe");

        // Create a mock UnitOfWork that prevents duplicate detection
        var documentsMock = new Mock<IDocumentRepository>();
        documentsMock.Setup(d => d.ExistsHashAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false); // Never detect duplicates

        var extractedFieldsMock = new Mock<IExtractedFieldRepository>();
        extractedFieldsMock.Setup(e => e.FirstVinAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync("1HGBH41JXMN109186");

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Users).Returns(realUow.Users);
        uowMock.Setup(u => u.Documents).Returns(documentsMock.Object);
        uowMock.Setup(u => u.ExtractedFields).Returns(extractedFieldsMock.Object);
        uowMock.Setup(u => u.Policies).Returns(realUow.Policies);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        try
        {
            // Act 1: Start command - Use real UnitOfWork to create user
            var startHandler = new StartCommandHandler(realUow);
            var startResult = await startHandler.Handle(new StartCommand(12345, "John"), default);

            // Assert 1: User created
            startResult.ShouldBeWelcomeMessage();
            var user = await _db.Users.SingleOrDefaultAsync(u => u.TelegramUserId == 12345);
            user.Should().NotBeNull();
            user!.TelegramUserId.Should().Be(12345);

            // Act 2: Upload passport - Use mock UnitOfWork to prevent duplicates
            var uploadHandler = new UploadDocumentCommandHandler(uowMock.Object, store.Object, mediator.Object, ocr.Object, bot.Object);
            var uploadResult = await uploadHandler.Handle(
                new UploadDocumentCommand(12345, new TelegramFile { FileId = "passport", FilePath = "test_path" }, true),
                default);

            // Assert 2: Stage updated
            uploadResult.Should().ContainEquivalentOf("vehicle registration");
            user.Stage.ShouldBeWaitingForVehicle();

            // Act 3: Upload vehicle registration
            var uploadVehicleResult = await uploadHandler.Handle(
                new UploadDocumentCommand(12345, new TelegramFile { FileId = "vehicle", FilePath = "test_path" }, false),
                default);

            // Assert 3: Both documents processed
            uploadVehicleResult.Should().ContainEquivalentOf("review");

            // Act 4: Get price quote
            var priceHandler = new QuotePriceCommandHandler(uowMock.Object);
            var priceResult = await priceHandler.Handle(new QuotePriceCommand(user.Id), default);

            // Assert 4: Price returned
            priceResult.ShouldBePriceQuote();

            // Act 5: Generate policy - Use real UnitOfWork to save policy to database
            var policyHandler = new GeneratePolicyCommandHandler(realUow, store.Object, bot.Object, geminiService.Object);
            var policyResult = await policyHandler.Handle(new GeneratePolicyCommand(user.Id), default);

            // Assert 5: Policy generated
            policyResult.ShouldBeSuccessfulPolicyGeneration();

            // Verify final state
            var finalUser = await _db.Users.FindAsync(user.Id);
            finalUser!.Stage.ShouldBeFinished();

            var policies = _db.Policies.Where(p => p.UserId == user.Id).ToList();
            policies.Should().HaveCount(1);
            policies[0].Status.Should().Be(PolicyStatus.Issued);
        }
        finally
        {
            // Clean up test data
            _db.Users.RemoveRange(_db.Users);
            _db.Policies.RemoveRange(_db.Policies);
            _db.Documents.RemoveRange(_db.Documents);
            _db.ExtractedFields.RemoveRange(_db.ExtractedFields);
            _db.Conversations.RemoveRange(_db.Conversations);
            await _db.SaveChangesAsync();
        }
    }
}