namespace CarInsuranceBot.Infrastructure.Options
{
    public record OpenAIOptions
    {
        public const string Section = "OpenAI";
        public string ApiKey { get; init; } = default!;
        public string BaseUrl { get; init; } = "https://api.openai.com";
    }
}