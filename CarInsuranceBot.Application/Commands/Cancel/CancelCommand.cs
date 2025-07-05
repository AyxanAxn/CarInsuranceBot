using MediatR;

namespace CarInsuranceBot.Application.Commands.Flow;

public record CancelCommand(long ChatId) : IRequest<string>;
