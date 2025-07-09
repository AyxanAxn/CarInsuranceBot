namespace CarInsuranceBot.Bot;

public class TelegramBotWorker(
    ITelegramBotClient bot,
    IServiceProvider sp,
    ILogger<TelegramBotWorker> log,
    IOptions<AdminOptions> adminOpts) : BackgroundService
{
    private readonly ITelegramBotClient _bot = bot;
    private readonly IServiceProvider _sp = sp;
    private readonly ILogger<TelegramBotWorker> _log = log;
    private readonly long[] _admins = adminOpts.Value.TelegramAdminIds;

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
        await _bot.SetMyCommands(
        [
            new BotCommand { Command = "start", Description = "Start the insurance process" },
            new BotCommand { Command = "cancel", Description = "Cancel and restart the flow" },
            new BotCommand { Command = "resendpolicy", Description = "Resend your last issued policy" }
        ]);

        _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = [] },
            ct);

        _log.LogInformation("Telegram bot started.");
        await Task.Delay(Timeout.Infinite, ct);
    }

    // --------------------------------------------------------------------
    private async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken ct)
    {
        try
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
                            string reply = await mediator.Send(
                                new EnsureFreshStartCommand(chatId, msgTxt.From?.FirstName), ct);

                            await _bot.SendMessage(chatId, reply,
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
                                "💰 The price is fixed at **100 USD**. Type *yes* whenever you're ready.",
                                cancellationToken: ct);
                            break;
                        }

                    case "/resendpolicy":
                        {
                            var reply = await mediator.Send(new ResendPolicyCommand(chatId), ct);
                            await _bot.SendMessage(chatId, reply, cancellationToken: ct);
                            break;
                        }
                    case "/cancel":
                        {
                            var reply = await mediator.Send(new CancelCommand(chatId), ct);
                            await _bot.SendMessage(chatId, reply, cancellationToken: ct, replyMarkup: MainMenu);
                            break;
                        }

                    case "retry" when user?.Stage == RegistrationStage.WaitingForReview:
                        {
                            var reply = await mediator.Send(new RetryCommand(chatId), ct);
                            await _bot.SendMessage(chatId, reply,
                                parseMode: ParseMode.Markdown,
                                cancellationToken: ct,
                                replyMarkup: MainMenu);
                            break;
                        }

                    // ---- Admin only --------------------------------------------------
                    #region Admin
                    case "/stats" when _admins.Contains(chatId):
                        {
                            var reply = await mediator.Send(new StatsQuery(chatId), ct);
                            _log.LogDebug("Sending admin stats message to {ChatId}: {Message}", chatId, reply);
                            await _bot.SendMessage(chatId, reply, parseMode: ParseMode.Markdown, cancellationToken: ct);
                            break;
                        }
                    case "/faillogs" when _admins.Contains(chatId):
                        {
                            var reply = await mediator.Send(new FailLogsQuery(chatId), ct);
                            _log.LogDebug("Sending admin fail logs message to {ChatId}: {Message}", chatId, reply);
                            await _bot.SendMessage(chatId, reply, parseMode: ParseMode.Markdown, cancellationToken: ct);
                            break;
                        }
                    case "/auditlogs" when _admins.Contains(chatId):
                        {
                            var reply = await mediator.Send(new AuditLogsQuery(chatId), ct);
                            _log.LogDebug("Sending admin audit logs message to {ChatId}: {Message}", chatId, reply);
                            await _bot.SendMessage(chatId, reply, parseMode: ParseMode.Markdown, cancellationToken: ct);
                            break;
                        }
                    case "/adminhelp" when _admins.Contains(chatId):
                        {
                            var reply = await mediator.Send(new AdminHelpQuery(chatId), ct);
                            _log.LogDebug("Sending admin help message to {ChatId}: {Message}", chatId, reply);
                            await _bot.SendMessage(chatId, reply, parseMode: ParseMode.Markdown, cancellationToken: ct);
                            break;
                        }
                    case var cmd when cmd.StartsWith("/simulateocr") && _admins.Contains(chatId):
                        {
                            var reply = await mediator.Send(new ToggleOcrSimulationCommand(), ct);
                            _log.LogDebug("Sending admin simulate OCR message to {ChatId}: {Message}", chatId, reply);
                            await _bot.SendMessage(chatId, reply, parseMode: ParseMode.Markdown, cancellationToken: ct);
                            break;
                        }
                    #endregion
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
                                _log.LogWarning("ChatQuery failed for user {ChatId}, using fallback message", chatId);
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
                
                try
                {
                    var tgFile = await _bot.GetFile(photo.FileId, ct);

                    using var scope = _sp.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var user = await uow.Users.GetAsync(chatId, ct);
                    
                    // Determine if this is a passport or vehicle registration upload
                    bool isPassport;
                    if (user == null)
                    {
                        // New user - assume passport
                        isPassport = true;
                    }
                    else if (user.Stage == RegistrationStage.WaitingForPassport)
                    {
                        // User is explicitly waiting for passport
                        isPassport = true;
                    }
                    else if (user.Stage == RegistrationStage.WaitingForVehicle)
                    {
                        // User is waiting for vehicle registration
                        isPassport = false;
                    }
                    else
                    {
                        // For any other stage, check if they already have a passport
                        var documents = await uow.Documents.GetByUserAsync(user.Id, ct);
                        var hasPassport = documents.Any(d => d.Type == DocumentType.Passport);
                        isPassport = !hasPassport;
                    }

                    var reply = await mediator.Send(
                        new UploadDocumentCommand(chatId, tgFile, isPassport), ct);

                    await _bot.SendMessage(chatId, reply,
                        parseMode: ParseMode.Markdown, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error processing photo upload for chat {ChatId}", chatId);
                    await _bot.SendMessage(chatId, 
                        "❌ Sorry, there was an error processing your photo. Please try again.", 
                        cancellationToken: ct);
                }
            }
        }
        catch (Exception ex)
        {
            using var scope = _sp.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            uow.ErrorLogs.Add(new CarInsuranceBot.Domain.Entities.ErrorLog
            {
                Message = ex.Message,
                StackTrace = ex.ToString(),
                LoggedUtc = DateTime.UtcNow
            });
            await uow.SaveChangesAsync(ct);
            _log.LogError(ex, "Exception caught in HandleUpdateAsync and logged to ErrorLog table.");
            // Optionally, notify the user
            if (update.Message != null)
            {
                await _bot.SendMessage(update.Message.Chat.Id, "An error occurred. The team has been notified.", cancellationToken: ct);
            }
        }
    }

    // --------------------------------------------------------------------
    private async Task HandleErrorAsync(ITelegramBotClient _, Exception ex, CancellationToken ct)
    {
        string msg = ex switch
        {
            ApiRequestException apiEx => $"Telegram API Error:\n[{apiEx.ErrorCode}] {apiEx.Message}",
            _ => ex.ToString()
        };
        _log.LogError(msg);

        // Log to ErrorLog table
        using var scope = _sp.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        uow.ErrorLogs.Add(new CarInsuranceBot.Domain.Entities.ErrorLog
        {
            Message = ex.Message,
            StackTrace = ex.ToString(),
            LoggedUtc = DateTime.UtcNow
        });
        await uow.SaveChangesAsync(ct);
    }
}
