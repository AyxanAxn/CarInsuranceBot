using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Application.Common.Utils;
using MediatR;
using System.Text;

namespace CarInsuranceBot.Application.Admin;

public class AuditLogsQueryHandler : IRequestHandler<AuditLogsQuery, string>
{
    private readonly IUnitOfWork _uow;

    public AuditLogsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<string> Handle(AuditLogsQuery query, CancellationToken ct)
    {
        var auditLogs = await _uow.AuditLogs.GetRecentAsync(query.Take, ct);

        if (!auditLogs.Any())
            return "📋 No audit logs found.";

        var sb = new StringBuilder();
        sb.AppendLine($"📋 **Recent Audit Logs** (Last {auditLogs.Count} entries):\n");

        foreach (var log in auditLogs.Take(10)) // Show only first 10 for readability
        {
            var timeAgo = DateTime.UtcNow - log.CreatedUtc;
            var timeString = timeAgo.TotalMinutes < 1 ? "just now" :
                           timeAgo.TotalMinutes < 60 ? $"{(int)timeAgo.TotalMinutes}m ago" :
                           timeAgo.TotalHours < 24 ? $"{(int)timeAgo.TotalHours}h ago" :
                           $"{(int)timeAgo.TotalDays}d ago";

            sb.AppendLine($"🕐 **{timeString}**");
            sb.AppendLine($"📄 **{MarkdownHelper.EscapeMarkdown(log.TableName)}** ({MarkdownHelper.EscapeMarkdown(log.Action)})");
            sb.AppendLine($"🆔 Record: {MarkdownHelper.SafeCodeBlock(log.RecordId.ToString())}");
            
            if (!string.IsNullOrEmpty(log.JsonDiff) && log.JsonDiff.Length < 200)
            {
                var cleanJson = log.JsonDiff.Replace("\n", " ").Replace("\r", "");
                sb.AppendLine($"📝 Details: {MarkdownHelper.SafeCodeBlock(cleanJson)}");
            }
            else if (!string.IsNullOrEmpty(log.JsonDiff))
            {
                var truncatedJson = log.JsonDiff.Substring(0, 200);
                sb.AppendLine($"📝 Details: {MarkdownHelper.SafeCodeBlock(truncatedJson)}...");
            }
            
            sb.AppendLine();
        }

        if (auditLogs.Count > 10)
        {
            sb.AppendLine($"... and {auditLogs.Count - 10} more entries.");
        }

        return sb.ToString();
    }
} 