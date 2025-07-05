using MediatR;

namespace CarInsuranceBot.Application.Admin;

public record StatsQuery(long ChatId) : IRequest<string>;
