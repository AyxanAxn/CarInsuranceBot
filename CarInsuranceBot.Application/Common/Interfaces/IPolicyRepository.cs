// Application/Common/Interfaces/IPolicyRepository.cs
using CarInsuranceBot.Domain.Entities;

public interface IPolicyRepository
{
    Task<Policy?> GetLatestByUserAsync(long chatId, CancellationToken ct);
    void Add(Policy policy);
}
