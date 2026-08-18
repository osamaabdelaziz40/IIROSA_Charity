using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class AddAttachContentSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttachmentContents_Attachment_AttachmentId",
                table: "AttachmentContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttachmentContents",
                table: "AttachmentContents");

            migrationBuilder.DropIndex(
                name: "IX_AttachmentContents_AttachmentId",
                table: "AttachmentContents");

            migrationBuilder.DropColumn(
                name: "AttachmentId",
                table: "AttachmentContents");

            migrationBuilder.RenameTable(
                name: "AttachmentContents",
                newName: "AttachmentContent",
                newSchema: "common");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttachmentContent",
                schema: "common",
                table: "AttachmentContent",
                column: "Id");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_AttachmentContent_Id",
                schema: "common",
                table: "Attachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttachmentContent",
                schema: "common",
                table: "AttachmentContent");

            migrationBuilder.RenameTable(
                name: "AttachmentContent",
                schema: "common",
                newName: "AttachmentContents");

            migrationBuilder.AddColumn<Guid>(
                name: "AttachmentId",
                table: "AttachmentContents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttachmentContents",
                table: "AttachmentContents",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentContents_AttachmentId",
                table: "AttachmentContents",
                column: "AttachmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttachmentContents_Attachment_AttachmentId",
                table: "AttachmentContents",
                column: "AttachmentId",
                principalSchema: "common",
                principalTable: "Attachment",
                principalColumn: "Id");
        }
    }
}
