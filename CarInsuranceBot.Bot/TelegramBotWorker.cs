using CarInsuranceBot.Application.Common.Utils;

namespace CarInsuranceBot.Bot;

public sealed class TelegramBotWorker(
    ITelegramBotClient bot,
    IServiceProvider sp,
    ILogger<TelegramBotWorker> log,
    IOptions<AdminOptions> adminOpts) : BackgroundService
{
    #region Fields & Constructor
    // Core dependencies and admin list
    private readonly ITelegramBotClient _bot = bot;
    private readonly IServiceProvider _sp = sp;
    private readonly ILogger<TelegramBotWorker> _log = log;
    private readonly long[] _admins = adminOpts.Value.TelegramAdminIds;

    // Main menu keyboard shown to users
    private static readonly ReplyKeyboardMarkup MainMenu = new(new[]
    {
        new[] { new KeyboardButton("/start"), new KeyboardButton("/cancel"), new KeyboardButton("/resendpolicy") }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = false
    };
    #endregion

    #region Worker Startup
    /// <summary>
    /// Starts the Telegram bot, sets up commands, and begins listening for updates.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await _bot.SetMyCommands(
        [
            new BotCommand("start"       , "Start the insurance process"),
            new BotCommand("cancel"      , "Cancel and restart the flow"),
            new BotCommand("resendpolicy", "Resend your last issued policy")
        ], cancellationToken: ct);

        _bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = [] },
            ct);

        _log.LogInformation("Telegram bot started.");
        await Task.Delay(Timeout.Infinite, ct);
    }
    #endregion Worker Startup

    #region Update Routing
    /// <summary>
    /// Routes each incoming update to either the photo or text handler.
    /// </summary>
    private Task HandleUpdateAsync(ITelegramBotClient _, Update u, CancellationToken ct)
        => u.Message?.Photo?.Any() == true
            ? HandlePhotoAsync(u, ct)
            : HandleTextAsync(u, ct);
    #endregion Update Routing

    #region Text Message Handling
    /// <summary>
    /// Handles all incoming text messages from users. Routes commands and user input to the appropriate CQRS handler.
    /// </summary>
    private async Task HandleTextAsync(Update update, CancellationToken ct)
    {
        if (update.Message is not { Text: { } text } msg) return;

        try
        {
            // Create a DI scope for this message
            await using var scope = _sp.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var chatId = msg.Chat.Id;
            var user = await uow.Users.GetAsync(chatId, ct);

            // Route the message to the correct handler based on text and user state
            switch (text.ToLowerInvariant())
            {
                #region User Commands
                case "/start":
                    // Start or reset the insurance flow for this user
                    await Reply(await mediator.Send(
                        new EnsureFreshStartCommand(chatId, msg.From?.FirstName), ct));
                    break;

                case "yes" when user?.Stage == RegistrationStage.WaitingForReview:
                    // User confirms review, quote the price
                    await Reply(await mediator.Send(new QuotePriceCommand(user.Id), ct));
                    break;

                case "yes" when user?.Stage == RegistrationStage.WaitingForPayment:
                    // User confirms payment, generate the policy
                    await Reply(await mediator.Send(new GeneratePolicyCommand(user.Id), ct));
                    break;

                case "no" when user?.Stage == RegistrationStage.WaitingForPayment:
                    // User declines payment, remind about the fixed price
                    await Reply("💰 The price is fixed at **100 USD**. Type *yes* whenever you're ready.");
                    break;

                case "/resendpolicy":
                    // User requests to resend their last policy
                    await Reply(await mediator.Send(new ResendPolicyCommand(chatId), ct));
                    break;

                case "/cancel":
                    // User cancels the current session
                    await Reply(await mediator.Send(new CancelCommand(chatId), ct));
                    break;

                case "retry" when user?.Stage == RegistrationStage.WaitingForReview:
                    // User wants to retry the review step
                    await Reply(await mediator.Send(new RetryCommand(chatId), ct));
                    break;
                #endregion User Commands

                #region Admin Commands
                case "/stats" when IsAdmin(chatId):
                    // Admin: show system statistics
                    await Reply(await mediator.Send(new StatsQuery(chatId), ct)); break;
                case "/faillogs" when IsAdmin(chatId):
                    // Admin: show failed policy logs
                    await Reply(await mediator.Send(new FailLogsQuery(chatId), ct)); break;
                case "/auditlogs" when IsAdmin(chatId):
                    // Admin: show audit logs
                    await Reply(await mediator.Send(new AuditLogsQuery(chatId), ct)); break;
                case "/adminhelp" when IsAdmin(chatId):
                    // Admin: show help for admin commands
                    await Reply(await mediator.Send(new AdminHelpQuery(chatId), ct)); break;
                case var cmd when cmd.StartsWith("/simulateocr") && IsAdmin(chatId):
                    // Admin: toggle OCR simulation mode
                    await Reply(await mediator.Send(new ToggleOcrSimulationCommand(), ct)); break;
                #endregion Admin Commands

                #region Fallback (AI Chat)
                default:
                    // If not a recognized command, send to AI chat handler
                    var ai = await SafeChatAsync(mediator, chatId, text, ct);
                    await Reply(CarInsuranceBot.Application.Common.Utils.MarkdownHelper.EscapeMarkdown(ai));
                    break;
                    #endregion  Fallback (AI Chat)
            }

            // Helper for sending replies with consistent markup and keyboard
            async Task Reply(string message) =>
            await _bot.SendMessage(chatId, MarkdownHelper.EscapeMarkdown(message),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: MainMenu,
                cancellationToken: ct);
        }
        catch (Exception ex) { await LogAndAcknowledgeError(update, ex, ct); }
    }
    #endregion Text Message Handling

    #region Photo Message Handling
    /// <summary>
    /// Handles photo uploads from users. Determines if the photo is a passport or vehicle registration and routes to the upload handler.
    /// </summary>
    private async Task HandlePhotoAsync(Update update, CancellationToken ct)
    {
        var chatId = update.Message!.Chat.Id;

        try
        {
            var photo = update.Message.Photo!.Last();
            var tgFile = await _bot.GetFile(photo.FileId, ct);

            await using var scope = _sp.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var user = await uow.Users.GetAsync(chatId, ct);
            // Decide if this upload is a passport or vehicle registration
            var isPassport = await DetermineIfPassportAsync(user!, uow, ct);

            var reply = await mediator.Send(new UploadDocumentCommand(chatId, tgFile, isPassport), ct);

            await _bot.SendMessage(chatId, MarkdownHelper.EscapeMarkdown(reply),
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: ct);
        }
        catch (Exception ex) { await LogAndAcknowledgeError(update, ex, ct); }
    }

    /// <summary>
    /// Determines if the next uploaded document should be treated as a passport.
    /// </summary>
    private static async Task<bool> DetermineIfPassportAsync(CarInsuranceBot.Domain.Entities.User user, IUnitOfWork uow, CancellationToken ct)
    {
        if (user == null)
            return true;
        if (user.Stage == RegistrationStage.WaitingForPassport)
            return true;
        if (user.Stage == RegistrationStage.WaitingForVehicle)
            return false;
        // If user already has a passport, next should be vehicle registration
        return !(await uow.Documents.GetByUserAsync(user.Id, ct)).Any(d => d.Type == DocumentType.Passport);
    }
    #endregion  Photo Message Handling

    #region Helpers
    /// <summary>
    /// Fallback for AI chat (OpenAI/Gemini) if no command matches.
    /// </summary>
    private async Task<string> SafeChatAsync(IMediator m, long chat, string text, CancellationToken ct)
    {
        try
        {
            return await m.Send(new ChatQuery(chat, text), ct);
        }
        catch
        {
            return "🤖 Sorry, I'm overloaded. Please try again soon.";
        }
    }

    /// <summary>
    /// Checks if a user is an admin based on their Telegram chat ID.
    /// </summary>
    private bool IsAdmin(long chatId) => Array.IndexOf(_admins, chatId) >= 0;
    #endregion Helpers

    #region Error Handling
    /// <summary>
    /// Handles and logs all unhandled exceptions, optionally notifies the user.
    /// </summary>
    private async Task HandleErrorAsync(ITelegramBotClient _, Exception ex, CancellationToken ct)
        => await LogAndAcknowledgeError(null, ex, ct);

    /// <summary>
    /// Logs the error to the database and optionally notifies the user.
    /// </summary>
    private async Task LogAndAcknowledgeError(Update? update, Exception ex, CancellationToken ct)
    {
        _log.LogError(ex, "Unhandled exception");

        using var scope = _sp.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        uow.ErrorLogs.Add(new ErrorLog
        {
            Message = ex.Message,
            StackTrace = ex.ToString(),
            LoggedUtc = DateTime.UtcNow
        });
        await uow.SaveChangesAsync(ct);

        // optional user notification
        if (update?.Message != null)
        {
            await _bot.SendMessage(update.Message.Chat.Id,
                "❌ An internal error occurred — our team has been notified.",
                cancellationToken: ct);
        }
    }
    #endregion Error Handling
}