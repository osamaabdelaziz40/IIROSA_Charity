using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class removeTtachmentContentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_AttachmentContent_Id",
                schema: "common",
                table: "Attachment");

            migrationBuilder.DropTable(
                name: "AttachmentContent",
                schema: "common");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                schema: "common",
                table: "Attachment",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                schema: "common",
                table: "Attachment");

            migrationBuilder.CreateTable(
                name: "AttachmentContent",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentContent", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_AttachmentContent_Id",
                schema: "common",
                table: "Attachment",
                column: "Id",
                principalSchema: "common",
                principalTable: "AttachmentContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}