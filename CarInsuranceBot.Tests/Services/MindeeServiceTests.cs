namespace CarInsuranceBot.Tests.Services;
public class MindeeServiceTests
{
    [Fact]
    public async Task Simulation_mode_returns_mock_fields()
    {
        var mindeeOpts = Options.Create(new MindeeOptions { ApiKey = "" });
        var driverRegOpts = Options.Create(new MindeeDriverRegOptions { ModelId = "" });
        var vehiclePassOpts = Options.Create(new MindeeVehiclePassportOptions { ModelId = "" });
        var switchSvc = new Mock<IOcrSimulationSwitch>();
        switchSvc.Setup(s => s.ForceSimulation).Returns(true);

        var svc = new MindeeService(
            mindeeOpts,
            driverRegOpts,
            vehiclePassOpts,
            switchSvc.Object,
            NullLogger<MindeeService>.Instance
        );

        await using var ms = new MemoryStream(new byte[10]);
        var doc = await svc.ExtractAsync(ms, DocumentType.Passport, default);

        doc.Values.Should().ContainKey("FullName");
        doc.Values.Should().ContainKey("PassportNumber");
    }
}