using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for Region lookup entity
/// </summary>
public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable(nameof(Region), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, etc.) are inherited from LookupEntityBase

        // Custom properties
        builder.Property(x => x.RegionCode)
            .HasMaxLength(50);

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.RegionCode);
        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.SortOrder);
    }
}
