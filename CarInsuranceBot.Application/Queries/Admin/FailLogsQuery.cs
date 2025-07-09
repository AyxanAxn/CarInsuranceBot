using MediatR;

namespace CarInsuranceBot.Application.Queries.Admin;

public record FailLogsQuery(long ChatId, int Take = 5) : IRequest<string>;
