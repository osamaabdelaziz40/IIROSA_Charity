using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic05_GuardianSnapshotHoldingFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GuardianChangeRequest_FamilyId",
                schema: "IIROSA",
                table: "GuardianChangeRequest");

            migrationBuilder.AddColumn<string>(
                name: "OldGuardianRelationship",
                schema: "IIROSA",
                table: "GuardianChangeRequest",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHoldingFamily",
                schema: "IIROSA",
                table: "Family",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_GuardianChangeRequest_OnePendingPerFamily",
                schema: "IIROSA",
                table: "GuardianChangeRequest",
                column: "FamilyId",
                unique: true,
                filter: "[Status] = 1 AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GuardianChangeRequest_OnePendingPerFamily",
                schema: "IIROSA",
                table: "GuardianChangeRequest");

            migrationBuilder.DropColumn(
                name: "OldGuardianRelationship",
                schema: "IIROSA",
                table: "GuardianChangeRequest");

            migrationBuilder.DropColumn(
                name: "IsHoldingFamily",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.CreateIndex(
                name: "IX_GuardianChangeRequest_FamilyId",
                schema: "IIROSA",
                table: "GuardianChangeRequest",
                column: "FamilyId");
        }
    }
}
