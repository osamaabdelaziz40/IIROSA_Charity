using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for HouseStatus lookup entity (epic 7, UC-REF-03)
/// </summary>
public class HouseStatusConfiguration : IEntityTypeConfiguration<HouseStatus>
{
    public void Configure(EntityTypeBuilder<HouseStatus> builder)
    {
        builder.ToTable(nameof(HouseStatus), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, SortOrder, etc.) are inherited from LookupEntityBase
    }
}
