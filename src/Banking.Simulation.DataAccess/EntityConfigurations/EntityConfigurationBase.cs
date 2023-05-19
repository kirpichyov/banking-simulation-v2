using Banking.Simulation.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Simulation.DataAccess.EntityConfigurations;

public abstract class EntityConfigurationBase<TEntity, TId> : IEntityTypeConfiguration<TEntity>
	where TEntity : EntityBase<TId>
{
	public virtual void Configure(EntityTypeBuilder<TEntity> builder)
	{
		builder.HasKey(entity => entity.Id);
	}
}
