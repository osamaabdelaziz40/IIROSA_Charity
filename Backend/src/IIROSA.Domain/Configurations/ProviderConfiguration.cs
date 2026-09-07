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
        // §11.S.2 multi-guardian (اضافة الاباء · AddNewParent() always): 1:N from the
        // housing register. The legacy one-live-seat rule (BR-06) remains enforced in code
        // for the standalone member endpoint (AddProviderToFamilyAsync) and the
        // guardian-change-request flows — not by the database.
        builder.HasOne(x => x.Family)
            .WithMany(f => f.Providers)
            .HasForeignKey(x => x.FamilyId)
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
        // Was a filtered UNIQUE index (BR-06 one live seat, epic-5) — the §11.S.2 housing
        // register now carries several live guardians per family (اضافة الاباء ·
        // AddNewParent() always), so this is a plain lookup index; the seat rule for the
        // regular register stays enforced in code (AddProviderToFamilyAsync).
        builder.HasIndex(x => x.FamilyId)
            .HasDatabaseName("IX_Provider_FamilyId");
        builder.HasIndex(x => x.NationalId);
    }
}
