using MediatR;
using TelegramFile = Telegram.Bot.Types.TGFile;   //  ← add alias

namespace CarInsuranceBot.Application.Commands.Upload;

public record UploadDocumentCommand(
    long ChatId,
    TelegramFile TelegramFile,
    bool IsPassport) : IRequest<string>;
