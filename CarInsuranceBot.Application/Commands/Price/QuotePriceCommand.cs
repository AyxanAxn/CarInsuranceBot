using MediatR;
using System;

namespace CarInsuranceBot.Application.Commands.Price;

public record QuotePriceCommand(Guid UserId) : IRequest<string>;
