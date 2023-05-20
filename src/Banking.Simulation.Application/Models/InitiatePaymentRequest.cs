namespace Banking.Simulation.Application.Models;

public sealed record InitiatePaymentRequest
{
    public PaymentMethodModel Source { get; init; }
    public PaymentMethodModel Destination { get; init; }
    public PaymentCreditAllowanceModel CreditAllowance { get; init; }
    public decimal Amount { get; init; }
    public string Comment { get; init; }
}