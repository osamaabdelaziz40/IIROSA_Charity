using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

public class FamilyPhoneConfiguration : IEntityTypeConfiguration<FamilyPhone>
{
    public void Configure(EntityTypeBuilder<FamilyPhone> builder)
    {
        builder.ToTable(nameof(FamilyPhone), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(20);

        // One live default number per family — filtered unique index, same convention as
        // IX_GuardianChangeRequest_OnePendingPerFamily. Soft-deleted rows leave the filter,
        // so replacing the default never trips the index.
        builder.HasIndex(x => x.FamilyId)
            .IsUnique()
            .HasDatabaseName("UX_FamilyPhone_OneDefaultPerFamily")
            .HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0");

        builder.HasIndex(x => x.FamilyId);

        builder.HasOne(x => x.Family)
            .WithMany(x => x.Phones)
            .HasForeignKey(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
