using CarInsuranceBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarInsuranceBot.Infrastructure.Persistence.Configurations
{
    public class ExtractedFieldConfiguration : IEntityTypeConfiguration<ExtractedField>
    {
        public void Configure(EntityTypeBuilder<ExtractedField> b)
        {
            b.ToTable("ExtractedFields");
            b.HasKey(f => f.Id);

            b.Property(f => f.FieldName).HasMaxLength(100);
            b.Property(f => f.FieldValue).HasMaxLength(300);
        }
    }
}
