using MediatR;

namespace CarInsuranceBot.Application.Commands.Policy
{
    // Application/Commands/Policy/GeneratePolicyCommand.cs
    public record GeneratePolicyCommand(Guid UserId) : IRequest<string>;

}
