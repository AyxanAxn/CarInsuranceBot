using CarInsuranceBot.Domain.Entities;

namespace CarInsuranceBot.Application.Common.Interfaces
{
    public interface IErrorLogRepository
    {
        void Add(ErrorLog log);
    }
} 