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

        // §11.S.2 «حاصل على مؤهل دراسى» (review D3 2026-08-24) — same catalogue, second
        // edge; explicit FK keeps the two EducationLevel navigations unambiguous.
        builder.HasOne(x => x.EducationalQualification)
            .WithMany()
            .HasForeignKey(x => x.EducationalQualificationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HealthStatus)
            .WithMany()
            .HasForeignKey(x => x.HealthStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Refugee register extension (epic 7, UC-REF-03)
        builder.HasOne(x => x.SocialStatus)
            .WithMany()
            .HasForeignKey(x => x.SocialStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => x.SponsorId);
    }
}
