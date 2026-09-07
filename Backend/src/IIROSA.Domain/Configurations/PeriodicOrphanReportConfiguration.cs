using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

public class PeriodicOrphanReportConfiguration : IEntityTypeConfiguration<PeriodicOrphanReport>
{
    public void Configure(EntityTypeBuilder<PeriodicOrphanReport> builder)
    {
        builder.ToTable(nameof(PeriodicOrphanReport), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReportDate)
            .IsRequired();

        builder.Property(x => x.ReportYear)
            .IsRequired();

        builder.Property(x => x.ReportMonth)
            .IsRequired();

        builder.Property(x => x.ReviewStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ReviewComments)
            .HasMaxLength(1000);

        builder.Property(x => x.PrayerStatus)
            .HasMaxLength(100);

        builder.Property(x => x.QuranMemorization)
            .HasMaxLength(100);

        builder.Property(x => x.QuranParts)
            .HasMaxLength(50);

        builder.Property(x => x.MannersStatus)
            .HasMaxLength(100);

        builder.Property(x => x.EducationLevel)
            .HasMaxLength(100);

        builder.Property(x => x.EducationStage)
            .HasMaxLength(100);

        builder.Property(x => x.AcademicPerformance)
            .HasMaxLength(100);

        builder.Property(x => x.SchoolName)
            .HasMaxLength(200);

        builder.Property(x => x.HealthStatus)
            .HasMaxLength(500);

        builder.Property(x => x.ChronicDiseases)
            .HasMaxLength(500);

        builder.Property(x => x.MedicalNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.Skills)
            .HasMaxLength(500);

        builder.Property(x => x.Hobbies)
            .HasMaxLength(500);

        builder.Property(x => x.PersonalNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.MajorEvents)
            .HasMaxLength(1000);

        builder.Property(x => x.Achievements)
            .HasMaxLength(1000);

        builder.Property(x => x.Challenges)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(x => x.Orphan)
            .WithMany(x => x.PeriodicReports)
            .HasForeignKey(x => x.OrphanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Charity)
            .WithMany()
            .HasForeignKey(x => x.CharityId)
            .OnDelete(DeleteBehavior.Restrict);

        // UC-HOU-06/08 (§11.S.3): the housing-beneficiary discriminator + family link.
        // ChildOrParent defaults to Child — pre-housing rows are child reports by definition
        // (the 6-6 migration backfills explicitly for databases that predate the default).
        builder.Property(x => x.ChildOrParent)
            .IsRequired()
            .HasDefaultValue(Enums.ReportBeneficiaryType.Child);

        builder.HasOne(x => x.HousingFamily)
            .WithMany()
            .HasForeignKey(x => x.FK_HousingFamilyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.ReportNo)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(x => x.OrphanId);
        builder.HasIndex(x => x.ReportMonth);
        builder.HasIndex(x => x.ReportYear);
        // One CHILD report per orphan per month. The filter matters (review follow-up
        // 2026-08-24): guardian rows ride the carrier child's OrphanId, so unfiltered this
        // index made (a) a live child + live guardian report in the same month on a
        // single-child family, and (b) re-creating a soft-deleted guardian report, throw
        // unique-violation 500s the service checks explicitly allow. Pre-housing rows are
        // all Child (=1).
        // Review P7/P7b 2026-08-24 (epic-9 review — supersedes the earlier "soft-deleted
        // rows hold slots" note): [IsDeleted] = 0 added. 9-6's delete is the correction
        // flow (delete a wrong report, re-enter the same month) and there is no restore
        // endpoint, so a slot held by a soft-deleted row bricks that orphan's month
        // forever. Same liveness rule the guardian index below already applies.
        builder.HasIndex(x => new { x.OrphanId, x.ReportMonth, x.ReportYear })
            .IsUnique()
            .HasFilter("[ChildOrParent] = 1 AND [IsDeleted] = 0");
        // The housing screen's read path: one family × discriminator
        builder.HasIndex(x => new { x.FK_HousingFamilyId, x.ChildOrParent });

        // Review decision D4 (2026-08-24): DB backing for the duplicate rules the service
        // checks check-then-insert. Guardian rule — one LIVE Parent report per family per
        // month (the filter frees the slot on soft delete, matching the service check).
        builder.HasIndex(x => new { x.FK_HousingFamilyId, x.ReportYear, x.ReportMonth })
            .IsUnique()
            .HasFilter("[ChildOrParent] = 2 AND [IsDeleted] = 0");
        // Report number — unique among non-null values: the count-based sequence races
        // under concurrent creates; the index turns a silent duplicate number into a
        // hard refusal instead. Requires the column sized off nvarchar(max) (above).
        builder.HasIndex(x => x.ReportNo)
            .IsUnique()
            .HasFilter("[ReportNo] IS NOT NULL");
    }
}
