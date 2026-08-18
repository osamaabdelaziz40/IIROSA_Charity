using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for ChequeBeneficiary lookup entity
/// </summary>
public class ChequeBeneficiaryConfiguration : IEntityTypeConfiguration<ChequeBeneficiary>
{
    public void Configure(EntityTypeBuilder<ChequeBeneficiary> builder)
    {
        builder.ToTable(nameof(ChequeBeneficiary), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, etc.) are inherited from LookupEntityBase

        // Custom properties
        builder.Property(x => x.BeneficiaryType)
            .HasMaxLength(50);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Phone)
            .HasMaxLength(30);

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.IdNumber)
            .HasMaxLength(100);

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(x => x.Bank)
            .WithMany()
            .HasForeignKey(x => x.FK_BankId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.BeneficiaryType);
        builder.HasIndex(x => x.FK_BankId);
    }
}
