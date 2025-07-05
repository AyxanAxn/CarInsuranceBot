using MediatR;

namespace CarInsuranceBot.Application.Commands.Policy;

public record ResendPolicyCommand(long ChatId) : IRequest<string>;
