using CarInsuranceBot.Domain.Common;

namespace CarInsuranceBot.Domain.Entities.Events
{
    public record PolicyIssuedEvent(Guid PolicyId) : IDomainEvent;
}