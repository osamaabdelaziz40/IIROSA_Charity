using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCharityIdToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CharityId",
                schema: "common",
                table: "AuditLog",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CharityId_Timestamp",
                schema: "common",
                table: "AuditLog",
                columns: new[] { "CharityId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLog_CharityId_Timestamp",
                schema: "common",
                table: "AuditLog");

            migrationBuilder.DropColumn(
                name: "CharityId",
                schema: "common",
                table: "AuditLog");
        }
    }
}
