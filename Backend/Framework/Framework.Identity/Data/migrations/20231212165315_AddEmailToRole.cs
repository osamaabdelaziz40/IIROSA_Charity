using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class AddEmailToRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "identity",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("ac40739c-ea5a-4220-a39e-4514591d6ccf"),
                column: "Email",
                value: "oibrahim@sure.com.sa");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                schema: "identity",
                table: "Roles");
        }
    }
}
