using System.Text.Json;
using CarInsuranceBot.Domain.Common;

namespace CarInsuranceBot.Infrastructure.Services;

/// <summary>
/// Persists an audit-trail record for every CREATE / UPDATE / DELETE your
/// repositories perform.  
/// Serialisation is done with <see cref="ReferenceHandler.IgnoreCycles"/>
/// so navigation-property loops never blow up the logger.
/// </summary>
public sealed class AuditService : IAuditService
{
    private readonly IUnitOfWork _uow;

    /// <summary>Shared Json-options: indented + cycle-safe.</summary>
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public AuditService(IUnitOfWork uow) => _uow = uow;

    /*────────────────────────────  PUBLIC API  ────────────────────────────*/

    public Task LogCreateAsync<T>(T entity, CancellationToken ct = default)
        where T : BaseEntity =>
        LogActionAsync(typeof(T).Name,
                       entity.Id,
                       "CREATE",
                       JsonSerializer.Serialize(entity, _jsonOpts),
                       ct);

    public Task LogUpdateAsync<T>(T original, T updated, CancellationToken ct = default)
        where T : BaseEntity
    {
        var diff = new
        {
            Before = original,
            After = updated,
            Changes = GetPropertyChanges(original, updated)
        };

        return LogActionAsync(typeof(T).Name,
                              updated.Id,
                              "UPDATE",
                              JsonSerializer.Serialize(diff, _jsonOpts),
                              ct);
    }

    public Task LogDeleteAsync<T>(T entity, CancellationToken ct = default)
        where T : BaseEntity =>
        LogActionAsync(typeof(T).Name,
                       entity.Id,
                       "DELETE",
                       JsonSerializer.Serialize(entity, _jsonOpts),
                       ct);

    /*────────────────────────────  INTERNALS  ─────────────────────────────*/

    //  AuditService implementation – make the helper PUBLIC and nullable-aware
    public async Task LogActionAsync(string tableName,
                                     Guid recordId,
                                     string action,
                                     string? jsonDiff = null,
                                     CancellationToken ct = default)
    {
        _uow.AuditLogs.Add(new AuditLog
        {
            TableName = tableName,
            RecordId = recordId,
            Action = action,
            JsonDiff = jsonDiff ?? string.Empty,
            CreatedUtc = DateTime.UtcNow
        });

        await _uow.SaveChangesAsync(ct);
    }


    private static Dictionary<string, object> GetPropertyChanges<T>(T before, T after)
        where T : BaseEntity
    {
        var changes = new Dictionary<string, object>();

        foreach (var prop in typeof(T).GetProperties()
                                      .Where(p => p.CanRead && p.CanWrite &&
                                                  p.Name != nameof(BaseEntity.Id)))
        {
            var oldVal = prop.GetValue(before);
            var newVal = prop.GetValue(after);

            if (!Equals(oldVal, newVal))
            {
                changes[prop.Name] = new { From = oldVal, To = newVal };
            }
        }

        return changes;
    }
}
