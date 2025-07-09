using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;

namespace CarInsuranceBot.Infrastructure.FileStorage;

// Stores user-supplied files in an Azure Blob container.
public sealed class BlobFileStore : IFileStore
{
    private readonly ITelegramBotClient _bot;
    private readonly BlobContainerClient _container;
    private readonly IOptions<AzureStorageOptions> _opts;
    public BlobFileStore(IOptions<AzureStorageOptions> opts,
                        IOptions<AzureBlobContainerOptions> containerOpts,
                        ITelegramBotClient bot,
                         BlobServiceClient blobServiceClient,
                         IConfiguration cfg)
    {
        _bot = bot;
        _opts = opts;
        // container name can live in appsettings / portal
        var containerName = containerOpts.Value.FilesContainerName ?? "tg-files";
        _container = blobServiceClient.GetBlobContainerClient(containerName);

        // idempotent – creates if missing, no-op otherwise
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    /// <summary>
    /// Downloads the file from Telegram and uploads it to Blob Storage.
    /// Returns the blob's URI (without a SAS token).
    /// </summary>
    #region SaveAsync
    public async Task<string> SaveAsync(TelegramFile tgFile, CancellationToken ct)
    {
        var ext = Path.GetExtension(tgFile.FilePath);
        var blobName = $"{Guid.NewGuid():N}{ext}";

        BlobClient blob = _container.GetBlobClient(blobName);

        await using var download = new MemoryStream();
        await _bot.DownloadFile(tgFile.FilePath!, download, ct);

        download.Position = 0;                     // rewind
        await blob.UploadAsync(download, overwrite: false, cancellationToken: ct);

        return blob.Uri.ToString();
    }
    #endregion SaveAsync

    // Open a blob for reading.
    #region OpenReadAsync
    public async Task<Stream> OpenReadAsync(string path, CancellationToken ct = default)
    {
        var blobName = path;
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            blobName = uri.Segments.Last();
        }
        var blob = _container.GetBlobClient(blobName);

        return await _container.GetBlobClient(blobName).OpenReadAsync();
    }
    #endregion OpenReadAsync

    //Remove a file from Azure Blob Storage by its path.
    #region DeleteAsync
    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        // Extract blob name from the full URI if needed
        var blobName = path;
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            blobName = uri.Segments.Last();
        }
        var blob = _container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }
    #endregion DeleteAsync
}