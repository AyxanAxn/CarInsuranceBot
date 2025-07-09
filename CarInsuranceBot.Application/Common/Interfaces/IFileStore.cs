using TelegramFile = Telegram.Bot.Types.TGFile;

namespace CarInsuranceBot.Application.Common.Interfaces;

public interface IFileStore
{
    Task<string> SaveAsync(TelegramFile telegramFile, CancellationToken ct);
    Task<Stream> OpenReadAsync(string path, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
}