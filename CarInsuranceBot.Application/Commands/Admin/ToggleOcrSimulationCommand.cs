using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Admin;

public record ToggleOcrSimulationCommand() : IRequest<string>;