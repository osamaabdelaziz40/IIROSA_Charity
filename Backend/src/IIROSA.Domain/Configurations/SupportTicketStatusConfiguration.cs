using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.TechnicalSupport.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for SupportTicketStatus lookup entity
/// </summary>
public class SupportTicketStatusConfiguration : IEntityTypeConfiguration<SupportTicketStatus>
{
    public void Configure(EntityTypeBuilder<SupportTicketStatus> builder)
    {
        builder.ToTable(nameof(SupportTicketStatus), MappingDefaults.LOOKUP_SCHEMA);

        // Primary Key (Id is int, inherited from LookupEntity)
        builder.HasKey(x => x.Id);

        // Properties (NameAr, NameEn, Name, IsActive are inherited)
        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.ColorCode)
            .HasMaxLength(20);

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        builder.Property(x => x.IsTerminalStatus)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsTerminalStatus);
    }
}
