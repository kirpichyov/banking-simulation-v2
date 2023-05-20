namespace Banking.Simulation.Application.Models;

public sealed record PaymentCreditAllowanceModel
{
    public bool IsAllowed { get; init; }
    public float MaxPercent { get; init; }
    public decimal MaxPricePerMonth { get; init; }
}