using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// OrphanPayment Entity Configuration
/// </summary>
public class OrphanPaymentConfiguration : IEntityTypeConfiguration<OrphanPayment>
{
    public void Configure(EntityTypeBuilder<OrphanPayment> builder)
    {
        builder.ToTable(nameof(OrphanPayment), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // Basic Information
        builder.Property(x => x.GroupName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        // Payment Period
        builder.Property(x => x.PaymentPeriodFrom)
            .IsRequired();

        builder.Property(x => x.PaymentPeriodTo)
            .IsRequired();

        builder.Property(x => x.GroupDate)
            .IsRequired();

        // Financial Information
        builder.Property(x => x.ExchangeRate)
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.Currency)
            .HasMaxLength(10);

        builder.Property(x => x.DontRemoveRate)
            .IsRequired();

        // Batch Information
        builder.Property(x => x.BatchNo)
            .HasMaxLength(50);

        builder.Property(x => x.ShowOrder)
            .IsRequired();

        // Status
        builder.Property(x => x.IsBatchUploaded)
            .IsRequired();

        builder.Property(x => x.UploadDate)
            .IsRequired(false);

        // Notes
        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(x => x.BatchNo)
            .IsUnique()
            .HasFilter("\"BatchNo\" IS NOT NULL AND \"IsDeleted\" = false");
        builder.HasIndex(x => x.GroupName);
        builder.HasIndex(x => x.PaymentPeriodFrom);
        builder.HasIndex(x => x.PaymentPeriodTo);
        builder.HasIndex(x => x.GroupDate);
        builder.HasIndex(x => x.IsBatchUploaded);
        builder.HasIndex(x => x.ShowOrder);

        // Relationships - Orphans collection is configured in OrphanPaymentItemConfiguration
    }
}
