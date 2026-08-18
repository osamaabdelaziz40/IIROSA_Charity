using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class addcolumnuserTypeusers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserTypeId",
                schema: "identity",
                table: "Users",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserTypeId",
                schema: "identity",
                table: "Users");
        }
    }
}
