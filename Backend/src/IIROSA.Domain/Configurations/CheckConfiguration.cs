using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for Check entity (chapter 16, UC-CHQ)
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

        // ========== Bank Information ==========
        builder.Property(x => x.BankBranch)
            .HasMaxLength(200);

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(50);

        // ========== Tenancy & flags ==========
        builder.Property(x => x.ChequeType)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Individuals");

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

        builder.HasOne(x => x.Charity)
            .WithMany()
            .HasForeignKey(x => x.FK_CharityId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        // A cheque number identifies a leaf inside one bank's cheque book for one charity;
        // soft-deleted rows must not block re-using a number from a cancelled booklet.
        builder.HasIndex(x => new { x.CheckNumber, x.FK_BankId, x.FK_CharityId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.CheckDate);
        builder.HasIndex(x => x.FK_CharityId);
        builder.HasIndex(x => x.FK_ChequeBeneficiaryId);
        builder.HasIndex(x => x.FK_BankId);
    }
}
