using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for OfficeProject entity
/// </summary>
public class OfficeProjectConfiguration : IEntityTypeConfiguration<OfficeProject>
{
    public void Configure(EntityTypeBuilder<OfficeProject> builder)
    {
        builder.ToTable(nameof(OfficeProject), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Basic Information ==========
        builder.Property(x => x.ProjectName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ProjectHint)
            .HasMaxLength(1000);

        builder.Property(x => x.ProjectDate)
            .IsRequired();

        builder.Property(x => x.ProjectEndDate);

        // ========== Project Classification ==========
        builder.Property(x => x.FK_OfficeProjectTypeId);

        // ========== Location ==========
        builder.Property(x => x.FK_CountryId);
        builder.Property(x => x.FK_RegionId);
        builder.Property(x => x.FK_CenterId);

        builder.Property(x => x.VillageName)
            .HasMaxLength(100);

        // ========== Financial Information ==========
        builder.Property(x => x.ProjectCostEGP)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ProjectCostSAR)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.DonorName)
            .HasMaxLength(200);

        // ========== Beneficiaries ==========
        builder.Property(x => x.BeneficiariesCount);

        builder.Property(x => x.BeneficiariesType)
            .HasMaxLength(50);

        // ========== Charity Assignment ==========
        builder.Property(x => x.FK_CharityId);

        // ========== Documents ==========
        builder.Property(x => x.FK_AttachedFileId);
        builder.Property(x => x.FK_ProjectReportFileId);

        // ========== Status ==========
        builder.Property(x => x.IsFinished)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // ========== Relationships ==========

        // Office Project Type (lookup)
        builder.HasOne(x => x.OfficeProjectType)
            .WithMany()
            .HasForeignKey(x => x.FK_OfficeProjectTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Country (lookup)
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.FK_CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Region (lookup)
        builder.HasOne(x => x.Region)
            .WithMany()
            .HasForeignKey(x => x.FK_RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Center (lookup)
        builder.HasOne(x => x.Center)
            .WithMany()
            .HasForeignKey(x => x.FK_CenterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Assigned Charity
        builder.HasOne(x => x.Charity)
            .WithMany()
            .HasForeignKey(x => x.FK_CharityId)
            .OnDelete(DeleteBehavior.Restrict);

        //// Attached File
        //builder.HasOne(x => x.AttachedFile)
        //    .WithMany()
        //    .HasForeignKey(x => x.FK_AttachedFileId)
        //    .OnDelete(DeleteBehavior.SetNull);

        //// Project Report File
        //builder.HasOne(x => x.ProjectReportFile)
        //    .WithMany()
        //    .HasForeignKey(x => x.FK_ProjectReportFileId)
        //    .OnDelete(DeleteBehavior.SetNull);

        // ========== Indexes ==========

        // Core indexes for performance
        builder.HasIndex(x => x.ProjectName);
        builder.HasIndex(x => x.ProjectDate);
        builder.HasIndex(x => x.IsFinished);
        builder.HasIndex(x => x.ProjectEndDate);

        // Foreign key indexes
        builder.HasIndex(x => x.FK_OfficeProjectTypeId);
        builder.HasIndex(x => x.FK_CountryId);
        builder.HasIndex(x => x.FK_RegionId);
        builder.HasIndex(x => x.FK_CenterId);
        builder.HasIndex(x => x.FK_CharityId);
        builder.HasIndex(x => x.FK_AttachedFileId);
        builder.HasIndex(x => x.FK_ProjectReportFileId);

        // Composite indexes for common queries
        builder.HasIndex(x => new { x.FK_CharityId, x.IsFinished });
        builder.HasIndex(x => new { x.ProjectDate, x.IsFinished });
        builder.HasIndex(x => new { x.FK_CountryId, x.FK_RegionId, x.FK_CenterId });
    }
}
