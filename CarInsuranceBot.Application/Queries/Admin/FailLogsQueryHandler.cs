using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Application.Common.Utils;
using CarInsuranceBot.Domain.Enums;
using MediatR;
using System.Text;

namespace CarInsuranceBot.Application.Queries.Admin;

public class FailLogsQueryHandler : IRequestHandler<FailLogsQuery, string>
{
    private readonly IUnitOfWork _uow;
    public FailLogsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public Task<string> Handle(FailLogsQuery q, CancellationToken ct)
    {
        var logs = _uow.Errors
            .OrderByDescending(e => e.LoggedUtc)
            .Take(q.Take)
            .ToList();

        if (logs.Count == 0)
        {
            return Task.FromResult("🎉 No errors logged.");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"⚠️ *Last {logs.Count} Errors:*\n");

        foreach (var log in logs)
        {
            var timeAgo = DateTime.UtcNow - log.LoggedUtc;
            var timeString = timeAgo.TotalMinutes < 1 ? "just now" :
                           timeAgo.TotalMinutes < 60 ? $"{(int)timeAgo.TotalMinutes}m ago" :
                           timeAgo.TotalHours < 24 ? $"{(int)timeAgo.TotalHours}h ago" :
                           $"{(int)timeAgo.TotalDays}d ago";

            sb.AppendLine($"🕐 **{timeString}**");
            sb.AppendLine($"❌ {MarkdownHelper.SafeCodeBlock(log.Message)}");

            // If it's a policy generation error, highlight it
            if (log.Message.Contains("policy", StringComparison.OrdinalIgnoreCase) ||
                log.Message.Contains("Policy", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("🚨 *Policy Generation Error*");
            }

            sb.AppendLine();
        }

        // Get failed policies count
        var failedPolicies = _uow.PoliciesQuery.Count(p => p.Status == PolicyStatus.Failed);
        if (failedPolicies > 0)
        {
            sb.AppendLine($"\n📄 *Failed Policies: {failedPolicies}*");
        }

        return Task.FromResult(sb.ToString());
    }
}