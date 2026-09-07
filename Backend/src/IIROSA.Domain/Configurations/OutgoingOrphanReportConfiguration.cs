using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// EF Core configuration for the Outgoing ↔ Orphan report link (epic 16, UC-COR-18).
/// BR-26: an orphan's report attaches to at most one outgoing letter — the filtered
/// unique index on OrphanId is the last line of defence; the service refuses with
/// «Operation Faild» first. Filtering on IsDeleted keeps a detached link from blocking
/// a later re-attach to another letter.
/// </summary>
public class OutgoingOrphanReportConfiguration : IEntityTypeConfiguration<OutgoingOrphanReport>
{
    public void Configure(EntityTypeBuilder<OutgoingOrphanReport> builder)
    {
        builder.ToTable(nameof(OutgoingOrphanReport), MappingDefaults.IIROSA_SCHEMA);

        builder.HasOne(r => r.Outgoing)
            .WithMany(o => o.OrphanReports)
            .HasForeignKey(r => r.OutgoingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Orphan)
            .WithMany()
            .HasForeignKey(r => r.OrphanId)
            .OnDelete(DeleteBehavior.Restrict);

        // BR-26 — one live link per orphan
        builder.HasIndex(r => r.OrphanId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(r => r.OutgoingId);
    }
}
