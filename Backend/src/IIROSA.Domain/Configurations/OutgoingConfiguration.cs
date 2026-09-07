using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Core configuration for Outgoing entity (epic 16)
/// </summary>
public class OutgoingConfiguration : IEntityTypeConfiguration<Outgoing>
{
    public void Configure(EntityTypeBuilder<Outgoing> builder)
    {
        builder.ToTable(nameof(Outgoing), MappingDefaults.IIROSA_SCHEMA);

        // An outgoing letter can be a reply to one incoming letter — blocks deleting a
        // letter that was replied to (16-16 referential guard)
        builder.HasOne(o => o.IncomingLetter)
            .WithMany(i => i.Replies)
            .HasForeignKey(o => o.IncomingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Owning charity — tenancy
        builder.HasOne(o => o.Charity)
            .WithMany()
            .HasForeignKey(o => o.FK_CharityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Orphan reports attached to the letter (UC-COR-18)
        builder.HasMany(o => o.OrphanReports)
            .WithOne(r => r.Outgoing)
            .HasForeignKey(r => r.OutgoingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Serial is allocated per charity + year (UC-COR-12). The unique filtered index is
        // the concurrency backstop (review P6): Max+1 inside the create transaction cannot
        // collide — soft-deleted rows leave the index (their serial is reusable) and legacy
        // NULL-year rows stay outside it.
        builder.HasIndex(o => new { o.FK_CharityId, o.Year, o.Serial })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [Year] IS NOT NULL");
        builder.HasIndex(o => o.Fk_DepartmentId);
        builder.HasIndex(o => o.OutgoingCategoryId);
    }
}
