using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class updatetableNotificationsLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemSetting",
                schema: "common",
                table: "SystemSetting");

            migrationBuilder.RenameTable(
                name: "SystemSetting",
                schema: "common",
                newName: "NotificationsLog",
                newSchema: "common");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationsLog",
                schema: "common",
                table: "NotificationsLog",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationsLog",
                schema: "common",
                table: "NotificationsLog");

            migrationBuilder.RenameTable(
                name: "NotificationsLog",
                schema: "common",
                newName: "SystemSetting",
                newSchema: "common");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemSetting",
                schema: "common",
                table: "SystemSetting",
                column: "Id");
        }
    }
}
