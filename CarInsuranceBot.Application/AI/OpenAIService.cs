using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Application.AI
{
    public class OpenAIService : IOpenAIService
    {
        public Task<string> GeneratePolicyIntroAsync(User user, CancellationToken ct) =>
            Task.FromResult($"Hello {user.FirstName}, this is a placeholder intro.");
    }
}
