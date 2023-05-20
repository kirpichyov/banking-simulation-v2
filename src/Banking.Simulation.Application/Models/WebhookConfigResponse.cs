using System;
using Banking.Simulation.Core.Models.Enums;

namespace Banking.Simulation.Application.Models;

public sealed record WebhookConfigResponse
{
    public Guid Id { get; init; }
    public string Url { get; init; }
    public WebhookType Type { get; init; }
}