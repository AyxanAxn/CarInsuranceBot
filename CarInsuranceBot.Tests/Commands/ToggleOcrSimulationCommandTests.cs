namespace CarInsuranceBot.Tests.Commands;

public class ToggleOcrSimulationCommandTests : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db;
    public ToggleOcrSimulationCommandTests(InMemoryFixture fx) => _db = fx.Db;

    [Fact]
    public async Task Toggles_ocr_simulation_mode_on()
    {
        // Arrange
        var switchMock = new Mock<IOcrSimulationSwitch>();
        switchMock.Setup(s => s.ForceSimulation).Returns(false);
        
        var handler = new ToggleOcrSimulationHandler(switchMock.Object);

        // Act
        var result = await handler.Handle(new ToggleOcrSimulationCommand(ChatId: 12345, Enable: true), default);

        // Assert
        result.Should().ContainEquivalentOf("🔧 OCR simulation has been *enabled*.");
        switchMock.VerifySet(s => s.ForceSimulation = true, Times.Once);
    }

    [Fact]
    public async Task Toggles_ocr_simulation_mode_off()
    {
        // Arrange
        var switchMock = new Mock<IOcrSimulationSwitch>();
        switchMock.Setup(s => s.ForceSimulation).Returns(true);
        
        var handler = new ToggleOcrSimulationHandler(switchMock.Object);

        // Act
        var result = await handler.Handle(new ToggleOcrSimulationCommand(ChatId: 12345, Enable: false), default);

        // Assert
        result.Should().Be("🔧 OCR simulation has been *disabled*.");
        switchMock.VerifySet(s => s.ForceSimulation = false, Times.Once);
    }
} 