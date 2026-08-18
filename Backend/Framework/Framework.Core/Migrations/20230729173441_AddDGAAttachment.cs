using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class AddDGAAttachment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "common",
                table: "SystemSetting",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "GroupName", "IsActive", "IsSecure", "IsSticky", "Name", "UpdatedBy", "UpdatedOn", "Value", "ValueType" },
                values: new object[] { 9, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Integration", true, false, false, "AttachmentsPath", null, null, "Attachments", "string" });

            migrationBuilder.InsertData(
                schema: "common",
                table: "SystemSetting",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "GroupName", "IsActive", "IsSecure", "IsSticky", "Name", "UpdatedBy", "UpdatedOn", "Value", "ValueType" },
                values: new object[] { 10, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Integration", true, false, false, "AttachmentsServer", null, null, "C:\\inetpub\\wwwroot\\DGA-Internal", "string" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
