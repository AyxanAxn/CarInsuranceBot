using CarInsuranceBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarInsuranceBot.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(a => a.Id);

        b.Property(a => a.TableName).HasMaxLength(100);
        b.Property(a => a.Action).HasMaxLength(50);
        b.Property(a => a.JsonDiff).HasColumnType("nvarchar(max)");
    }
}
