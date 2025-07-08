using MediatR;

namespace CarInsuranceBot.Application.Commands.Cancel;

public record CancelCommand(long ChatId) : IRequest<string>;
