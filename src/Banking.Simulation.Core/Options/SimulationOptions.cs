namespace Banking.Simulation.Core.Options;

public sealed record SimulationOptions
{
    public int AfterInitiateSuccessChance { get; init; }
    public int CreditApprovalSuccessChance { get; init; }
    public int SecondsBetweenSimulationsMin { get; init; }
    public int SecondsBetweenSimulationsMax { get; init; }
}