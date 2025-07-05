using CarInsuranceBot.Domain.Shared;
using MediatR;

namespace CarInsuranceBot.Application.Admin;

public class ToggleOcrSimulationHandler :
    IRequestHandler<ToggleOcrSimulationCommand, string>
{
    private readonly IOcrSimulationSwitch _switch;
    public ToggleOcrSimulationHandler(IOcrSimulationSwitch sw) => _switch = sw;

    public Task<string> Handle(ToggleOcrSimulationCommand c, CancellationToken _)
    {
        _switch.ForceSimulation = c.Enable;
        var state = c.Enable ? "enabled" : "disabled";
        return Task.FromResult($"🔧 OCR simulation has been *{state}*.");
    }
}
