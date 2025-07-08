using CarInsuranceBot.Application.OCR;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Admin;
public class ToggleOcrSimulationCommandHandler(IMindeeService mindee) : IRequestHandler<ToggleOcrSimulationCommand, string>
{
    private readonly IMindeeService _mindee = mindee;

    public Task<string> Handle(ToggleOcrSimulationCommand c, CancellationToken _)
    {
        // Simulate passport extraction
        var passportData = _mindee.SimulateExtraction(DocumentType.Passport);

        // Simulate vehicle registration (driver license) extraction
        var driverLicenseData = _mindee.SimulateExtraction(DocumentType.VehicleRegistration);

        // Helper to format fields for Telegram
        string FormatFields(ExtractedDocument doc) =>
            string.Join("\n", doc.Fields.Select(f => $"{f.Key}: {f.Value}"));

        var result =
            "*[SIMULATED OCR]*\n\n" +
            "*Passport:*\n" +
            FormatFields(passportData) + "\n\n" +
            "*Vehicle Registration:*\n" +
            FormatFields(driverLicenseData);

        return Task.FromResult(result);
    }
}