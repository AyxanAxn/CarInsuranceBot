namespace CarInsuranceBot.Bot;

public class TelegramBotWorker(
    ITelegramBotClient bot,
    IServiceProvider sp,
    ILogger<TelegramBotWorker> log) : BackgroundService
{
    private readonly ITelegramBotClient _bot = bot;
    private readonly IServiceProvider _sp = sp;
    private readonly ILogger<TelegramBotWorker> _log = log;

    private static readonly ReplyKeyboardMarkup MainMenu = new(new[]
    {
        new KeyboardButton[] { "/start", "/cancel", "/resendpolicy" }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = false
    };

    // --------------------------------------------------------------------
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await _bot.SetMyCommands(new[]
        {
            new BotCommand { Command = "start", Description = "Start the insurance process" },
            new BotCommand { Command = "cancel", Description = "Cancel and restart the flow" },
            new BotCommand { Command = "resendpolicy", Description = "Resend your last issued policy" }
        });

        _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
            ct);

        _log.LogInformation("Telegram bot started.");
        await Task.Delay(Timeout.Infinite, ct);
    }

    // --------------------------------------------------------------------
    private async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken ct)
    {
        // ---------- TEXT messages --------------------------------------
        if (update.Message is { Text: { } text } msgTxt)
        {
            var chatId = msgTxt.Chat.Id;                     // ① capture once
            using var scope = _sp.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var user = await uow.Users.GetAsync(chatId, ct);   // nullable

            switch (text.ToLowerInvariant())
            {
                case "/start":
                    {
                        var greeting = await mediator.Send(
                            new StartCommand(chatId, msgTxt.From?.FirstName), ct);

                        await _bot.SendMessage(chatId, greeting,
                            parseMode: ParseMode.Markdown, cancellationToken: ct,
                            replyMarkup: MainMenu);
                        break;
                    }

                case "yes" when user?.Stage == RegistrationStage.WaitingForReview:
                    {
                        var price = await mediator.Send(new QuotePriceCommand(user.Id), ct);
                        await _bot.SendMessage(chatId, price,
                            parseMode: ParseMode.Markdown, cancellationToken: ct);
                        break;
                    }

                case "yes" when user?.Stage == RegistrationStage.WaitingForPayment:
                    {
                        var done = await mediator.Send(new GeneratePolicyCommand(user.Id), ct);
                        await _bot.SendMessage(chatId, done, cancellationToken: ct);
                        break;
                    }

                case "no" when user?.Stage == RegistrationStage.WaitingForPayment:
                    {
                        await _bot.SendMessage(chatId,
                            "The price is fixed at 100 USD. Type *yes* whenever you're ready.",
                            parseMode: ParseMode.Markdown, cancellationToken: ct);
                        break;
                    }

                case "/resendpolicy":
                    {
                        var reply = await mediator.Send(new ResendPolicyCommand(chatId), ct);
                        // The handler already sends the PDF; we just send the textual reply.
                        await _bot.SendMessage(chatId, reply, cancellationToken: ct);
                        break;
                    }
                case "/cancel":
                    {
                        var reply = await mediator.Send(new CancelCommand(chatId), ct);
                        await _bot.SendMessage(chatId, reply, cancellationToken: ct, replyMarkup: MainMenu);
                        break;
                    }


                default:
                    {
                        string aiReply;
                        try
                        {
                            aiReply = await mediator.Send(new ChatQuery(chatId, text), ct);
                        }
                        catch
                        {
                            aiReply = "🤖 Sorry, I'm a bit overloaded. Please try again in a minute.";
                        }
                        await _bot.SendMessage(chatId, aiReply, cancellationToken: ct);
                        break;
                    }

            }

            return; // handled text message
        }

        // ---------- PHOTO uploads --------------------------------------
        if (update.Message?.Photo?.Any() == true)
        {
            var chatId = update.Message.Chat.Id;
            var photo = update.Message.Photo[^1];                 // highest-res
            var tgFile = await _bot.GetFile(photo.FileId, ct);

            using var scope = _sp.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var user = await uow.Users.GetAsync(chatId, ct);
            bool isPassport = user?.Stage is RegistrationStage.WaitingForPassport or RegistrationStage.None;

            var reply = await mediator.Send(
                new UploadDocumentCommand(chatId, tgFile, isPassport), ct);

            await _bot.SendMessage(chatId, reply,
                parseMode: ParseMode.Markdown, cancellationToken: ct);
        }
    }

    // --------------------------------------------------------------------
    private Task HandleErrorAsync(ITelegramBotClient _, Exception ex, CancellationToken __)
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
