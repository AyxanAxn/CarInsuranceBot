namespace CarInsuranceBot.Tests.Commands;
public class GeneratePolicyCommandTests(InMemoryFixture fx) : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db = fx.Db;

    [Fact]
    public async Task Marks_policy_as_issued_and_saves_pdf_path()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), TelegramUserId = 33 };
        _db.Users.Add(user);
        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Type = DocumentType.VehicleRegistration,
            Path = "test.pdf",
            UploadedUtc = DateTime.UtcNow
        };
        _db.Documents.Add(document);

        _db.ExtractedFields.Add(new ExtractedField
        {
            DocumentId = document.Id,
            FieldName = "VIN",
            FieldValue = "1ABC",
        });
        await _db.SaveChangesAsync();

        var uow = new UnitOfWork(_db);
        var store = new Mock<IFileStore>();
        store.Setup(s => s.SavePdf(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
             .ReturnsAsync("policy.pdf");

        var bot = new Mock<ITelegramBotClient>();

        var geminiService = new Mock<IGeminiService>();
        geminiService.Setup(g => g.AskAsync(
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync("Personalized policy text.");

        var sut = new GeneratePolicyCommandHandler(uow, store.Object, bot.Object, geminiService.Object);

        // Act
        await sut.Handle(new GeneratePolicyCommand(user.Id), default);

        // Assert
        var policy = _db.Policies.Single();
        policy.Status.Should().Be(PolicyStatus.Issued);
        policy.PdfPath.Should().Be("policy.pdf");
    }
}