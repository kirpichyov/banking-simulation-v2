namespace Banking.Simulation.Core.Models.Dto;

public sealed record PaymentMethodDto
{
    public string CardNumber { get; init; }
    public string BankAccountNumber { get; init; }
}