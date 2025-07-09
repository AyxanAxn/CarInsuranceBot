using MediatR;

namespace CarInsuranceBot.Application.Queries.Admin;

public record StatsQuery(long ChatId) : IRequest<string>;
