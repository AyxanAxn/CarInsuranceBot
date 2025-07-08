namespace CarInsuranceBot.Infrastructure.Options
{
    public sealed class AzureStorageOptions
    {
        public const string Section = "AzureStorage";
        public string ConnectionString { get; init; } = "";
    }
}