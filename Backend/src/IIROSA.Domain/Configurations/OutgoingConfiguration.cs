using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Core configuration for Outgoing entity
/// Configures the relationship between Outgoing and Incoming entities
/// </summary>
public class OutgoingConfiguration : IEntityTypeConfiguration<Outgoing>
{
    public void Configure(EntityTypeBuilder<Outgoing> builder)
    {
        builder.ToTable(nameof(Outgoing), MappingDefaults.IIROSA_SCHEMA);

        // Configure the relationship: An outgoing letter can be a reply to one incoming letter
        builder.HasOne(o => o.IncomingLetter)
            .WithMany(i => i.Replies)
            .HasForeignKey(o => o.IncomingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
