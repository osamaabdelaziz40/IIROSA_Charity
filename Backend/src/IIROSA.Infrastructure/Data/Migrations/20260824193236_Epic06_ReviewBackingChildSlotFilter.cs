using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic06_ReviewBackingChildSlotFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                columns: new[] { "OrphanId", "ReportMonth", "ReportYear" },
                unique: true,
                filter: "[ChildOrParent] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                columns: new[] { "OrphanId", "ReportMonth", "ReportYear" },
                unique: true);
        }
    }
}
