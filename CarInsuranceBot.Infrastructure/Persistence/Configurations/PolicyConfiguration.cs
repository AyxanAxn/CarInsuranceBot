using CarInsuranceBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarInsuranceBot.Infrastructure.Persistence.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> b)
    {
        b.ToTable("Policies");
        b.HasKey(p => p.Id);

        b.Property(p => p.PolicyNumber).HasMaxLength(50);
        b.Property(p => p.Status).HasConversion<int>();
        b.Property(p => p.PdfPath).HasMaxLength(300);
    }
}
