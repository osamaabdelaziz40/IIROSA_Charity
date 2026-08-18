using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class AddColumnForPreferredNotificationLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredNotificationLanguage",
                schema: "identity",
                table: "Users",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "ar");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredNotificationLanguage",
                schema: "identity",
                table: "Users");
        }
    }
}
