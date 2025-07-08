using MediatR;

namespace CarInsuranceBot.Application.Commands.Retry;

public record RetryCommand(long ChatId) : IRequest<string>; 