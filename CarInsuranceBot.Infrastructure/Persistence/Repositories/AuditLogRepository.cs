namespace CarInsuranceBot.Infrastructure.Persistence.Repositories;

public class AuditLogRepository(ApplicationDbContext db) : IAuditLogRepository
{
    private readonly ApplicationDbContext _db = db;

    public void Add(AuditLog auditLog)
    {
        _db.AuditLogs.Add(auditLog);
    }

    public async Task<List<AuditLog>> GetByTableNameAsync(string tableName, CancellationToken ct = default)
    {
        return await _db.AuditLogs
            .Where(a => a.TableName == tableName)
            .OrderByDescending(a => a.CreatedUtc)
            .ToListAsync(ct);
    }

    public async Task<List<AuditLog>> GetByRecordIdAsync(Guid recordId, CancellationToken ct = default)
    {
        return await _db.AuditLogs
            .Where(a => a.RecordId == recordId)
            .OrderByDescending(a => a.CreatedUtc)
            .ToListAsync(ct);
    }

    public async Task<List<AuditLog>> GetByActionAsync(string action, CancellationToken ct = default)
    {
        return await _db.AuditLogs
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.CreatedUtc)
            .ToListAsync(ct);
    }

    public async Task<List<AuditLog>> GetRecentAsync(int count = 100, CancellationToken ct = default)
    {
        return await _db.AuditLogs
            .OrderByDescending(a => a.CreatedUtc)
            .Take(count)
            .ToListAsync(ct);
    }
} 