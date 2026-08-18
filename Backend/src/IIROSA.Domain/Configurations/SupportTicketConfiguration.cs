using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.TechnicalSupport;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for SupportTicket entity
/// </summary>
public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable(nameof(SupportTicket), MappingDefaults.IIROSA_SCHEMA);

        // Primary Key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.ResolutionDescription)
            .HasMaxLength(4000);

        builder.Property(x => x.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.AssignedTo)
            .HasMaxLength(450);

        builder.Property(x => x.ResolvedBy)
            .HasMaxLength(450);

        builder.Property(x => x.BrowserInfo)
            .HasMaxLength(500);

        builder.Property(x => x.PageUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.UserAction)
            .HasMaxLength(1000);

        builder.Property(x => x.AttachmentFilePath)
            .HasMaxLength(1000);

        builder.Property(x => x.AttachmentFileName)
            .HasMaxLength(500);

        builder.Property(x => x.AttachmentFileSize);

        // Foreign Keys
        builder.HasOne(x => x.Category)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Priority)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.PriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Status)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.PriorityId);
        builder.HasIndex(x => x.StatusId);
        builder.HasIndex(x => x.IsSolved);
        builder.HasIndex(x => x.AssignedTo);
        builder.HasIndex(x => x.CreatedOn);
        builder.HasIndex(x => x.ResolvedOn);
    }
}
