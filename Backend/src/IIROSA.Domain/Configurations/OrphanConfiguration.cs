using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

public class OrphanConfiguration : IEntityTypeConfiguration<Orphan>
{
    public void Configure(EntityTypeBuilder<Orphan> builder)
    {
        builder.ToTable(nameof(Orphan), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Gender)
            .HasMaxLength(10);

        builder.Property(x => x.NationalId)
            .HasMaxLength(50);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(x => x.Family)
            .WithMany(x => x.Orphans)
            .HasForeignKey(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sponsor)
            .WithMany(x => x.Orphans)
            .HasForeignKey(x => x.SponsorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EducationLevel)
            .WithMany()
            .HasForeignKey(x => x.EducationLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HealthStatus)
            .WithMany()
            .HasForeignKey(x => x.HealthStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => x.SponsorId);
    }
}
