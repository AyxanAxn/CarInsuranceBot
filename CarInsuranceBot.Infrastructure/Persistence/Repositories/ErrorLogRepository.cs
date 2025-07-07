namespace CarInsuranceBot.Infrastructure.Persistence.Repositories;

public class ErrorLogRepository : IErrorLogRepository
{
    private readonly ApplicationDbContext _db;
    public ErrorLogRepository(ApplicationDbContext db) => _db = db;
    public void Add(ErrorLog log) => _db.Errors.Add(log);
} 