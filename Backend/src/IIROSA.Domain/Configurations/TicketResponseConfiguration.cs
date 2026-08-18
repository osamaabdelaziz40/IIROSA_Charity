using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.TechnicalSupport;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for TicketResponse entity
/// </summary>
public class TicketResponseConfiguration : IEntityTypeConfiguration<TicketResponse>
{
    public void Configure(EntityTypeBuilder<TicketResponse> builder)
    {
        builder.ToTable(nameof(TicketResponse), MappingDefaults.IIROSA_SCHEMA);

        // Primary Key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.ResponseText)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.RespondedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.ResponderName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ResponderEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AttachmentFilePath)
            .HasMaxLength(1000);

        builder.Property(x => x.AttachmentFileName)
            .HasMaxLength(500);

        builder.Property(x => x.AttachmentFileSize);

        // Foreign Key
        builder.HasOne(x => x.Ticket)
            .WithMany(x => x.Responses)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => x.RespondedByUserId);
        builder.HasIndex(x => x.IsInternalNote);
        builder.HasIndex(x => x.CreatedOn);
    }
}
