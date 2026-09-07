using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for HousingFlat lookup entity (UC-HOU-05)
/// </summary>
public class HousingFlatConfiguration : IEntityTypeConfiguration<HousingFlat>
{
    public void Configure(EntityTypeBuilder<HousingFlat> builder)
    {
        builder.ToTable(nameof(HousingFlat), MappingDefaults.LOOKUP_SCHEMA);

        // ========== Base Properties (from LookupEntity) ==========

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameEn)
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // ========== HousingFlat-Specific Properties ==========

        builder.Property(x => x.Number);

        builder.Property(x => x.SizeInMtr);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // ========== Relationships ==========

        builder.HasOne(x => x.Building)
            .WithMany(x => x.Flats)
            .HasForeignKey(x => x.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========

        builder.HasIndex(x => x.BuildingId);
        builder.HasIndex(x => x.IsActive);
    }
}
