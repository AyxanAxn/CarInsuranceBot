using Microsoft.EntityFrameworkCore;

namespace CarInsuranceBot.Infrastructure.Persistence.Repositories
{
    public class UserRepository(ApplicationDbContext db) : IUserRepository
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<User?> GetAsync(long telegramUserId, CancellationToken ct) =>
           await _db.Users
               .Include(u => u.Documents)
               .SingleOrDefaultAsync(u => u.TelegramUserId == telegramUserId, ct);

        public void Add(User user) => _db.Users.Add(user);

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)=>
           await _db.Users.SingleOrDefaultAsync(u=> u.Id == id, ct);
    }
}