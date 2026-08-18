using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for HealthStatus lookup entity
/// </summary>
public class HealthStatusConfiguration : IEntityTypeConfiguration<HealthStatus>
{
    public void Configure(EntityTypeBuilder<HealthStatus> builder)
    {
        builder.ToTable(nameof(HealthStatus), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, SortOrder, etc.) are inherited from LookupEntityBase
    }
}
