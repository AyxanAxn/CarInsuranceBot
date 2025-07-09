namespace CarInsuranceBot.Infrastructure.Options
{
    public sealed class AzureBlobContainerOptions
    {
        public const string Section = "BlobContainer";
        public string PolicyContainerName { get; init; } = string.Empty;
        public string FilesContainerName { get; init; } = string.Empty;
    }
}