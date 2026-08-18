using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for Center lookup entity
/// </summary>
public class CenterConfiguration : IEntityTypeConfiguration<Center>
{
    public void Configure(EntityTypeBuilder<Center> builder)
    {
        builder.ToTable(nameof(Center), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, etc.) are inherited from LookupEntityBase

        // Custom properties
        builder.Property(x => x.CenterCode)
            .HasMaxLength(50);

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Region)
            .WithMany(r => r.Centers)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.CenterCode);
        builder.HasIndex(x => x.RegionId);
        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.SortOrder);
    }
}
