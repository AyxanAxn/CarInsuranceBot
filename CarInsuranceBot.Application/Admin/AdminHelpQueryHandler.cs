using MediatR;

namespace CarInsuranceBot.Application.Admin;

public class AdminHelpQueryHandler : IRequestHandler<AdminHelpQuery, string>
{
    public Task<string> Handle(AdminHelpQuery query, CancellationToken ct)
    {
        var helpText = "🔧 *Admin Commands*\n\n" +
                      "📊 `/stats` - Get system statistics and revenue summary\n" +
                      "⚠️ `/faillogs` - View failed policy generations and errors\n" +
                      "📋 `/auditlogs` - View recent audit trail\n" +
                      "🤖 `/simulateocr` - Toggle Mindee OCR simulation mode\n" +
                      "❓ `/adminhelp` - Show this help message\n\n" +
                      "All commands are admin-only and require proper configuration.";

        return Task.FromResult(helpText);
    }
} 