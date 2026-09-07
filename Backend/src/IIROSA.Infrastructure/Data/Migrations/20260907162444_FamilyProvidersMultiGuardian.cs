using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <summary>
    /// §11.S.2 multi-guardian housing register (اضافة الاباء · AddNewParent() always):
    /// Provider.FamilyId becomes a plain lookup index — the BR-06 filtered UNIQUE
    /// "one live guardian seat per family" index is retired; the seat rule for the
    /// regular register stays enforced in code (AddProviderToFamilyAsync).
    ///
    /// Also carries two model clean-ups that were already staged in the working tree
    /// when this migration was scaffolded: the SeasonalAidCampaign.CharityId link and
    /// the Incoming.OutgoingId reply link (replies now flow one way through
    /// Outgoing.IncomingId). The dev database was patched ahead of the code for
    /// SeasonalAidCampaign, so those operations are guarded per-statement and the
    /// migration stays replayable on a fresh database.
    /// </summary>
    public partial class FamilyProvidersMultiGuardian : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Provider_FamilyId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_FamilyId",
                schema: "IIROSA",
                table: "Provider",
                column: "FamilyId");

            migrationBuilder.DropForeignKey(
                name: "FK_Incoming_Outgoing_OutgoingId",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.DropIndex(
                name: "IX_Incoming_OutgoingId",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.DropColumn(
                name: "OutgoingId",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SeasonalAidCampaign_Charity_CharityId')
    ALTER TABLE dbo.SeasonalAidCampaign DROP CONSTRAINT FK_SeasonalAidCampaign_Charity_CharityId;
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SeasonalAidCampaign_CharityId' AND object_id = OBJECT_ID(N'dbo.SeasonalAidCampaign'))
    DROP INDEX IX_SeasonalAidCampaign_CharityId ON dbo.SeasonalAidCampaign;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.SeasonalAidCampaign') AND name = N'CharityId')
    ALTER TABLE dbo.SeasonalAidCampaign DROP COLUMN CharityId;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Provider_FamilyId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_FamilyId",
                schema: "IIROSA",
                table: "Provider",
                column: "FamilyId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddColumn<Guid>(
                name: "OutgoingId",
                schema: "IIROSA",
                table: "Incoming",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_OutgoingId",
                schema: "IIROSA",
                table: "Incoming",
                column: "OutgoingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incoming_Outgoing_OutgoingId",
                schema: "IIROSA",
                table: "Incoming",
                column: "OutgoingId",
                principalSchema: "IIROSA",
                principalTable: "Outgoing",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.SeasonalAidCampaign') AND name = N'CharityId')
BEGIN
    ALTER TABLE dbo.SeasonalAidCampaign ADD CharityId uniqueidentifier NULL;
    CREATE INDEX IX_SeasonalAidCampaign_CharityId ON dbo.SeasonalAidCampaign(CharityId);
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SeasonalAidCampaign_Charity_CharityId')
    ALTER TABLE dbo.SeasonalAidCampaign ADD CONSTRAINT FK_SeasonalAidCampaign_Charity_CharityId
        FOREIGN KEY (CharityId) REFERENCES IIROSA.Charity(Id);");
        }
    }
}
