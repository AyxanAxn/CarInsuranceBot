using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Application.AI
{
    public interface IOpenAIService
    {
        Task<string> GeneratePolicyIntroAsync(User user, CancellationToken ct);
    }
}
