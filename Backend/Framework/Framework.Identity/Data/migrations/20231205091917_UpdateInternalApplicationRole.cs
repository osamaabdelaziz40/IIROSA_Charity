using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class UpdateInternalApplicationRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                keyValue: new Guid("1e985200-c827-4938-9c1f-38356f102701"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("5cc42d23-0b0d-4e38-8547-e4c45c0cf660"));

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

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("cb857db8-790e-4cf1-b0b4-cacda476e901"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("e7b00baf-5f44-40eb-939f-d5b090170899"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("ecfbe941-867e-4641-806c-b3337f7f2554"));

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedBy", "CreatedOn", "DisplayNameAr", "DisplayNameEn", "Name", "NormalizedName", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("d29995f4-abeb-468c-889b-6d6d392cb852"), null, "", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "مدير النظام", "admin", "admin", "admin", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d29995f4-abeb-468c-889b-6d6d392cb852"));

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedBy", "CreatedOn", "DisplayNameAr", "DisplayNameEn", "Name", "NormalizedName", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("02453a3f-f461-44a7-a72c-6d2b8ab915ee"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "المدير المباشر لطلب التاشيرة", "Visa application Direct Manager", "VisaApplicationDirectManager", "VisaApplicationDirectManager", null, null },
                    { new Guid("09e37702-78bf-4d4f-845a-322ee3e560e2"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "نائب الخدمات المشتركة", "Deputy Joint Services", "DeputyJointServices", "DeputyJointServices", null, null },
                    { new Guid("1e985200-c827-4938-9c1f-38356f102701"), null, "", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "مدير النظام", "admin", "admin", "admin", null, null },
                    { new Guid("5cc42d23-0b0d-4e38-8547-e4c45c0cf660"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "المدير المباشر لاستضافة", "Hosting Direct Manager", "HostingDirectManager", "HostingDirectManager", null, null },
                    { new Guid("71ec8bdd-2cf6-4c8a-a4b8-6c479b8fbda5"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "مدير الخدمات الادارية", "Administrative services manager", "AdministrativeServicesManager", "AdministrativeServicesManager", null, null },
                    { new Guid("a6b6d446-db1f-4132-b7ca-73cffd9b5f96"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "مدير ادارة الشؤون الإدارية والمرافق", "Director of Administrative Affairs and Facilities Department", "DirectorOfAdministrativeAffairsAndFacilitiesDepartment", "DirectorOfAdministrativeAffairsAndFacilitiesDepartment", null, null },
                    { new Guid("cb857db8-790e-4cf1-b0b4-cacda476e901"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ادارة الأمن", "Security Department", "SecurityDepartment", "SecurityDepartment", null, null },
                    { new Guid("e7b00baf-5f44-40eb-939f-d5b090170899"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ادارة الشؤون الإدارية والمرافق", "Administration And Facilities Department", "AdministrationAndFacilitiesDepartment", "AdministrationAndFacilitiesDepartment", null, null },
                    { new Guid("ecfbe941-867e-4641-806c-b3337f7f2554"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "مشرف الأمن", "Security supervisor", "SecuritySupervisor", "SecuritySupervisor", null, null }
                });
        }
    }
}
