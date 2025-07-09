namespace CarInsuranceBot.Infrastructure.FileStorage;

public sealed class PolicyBlobFileStore : IPolicyFileStore
{
    private readonly BlobContainerClient _container;
    private readonly IOptions<AzureStorageOptions> _opts;
    public PolicyBlobFileStore(IOptions<AzureStorageOptions> opts, IOptions<AzureBlobContainerOptions> containerOpts, BlobServiceClient blobServiceClient)
    {
        _opts = opts;
        var containerName = containerOpts.Value.PolicyContainerName ?? "policies";
        _container = blobServiceClient.GetBlobContainerClient(containerName);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> SavePdf(byte[] pdfBytes, string? fileName = null, CancellationToken ct = default)
    {
        fileName ??= $"{Guid.NewGuid():N}.pdf";
        BlobClient blob = _container.GetBlobClient(fileName);
        await using var ms = new MemoryStream(pdfBytes);
        await blob.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/pdf" }, cancellationToken: ct);
        return blob.Uri.ToString();
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var blobName = path;
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            blobName = uri.Segments.Last();
        }
        var blob = _container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }

    // Not needed for policy PDFs
    public Task<string> SaveAsync(Telegram.Bot.Types.TGFile telegramFile, CancellationToken ct) => Task.FromResult("");
    public async Task<Stream> OpenReadAsync(string path, CancellationToken ct = default)
    {
        var blobName = path;
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            blobName = uri.Segments.Last();
        }
        BlobClient blob = _container.GetBlobClient(blobName);

        return await _container.GetBlobClient(blobName).OpenReadAsync();
    }
} 