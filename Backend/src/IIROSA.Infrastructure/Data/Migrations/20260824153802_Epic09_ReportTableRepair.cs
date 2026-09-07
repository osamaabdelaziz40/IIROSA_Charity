using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Repairs the PeriodicOrphanReport table: the re-platform copy brought a reworked
    /// §14.S.2 / §11.S.4 report entity (ReportNo, review flags, health/education/activities
    /// blocks, five attachment ids) and a model snapshot already in that shape, but NO
    /// migration ever materialized the delta — every later migrations add diffed against the
    /// matching snapshot, so the table kept the InitialCreate legacy shape and every report
    /// write/read failed at runtime (invalid column). Found live by the 6-8 battery
    /// (2026-08-24); the table was EMPTY, so the four legacy columns are dropped data-free.
    /// Column-exact against ApplicationDbContextModelSnapshot; indexes already conformed.
    /// </summary>
    public partial class Epic09_ReportTableRepair : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Legacy InitialCreate columns the reworked entity no longer maps (table was empty).
            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "SubmissionDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "ReviewDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");

            // The reworked §14.S.2/§11.S.4 report shape — present in the model snapshot since the
            // re-platform copy, never materialized by any migration (snapshot-vs-database drift).
            migrationBuilder.AddColumn<string>(
                name: "Achievement",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "AchievementArr",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                // Review 2026-08-24: model declares these bools NON-nullable — the file
                // originally said nullable: true, so the next migrations add would have
                // emitted surprise AlterColumns (dev DB aligned by hand the same day).
                nullable: false);
            migrationBuilder.AddColumn<DateTime?>(
                name: "ActiveDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<decimal?>(
                name: "AnnualFeeForStudy",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Course",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "CourseName",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<bool?>(
                name: "Dead",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                nullable: true);
            migrationBuilder.AddColumn<DateTime?>(
                name: "DeathDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                nullable: false);
            migrationBuilder.AddColumn<DateTime?>(
                name: "DeletedDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Disability",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "DisabilityDescription",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Disease",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "DiseaseDescription",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<bool?>(
                name: "DropOut",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "DropOutStageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "DropOutYear",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "EducationDegree",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "EducationalLevelId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "EducationalStageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "EducationalYear",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Faculty",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "GraduationYear",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "HadeethStatus",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "HighestEducationalLevel",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "HighestEducationalLevelYear",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Hobby",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                nullable: false);
            migrationBuilder.AddColumn<bool?>(
                name: "IsOrphanStudent",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsRefused",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                nullable: false);
            migrationBuilder.AddColumn<bool>(
                name: "Locked",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                nullable: false);
            migrationBuilder.AddColumn<DateTime?>(
                name: "LockedDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<DateTime?>(
                name: "MarriageDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<bool?>(
                name: "Married",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                nullable: true);
            migrationBuilder.AddColumn<Guid?>(
                name: "MedicalReportImageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "MedicalStatus",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "MessageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<bool?>(
                name: "MissingDocuments",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "MissingDocumentsName",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<Guid?>(
                name: "OrphanCertificateImageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid?>(
                name: "OrphanDeadImageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid?>(
                name: "OrphanImageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid?>(
                name: "OrphanMarriageImageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "OrphanMessage",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ProfessionName",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "QuranVerses",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "RefuseReason",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "RefuseReasonId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ReportNo",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<DateTime?>(
                name: "ReportPeriodFrom",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<DateTime?>(
                name: "ReportPeriodTo",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "RestStudyingYears",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "Reviewed",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "bit",
                nullable: true);
            migrationBuilder.AddColumn<DateTime?>(
                name: "ReviewedDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<Guid?>(
                name: "ReviewerId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "School",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "SchoolType",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "SportName",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<int?>(
                name: "StudyingYears",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Wish",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "WishArr",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Inverse: remove the reworked columns and restore the legacy InitialCreate shape.
            migrationBuilder.DropColumn(
                name: "Achievement",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "AchievementArr",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Active",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "ActiveDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "AnnualFeeForStudy",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Course",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "CourseName",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Dead",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "DeathDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Department",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Disability",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "DisabilityDescription",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Disease",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "DiseaseDescription",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "DropOut",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "DropOutStageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "DropOutYear",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "EducationDegree",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "EducationalLevelId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "EducationalStageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "EducationalYear",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Faculty",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "GraduationYear",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "HadeethStatus",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "HighestEducationalLevel",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "HighestEducationalLevelYear",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Hobby",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "IsOrphanStudent",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "IsRefused",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Locked",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "LockedDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "MarriageDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Married",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "MedicalReportImageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "MedicalStatus",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "MissingDocuments",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "MissingDocumentsName",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "OrphanCertificateImageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "OrphanDeadImageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "OrphanImageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "OrphanMarriageImageId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "OrphanMessage",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "ProfessionName",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "QuranVerses",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "RefuseReason",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "RefuseReasonId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "ReportNo",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "ReportPeriodFrom",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "ReportPeriodTo",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "RestStudyingYears",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Reviewed",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "ReviewedDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "ReviewerId",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "School",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "SchoolType",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "SportName",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "StudyingYears",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "Wish",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.DropColumn(
                name: "WishArr",
                table: "PeriodicOrphanReport",
                schema: "IIROSA");
            migrationBuilder.AddColumn<Guid?>(
                name: "SubmittedBy",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<DateTime?>(
                name: "SubmissionDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<Guid?>(
                name: "ReviewedBy",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<DateTime?>(
                name: "ReviewDate",
                table: "PeriodicOrphanReport",
                schema: "IIROSA",
                type: "datetime2",
                nullable: true);
        }
    }
}
