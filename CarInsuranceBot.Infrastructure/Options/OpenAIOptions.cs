namespace CarInsuranceBot.Infrastructure.Options
{
    public record OpenAIOptions
    {
        public const string Section = "OpenAI";
        public string ApiKey { get; init; } = default!;
        public string Model { get; set; } = "gpt-3.5-turbo";
        public string BaseUrl { get; init; } = "https://api.openai.com";
    }
}