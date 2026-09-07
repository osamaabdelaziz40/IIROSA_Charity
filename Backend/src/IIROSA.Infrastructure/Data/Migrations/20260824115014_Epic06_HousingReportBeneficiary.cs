using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// UC-HOU-06/08 (§11.S.3): ChildOrParent discriminates child vs guardian (housing family)
    /// periodic reports; FK_HousingFamilyId links guardian-subject reports to the family.
    /// Regenerated 2026-08-24: the first attempt (--no-build) loaded a stale Infrastructure
    /// Efmig assembly from a parallel session's rebuild race and produced an EMPTY migration
    /// while still rewriting the snapshot. Always full-build the Efmig configuration before
    /// `ef migrations add` in this repo (live-API bin lock makes Debug unbuildable).
    /// </remarks>
    public partial class Epic06_HousingReportBeneficiary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChildOrParent",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "FK_HousingFamilyId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReport_FK_HousingFamilyId_ChildOrParent",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                columns: new[] { "FK_HousingFamilyId", "ChildOrParent" });

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicOrphanReport_Family_FK_HousingFamilyId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                column: "FK_HousingFamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicOrphanReport_Family_FK_HousingFamilyId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropIndex(
                name: "IX_PeriodicOrphanReport_FK_HousingFamilyId_ChildOrParent",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropColumn(
                name: "ChildOrParent",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropColumn(
                name: "FK_HousingFamilyId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");
        }
    }
}
