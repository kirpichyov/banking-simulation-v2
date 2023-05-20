namespace Banking.Simulation.Application.Models;

public sealed record PaymentMethodModel
{
    public string CardNumber { get; init; }
    public string BankAccountNumber { get; init; }
}