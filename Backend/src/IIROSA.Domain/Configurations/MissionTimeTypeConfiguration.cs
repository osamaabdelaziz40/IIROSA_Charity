using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for MissionTimeType lookup entity
/// </summary>
public class MissionTimeTypeConfiguration : IEntityTypeConfiguration<MissionTimeType>
{
    public void Configure(EntityTypeBuilder<MissionTimeType> builder)
    {
        builder.ToTable(nameof(MissionTimeType), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, etc.) are inherited from LookupEntityBase

        // Custom properties
        builder.Property(x => x.TimeTypeCode)
            .HasMaxLength(20);

        builder.Property(x => x.TypeDescription)
            .HasMaxLength(500);
    }
}
