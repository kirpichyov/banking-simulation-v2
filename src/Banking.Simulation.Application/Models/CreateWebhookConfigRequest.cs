using Banking.Simulation.Core.Models.Enums;

namespace Banking.Simulation.Application.Models;

public sealed record CreateWebhookConfigRequest
{
    public string Url { get; init; }
    public string Secret { get; init; }
    public WebhookType Type { get; init; }
}