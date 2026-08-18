using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentIdToAttachmentContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add AttachmentId column to AttachmentContent table
            migrationBuilder.AddColumn<Guid>(
                name: "AttachmentId",
                schema: "common",
                table: "AttachmentContent",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Create unique index on AttachmentId
            migrationBuilder.CreateIndex(
                name: "IX_AttachmentContent_AttachmentId",
                schema: "common",
                table: "AttachmentContent",
                column: "AttachmentId",
                unique: true);

            // Add foreign key constraint (NoAction to avoid multiple cascade paths)
            migrationBuilder.AddForeignKey(
                name: "FK_AttachmentContent_Attachment_AttachmentId",
                schema: "common",
                table: "AttachmentContent",
                column: "AttachmentId",
                principalSchema: "common",
                principalTable: "Attachment",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttachmentContent_Attachment_AttachmentId",
                schema: "common",
                table: "AttachmentContent");

            migrationBuilder.DropIndex(
                name: "IX_AttachmentContent_AttachmentId",
                schema: "common",
                table: "AttachmentContent");

            migrationBuilder.DropColumn(
                name: "AttachmentId",
                schema: "common",
                table: "AttachmentContent");
        }
    }
}
