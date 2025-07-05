using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Flow;

public class CancelCommandHandler : IRequestHandler<CancelCommand, string>
{
    private readonly IUnitOfWork _uow;
    public CancelCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<string> Handle(CancelCommand cmd, CancellationToken ct)
    {
        var user = await _uow.Users.GetAsync(cmd.ChatId, ct);
        if (user is null)
            return "Nothing to cancel.";

        user.Stage = RegistrationStage.None;
        await _uow.SaveChangesAsync(ct);

        return "🔄 Flow reset. Type /start to begin again.";
    }
}