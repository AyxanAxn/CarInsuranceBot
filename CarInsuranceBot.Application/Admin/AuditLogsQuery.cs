using MediatR;

namespace CarInsuranceBot.Application.Admin;

public record AuditLogsQuery(long ChatId, int Take = 50) : IRequest<string>; 