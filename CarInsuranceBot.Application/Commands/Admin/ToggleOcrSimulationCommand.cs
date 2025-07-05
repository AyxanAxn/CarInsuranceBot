using MediatR;

namespace CarInsuranceBot.Application.Admin;

public record ToggleOcrSimulationCommand(long ChatId, bool Enable) : IRequest<string>;
