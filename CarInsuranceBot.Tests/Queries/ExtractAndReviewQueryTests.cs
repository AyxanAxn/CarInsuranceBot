using CarInsuranceBot.Infrastructure.FileStorage;

namespace CarInsuranceBot.Tests.Queries;
public class ExtractAndReviewQueryTests(InMemoryFixture fx) : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db = fx.Db;

    [Fact]
    public async Task Builds_review_message_from_existing_fields()
    {
        // Create user and documents using helpers
        var user = TestDataBuilder.CreateUser(44);
        var passportDoc = TestDataBuilder.CreateDocument(user.Id, DocumentType.Passport);
        var vehicleDoc = TestDataBuilder.CreateDocument(user.Id, DocumentType.VehicleRegistration);
        
        _db.Users.Add(user);
        _db.Documents.Add(passportDoc);
        _db.Documents.Add(vehicleDoc);
        await _db.SaveChangesAsync();

        // Add extracted fields using helpers
        _db.ExtractedFields.Add(TestDataBuilder.CreateExtractedField(passportDoc.Id, "FullName", "John Doe"));
        _db.ExtractedFields.Add(TestDataBuilder.CreateExtractedField(passportDoc.Id, "PassportNumber", "123"));
        _db.ExtractedFields.Add(TestDataBuilder.CreateExtractedField(vehicleDoc.Id, "VIN", "1HGBH41JXMN109186"));
        _db.ExtractedFields.Add(TestDataBuilder.CreateExtractedField(vehicleDoc.Id, "Make", "Honda"));
        await _db.SaveChangesAsync();

        var uow = new UnitOfWork(_db);
        var ocr = new Mock<IMindeeService>();
        var fileStore = new Mock<IFileStore>();

        var handler = new ExtractAndReviewCommandHandler(uow, ocr.Object, fileStore.Object);

        var txt = await handler.Handle(new ExtractAndReviewCommand(vehicleDoc.Id), default);

        txt.Should().Contain("*FullName*");
        txt.Should().Contain("*VIN*");
        txt.Should().Contain("Passport Data");
        txt.Should().Contain("Vehicle Registration Data");
        txt.Should().Contain("Type *yes* to continue");
    }
}