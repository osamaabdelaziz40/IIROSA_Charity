using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class removeUnusedColumns2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Departement",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                schema: "identity",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Departement",
                schema: "identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                schema: "identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}