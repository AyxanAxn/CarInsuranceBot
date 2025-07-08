using CarInsuranceBot.Application.Commands.Review;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;
using CarInsuranceBot.Tests.Helpers;

namespace CarInsuranceBot.Tests.Commands;

public class ExtractAndReviewCommandTests : IClassFixture<InMemoryFixture>
{
    private readonly InMemoryFixture _fx;

    public ExtractAndReviewCommandTests(InMemoryFixture fx) => _fx = fx;

    [Fact]
    public async Task Handle_ShowsBothPassportAndVehicleData()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var ocr = new Mock<IMindeeService>();
        var fileStore = new Mock<IFileStore>();
        var handler = new ExtractAndReviewCommandHandler(uow, ocr.Object, fileStore.Object);

        // Create user using helper
        var user = TestDataBuilder.CreateUser(1001, "John");
        user.Stage = RegistrationStage.WaitingForReview;
        uow.Users.Add(user);
        await uow.SaveChangesAsync(CancellationToken.None);

        // Create documents using helpers
        var passportDoc = TestDataBuilder.CreateDocument(user.Id, DocumentType.Passport);
        var vehicleDoc = TestDataBuilder.CreateDocument(user.Id, DocumentType.VehicleRegistration);
        uow.Documents.Add(passportDoc);
        uow.Documents.Add(vehicleDoc);
        await uow.SaveChangesAsync(CancellationToken.None);

        // Add extracted fields using helpers
        uow.ExtractedFields.Add(TestDataBuilder.CreateExtractedField(passportDoc.Id, "FullName", "John Doe"));
        uow.ExtractedFields.Add(TestDataBuilder.CreateExtractedField(passportDoc.Id, "PassportNumber", "P1234567"));
        uow.ExtractedFields.Add(TestDataBuilder.CreateExtractedField(vehicleDoc.Id, "VIN", "1HGBH41JXMN109186"));
        uow.ExtractedFields.Add(TestDataBuilder.CreateExtractedField(vehicleDoc.Id, "Make", "Honda"));
        await uow.SaveChangesAsync(CancellationToken.None);

        var command = new ExtractAndReviewCommand(vehicleDoc.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("Passport Data", result);
        Assert.Contains("Vehicle Registration Data", result);
        Assert.Contains("*FullName*: `John Doe`", result);
        Assert.Contains("*PassportNumber*: `P1234567`", result);
        Assert.Contains("*VIN*: `1HGBH41JXMN109186`", result);
        Assert.Contains("*Make*: `Honda`", result);
        Assert.Contains("Type *yes* to continue", result);
    }
} 