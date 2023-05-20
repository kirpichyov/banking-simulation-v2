using System;
using Banking.Simulation.Core.Models.Enums;

namespace Banking.Simulation.Core.Models.Messaging;

public sealed record SendWebhookCommand
{
    public Guid PaymentId { get; init; }
    public string WebhookUrl { get; init; }
    public string WebhookSecret { get; init; }
    public PaymentStatus PreviousStatus { get; init; }
    public PaymentStatus CurrentStatus { get; init; }
}