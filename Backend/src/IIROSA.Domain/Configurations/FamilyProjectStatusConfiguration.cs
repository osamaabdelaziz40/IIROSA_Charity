using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Entity Framework Configuration for FamilyProjectStatus lookup entity (حالة المشروع)
/// </summary>
public class FamilyProjectStatusConfiguration : IEntityTypeConfiguration<FamilyProjectStatus>
{
    public void Configure(EntityTypeBuilder<FamilyProjectStatus> builder)
    {
        builder.ToTable(nameof(FamilyProjectStatus), MappingDefaults.LOOKUP_SCHEMA);

        builder.HasKey(x => x.Id);

        // All properties (NameAr, NameEn, Name, IsActive, SortOrder, etc.) are inherited from LookupEntityBase
    }
}
