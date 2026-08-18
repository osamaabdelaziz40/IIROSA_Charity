using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for LivingCondition lookup entity
/// </summary>
public class LivingConditionConfiguration : IEntityTypeConfiguration<LivingCondition>
{
    public void Configure(EntityTypeBuilder<LivingCondition> builder)
    {
        builder.ToTable(nameof(LivingCondition), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, SortOrder, etc.) are inherited from LookupEntityBase
    }
}
