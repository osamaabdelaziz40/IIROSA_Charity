using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for EducationLevel lookup entity
/// </summary>
public class EducationLevelConfiguration : IEntityTypeConfiguration<EducationLevel>
{
    public void Configure(EntityTypeBuilder<EducationLevel> builder)
    {
        builder.ToTable(nameof(EducationLevel), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, SortOrder, etc.) are inherited from LookupEntityBase
    }
}
