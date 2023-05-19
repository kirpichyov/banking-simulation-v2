using System;
using Banking.Simulation.Core.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Simulation.DataAccess.EntityConfigurations;

public sealed class OrganizationConfiguration : EntityConfigurationBase<Organization, Guid>
{
    public override void Configure(EntityTypeBuilder<Organization> builder)
    {
        base.Configure(builder);

        builder.Property(entity => entity.Name).IsRequired();
        builder.Property(entity => entity.Email).IsRequired();
        builder.Property(entity => entity.PasswordHash).IsRequired();
    }
}