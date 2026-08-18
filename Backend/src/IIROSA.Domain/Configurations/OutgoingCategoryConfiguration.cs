using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for OutgoingCategory lookup entity
/// </summary>
public class OutgoingCategoryConfiguration : IEntityTypeConfiguration<OutgoingCategory>
{
    public void Configure(EntityTypeBuilder<OutgoingCategory> builder)
    {
        builder.ToTable(nameof(OutgoingCategory), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, etc.) are inherited from LookupEntityBase

        // Custom properties
        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}
