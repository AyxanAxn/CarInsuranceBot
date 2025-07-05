using CarInsuranceBot.Application.Common.Interfaces;
using MediatR;

namespace CarInsuranceBot.Application.Admin;

public class FailLogsQueryHandler : IRequestHandler<FailLogsQuery, string>
{
    private readonly IUnitOfWork _uow;
    public FailLogsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public Task<string> Handle(FailLogsQuery q, CancellationToken ct)
    {
        var logs = _uow.Errors
                             .OrderByDescending(e => e.LoggedUtc)
                             .Take(q.Take)
                             .Select(e => $"• {e.LoggedUtc:u} — `{e.Message}`")
                             .ToList();

        return Task.FromResult(logs.Count == 0
            ? "🎉 No errors logged."
            : "⚠️ *Last errors:*\n" + string.Join('\n', logs));
    }
}