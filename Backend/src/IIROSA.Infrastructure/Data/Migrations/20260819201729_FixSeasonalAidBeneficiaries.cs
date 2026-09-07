using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Seasonal aid data-integrity fixes (EP-12):
    ///  - BR-23: unique filtered index enforcing one live registration per family per campaign.
    ///  - Data repair: campaigns created before the IsDeleted-initialiser fix were born
    ///    soft-deleted and invisible in every list.
    ///
    /// Deliberately scoped to seasonal aid only. EF's diff also surfaced unrelated model drift
    /// (PeriodicOrphanReport columns, an ApplicationUser duplicate table) inherited from earlier
    /// epics whose entity changes were never migrated — that drift is not ours to ship here.
    /// </summary>
    public partial class FixSeasonalAidBeneficiaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Collapse any duplicate live registrations to the earliest one, so the unique
            // index below can be created on databases that predate the duplicate-skip logic.
            migrationBuilder.Sql(@"
;WITH d AS (
    SELECT Id, ROW_NUMBER() OVER (PARTITION BY CampaignId, FamilyId ORDER BY RegistrationDate, CreatedOn, Id) AS rn
    FROM dbo.SeasonalAidBeneficiary
    WHERE IsDeleted = 0
)
UPDATE b
SET b.IsDeleted = 1, b.IsRegistered = 0
FROM dbo.SeasonalAidBeneficiary b
JOIN d ON b.Id = d.Id
WHERE d.rn > 1;");

            migrationBuilder.DropIndex(
                name: "IX_SeasonalAidBeneficiary_CampaignId",
                table: "SeasonalAidBeneficiary");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalAidBeneficiary_CampaignId_FamilyId",
                table: "SeasonalAidBeneficiary",
                columns: new[] { "CampaignId", "FamilyId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            // Campaign deletion was hard-remove until this epic, so no campaign row was ever
            // soft-deleted deliberately — every IsDeleted = 1 is the initialiser defect.
            migrationBuilder.Sql("UPDATE dbo.SeasonalAidCampaign SET IsDeleted = 0 WHERE IsDeleted = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SeasonalAidBeneficiary_CampaignId_FamilyId",
                table: "SeasonalAidBeneficiary");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalAidBeneficiary_CampaignId",
                table: "SeasonalAidBeneficiary",
                column: "CampaignId");
        }
    }
}
