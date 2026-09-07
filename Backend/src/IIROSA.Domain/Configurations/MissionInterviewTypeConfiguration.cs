using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for MissionInterviewType lookup entity
/// </summary>
public class MissionInterviewTypeConfiguration : IEntityTypeConfiguration<MissionInterviewType>
{
    public void Configure(EntityTypeBuilder<MissionInterviewType> builder)
    {
        builder.ToTable(nameof(MissionInterviewType), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, etc.) are inherited from LookupEntityBase

        // Custom properties
        builder.Property(x => x.TypeCode)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(x => x.TypeCode);
    }
}
