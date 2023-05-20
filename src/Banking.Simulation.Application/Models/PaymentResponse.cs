using System;
using Banking.Simulation.Core.Models.Dto;
using Banking.Simulation.Core.Models.Enums;

namespace Banking.Simulation.Application.Models;

public sealed record PaymentResponse
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public PaymentMethodDto Source { get; init; }
    public PaymentMethodDto Destination { get; init; }
    public PaymentCreditAllowanceDto CreditAllowance { get; init; }
    public string Comment { get; init; }
    public PaymentStatus Status { get; init; }
    public string FailReason { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}