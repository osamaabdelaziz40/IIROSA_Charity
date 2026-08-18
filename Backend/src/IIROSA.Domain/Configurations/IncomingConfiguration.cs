using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Core configuration for Incoming entity
/// Configures the relationship between Incoming and Outgoing entities
/// </summary>
public class IncomingConfiguration : IEntityTypeConfiguration<Incoming>
{
    public void Configure(EntityTypeBuilder<Incoming> builder)
    {
        builder.ToTable(nameof(Incoming), MappingDefaults.IIROSA_SCHEMA);

        // Configure the relationship: An incoming letter can have one related outgoing letter (reply to)
        builder.HasOne(i => i.OutgoingLetter)
            .WithMany()
            .HasForeignKey(i => i.OutgoingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure the relationship: An incoming letter can have many outgoing replies
        builder.HasMany(i => i.Replies)
            .WithOne(o => o.IncomingLetter)
            .HasForeignKey(o => o.IncomingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
