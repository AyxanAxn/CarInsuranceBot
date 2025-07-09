namespace CarInsuranceBot.Infrastructure.Options;
public record GeminiOptions
{
    public const string Section = "Gemini";
    public string ApiKey { get; init; } = default!;
    public string Model { get; set; } = "gemini-2.0-flash";
    public string BaseUrl { get; init; } = "https://api.openai.com";
}