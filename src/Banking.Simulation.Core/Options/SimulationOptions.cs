namespace Banking.Simulation.Core.Options;

public sealed record SimulationOptions
{
    public int AfterInitiateFailChance { get; init; }
    public int CreditApprovalFailChance { get; init; }
    public int SecondsBetweenSimulationsMin { get; init; }
    public int SecondsBetweenSimulationsMax { get; init; }
}