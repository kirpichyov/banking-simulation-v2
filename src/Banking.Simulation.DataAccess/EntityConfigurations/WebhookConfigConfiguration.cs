using System;
using Banking.Simulation.Core.Models.Entities;
using Banking.Simulation.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Simulation.DataAccess.EntityConfigurations;

public class WebhookConfigConfiguration : EntityConfigurationBase<WebhookConfig, Guid>
{
    public override void Configure(EntityTypeBuilder<WebhookConfig> builder)
    {
        base.Configure(builder);

        builder.Property(entity => entity.Url).IsRequired();
        builder.Property(entity => entity.Secret).IsRequired();

        builder.Property(entity => entity.Type)
            .HasConversion(@enum => @enum.ToStringFast(), @string => Convert(@string))
            .IsRequired();

        builder.HasOne<Organization>()
            .WithMany(organization => organization.WebhookConfigs)
            .HasForeignKey(webhookConfig => webhookConfig.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }

    private static WebhookType Convert(string @string)
    {
        var isConverted = WebhookTypeExtensions.TryParse(@string, out var converted, ignoreCase: true);

        if (!isConverted)
        {
            throw new ArgumentException($"Conversion to {nameof(WebhookType)} failed for value '{@string}'.");
        }
        
        return converted;
    }
}