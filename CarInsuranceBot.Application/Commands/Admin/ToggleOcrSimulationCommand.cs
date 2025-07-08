using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Admin;

public record ToggleOcrSimulationCommand() : IRequest<string>;