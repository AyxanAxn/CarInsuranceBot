// Application/Common/Interfaces/IPolicyRepository.cs
using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Application.Common.Interfaces;
public interface IPolicyRepository
{
    Task<Policy?> GetLatestByUserAsync(long chatId, CancellationToken ct);
    void Add(Policy policy);
}