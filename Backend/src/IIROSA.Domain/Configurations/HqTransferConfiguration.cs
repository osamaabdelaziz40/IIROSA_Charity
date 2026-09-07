using IIROSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for the HqTransfer entity (UC-TRF-01…08)
/// </summary>
public class HqTransferConfiguration : IEntityTypeConfiguration<HqTransfer>
{
    public void Configure(EntityTypeBuilder<HqTransfer> builder)
    {
        builder.ToTable(nameof(HqTransfer), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Destination / Department ==========
        builder.Property(x => x.FK_CountryId)
            .IsRequired();

        builder.Property(x => x.FK_DepartmentId)
            .IsRequired();

        // ========== Operation identification ==========
        builder.Property(x => x.OperationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FinYear)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.PaymentNumber)
            .IsRequired();

        // ========== Period ==========
        builder.Property(x => x.DateFrom)
            .IsRequired();

        builder.Property(x => x.DateTo)
            .IsRequired();

        // ========== Financials ==========
        builder.Property(x => x.AmountOfPayment)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Statement)
            .HasMaxLength(500);

        builder.Property(x => x.BeneficiariesNumber)
            .IsRequired();

        // ========== Transaction ==========
        builder.Property(x => x.TransactionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TransactionDate)
            .IsRequired();

        // ========== Relationships ==========

        // Country (lookup)
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.FK_CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Department (lookup)
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.FK_DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        builder.HasIndex(x => x.FK_CountryId);
        builder.HasIndex(x => x.FK_DepartmentId);
        builder.HasIndex(x => x.TransactionDate);
        builder.HasIndex(x => x.OperationNumber);
    }
}
