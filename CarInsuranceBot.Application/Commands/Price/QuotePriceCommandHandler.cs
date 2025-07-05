using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Enums;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Price;

public class QuotePriceCommandHandler(IUnitOfWork uow) : IRequestHandler<QuotePriceCommand, string>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<string> Handle(QuotePriceCommand cmd, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(cmd.UserId, ct)
                   ?? throw new KeyNotFoundException("User not found");

        user.Stage = RegistrationStage.WaitingForPayment;
        await _uow.SaveChangesAsync(ct);

        return "💰 The price is *100 USD*.\nType *yes* to proceed or *no* to cancel.";
    }
}
