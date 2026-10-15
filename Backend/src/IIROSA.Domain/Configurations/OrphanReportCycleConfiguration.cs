using IIROSA.Domain.Contracts;
using IIROSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// EF configuration for OrphanReportCycle (UC-ORC-01…09). Discovered automatically by
/// ApplicationDbContext's ApplyConfigurationsFromAssembly — no DbSet declaration needed.
/// </summary>
public class OrphanReportCycleConfiguration : IEntityTypeConfiguration<OrphanReportCycle>
{
    public void Configure(EntityTypeBuilder<OrphanReportCycle> builder)
    {
        builder.ToTable(nameof(OrphanReportCycle), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TitleAr)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.TitleEn)
            .HasMaxLength(250);

        builder.Property(x => x.DescriptionAr)
            .HasMaxLength(2000);

        builder.Property(x => x.DescriptionEn)
            .HasMaxLength(2000);

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(Enums.OrphanReportCycleStatus.NotStarted);

        // The register filters by status (UC-ORC-01) and the chooser reads the In Progress
        // set (UC-ORC-06); the coverage screen pivots on both. The date pair serves the
        // background worker's periodic scan (UC-ORC-05).
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.StartDate, x.EndDate });

        // The Reports navigation and its Restrict FK are mapped on the dependent side,
        // in PeriodicOrphanReportConfiguration, beside every other FK of that entity.
    }
}
