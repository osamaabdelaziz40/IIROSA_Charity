using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.SharedServices.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedByColumnToSystemSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "common",
                table: "SystemSetting",
                type: "nvarchar(100)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "common",
                table: "SystemSetting");
        }
    }
}
