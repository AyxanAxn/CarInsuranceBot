using CarInsuranceBot.Application.Common.Interfaces;
using TelegramFile = Telegram.Bot.Types.TGFile;
using Telegram.Bot;

namespace CarInsuranceBot.Infrastructure.FileStorage;

public sealed class DiskFileStore : IFileStore
{
    private readonly ITelegramBotClient _bot;
    private readonly string _basePath;

    public DiskFileStore(ITelegramBotClient bot)
    {
        _bot = bot;
        _basePath = Path.Combine(AppContext.BaseDirectory, "Files");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(TelegramFile tgFile, CancellationToken ct)
    {
        
        
        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(tgFile.FilePath)}";
        string fullPath = Path.Combine(_basePath, fileName);

        await using var fs = File.OpenWrite(fullPath);
        await _bot.DownloadFile(tgFile.FilePath!, fs, ct);

        return fullPath;
    }
    public Task<string> SavePdf(byte[] pdfBytes, string fileName)
    {
        // Example implementation: save to disk and return the path
        var path = Path.Combine("./policies", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, pdfBytes);
        return Task.FromResult(path);
    }
}