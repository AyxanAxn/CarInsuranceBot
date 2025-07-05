using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Application.Common.Interfaces
{
    public interface IConversationRepository
    {
        void Add(Conversation convo);
        Task<int> CountAsync(CancellationToken ct = default);
    }
}