using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class AddSmtpSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "common",
                table: "SystemSetting",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "GroupName", "IsActive", "IsSecure", "IsSticky", "Name", "UpdatedBy", "UpdatedOn", "Value", "ValueType" },
                values: new object[,]
                {
                    { 2, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Notifications", true, false, false, "SmtpServer", null, null, "192.168.1.166", "string" },
                    { 3, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Notifications", true, false, false, "SmtpUserName", null, null, "", "string" },
                    { 4, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Notifications", true, false, false, "SmtpPassword", null, null, "", "string" },
                    { 5, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Notifications", true, false, false, "SmtpEnableSSL", null, null, "False", "bool" },
                    { 6, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Notifications", true, false, false, "SmtpPort", null, null, "25", "int" },
                    { 7, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Notifications", true, false, false, "EmailFromName", null, null, "DGA", "string" },
                    { 8, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Notifications", true, false, false, "EmailFromAddress", null, null, "info@suredemos.com", "string" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
