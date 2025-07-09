using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Common;

namespace CarInsuranceBot.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogCreateAsync<T>(T entity, CancellationToken ct = default) where T : BaseEntity;
    Task LogUpdateAsync<T>(T originalEntity, T updatedEntity, CancellationToken ct = default) where T : BaseEntity;
    Task LogDeleteAsync<T>(T entity, CancellationToken ct = default) where T : BaseEntity;
    Task LogActionAsync(string tableName,
                     Guid recordId,
                     string action,
                     string? jsonDiff = null,
                     CancellationToken ct = default);
} 