using MediatR;

namespace CarInsuranceBot.Application.Commands.Telegram.Start;

public record StartCommand(long ChatId, string? FirstName) : IRequest<string>;