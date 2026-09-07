using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework configuration for the RefuseReason lookup (UC-ORR-08, epic 9).
/// </summary>
public class RefuseReasonConfiguration : IEntityTypeConfiguration<RefuseReason>
{
    public void Configure(EntityTypeBuilder<RefuseReason> builder)
    {
        builder.ToTable(nameof(RefuseReason), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, SortOrder, etc.) are inherited from LookupEntityBase
    }
}
