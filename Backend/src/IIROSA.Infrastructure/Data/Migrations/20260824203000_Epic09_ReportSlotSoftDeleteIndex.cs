using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic09_ReportSlotSoftDeleteIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Review P7/P7b (epic-9 review 2026-08-24): the child slot index kept
            // soft-deleted CHILD rows inside it, so a deleted report permanently
            // squatted its (orphan, month) slot — with no restore endpoint, the 9-6
            // delete-then-re-enter correction flow could never re-file that month.
            // Same [IsDeleted] = 0 liveness filter the guardian family+month index
            // (Epic06_ReviewBacking) already applies. Supersedes the "soft-deleted
            // rows hold slots" note this index previously carried.
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
                filter: "[ChildOrParent] = 1 AND [IsDeleted] = 0");

            // Review P12 (epic-9 review 2026-08-24): Epic09_ReportTableRepair created
            // [Reviewed] as nullable while the model declares it non-nullable. Legacy
            // NULL rows silently fall out of every !Reviewed predicate (SQL NULL != 0),
            // so they vanished from the pending filter and its counters. Backfill to
            // pending — an unreviewed report is pending by definition — then align the
            // column to the model.
            migrationBuilder.Sql(
                "UPDATE [IIROSA].[PeriodicOrphanReport] SET [Reviewed] = CAST(0 AS bit) WHERE [Reviewed] IS NULL");

            migrationBuilder.AlterColumn<bool>(
                name: "Reviewed",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Reviewed",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: false);

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
    }
}
