using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class AddApplicationRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedBy", "CreatedOn", "DisplayNameAr", "DisplayNameEn", "Name", "NormalizedName", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("ac40739c-ea5a-4220-a39e-4514591d6ccf"), null, "admin", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "فريق التراخيص", "License Team", "LicenseTeam", "LicenseTeam", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("ac40739c-ea5a-4220-a39e-4514591d6ccf"));
        }
    }
}
