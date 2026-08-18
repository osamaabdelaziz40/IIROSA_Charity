using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for ChildOutGoing entity
/// </summary>
public class ChildOutGoingConfiguration : IEntityTypeConfiguration<ChildOutGoing>
{
    public void Configure(EntityTypeBuilder<ChildOutGoing> builder)
    {
        builder.ToTable(nameof(ChildOutGoing), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Basic Letter Information ==========
        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Date);

        builder.Property(x => x.Body)
            .HasMaxLength(4000);

        builder.Property(x => x.Year);

        // ========== Relationships ==========
        builder.HasOne(x => x.Outgoing)
            .WithMany()
            .HasForeignKey(x => x.OutgoingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UploadedFile)
            .WithMany()
            .HasForeignKey(x => x.UploadedFileId)
            .OnDelete(DeleteBehavior.SetNull);

        // ========== Indexes ==========
        builder.HasIndex(x => x.OutgoingId);
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.Year);
    }
}
