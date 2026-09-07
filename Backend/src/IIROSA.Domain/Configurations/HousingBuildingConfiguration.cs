using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for HousingBuilding lookup entity (UC-HOU-05)
/// </summary>
public class HousingBuildingConfiguration : IEntityTypeConfiguration<HousingBuilding>
{
    public void Configure(EntityTypeBuilder<HousingBuilding> builder)
    {
        builder.ToTable(nameof(HousingBuilding), MappingDefaults.LOOKUP_SCHEMA);

        // ========== Base Properties (from LookupEntity) ==========

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameEn)
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // ========== HousingBuilding-Specific Properties ==========

        builder.Property(x => x.BuildingNumber);

        builder.Property(x => x.BuildingAddress)
            .HasMaxLength(200);

        builder.Property(x => x.BuildingDescription)
            .HasMaxLength(500);

        // ========== Indexes ==========

        builder.HasIndex(x => x.IsActive);
    }
}
