using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class AddRoleSeed2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedBy", "CreatedOn", "DisplayNameAr", "DisplayNameEn",  "Name", "NormalizedName", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("02453a3f-f461-44a7-a72c-6d2b8ab915ee"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "المدير المباشر لطلب التاشيرة", "Visa application Direct Manager", "VisaApplicationDirectManager", "VisaApplicationDirectManager", null, null },
                    { new Guid("09e37702-78bf-4d4f-845a-322ee3e560e2"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "نائب الخدمات المشتركة", "Deputy Joint Services", "DeputyJointServices", "DeputyJointServices", null, null },
                    { new Guid("71ec8bdd-2cf6-4c8a-a4b8-6c479b8fbda5"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "مدير الخدمات الادارية", "Administrative services manager", "AdministrativeServicesManager", "AdministrativeServicesManager", null, null },
                    { new Guid("a6b6d446-db1f-4132-b7ca-73cffd9b5f96"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "مدير ادارة الشؤون الإدارية والمرافق", "Director of Administrative Affairs and Facilities Department", "DirectorOfAdministrativeAffairsAndFacilitiesDepartment", "DirectorOfAdministrativeAffairsAndFacilitiesDepartment", null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("02453a3f-f461-44a7-a72c-6d2b8ab915ee"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("09e37702-78bf-4d4f-845a-322ee3e560e2"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("71ec8bdd-2cf6-4c8a-a4b8-6c479b8fbda5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a6b6d446-db1f-4132-b7ca-73cffd9b5f96"));
        }
    }
}
