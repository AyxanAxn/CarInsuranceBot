using MediatR;

namespace CarInsuranceBot.Application.Commands.Start;

public record EnsureFreshStartCommand(long ChatId, string? FirstName) : IRequest<string>; 