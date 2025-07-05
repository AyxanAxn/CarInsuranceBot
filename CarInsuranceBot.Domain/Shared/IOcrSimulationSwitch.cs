namespace CarInsuranceBot.Domain.Shared;

/// <summary>Singleton flag that forces Mindee simulation at runtime.</summary>
public interface IOcrSimulationSwitch
{
    bool ForceSimulation { get; set; }
}
