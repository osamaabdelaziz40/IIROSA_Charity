using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for NGOType lookup entity
/// </summary>
public class NGOTypeConfiguration : IEntityTypeConfiguration<NGOType>
{
    public void Configure(EntityTypeBuilder<NGOType> builder)
    {
        builder.ToTable(nameof(NGOType), MappingDefaults.LOOKUP_SCHEMA);

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
