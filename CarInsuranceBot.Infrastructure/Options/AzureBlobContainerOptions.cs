namespace CarInsuranceBot.Infrastructure.Options
{
    public sealed class AzureBlobContainerOptions
    {
        public const string Section = "BlobContainer";
        public string ContainerName { get; init; } = string.Empty;
    }
}