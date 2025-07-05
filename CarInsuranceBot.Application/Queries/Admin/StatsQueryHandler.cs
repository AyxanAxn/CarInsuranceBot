using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Admin;

public class StatsQueryHandler : IRequestHandler<StatsQuery, string>
{
    private readonly IUnitOfWork _uow;
    public StatsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public Task<string> Handle(StatsQuery q, CancellationToken ct)
    {
        var issued =  _uow.PoliciesQuery.Count(p => p.Status == PolicyStatus.Issued);
        var pending =  _uow.PoliciesQuery.Count(p => p.Status == PolicyStatus.Pending);
        var users =  _uow.UsersQuery.Count();

        return Task.FromResult($"📊 *System stats*\n" +
               $"• Issued policies: *{issued}*\n" +
               $"• Pending approvals: *{pending}*\n" +
               $"• Registered users: *{users}*");
    }

}