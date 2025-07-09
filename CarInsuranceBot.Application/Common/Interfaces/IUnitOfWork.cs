using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Application.Common.Interfaces;
public interface IUnitOfWork
{
    // Repositories (only one for now)
    IUserRepository Users { get; }
    IDocumentRepository Documents { get; }
    IExtractedFieldRepository ExtractedFields { get; }
    IPolicyRepository Policies { get; }
    IConversationRepository Conversations { get; }
    IErrorLogRepository ErrorLogs { get; }
    IAuditLogRepository AuditLogs { get; }

    IQueryable<ErrorLog> Errors { get; }
    IQueryable<User> UsersQuery { get; }
    IQueryable<Policy> PoliciesQuery { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}