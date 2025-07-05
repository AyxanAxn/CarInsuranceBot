using MediatR;

namespace CarInsuranceBot.Application.Commands.Start;

/// <summary>
/// Fired when the Telegram user types /start.
/// Returns the greeting text to send back.
/// </summary>
public record StartCommand(long ChatId, string? FirstName) : IRequest<string>;
