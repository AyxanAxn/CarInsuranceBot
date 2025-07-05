using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Application.Commands.Upload;
using CarInsuranceBot.Application.Commands.Start;
using CarInsuranceBot.Domain.Enums;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;
using MediatR;

namespace CarInsuranceBot.Bot;

public class TelegramBotWorker(
    ITelegramBotClient bot,
    IServiceProvider sp,
    ILogger<TelegramBotWorker> log) : BackgroundService
{
    private readonly ITelegramBotClient _bot = bot;
    private readonly IServiceProvider _sp = sp;
    private readonly ILogger<TelegramBotWorker> _log = log;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _bot.StartReceiving(
            HandleUpdateAsync,   // update handler
            HandleErrorAsync,    // error handler
            receiverOptions,
            ct);

        _log.LogInformation("Telegram bot started.");
        await Task.Delay(Timeout.Infinite, ct);
    }


    // ---- Handlers ---------------------------------------------------------

    private async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken ct)
    {
        // 1️- text commands
        if (update.Message is { Text: { } text } msgTxt)
        {
            using var scope = _sp.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            switch (text)
            {
                case "/start":
                    var greeting = await mediator.Send(
                        new StartCommand(msgTxt.Chat.Id, msgTxt.From?.FirstName), ct);

                    await _bot.SendMessage(
                        chatId: msgTxt.Chat.Id,
                        text: greeting,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: ct);
                    break;

                    // TODO: /cancel, /resendpolicy
            }

            return;   // ⬅ we’ve handled this update
        }

        // 2️- photo uploads 
        if (update.Message?.Photo?.Any() == true)
        {
            var chatId = update.Message.Chat.Id;
            var photo = update.Message.Photo[^1];            // highest-res variant
            var tgFile = await _bot.GetFile(photo.FileId, ct);

            using var scope = _sp.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Determine what we expect based on user.Stage
            var user = await uow.Users.GetAsync(chatId, ct);
            bool isPassport = user?.Stage is RegistrationStage.WaitingForPassport or RegistrationStage.None;

            var reply = await mediator.Send(
                new UploadDocumentCommand(chatId, tgFile, isPassport), ct);

            await _bot.SendMessage(chatId, reply, parseMode: ParseMode.Markdown, cancellationToken: ct);
            return;
        }

    }


    private Task HandleErrorAsync(ITelegramBotClient _, Exception ex, CancellationToken ct)
    {
        string msg = ex switch
        {
            ApiRequestException apiEx => $"Telegram API Error:\n[{apiEx.ErrorCode}] {apiEx.Message}",
            _ => ex.ToString()
        };

        _log.LogError(msg);
        return Task.CompletedTask;
    }

}
