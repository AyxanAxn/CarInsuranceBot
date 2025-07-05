using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Infrastructure.Persistence;

public class PolicyRepository : IPolicyRepository
{
    private readonly ApplicationDbContext _db;
    public PolicyRepository(ApplicationDbContext db) => _db = db;
    public void Add(Policy policy) => _db.Policies.Add(policy);
}
