using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for Relative entity
/// </summary>
public class RelativeConfiguration : IEntityTypeConfiguration<Relative>
{
    public void Configure(EntityTypeBuilder<Relative> builder)
    {
        builder.ToTable(nameof(Relative), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Basic Information ==========
        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.RelationshipType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Gender)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.DateOfBirth)
            .IsRequired();

        builder.Property(x => x.PlaceOfBirth)
            .HasMaxLength(200);

        builder.Property(x => x.NationalId)
            .HasMaxLength(50);

        builder.Property(x => x.Job)
            .HasMaxLength(200);

        builder.Property(x => x.MonthlyIncome)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Phone)
            .HasMaxLength(30);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // ========== Status ==========
        builder.Property(x => x.IsAlive)
            .HasDefaultValue(true);

        builder.Property(x => x.IsLivingWithFamily)
            .HasDefaultValue(false);

        builder.Property(x => x.DeathDate);

        // ========== Relationships ==========
        builder.HasOne(x => x.Family)
            .WithMany(f => f.Relatives)
            .HasForeignKey(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EducationLevel)
            .WithMany()
            .HasForeignKey(x => x.EducationLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HealthStatus)
            .WithMany()
            .HasForeignKey(x => x.HealthStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => x.NationalId);
        builder.HasIndex(x => x.RelationshipType);
        builder.HasIndex(x => x.IsAlive);
        builder.HasIndex(x => x.IsLivingWithFamily);
    }
}
