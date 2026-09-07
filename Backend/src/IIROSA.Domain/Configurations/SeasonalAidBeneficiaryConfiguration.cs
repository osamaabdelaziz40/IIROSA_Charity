using IIROSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IIROSA.Domain.Configurations;

/// <summary>
/// Configuration for SeasonalAidBeneficiary.
/// Deliberately sets no <see cref="EntityTypeBuilder{TEntity}.ToTable"/> call: the table was created
/// by convention in the default schema when the module was first migrated, and re-mapping it here
/// would churn the schema. Only the business-key index is declared.
/// </summary>
public class SeasonalAidBeneficiaryConfiguration : IEntityTypeConfiguration<SeasonalAidBeneficiary>
{
    public void Configure(EntityTypeBuilder<SeasonalAidBeneficiary> builder)
    {
        // BR-23: a family is registered at most once per campaign. The filter keeps the
        // constraint meaningful under soft delete — a removed registration stops blocking
        // a later re-registration of the same family.
        builder.HasIndex(x => new { x.CampaignId, x.FamilyId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
