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

        // Refugee register extensions (epic 7, UC-REF-03) — all nullable
        builder.Property(x => x.DeathReason)
            .HasMaxLength(100);

        // ========== Relationships ==========
        builder.HasOne(x => x.Family)
            .WithOne(f => f.Provider)
            .HasForeignKey<Provider>(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.NationalityCountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReasonOfRelation)
            .WithMany()
            .HasForeignKey(x => x.ReasonOfRelationId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        // The 1:1 Family↔Provider relationship conventionally makes this a PLAIN unique index
        // spanning soft-deleted rows — so once a guardian was removed (5-13) no new provider
        // could EVER be attached to that family, and 5-10's approve-on-empty-seat insert died
        // with duplicate-key 2601 (found live, epic-5 battery 2026-08-24). The explicit index
        // below supersedes the conventional one: one LIVE provider row per family (BR-06),
        // soft-deleted history may accumulate beneath the filter.
        builder.HasIndex(x => x.FamilyId)
            .IsUnique()
            .HasDatabaseName("IX_Provider_FamilyId")
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.NationalId);
    }
}
