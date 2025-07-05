namespace CarInsuranceBot.Infrastructure.Persistence.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> b)
        {
            b.ToTable("Documents");
            b.HasKey(d => d.Id);

            b.Property(d => d.Path).IsRequired().HasMaxLength(300);
            b.Property(d => d.Type).HasConversion<int>();

            b.HasMany(d => d.ExtractedFields)
             .WithOne(f => f.Document)
             .HasForeignKey(f => f.DocumentId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
