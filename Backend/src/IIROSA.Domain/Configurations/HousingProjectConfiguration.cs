using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for HousingProject entity
/// </summary>
public class HousingProjectConfiguration : IEntityTypeConfiguration<HousingProject>
{
    public void Configure(EntityTypeBuilder<HousingProject> builder)
    {
        builder.ToTable(nameof(HousingProject), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Basic Information ==========
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.ProjectType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.ExpectedEndDate);

        builder.Property(x => x.ActualEndDate);

        // ========== Location ==========
        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Village)
            .HasMaxLength(200);

        builder.Property(x => x.GPSCoordinates)
            .HasMaxLength(100);

        // ========== Specifications ==========
        builder.Property(x => x.HousingType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AreaPerUnit)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.TotalArea)
            .HasColumnType("decimal(18,2)");

        // ========== Financial Information ==========
        builder.Property(x => x.TotalBudget)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.BudgetCurrency)
            .HasMaxLength(10)
            .HasDefaultValue("EGP");

        builder.Property(x => x.DonorName)
            .HasMaxLength(200);

        builder.Property(x => x.FinalCost)
            .HasColumnType("decimal(18,2)");

        // ========== Status and Progress ==========
        builder.Property(x => x.ProjectStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Planning");

        builder.Property(x => x.CompletionPercentage)
            .HasDefaultValue(0);

        builder.Property(x => x.CurrentStage)
            .HasMaxLength(100);

        builder.Property(x => x.ProgressNotes)
            .HasMaxLength(2000);

        builder.Property(x => x.CompletionNotes)
            .HasMaxLength(2000);

        // ========== Relationships ==========
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Region)
            .WithMany()
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Center)
            .WithMany()
            .HasForeignKey(x => x.CenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Charity)
            .WithMany()
            .HasForeignKey(x => x.CharityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Family)
            .WithMany()
            .HasForeignKey(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.StartDate);
        builder.HasIndex(x => x.ProjectStatus);
        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.RegionId);
        builder.HasIndex(x => x.CenterId);
        builder.HasIndex(x => x.CharityId);
        builder.HasIndex(x => x.FamilyId);
    }
}
