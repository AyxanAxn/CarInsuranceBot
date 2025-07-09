using MediatR;

namespace CarInsuranceBot.Application.Commands.ResendPolicy;

public record ResendPolicyCommand(long ChatId) : IRequest<string>;
