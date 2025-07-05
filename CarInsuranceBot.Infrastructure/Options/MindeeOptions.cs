namespace CarInsuranceBot.Infrastructure.Options;

public record MindeeOptions
{
    public const string Section = "Mindee";
    public string ApiKey { get; init; } = string.Empty;
}
public record MindeeDriverRegOptions
{
    public const string Section = "MindeeDriverRegistration";
    public string ModelId { get; init; } = string.Empty;
}
public record MindeeVehiclePassportOptions
{
    public const string Section = "MindeeVehiclePassport";
    public string ModelId { get; init; } = string.Empty;
}