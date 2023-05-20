using System;
using Banking.Simulation.Core.Models.Entities;
using Banking.Simulation.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Simulation.DataAccess.EntityConfigurations;

public sealed class PaymentConfiguration : EntityConfigurationBase<Payment, Guid>
{
    public override void Configure(EntityTypeBuilder<Payment> builder)
    {
        base.Configure(builder);

        builder.Property(entity => entity.Amount).IsRequired();
        builder.Property(entity => entity.SourceJson).IsRequired();
        builder.Property(entity => entity.DestinationJson).IsRequired();
        builder.Property(entity => entity.CreditAllowanceJson).IsRequired();
        builder.Property(entity => entity.Comment).IsRequired();
        builder.Property(entity => entity.BankFee).IsRequired(false);
        builder.Property(entity => entity.FailReason).IsRequired(false);
        builder.Property(entity => entity.NextSimulationFailReason).IsRequired(false);
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
        builder.Property(entity => entity.UpdatedAtUtc).IsRequired(false);
        builder.Property(entity => entity.NextSimulationStatusAtUtc).IsRequired(false);

        builder.Property(entity => entity.Status)
            .HasConversion(@enum => @enum.ToStringFast(), @string => Convert(@string))
            .IsRequired();
        
        builder.Property(entity => entity.NextSimulationStatus)
            .HasConversion(
                @enum => @enum.HasValue ? @enum.Value.ToStringFast() : null,
                @string => ConvertNullable(@string))
            .IsRequired(false);

        builder.HasOne(payment => payment.Organization)
            .WithMany()
            .HasForeignKey(payment => payment.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }

    private static PaymentStatus Convert(string @string)
    {
        var isConverted = PaymentStatusExtensions.TryParse(@string, out var converted, ignoreCase: true);

        if (!isConverted)
        {
            throw new ArgumentException($"Conversion to {nameof(PaymentStatus)} failed for value '{@string}'.");
        }
        
        return converted;
    }
    
    private static PaymentStatus? ConvertNullable(string @string)
    {
        if (@string is null)
        {
            return null;
        }

        return Convert(@string);
    }
}