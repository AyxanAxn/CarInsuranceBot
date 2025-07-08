namespace CarInsuranceBot.Tests.Helpers;

public class InMemoryFixture : IDisposable
{
    public ApplicationDbContext Db { get; }

    public InMemoryFixture()
    {
        // Configure QuestPDF license for testing
        QuestPDF.Settings.License = LicenseType.Community;
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Db = new ApplicationDbContext(options);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db?.Dispose();
    }
}