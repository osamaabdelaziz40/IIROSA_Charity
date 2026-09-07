using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic06_ReviewBacking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReportNo",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EducationalQualificationId",
                schema: "IIROSA",
                table: "Orphan",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReport_FK_HousingFamilyId_ReportYear_ReportMonth",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                columns: new[] { "FK_HousingFamilyId", "ReportYear", "ReportMonth" },
                unique: true,
                filter: "[ChildOrParent] = 2 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReport_ReportNo",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                column: "ReportNo",
                unique: true,
                filter: "[ReportNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orphan_EducationalQualificationId",
                schema: "IIROSA",
                table: "Orphan",
                column: "EducationalQualificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orphan_EducationLevel_EducationalQualificationId",
                schema: "IIROSA",
                table: "Orphan",
                column: "EducationalQualificationId",
                principalSchema: "Lookup",
                principalTable: "EducationLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orphan_EducationLevel_EducationalQualificationId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropIndex(
                name: "IX_PeriodicOrphanReport_FK_HousingFamilyId_ReportYear_ReportMonth",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropIndex(
                name: "IX_PeriodicOrphanReport_ReportNo",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropIndex(
                name: "IX_Orphan_EducationalQualificationId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropColumn(
                name: "EducationalQualificationId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.AlterColumn<string>(
                name: "ReportNo",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
