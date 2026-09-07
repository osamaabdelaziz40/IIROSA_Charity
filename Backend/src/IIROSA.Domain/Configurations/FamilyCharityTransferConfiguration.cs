using IIROSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for the FamilyCharityTransfer entity (UC-FAM-06)
/// </summary>
public class FamilyCharityTransferConfiguration : IEntityTypeConfiguration<FamilyCharityTransfer>
{
    public void Configure(EntityTypeBuilder<FamilyCharityTransfer> builder)
    {
        builder.ToTable(nameof(FamilyCharityTransfer), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        // ========== Family / Movement ==========
        builder.Property(x => x.FamilyId)
            .IsRequired();

        builder.Property(x => x.FromCharityId)
            .IsRequired();

        builder.Property(x => x.ToCharityId)
            .IsRequired();

        // ========== Reason ==========
        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        // ========== Relationships ==========

        // Family — restrict: an audit record must never cascade away with its family
        builder.HasOne(x => x.Family)
            .WithMany()
            .HasForeignKey(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sending charity
        builder.HasOne(x => x.FromCharity)
            .WithMany()
            .HasForeignKey(x => x.FromCharityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Receiving charity
        builder.HasOne(x => x.ToCharity)
            .WithMany()
            .HasForeignKey(x => x.ToCharityId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== Indexes ==========
        builder.HasIndex(x => x.FamilyId);
    }
}
