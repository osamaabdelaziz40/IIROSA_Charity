using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for DeathReason lookup entity (سبب الوفاة)
/// </summary>
public class DeathReasonConfiguration : IEntityTypeConfiguration<DeathReason>
{
    public void Configure(EntityTypeBuilder<DeathReason> builder)
    {
        builder.ToTable(nameof(DeathReason), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, SortOrder, etc.) are inherited from LookupEntityBase
    }
}
