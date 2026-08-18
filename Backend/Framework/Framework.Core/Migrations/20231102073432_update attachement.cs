using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class updateattachement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                schema: "common",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "IsTransferred",
                schema: "common",
                table: "Attachment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                schema: "common",
                table: "Attachment",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTransferred",
                schema: "common",
                table: "Attachment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
