using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.TechnicalSupport.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for SupportTicketPriority lookup entity
/// </summary>
public class SupportTicketPriorityConfiguration : IEntityTypeConfiguration<SupportTicketPriority>
{
    public void Configure(EntityTypeBuilder<SupportTicketPriority> builder)
    {
        builder.ToTable(nameof(SupportTicketPriority), MappingDefaults.LOOKUP_SCHEMA);

        // Primary Key (Id is int, inherited from LookupEntity)
        builder.HasKey(x => x.Id);

        // Properties (NameAr, NameEn, Name, IsActive are inherited)
        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.ColorCode)
            .HasMaxLength(20);

        builder.Property(x => x.SeverityLevel)
            .HasDefaultValue(0);

        builder.Property(x => x.ResponseTimeHours)
            .HasDefaultValue(24);

        // Indexes
        builder.HasIndex(x => x.SeverityLevel);
        builder.HasIndex(x => x.IsActive);
    }
}
