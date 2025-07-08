namespace CarInsuranceBot.Tests.Commands;

public class ToggleOcrSimulationCommandTests
{
    [Fact]
    public async Task Returns_simulated_passport_and_vehicle_data()
    {
        // Arrange
        var mindeeMock = new Mock<IMindeeService>();
        mindeeMock.Setup(m => m.SimulateExtraction(Domain.Enums.DocumentType.Passport))
            .Returns(new ExtractedDocument(Domain.Enums.DocumentType.Passport)
                .Add("FullName", "John Doe")
                .Add("PassportNumber", "P1234567")
                .Add("Nationality", "USA"));

        mindeeMock.Setup(m => m.SimulateExtraction(Domain.Enums.DocumentType.VehicleRegistration))
            .Returns(new ExtractedDocument(Domain.Enums.DocumentType.VehicleRegistration)
                .Add("VIN", "1HGBH41JXMN109186")
                .Add("Make", "Hundai")
                .Add("Model", "Santa FE")
                .Add("Year", "2025"));

        var handler = new ToggleOcrSimulationCommandHandler(mindeeMock.Object);

        // Act
        var result = await handler.Handle(new ToggleOcrSimulationCommand(), default);

        // Assert
        result.Should().Contain("SIMULATED OCR");
        result.Should().Contain("Passport:");
        result.Should().Contain("FullName: John Doe");
        result.Should().Contain("Vehicle Registration:");
        result.Should().Contain("VIN: 1HGBH41JXMN109186");
    }
}