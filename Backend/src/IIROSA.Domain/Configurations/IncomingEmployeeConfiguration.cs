using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// EF Core configuration for the Incoming ↔ Employee link (epic 16, UC-COR-09).
/// The unique index is filtered on live rows so a detached (soft-deleted) link never
/// blocks re-attaching the same employee later.
/// </summary>
public class IncomingEmployeeConfiguration : IEntityTypeConfiguration<IncomingEmployee>
{
    public void Configure(EntityTypeBuilder<IncomingEmployee> builder)
    {
        builder.ToTable(nameof(IncomingEmployee), MappingDefaults.IIROSA_SCHEMA);

        builder.HasOne(e => e.Incoming)
            .WithMany(i => i.Employees)
            .HasForeignKey(e => e.IncomingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.IncomingId, e.UserId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
