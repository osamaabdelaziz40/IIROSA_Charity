using IIROSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for the GuardianChangeRequest entity (UC-FAM-09/10)
/// </summary>
public class GuardianChangeRequestConfiguration : IEntityTypeConfiguration<GuardianChangeRequest>
{
    public void Configure(EntityTypeBuilder<GuardianChangeRequest> builder)
    {
        builder.ToTable(nameof(GuardianChangeRequest), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Family & Charity ==========
        builder.Property(x => x.FamilyId)
            .IsRequired();

        builder.Property(x => x.CharityId)
            .IsRequired();

        // ========== Snapshot columns ==========
        builder.Property(x => x.OrphanCode)
            .HasMaxLength(50);

        builder.Property(x => x.OrphanName)
            .HasMaxLength(200);

        builder.Property(x => x.MotherName)
            .HasMaxLength(200);

        builder.Property(x => x.OldGuardianName)
            .HasMaxLength(200);

        builder.Property(x => x.OldGuardianNationalId)
            .HasMaxLength(50);

        builder.Property(x => x.OldGuardianRelationship)
            .HasMaxLength(100);

        builder.Property(x => x.NewGuardianName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.NewGuardianNationalId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Relationship)
            .HasMaxLength(100)
            .IsRequired();

        // ========== Reason & workflow ==========
        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.RequestedByName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.DecidedBy)
            .HasMaxLength(200);

        builder.Property(x => x.DecidedOn);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(500);

        // ========== Relationships ==========

        // Family — restrict: a request row must never cascade away with its family
        builder.HasOne(x => x.Family)
            .WithMany()
            .HasForeignKey(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Raising charity
        builder.HasOne(x => x.Charity)
            .WithMany()
            .HasForeignKey(x => x.CharityId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        // The review queue filters by status inside a charity scope — one composite covers it.
        builder.HasIndex(x => new { x.Status, x.CharityId });
        builder.HasIndex(x => x.FamilyId);

        // One PENDING request per family at the database level (5-9 ruling 2026-08-24): the
        // application-side guard is check-then-insert, so a concurrent double-raise could store
        // two pendings. The filter excludes decided and soft-deleted rows, so history stays
        // unrestricted. Status = 1 = Pending (GuardianChangeRequestStatus).
        builder.HasIndex(x => x.FamilyId)
            .IsUnique()
            .HasDatabaseName("IX_GuardianChangeRequest_OnePendingPerFamily")
            .HasFilter("[Status] = 1 AND [IsDeleted] = 0");
    }
}
