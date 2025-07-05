using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class PolicyRepository : IPolicyRepository
{
    private readonly ApplicationDbContext _db;
    public PolicyRepository(ApplicationDbContext db) => _db = db;
    public async Task<Policy?> GetLatestByUserAsync(long chatId, CancellationToken ct) =>
    await _db.Policies
        .Where(p => p.User.TelegramUserId == chatId)
        .OrderByDescending(p => p.ExpiresUtc)
        .FirstOrDefaultAsync(ct);
    public void Add(Policy policy) => _db.Policies.Add(policy);
}
