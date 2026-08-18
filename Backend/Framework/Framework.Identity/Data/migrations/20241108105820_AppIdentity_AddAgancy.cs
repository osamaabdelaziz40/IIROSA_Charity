using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class AppIdentity_AddAgancy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
       

            migrationBuilder.AddColumn<int>(
                name: "AgancyId",
                schema: "identity",
                table: "Users",
                type: "int",
                nullable: true);

               }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
     

            migrationBuilder.DropColumn(
                name: "AgancyId",
                schema: "identity",
                table: "Users");

        }
    }
}
