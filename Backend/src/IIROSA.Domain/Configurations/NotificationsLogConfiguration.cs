using IIROSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IIROSA.Domain.Configurations;

public class NotificationsLogConfiguration : IEntityTypeConfiguration<NotificationsLog>
{
    public void Configure(EntityTypeBuilder<NotificationsLog> builder)
    {
        builder.ToTable(nameof(NotificationsLog), MappingDefaults.IIROSA_SCHEMA);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Message)
            .IsRequired();

        // CSV id lists — read/written by the application layer, never queried by
        // SQL join; the recipient match uses a LIKE on the delimited list.
        builder.Property(x => x.RecipientUserIds);
        builder.Property(x => x.RecipientCharityIds);

        // Delivery bookkeeping
        builder.Property(x => x.SentCount)
            .HasDefaultValue(1);
    }
}
