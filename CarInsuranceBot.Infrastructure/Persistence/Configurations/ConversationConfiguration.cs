using CarInsuranceBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarInsuranceBot.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> b)
    {
        b.ToTable("Conversations");
        b.HasKey(c => c.Id);

        b.Property(c => c.Prompt).HasColumnType("nvarchar(max)");
        b.Property(c => c.Response).HasColumnType("nvarchar(max)");
    }
}
