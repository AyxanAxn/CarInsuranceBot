using MediatR;

namespace CarInsuranceBot.Application.Admin;

public record FailLogsQuery(long ChatId, int Take = 5) : IRequest<string>;
