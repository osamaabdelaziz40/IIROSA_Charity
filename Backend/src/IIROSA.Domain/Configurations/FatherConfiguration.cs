using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for Father entity
/// </summary>
public class FatherConfiguration : IEntityTypeConfiguration<Father>
{
    public void Configure(EntityTypeBuilder<Father> builder)
    {
        builder.ToTable(nameof(Father), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Basic Information ==========
        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NationalId)
            .IsRequired()
            .HasMaxLength(50);

        // ========== Four-part name (§10 اضافة معيل — legacy WAR.IIROSA) ==========
        builder.Property(x => x.FirstName)
            .HasMaxLength(100);

        builder.Property(x => x.SecondName)
            .HasMaxLength(100);

        builder.Property(x => x.ThirdName)
            .HasMaxLength(100);

        builder.Property(x => x.FamilyName)
            .HasMaxLength(100);

        builder.Property(x => x.DateOfBirth);

        builder.Property(x => x.PlaceOfBirth)
            .HasMaxLength(200);

        // ========== Education & Work ==========
        builder.Property(x => x.Job)
            .HasMaxLength(200);

        builder.Property(x => x.MonthlyIncome)
            .HasColumnType("decimal(18,2)");

        // ========== Contact ==========
        builder.Property(x => x.Phone)
            .HasMaxLength(30);

        // ========== Status ==========
        builder.Property(x => x.IsAlive)
            .HasDefaultValue(true);

        builder.Property(x => x.IsProvider)
            .HasDefaultValue(false);

        builder.Property(x => x.DeathDate);

        builder.Property(x => x.DeathReasonId);

        builder.Property(x => x.DeathCertificateAttachmentId);

        builder.Property(x => x.MezaCard)
            .HasMaxLength(50);

        builder.Property(x => x.MezaCardExpirationDate);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // ========== Relationships ==========
        builder.HasOne(x => x.Family)
            .WithOne(f => f.Father)
            .HasForeignKey<Father>(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EducationLevel)
            .WithMany()
            .HasForeignKey(x => x.EducationLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HealthStatus)
            .WithMany()
            .HasForeignKey(x => x.HealthStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Nationality / death reason (§10 اضافة معيل) ==========
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.NationalityCountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DeathReason)
            .WithMany()
            .HasForeignKey(x => x.DeathReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => x.NationalId);
        builder.HasIndex(x => x.IsAlive);
        builder.HasIndex(x => x.IsProvider);
    }
}
