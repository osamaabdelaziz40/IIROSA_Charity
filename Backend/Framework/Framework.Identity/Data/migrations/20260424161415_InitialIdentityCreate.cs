using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Framework.Identity.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentityCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("393bf7cf-e959-4d86-a8ed-88a1fde51ae4"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4f12bea4-3d8a-4922-b9f6-2c2012692e55"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("65f20349-837c-42e0-8ec9-63e328b7828b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6cab6d0b-9380-4037-9950-673ea744898c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("7b60274d-7321-40f3-8bcc-72ca9e6b08a5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("aa1f5c21-89b7-4887-bd9e-b5717d2a327f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("ce6aa1a5-b591-4db1-b4ff-0c43577b93a3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("f1afb37a-e0f3-47dd-97ba-f39f1a9ad0b9"));

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedBy", "CreatedOn", "DisplayNameAr", "DisplayNameEn", "Email", "Name", "NormalizedName", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("2821231e-6abc-4a3d-afb6-eb7495a64b42"), null, "admin", new DateTime(2025, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "مدخل بيانات", "DataEntry", null, "DataEntry", "DataEntry", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("2821231e-6abc-4a3d-afb6-eb7495a64b42"));

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedBy", "CreatedOn", "DisplayNameAr", "DisplayNameEn", "Email", "Name", "NormalizedName", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("393bf7cf-e959-4d86-a8ed-88a1fde51ae4"), null, "admin", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "فريق التراخيص", "License Team", "oibrahim@sure.com.sa", "LicenseTeam", "LicenseTeam", null, null },
                    { new Guid("4f12bea4-3d8a-4922-b9f6-2c2012692e55"), null, "admin", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "المدير العام", "General Manager", "oibrahim@sure.com.sa", "GeneralManager", "GeneralManager", null, null },
                    { new Guid("65f20349-837c-42e0-8ec9-63e328b7828b"), null, "admin", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "مدير الإدارة التصديق الرقمي", "Administration Manager Digital Authentication", "oibrahim@sure.com.sa", "AdministrationManagerDigitalAuthentication", "AdministrationManagerDigitalAuthentication", null, null },
                    { new Guid("6cab6d0b-9380-4037-9950-673ea744898c"), null, "admin", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "فريق التراخيص التصديق الرقمي", "License Team Digital Authentication", "oibrahim@sure.com.sa", "LicenseTeamDigitalAuthentication", "LicenseTeamDigitalAuthentication", null, null },
                    { new Guid("7b60274d-7321-40f3-8bcc-72ca9e6b08a5"), null, "admin", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "سعادة النائب", "Vice Governerator", "oibrahim@sure.com.sa", "ViceGovernerator", "ViceGovernerator", null, null },
                    { new Guid("aa1f5c21-89b7-4887-bd9e-b5717d2a327f"), null, "admin", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "معالى المحافظ", "Governerator", "oibrahim@sure.com.sa", "Governerator", "Governerator", null, null },
                    { new Guid("ce6aa1a5-b591-4db1-b4ff-0c43577b93a3"), null, "admin", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "مدير الإدارة المالية", "Finance Director", "oibrahim@sure.com.sa", "FinanceDirector", "FinanceDirector", null, null },
                    { new Guid("f1afb37a-e0f3-47dd-97ba-f39f1a9ad0b9"), null, "admin", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "مدير العام الإدارة المالية", "Finance General Manager", "oibrahim@sure.com.sa", "FinanceGeneralManager", "FinanceGeneralManager", null, null }
                });
        }
    }
}
