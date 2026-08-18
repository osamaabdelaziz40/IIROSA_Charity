using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for Bank lookup entity
/// </summary>
public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ToTable(nameof(Bank), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, etc.) are inherited from LookupEntityBase

        // Custom properties
        builder.Property(x => x.BankCode)
            .HasMaxLength(50);

        builder.Property(x => x.SwiftCode)
            .HasMaxLength(20);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Phone)
            .HasMaxLength(30);

        // Indexes
        builder.HasIndex(x => x.BankCode);
    }
}
