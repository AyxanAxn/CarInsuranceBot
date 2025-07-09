using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Queries.Admin;

public class StatsQueryHandler : IRequestHandler<StatsQuery, string>
{
    private readonly IUnitOfWork _uow;
    public StatsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public Task<string> Handle(StatsQuery q, CancellationToken ct)
    {
        var issued = _uow.PoliciesQuery.Count(p => p.Status == PolicyStatus.Issued);
        var pending = _uow.PoliciesQuery.Count(p => p.Status == PolicyStatus.Pending);
        var failed = _uow.PoliciesQuery.Count(p => p.Status == PolicyStatus.Failed);
        var users = _uow.UsersQuery.Count();

        // Get recent policies (last 7 days)
        var lastWeek = DateTime.UtcNow.AddDays(-7);
        var recentPolicies = _uow.PoliciesQuery
            .Where(p => p.Status == PolicyStatus.Issued && p.IssuedUtc >= lastWeek)
            .Count();

        // Get total revenue (assuming 100 USD per policy)
        var totalRevenue = issued * 100;

        // Get recent revenue (last 7 days)
        var recentRevenue = recentPolicies * 100;

        // Get users in different stages
        var usersWaitingForPassport = _uow.UsersQuery.Count(u => u.Stage == RegistrationStage.WaitingForPassport);
        var usersWaitingForVehicle = _uow.UsersQuery.Count(u => u.Stage == RegistrationStage.WaitingForVehicle);
        var usersWaitingForReview = _uow.UsersQuery.Count(u => u.Stage == RegistrationStage.WaitingForReview);
        var usersWaitingForPayment = _uow.UsersQuery.Count(u => u.Stage == RegistrationStage.WaitingForPayment);
        var usersFinished = _uow.UsersQuery.Count(u => u.Stage == RegistrationStage.Finished);

        return Task.FromResult($"📊 *System Statistics*\n\n" +
               $"💰 *Revenue*\n" +
               $"• Total Revenue: *${totalRevenue:N0}*\n" +
               $"• Last 7 days: *${recentRevenue:N0}*\n\n" +
               $"📄 *Policies*\n" +
               $"• Issued: *{issued}*\n" +
               $"• Pending: *{pending}*\n" +
               $"• Failed: *{failed}*\n" +
               $"• Last 7 days: *{recentPolicies}*\n\n" +
               $"👥 *Users*\n" +
               $"• Total registered: *{users}*\n" +
               $"• Waiting for passport: *{usersWaitingForPassport}*\n" +
               $"• Waiting for vehicle: *{usersWaitingForVehicle}*\n" +
               $"• Waiting for review: *{usersWaitingForReview}*\n" +
               $"• Waiting for payment: *{usersWaitingForPayment}*\n" +
               $"• Completed: *{usersFinished}*");
    }
}