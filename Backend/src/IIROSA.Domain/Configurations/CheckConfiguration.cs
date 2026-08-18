using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for Check entity
/// </summary>
public class CheckConfiguration : IEntityTypeConfiguration<Check>
{
    public void Configure(EntityTypeBuilder<Check> builder)
    {
        builder.ToTable(nameof(Check), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Check Information ==========
        builder.Property(x => x.CheckNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CheckDate)
            .IsRequired();

        builder.Property(x => x.DueDate);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("EGP");

        // ========== Beneficiary Information ==========
        builder.Property(x => x.BeneficiaryType)
            .HasMaxLength(50);

        builder.Property(x => x.BeneficiaryName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.BeneficiaryAddress)
            .HasMaxLength(500);

        builder.Property(x => x.BeneficiaryPhone)
            .HasMaxLength(30);

        builder.Property(x => x.BeneficiaryEmail)
            .HasMaxLength(200);

        builder.Property(x => x.BeneficiaryIdNumber)
            .HasMaxLength(100);

        // ========== Financial Information ==========
        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.AmountInWords)
            .HasMaxLength(500);

        builder.Property(x => x.PaymentReason)
            .HasMaxLength(100);

        builder.Property(x => x.PaymentDescription)
            .HasMaxLength(1000);

        // ========== Bank Information ==========
        builder.Property(x => x.BankBranch)
            .HasMaxLength(200);

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(50);

        // ========== Status Information ==========
        builder.Property(x => x.CheckStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(x => x.BankReference)
            .HasMaxLength(100);

        builder.Property(x => x.ClearanceNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.VoidReason)
            .HasMaxLength(100);

        builder.Property(x => x.VoidNotes)
            .HasMaxLength(1000);

        // ========== Additional Information ==========
        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // ========== Relationships ==========
        builder.HasOne(x => x.ChequeBeneficiary)
            .WithMany(c => c.Checks)
            .HasForeignKey(x => x.FK_ChequeBeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Bank)
            .WithMany()
            .HasForeignKey(x => x.FK_BankId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        builder.HasIndex(x => x.CheckNumber).IsUnique();
        builder.HasIndex(x => x.CheckDate);
        builder.HasIndex(x => x.CheckStatus);
        builder.HasIndex(x => x.DueDate);
        builder.HasIndex(x => x.FK_ChequeBeneficiaryId);
        builder.HasIndex(x => x.FK_BankId);
    }
}
