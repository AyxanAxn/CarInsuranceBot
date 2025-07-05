using TelegramFile = Telegram.Bot.Types.TGFile;

namespace CarInsuranceBot.Application.Common.Interfaces;

public interface IFileStore
{
    Task<string> SaveAsync(TelegramFile telegramFile, CancellationToken ct);
    Task<string> SavePdf(byte[] pdfBytes, string fileName);
}
