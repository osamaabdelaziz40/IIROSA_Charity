using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable(nameof(Family), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.HeadOfFamily)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.FamilyStatus)
            .HasMaxLength(50);

        builder.Property(x => x.FinancialStatus)
            .HasMaxLength(50);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // Refugee register household fields (epic 7, UC-REF-03) — all nullable
        builder.Property(x => x.NearBy)
            .HasMaxLength(100);

        builder.Property(x => x.Street)
            .HasMaxLength(100);

        builder.Property(x => x.RentAmount)
            .HasColumnType("decimal(18,2)");

        // Relationships
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Refugee register navigations (epic 7, UC-REF-03)
        builder.HasOne(x => x.Region)
            .WithMany()
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Center)
            .WithMany()
            .HasForeignKey(x => x.CenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HouseOwnership)
            .WithMany()
            .HasForeignKey(x => x.HouseOwnershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HouseStatus)
            .WithMany()
            .HasForeignKey(x => x.HouseStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IncomeType)
            .WithMany()
            .HasForeignKey(x => x.IncomeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Charity)
            .WithMany(x => x.Families)
            .HasForeignKey(x => x.CharityId)
            .OnDelete(DeleteBehavior.Restrict);

        // UC-HOU-05 allocation: building + flat (nullable; Regular/Refugee families have neither)
        builder.HasOne(x => x.HousingBuilding)
            .WithMany()
            .HasForeignKey(x => x.FK_HousingBuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HousingFlat)
            .WithMany()
            .HasForeignKey(x => x.FK_HousingFlatId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.CharityId);
        // Register-discriminator filter (UC-REF-01 ?familyType=) — 7-1 Task 1 deliverable,
        // dropped when 7-3's Epic7_RefugeeContract migration was absorbed un-minted (review P11)
        builder.HasIndex(x => x.FamilyType);
    }
}
