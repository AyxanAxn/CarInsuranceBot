using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetAsync(long telegramUserId, CancellationToken ct);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct);

        void Add(User user);
    }

}
