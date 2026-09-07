using IIROSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for the HqTransferDetail entity (UC-TRF-08, §22.S.3)
/// </summary>
public class HqTransferDetailConfiguration : IEntityTypeConfiguration<HqTransferDetail>
{
    public void Configure(EntityTypeBuilder<HqTransferDetail> builder)
    {
        builder.ToTable(nameof(HqTransferDetail), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Parent ==========
        builder.Property(x => x.FK_HqTransferId)
            .IsRequired();

        // ========== Line fields ==========
        builder.Property(x => x.TransferNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.EstimatedTransferDate);

        builder.Property(x => x.IsExecuted);

        builder.Property(x => x.ExecutionDate);

        builder.Property(x => x.ArrivalDate);

        builder.Property(x => x.ArrivalAmount)
            .HasColumnType("decimal(18,2)");

        // ========== Relationships ==========

        // Parent transfer — Restrict: no use case deletes a transfer (soft delete rules the
        // day one exists); a hard parent delete must not cascade the financial lines away
        builder.HasOne(x => x.HqTransfer)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.FK_HqTransferId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        builder.HasIndex(x => x.FK_HqTransferId);
        builder.HasIndex(x => x.TransferNumber);
    }
}
