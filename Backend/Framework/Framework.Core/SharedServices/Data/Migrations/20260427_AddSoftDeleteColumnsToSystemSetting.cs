using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.SharedServices.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteColumnsToSystemSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "common",
                table: "SystemSetting",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "common",
                table: "SystemSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "common",
                table: "SystemSetting");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "common",
                table: "SystemSetting");
        }
    }
}
