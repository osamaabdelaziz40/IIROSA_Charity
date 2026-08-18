using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

public class SponsorConfiguration : IEntityTypeConfiguration<Sponsor>
{
    public void Configure(EntityTypeBuilder<Sponsor> builder)
    {
        builder.ToTable(nameof(Sponsor), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(100);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.SponsorshipType)
            .HasMaxLength(50);

        builder.Property(x => x.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Email);
    }
}
