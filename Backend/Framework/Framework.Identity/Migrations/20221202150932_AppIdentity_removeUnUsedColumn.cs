using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Framework.Identity.Migrations
{
    public partial class AppIdentity_removeUnUsedColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TempEmail",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TempPhoneNumber",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PortId",
                schema: "identity",
                table: "UserRoles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TempEmail",
                schema: "identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TempPhoneNumber",
                schema: "identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PortId",
                schema: "identity",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}