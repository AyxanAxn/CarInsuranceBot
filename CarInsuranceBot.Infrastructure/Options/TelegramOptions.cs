namespace CarInsuranceBot.Infrastructure.Options
{
    public record TelegramOptions
    {
        public const string Section = "Telegram";
        public string BotToken { get; init; } = default!;
    }
}