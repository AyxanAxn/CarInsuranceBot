using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarInsuranceBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarInsuranceBot.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            b.ToTable("Users");
            b.HasKey(u => u.Id);

            b.HasIndex(u => u.TelegramUserId).IsUnique();
            b.Property(u => u.FirstName).HasMaxLength(100);
            b.Property(u => u.LastName).HasMaxLength(100);

            // navs
            b.HasMany(u => u.Documents)
             .WithOne(d => d.User)
             .HasForeignKey(d => d.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(u => u.Policies)
             .WithOne(p => p.User)
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
