using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class updatetableNotificationsLog1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "NotificationsLogs");

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "NotificationsLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationsLogs", x => x.Id);
                });
        }
    }
}
