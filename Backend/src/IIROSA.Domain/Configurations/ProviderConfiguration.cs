using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for Provider entity
/// </summary>
public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable(nameof(Provider), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Basic Information ==========
        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.RelationshipToFamily)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NationalId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Job)
            .HasMaxLength(200);

        builder.Property(x => x.MonthlyIncome)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // ========== Relationships ==========
        builder.HasOne(x => x.Family)
            .WithOne(f => f.Provider)
            .HasForeignKey<Provider>(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => x.NationalId);
    }
}
