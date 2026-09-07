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

        // §15.1 row ledger — whole-epic column set (EP-10)
        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.IsStopped)
            .IsRequired();

        builder.Property(x => x.StoppedOn)
            .IsRequired(false);

        builder.Property(x => x.IsPrinted)
            .IsRequired();

        builder.Property(x => x.PrintedOn)
            .IsRequired(false);

        builder.Property(x => x.IsGotIt)
            .IsRequired();

        builder.Property(x => x.ReceivedOn)
            .IsRequired(false);

        builder.Property(x => x.ChiqueNum)
            .HasMaxLength(50);

        builder.Property(x => x.Printdate)
            .IsRequired(false);

        builder.Property(x => x.BenificiaryName)
            .HasMaxLength(200);

        builder.Property(x => x.TransferNo)
            .HasMaxLength(100);

        builder.Property(x => x.ExchangeStatus)
            .IsRequired(false);

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
