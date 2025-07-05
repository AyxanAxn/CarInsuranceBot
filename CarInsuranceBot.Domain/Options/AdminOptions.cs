namespace CarInsuranceBot.Domain.Options;

public sealed class AdminOptions
{
    public const string Section = "Admin";
    public long[] TelegramAdminIds { get; set; } = [];
}
