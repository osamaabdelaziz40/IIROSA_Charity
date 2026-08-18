using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for ProjectType lookup entity
/// </summary>
public class ProjectTypeConfiguration : IEntityTypeConfiguration<ProjectType>
{
    public void Configure(EntityTypeBuilder<ProjectType> builder)
    {
        builder.ToTable(nameof(ProjectType), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, etc.) are inherited from LookupEntityBase

        // Custom properties
        builder.Property(x => x.TypeCode)
            .HasMaxLength(50);

        builder.Property(x => x.TypeDescription)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(x => x.TypeCode);
    }
}
