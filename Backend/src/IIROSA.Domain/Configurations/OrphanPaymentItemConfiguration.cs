using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// OrphanPaymentItem Entity Configuration
/// </summary>
public class OrphanPaymentItemConfiguration : IEntityTypeConfiguration<OrphanPaymentItem>
{
    public void Configure(EntityTypeBuilder<OrphanPaymentItem> builder)
    {
        builder.ToTable(nameof(OrphanPaymentItem), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // Foreign Keys
        builder.Property(x => x.OrphanPaymentId)
            .IsRequired();

        builder.Property(x => x.OrphanId)
            .IsRequired();

        // Display Order
        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        // Notes
        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(x => x.OrphanPaymentId);
        builder.HasIndex(x => x.OrphanId);
        builder.HasIndex(x => new { x.OrphanPaymentId, x.OrphanId }).IsUnique();

        // Relationships
        builder.HasOne(x => x.OrphanPayment)
            .WithMany(op => op.Orphans)
            .HasForeignKey(x => x.OrphanPaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Orphan)
            .WithMany()
            .HasForeignKey(x => x.OrphanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
