using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

public class CharityConfiguration : IEntityTypeConfiguration<Charity>
{
    public void Configure(EntityTypeBuilder<Charity> builder)
    {
        builder.ToTable(nameof(Charity), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // Basic Information
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NGOType)
            .HasMaxLength(100);

        // Contact Information
        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.StreetName)
            .HasMaxLength(200);

        builder.Property(x => x.Village)
            .HasMaxLength(100);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);

        builder.Property(x => x.MailBox)
            .HasMaxLength(20);

        // Phone/Contact
        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Phone2)
            .HasMaxLength(20);

        builder.Property(x => x.HomePhone)
            .HasMaxLength(20);

        builder.Property(x => x.Fax)
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100);

        // Location
        builder.Property(x => x.NgoMapLocation)
            .HasMaxLength(500);

        // Banking
        builder.Property(x => x.BankAccount)
            .HasMaxLength(50);

        builder.Property(x => x.IBAN)
            .HasMaxLength(34);

        // Management - Boss
        builder.Property(x => x.BossName)
            .HasMaxLength(100);

        builder.Property(x => x.BossJobName)
            .HasMaxLength(100);

        builder.Property(x => x.BossPhone1)
            .HasMaxLength(20);

        builder.Property(x => x.BossPhone2)
            .HasMaxLength(20);

        // Management - Responsible
        builder.Property(x => x.ResponsibleJobName)
            .HasMaxLength(100);

        builder.Property(x => x.ResponsiblePhone1)
            .HasMaxLength(20);

        builder.Property(x => x.ResponsiblePhone2)
            .HasMaxLength(20);

        // Settings
        builder.Property(x => x.IconId);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // User Account
        builder.Property(x => x.UserId)
            .HasMaxLength(450);

        // Legacy Fields
        builder.Property(x => x.ContactPerson)
            .HasMaxLength(100);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(20);

        builder.Property(x => x.ContactEmail)
            .HasMaxLength(100);

        builder.Property(x => x.LicenseNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Website)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Region)
            .WithMany(r => r.Charities)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Center)
            .WithMany(c => c.Charities)
            .HasForeignKey(x => x.CenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Bank)
            .WithMany()
            .HasForeignKey(x => x.BankId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsLocked);
        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.RegionId);
        builder.HasIndex(x => x.CenterId);
        builder.HasIndex(x => x.UserId);
    }
}
