using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    void Add(AuditLog auditLog);
    Task<List<AuditLog>> GetByTableNameAsync(string tableName, CancellationToken ct = default);
    Task<List<AuditLog>> GetByRecordIdAsync(Guid recordId, CancellationToken ct = default);
    Task<List<AuditLog>> GetByActionAsync(string action, CancellationToken ct = default);
    Task<List<AuditLog>> GetRecentAsync(int count = 100, CancellationToken ct = default);
} 