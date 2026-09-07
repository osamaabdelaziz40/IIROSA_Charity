using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for Mission entity
/// </summary>
public class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.ToTable(nameof(Mission), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Basic Information ==========
        builder.Property(x => x.MissionTarget)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.MissionDetails)
            .HasMaxLength(1000);

        builder.Property(x => x.Details)
            .HasMaxLength(2000);

        // ========== Mission Classification ==========
        builder.Property(x => x.FK_MissionTypeId);
        builder.Property(x => x.FK_MissionTimeTypeId);
        builder.Property(x => x.FK_MissionInterviewTypeId);

        // ========== Scheduling ==========
        builder.Property(x => x.MissionDate)
            .IsRequired();

        builder.Property(x => x.MissionCompletedDate);
        builder.Property(x => x.IsMissionCompleted)
            .IsRequired();

        builder.Property(x => x.MissionCompletedTxt)
            .HasMaxLength(1000);

        // ========== Location ==========
        builder.Property(x => x.FK_CountryId);
        builder.Property(x => x.FK_RegionId);
        builder.Property(x => x.FK_CenterId);

        builder.Property(x => x.MissionLocation)
            .HasMaxLength(500);

        builder.Property(x => x.Village)
            .HasMaxLength(100);

        // ========== Assignment ==========
        builder.Property(x => x.FK_UserId)
            .HasMaxLength(450);

        // ========== Ownership ==========
        builder.Property(x => x.FK_CharityId);

        // ========== Event Information ==========
        builder.Property(x => x.EntityName)
            .HasMaxLength(200);

        builder.Property(x => x.ConferenceName)
            .HasMaxLength(200);

        // ========== Relationships ==========

        // Mission Type (lookup)
        builder.HasOne(x => x.MissionType)
            .WithMany()
            .HasForeignKey(x => x.FK_MissionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Mission Time Type (lookup)
        builder.HasOne(x => x.MissionTimeType)
            .WithMany(m => m.Missions)
            .HasForeignKey(x => x.FK_MissionTimeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Mission Interview Type (lookup, UC-MSN-04)
        builder.HasOne(x => x.MissionInterviewType)
            .WithMany()
            .HasForeignKey(x => x.FK_MissionInterviewTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Owning Charity
        builder.HasOne(x => x.Charity)
            .WithMany()
            .HasForeignKey(x => x.FK_CharityId)
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

        // Assigned User (ApplicationUser)
        builder.HasOne(x => x.AssignedUser)
            .WithMany()
            .HasForeignKey(x => x.FK_UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========

        // Core indexes for performance
        builder.HasIndex(x => x.MissionDate);
        builder.HasIndex(x => x.IsMissionCompleted);
        builder.HasIndex(x => x.MissionCompletedDate);

        // Foreign key indexes
        builder.HasIndex(x => x.FK_MissionTypeId);
        builder.HasIndex(x => x.FK_MissionTimeTypeId);
        builder.HasIndex(x => x.FK_MissionInterviewTypeId);
        builder.HasIndex(x => x.FK_CountryId);
        builder.HasIndex(x => x.FK_RegionId);
        builder.HasIndex(x => x.FK_CenterId);
        builder.HasIndex(x => x.FK_UserId);
        builder.HasIndex(x => x.FK_CharityId);

        // Composite indexes for common queries
        builder.HasIndex(x => new { x.FK_UserId, x.IsMissionCompleted });
        builder.HasIndex(x => new { x.MissionDate, x.IsMissionCompleted });
    }
}
