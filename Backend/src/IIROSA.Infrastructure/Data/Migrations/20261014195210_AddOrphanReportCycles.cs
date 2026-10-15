using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrphanReportCycles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.AddColumn<Guid>(
                name: "CycleId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrphanReportCycle",
                schema: "IIROSA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StatusChangedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrphanReportCycle", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReport_CycleId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_CycleId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                columns: new[] { "OrphanId", "CycleId" },
                unique: true,
                filter: "[CycleId] IS NOT NULL AND [ChildOrParent] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                columns: new[] { "OrphanId", "ReportMonth", "ReportYear" },
                unique: true,
                filter: "[ChildOrParent] = 1 AND [IsDeleted] = 0 AND [CycleId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanReportCycle_StartDate_EndDate",
                schema: "IIROSA",
                table: "OrphanReportCycle",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_OrphanReportCycle_Status",
                schema: "IIROSA",
                table: "OrphanReportCycle",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicOrphanReport_OrphanReportCycle_CycleId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                column: "CycleId",
                principalSchema: "IIROSA",
                principalTable: "OrphanReportCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicOrphanReport_OrphanReportCycle_CycleId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropTable(
                name: "OrphanReportCycle",
                schema: "IIROSA");

            migrationBuilder.DropIndex(
                name: "IX_PeriodicOrphanReport_CycleId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_CycleId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropColumn(
                name: "CycleId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                columns: new[] { "OrphanId", "ReportMonth", "ReportYear" },
                unique: true,
                filter: "[ChildOrParent] = 1 AND [IsDeleted] = 0");
        }
    }
}
