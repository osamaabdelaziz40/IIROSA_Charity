using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "http://localhost:8080/");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "http://192.168.1.136:8080/");
        }
    }
}
