namespace CarInsuranceBot.Tests.Queries;
public class ExtractAndReviewQueryTests(InMemoryFixture fx) : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db = fx.Db;

    [Fact]
    public async Task Builds_review_message_and_persists_fields()
    {
        // Create a real test file
        var testFilePath = Path.GetTempFileName();
        try
        {
            // Write some dummy content to the file
            await File.WriteAllBytesAsync(testFilePath, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }); // JPEG header

            var user = new User { Id = Guid.NewGuid(), TelegramUserId = 44 };
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Path = testFilePath,
                Type = DocumentType.Passport
            };
            _db.Users.Add(user);
            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

            var uow = new UnitOfWork(_db);

            var ocr = new Mock<IMindeeService>();
            ocr.Setup(o => o.ExtractAsync(It.IsAny<Stream>(), DocumentType.Passport, default))
               .ReturnsAsync(new ExtractedDocument(DocumentType.Passport)
                            .Add("FullName", "John Doe")
                            .Add("PassportNumber", "123"));

            var handler = new ExtractAndReviewCommandHandler(uow, ocr.Object);

            var txt = await handler.Handle(new ExtractAndReviewCommand(doc.Id), default);

            txt.Should().Contain("FullName");
            _db.ExtractedFields.Count().Should().Be(2);
        }
        finally
        {
            // Clean up the test file
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }
    }
}