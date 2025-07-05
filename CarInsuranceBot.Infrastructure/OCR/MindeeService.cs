using CarInsuranceBot.Infrastructure.Options;
using CarInsuranceBot.Application.OCR;
using CarInsuranceBot.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mindee.Input;
using Mindee;

namespace CarInsuranceBot.Infrastructure.OCR;

public class MindeeService : IMindeeService
{
    private readonly MindeeClientV2? _client;
    private readonly bool _simulationMode;
    private readonly ILogger<MindeeService> _log;
    private readonly string? _driverRegOptions;
    private readonly string? _vehiclePassOptions;
    public MindeeService(IOptions<MindeeOptions> opts,
                         IOptions<MindeeDriverRegOptions> driverRegOptions,
                         IOptions<MindeeVehiclePassportOptions> vehiclePassOptions,
                         ILogger<MindeeService> log)
    {
        _log = log;
        var apiKey = opts.Value.ApiKey?.Trim();
        _simulationMode = string.IsNullOrWhiteSpace(apiKey) || apiKey == "your-api-key-here";
        _driverRegOptions = driverRegOptions.Value.ModelId?.Trim();
        _vehiclePassOptions = vehiclePassOptions.Value.ModelId?.Trim();
        _log.LogInformation("MindeeService initialized. SimulationMode={Mode}",
                            _simulationMode);

        if (!_simulationMode)
            _client = new MindeeClientV2(apiKey);
    }

    // --------------------------------------------------------------------
    public async Task<ExtractedDocument> ExtractAsync(
        Stream image, DocumentType docType, CancellationToken ct)
    {
        _log.LogInformation("OCR requested for {Type}", docType);

        // ---- Simulation branch -------------------------------------------------
        if (_simulationMode || _client is null)
        {
            _log.LogWarning("OCR running in simulation mode");
            return SimulateExtraction(docType);
        }

        // ---- Real Mindee call --------------------------------------------------
        var src = new LocalInputSource(image, "doc.jpg");

        if (docType == DocumentType.Passport)
        { return await ExtractPassportAsync(src, ct); }

        return await ExtractVehicleAsync(src, ct);   // custom model when ready
    }

    private async Task<ExtractedDocument> ExtractPassportAsync(
    LocalInputSource src,
    CancellationToken ct)
    {
        //const string modelId = "e00165ea-ba06-4dba-a0fd-c2d4674d8edf";  // ← your passport model

        var resp = await _client!.EnqueueAndParseAsync(
                       src,
                       new InferenceOptionsV2(_driverRegOptions));

        var pred = resp.Inference.Result.Fields;
        
        pred.TryGetValue("surname", out var surname);
        pred.TryGetValue("passport_number",out var passportNumber);
        pred.TryGetValue("nationality", out var nationality);
        pred.TryGetValue("date_of_birth", out var dob);
        pred.TryGetValue("date_of_issue", out var doi);
        pred.TryGetValue("date_of_expiry", out var doe);
        pred.TryGetValue("country_of_issue", out var countryOfIssue);
        pred.TryGetValue("sex", out var sex);

        // “given_names” is an ARRAY → concatenate all items
        pred.TryGetValue("given_names", out var fGiven);

        return new ExtractedDocument(DocumentType.Passport)
            .Add("FullName", $"{fGiven} {surname}".Trim())
            .Add("PassportNumber", passportNumber!.ToString())
            .Add("Nationality", nationality!.ToString())
            .Add("DateOfBirth", dob!.ToString())
            .Add("DateOfIssue", doi!.ToString())
            .Add("DateOfExpiry", doe!.ToString())
            .Add("CountryOfIssue", countryOfIssue!.ToString())
            .Add("Sex", sex!.ToString());
    }

    // --------------------------------------------------------------------
    private async Task<ExtractedDocument> ExtractVehicleAsync(
      LocalInputSource src,
      CancellationToken ct)
    {
        //const string modelId = "7951ee45-4bb5-4a0d-9fe0-f1a1fa243a1e";

        try
        {
            var resp = await _client!.EnqueueAndParseAsync(
                           src,
                           new InferenceOptionsV2(_vehiclePassOptions));

            // 1️⃣  The prediction object produced by your custom model
            var pred = resp.Inference.Result.Fields;

            // 3️⃣  Read the values (null-safe in case the model didn’t return something)
            pred.TryGetValue("model", out var model);
            pred.TryGetValue("vin", out var vin);
            pred.TryGetValue("year", out var year);
            pred.TryGetValue("make", out var make);


            return new ExtractedDocument(DocumentType.VehicleRegistration)
            .Add("VIN", vin!.ToString())
            .Add("Make", make!.ToString())
            .Add("Model", model!.ToString())
            .Add("Year", year!.ToString());
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex,
                "Custom vehicle-registration model not found. Falling back to simulation.");
            return SimulateExtraction(DocumentType.VehicleRegistration);
        }
    }

    // --------------------------------------------------------------------
    private static ExtractedDocument SimulateExtraction(DocumentType docType)
    {
        return docType switch
        {
            DocumentType.Passport => new ExtractedDocument(docType)
                .Add("FullName", "John Doe")
                .Add("PassportNumber", "P1234567")
                .Add("Nationality", "USA"),

            DocumentType.VehicleRegistration => new ExtractedDocument(docType)
                .Add("VIN", "1HGBH41JXMN109186")
                .Add("Make", "Hundai")
                .Add("Model", "Santa FE")
                .Add("Year", "2025"),

            _ => new ExtractedDocument(docType)
                 .Add("Error", "Unsupported document type")
        };
    }
}