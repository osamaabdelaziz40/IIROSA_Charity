using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable(nameof(Country), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, etc.) are inherited from LookupEntityBase
        builder.Property(x => x.IsoCode)
            .HasMaxLength(10);

        builder.Property(x => x.DialingCode)
            .HasMaxLength(10);

        builder.Property(x => x.Currency)
            .HasMaxLength(10);

        builder.Property(x => x.FlagIcon)
            .HasMaxLength(255);
    }
}
