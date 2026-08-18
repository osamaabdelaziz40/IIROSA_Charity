using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageArToNotificationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Message",
                schema: "common",
                table: "NotificationsLog",
                newName: "MessageEn");

            migrationBuilder.AddColumn<string>(
                name: "MessageAr",
                schema: "common",
                table: "NotificationsLog",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageAr",
                schema: "common",
                table: "NotificationsLog");

            migrationBuilder.RenameColumn(
                name: "MessageEn",
                schema: "common",
                table: "NotificationsLog",
                newName: "Message");
        }
    }
}
