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

        // Indexes
        builder.HasIndex(x => x.OrphanId);
        builder.HasIndex(x => x.ReportMonth);
        builder.HasIndex(x => x.ReportYear);
        builder.HasIndex(x => new { x.OrphanId, x.ReportMonth, x.ReportYear }).IsUnique();
    }
}
