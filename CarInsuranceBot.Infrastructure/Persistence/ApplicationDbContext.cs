namespace CarInsuranceBot.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ExtractedField> ExtractedFields => Set<ExtractedField>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ErrorLog> Errors => Set<ErrorLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        b.Entity<Document>()
        .HasIndex(d => new { d.UserId, d.ContentHash })
        .IsUnique()
        .HasFilter("[ContentHash] IS NOT NULL");
    }
}