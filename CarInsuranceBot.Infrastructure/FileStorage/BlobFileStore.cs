using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace CarInsuranceBot.Infrastructure.FileStorage;

/// <summary>
/// Stores user-supplied files in an Azure Blob container.
/// </summary>
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
        var containerName = cfg["BlobContainer"] ?? "tg-files";
        _container = blobServiceClient.GetBlobContainerClient(containerName);

        // idempotent – creates if missing, no-op otherwise
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    /// <summary>
    /// Downloads the file from Telegram and uploads it to Blob Storage.
    /// Returns the blob's URI (without a SAS token).
    /// </summary>
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

    /// <summary>
    /// Saves an in-memory PDF as &lt;guid&gt;.pdf (or supplied fileName) and returns the blob URI.
    /// </summary>
    public async Task<string> SavePdf(byte[] pdfBytes, string? fileName = null,
                                      CancellationToken ct = default)
    {
        fileName ??= $"{Guid.NewGuid():N}.pdf";

        BlobClient blob = _container.GetBlobClient(fileName);

        await using var ms = new MemoryStream(pdfBytes);
        await blob.UploadAsync(ms,
                               new BlobHttpHeaders { ContentType = "application/pdf" },
                               cancellationToken: ct);

        return blob.Uri.ToString();
    }
    public async Task<Stream> OpenReadAsync(string blobName, CancellationToken ct = default)
    {

        var bc = new BlobClient(
            $"{_opts.Value.ConnectionString}",   // paste the same one you use for upload
            "tg-files",                   // container
            "37a2d6048c57437bb5bcf163d96dfede.jpg");

        Console.WriteLine(await bc.ExistsAsync());
        // Add logging to see what blob name is being requested
        // Add logging to see what blob name is being requested
        Console.WriteLine();
        Console.WriteLine($"Trying to read blob: {blobName}");
        Console.WriteLine();
        return await _container.GetBlobClient(blobName).OpenReadAsync();
    }
}