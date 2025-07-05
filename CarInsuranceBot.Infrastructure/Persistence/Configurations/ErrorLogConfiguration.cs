using CarInsuranceBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarInsuranceBot.Infrastructure.Persistence.Configurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> b)
    {
        b.ToTable("Errors");
        b.HasKey(e => e.Id);

        b.Property(e => e.Message).HasColumnType("nvarchar(max)");
        b.Property(e => e.StackTrace).HasColumnType("nvarchar(max)");
    }
}
