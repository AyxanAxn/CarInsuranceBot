namespace CarInsuranceBot.Tests.Commands;

public class UploadDocumentCommandTests : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db;
    public UploadDocumentCommandTests(InMemoryFixture fx) => _db = fx.Db;

    [Fact]
    public async Task First_passport_sets_stage_waiting_for_vehicle()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(22);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var uow = new UnitOfWork(_db);
        var store = new Mock<IFileStore>();
        var mediator = new Mock<IMediator>();
        var ocr = new Mock<IMindeeService>();
        var bot = new Mock<ITelegramBotClient>();

        store.Setup(s => s.SaveAsync(It.IsAny<TelegramFile>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync("stored_path");

        bot.Setup(b => b.DownloadFile(It.IsAny<string>(), It.IsAny<Stream>(), default))
           .Returns(Task.CompletedTask);

        mediator.Setup(m => m.Send(It.IsAny<ExtractAndReviewCommand>(), default))
                .ReturnsAsync("Review message");

        // Setup OCR to return non-null result
        ocr.Setup(o => o.ExtractAsync(It.IsAny<Stream>(), It.IsAny<DocumentType>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new ExtractedDocument(DocumentType.Passport) { Values = new Dictionary<string, string>() });

        var sut = new UploadDocumentCommandHandler(uow, store.Object, mediator.Object, ocr.Object, bot.Object);

        var cmd = new UploadDocumentCommand(22, new TelegramFile { FileId = "x", FilePath = "test_path" }, true);

        // Act
        var reply = await sut.Handle(cmd, default);

        // Assert
        reply.Should().ContainEquivalentOf("vehicle registration");
        user.Stage.ShouldBeWaitingForVehicle();
    }

    [Fact]
    public async Task Second_vehicle_registration_sets_stage_waiting_for_review()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(33);
        user.Stage = RegistrationStage.WaitingForVehicle;
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var uow = new UnitOfWork(_db);
        var store = new Mock<IFileStore>();
        var mediator = new Mock<IMediator>();
        var ocr = new Mock<IMindeeService>();
        var bot = new Mock<ITelegramBotClient>();

        store.Setup(s => s.SaveAsync(It.IsAny<TelegramFile>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync("stored_path");

        bot.Setup(b => b.DownloadFile(It.IsAny<string>(), It.IsAny<Stream>(), default))
           .Returns(Task.CompletedTask);

        mediator.Setup(m => m.Send(It.IsAny<ExtractAndReviewCommand>(), default))
                .ReturnsAsync("Review message");

        // Setup OCR to return non-null result
        ocr.Setup(o => o.ExtractAsync(It.IsAny<Stream>(), It.IsAny<DocumentType>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new ExtractedDocument(DocumentType.VehicleRegistration) { Values = new Dictionary<string, string>() });

        var sut = new UploadDocumentCommandHandler(uow, store.Object, mediator.Object, ocr.Object, bot.Object);

        var cmd = new UploadDocumentCommand(33, new TelegramFile { FileId = "y", FilePath = "test_path" }, false);

        // Act
        var reply = await sut.Handle(cmd, default);

        // Assert
        reply.Should().ContainEquivalentOf("Review message");
        user.Stage.ShouldBeWaitingForReview();
        user.UploadAttempts.Should().Be(0);
    }

    [Fact]
    public async Task Returns_error_when_user_not_found()
    {
        // Arrange
        var uow = new UnitOfWork(_db);
        var store = new Mock<IFileStore>();
        var mediator = new Mock<IMediator>();
        var ocr = new Mock<IMindeeService>();
        var bot = new Mock<ITelegramBotClient>();

        var sut = new UploadDocumentCommandHandler(uow, store.Object, mediator.Object, ocr.Object, bot.Object);

        var cmd = new UploadDocumentCommand(999999, new TelegramFile { FileId = "z", FilePath = "test_path" }, true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Handle(cmd, default));
    }

    [Fact]
    public async Task Returns_error_when_max_attempts_reached()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(44);
        user.UploadAttempts = 5;
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var uow = new UnitOfWork(_db);
        var store = new Mock<IFileStore>();
        var mediator = new Mock<IMediator>();
        var ocr = new Mock<IMindeeService>();
        var bot = new Mock<ITelegramBotClient>();

        var sut = new UploadDocumentCommandHandler(uow, store.Object, mediator.Object, ocr.Object, bot.Object);

        var cmd = new UploadDocumentCommand(44, new TelegramFile { FileId = "w", FilePath = "test_path" }, true);

        // Act
        var reply = await sut.Handle(cmd, default);

        // Assert
        reply.ShouldBeMaxAttemptsReached();
    }

    [Fact]
    public async Task Returns_error_for_duplicate_document()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(55);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var uow = new UnitOfWork(_db);
        var store = new Mock<IFileStore>();
        var mediator = new Mock<IMediator>();
        var ocr = new Mock<IMindeeService>();
        var bot = new Mock<ITelegramBotClient>();

        store.Setup(s => s.SaveAsync(It.IsAny<TelegramFile>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync("stored_path");

        bot.Setup(b => b.DownloadFile(It.IsAny<string>(), It.IsAny<Stream>(), default))
           .Returns(Task.CompletedTask);

        // Setup Documents repository to return true for duplicate check
        var documentsMock = new Mock<IDocumentRepository>();
        documentsMock.Setup(d => d.ExistsHashAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Users).Returns(uow.Users);
        uowMock.Setup(u => u.Documents).Returns(documentsMock.Object);
        uowMock.Setup(u => u.ExtractedFields).Returns(uow.ExtractedFields);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new UploadDocumentCommandHandler(uowMock.Object, store.Object, mediator.Object, ocr.Object, bot.Object);

        var cmd = new UploadDocumentCommand(55, new TelegramFile { FileId = "v", FilePath = "test_path" }, true);

        // Act
        var reply = await sut.Handle(cmd, default);

        // Assert
        reply.ShouldBeDuplicateDocumentMessage();
    }
}