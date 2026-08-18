using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.TechnicalSupport.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for SupportTicketCategory lookup entity
/// </summary>
public class SupportTicketCategoryConfiguration : IEntityTypeConfiguration<SupportTicketCategory>
{
    public void Configure(EntityTypeBuilder<SupportTicketCategory> builder)
    {
        builder.ToTable(nameof(SupportTicketCategory), MappingDefaults.LOOKUP_SCHEMA);

        // Primary Key (Id is int, inherited from LookupEntity)
        builder.HasKey(x => x.Id);

        // Properties (NameAr, NameEn, Name, IsActive are inherited)
        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Icon)
            .HasMaxLength(100);

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.IsActive);  
    }
}
