using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for OfficeProjectType lookup entity
/// </summary>
public class OfficeProjectTypeConfiguration : IEntityTypeConfiguration<OfficeProjectType>
{
    public void Configure(EntityTypeBuilder<OfficeProjectType> builder)
    {
        builder.ToTable(nameof(OfficeProjectType), MappingDefaults.LOOKUP_SCHEMA);

        // ========== Base Properties (from LookupEntity) ==========

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameEn)
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // ========== OfficeProjectType-Specific Properties ==========

        builder.Property(x => x.TypeCode)
            .HasMaxLength(50);

        builder.Property(x => x.TypeDescription)
            .HasMaxLength(500);

        // ========== Indexes ==========

        builder.HasIndex(x => x.TypeCode)
            .IsUnique();

        builder.HasIndex(x => x.IsActive);
    }
}
