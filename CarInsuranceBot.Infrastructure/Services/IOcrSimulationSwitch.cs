using CarInsuranceBot.Domain.Shared;

namespace CarInsuranceBot.Infrastructure.Services;

public sealed class OcrSimulationSwitch : IOcrSimulationSwitch
{
    public bool ForceSimulation { get; set; }
}