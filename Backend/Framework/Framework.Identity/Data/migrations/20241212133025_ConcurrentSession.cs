using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class ConcurrentSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentToken",
                schema: "identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentToken",
                schema: "identity",
                table: "Users");
        }
    }
}
