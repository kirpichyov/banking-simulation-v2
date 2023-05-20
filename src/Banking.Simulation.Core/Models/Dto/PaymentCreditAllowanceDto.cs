namespace Banking.Simulation.Core.Models.Dto;

public sealed record PaymentCreditAllowanceDto
{
    public bool IsAllowed { get; init; }
    public float MaxPercent { get; init; }
    public decimal MaxPricePerMonth { get; init; }
}