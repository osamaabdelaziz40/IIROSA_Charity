using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Core configuration for Incoming entity (epic 16)
/// </summary>
public class IncomingConfiguration : IEntityTypeConfiguration<Incoming>
{
    public void Configure(EntityTypeBuilder<Incoming> builder)
    {
        builder.ToTable(nameof(Incoming), MappingDefaults.IIROSA_SCHEMA);

        // An incoming letter can have many outgoing replies — blocks deleting a letter
        // that was replied to (16-7 referential guard)
        builder.HasMany(i => i.Replies)
            .WithOne(o => o.IncomingLetter)
            .HasForeignKey(o => o.IncomingId)
            .OnDelete(DeleteBehavior.Restrict);

        // The employee the letter is routed to (الموظف المناط به) — identity Users table,
        // owned by AppIdentityDbContext (remapped in ApplicationDbContext, migrations excluded)
        builder.HasOne(i => i.AssignedUser)
            .WithMany()
            .HasForeignKey(i => i.FK_UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Owning charity — tenancy
        builder.HasOne(i => i.Charity)
            .WithMany()
            .HasForeignKey(i => i.FK_CharityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Employees attached to the letter (UC-COR-09)
        builder.HasMany(i => i.Employees)
            .WithOne(e => e.Incoming)
            .HasForeignKey(e => e.IncomingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Serial is allocated per charity + year (UC-COR-03). The unique filtered index is
        // the concurrency backstop (review P6): Max+1 inside the create transaction cannot
        // collide — soft-deleted rows leave the index (their serial is reusable) and legacy
        // NULL-year rows stay outside it.
        builder.HasIndex(i => new { i.FK_CharityId, i.Year, i.Serial })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [Year] IS NOT NULL");
        builder.HasIndex(i => i.FK_DepartmentId);
    }
}
