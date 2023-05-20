using System;
using Banking.Simulation.Core.Models.Enums;

namespace Banking.Simulation.Core.Models.Entities;

public sealed class WebhookConfig : EntityBase<Guid>
{
    public WebhookConfig(WebhookType type, string url, string secret, Guid organizationId)
    {
        Type = type;
        Url = url;
        Secret = secret;
        OrganizationId = organizationId;
    }

    private WebhookConfig()
    {
    }
    
    public WebhookType Type { get; init; }
    public string Url { get; init; }
    public string Secret { get; init; }
    public Guid OrganizationId { get; init; }
}