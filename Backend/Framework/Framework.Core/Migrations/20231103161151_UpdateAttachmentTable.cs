using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class UpdateAttachmentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_AttachmentType_AttachmentTypeId",
                schema: "common",
                table: "Attachment");

            migrationBuilder.AlterColumn<int>(
                name: "AttachmentTypeId",
                schema: "common",
                table: "Attachment",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_AttachmentType_AttachmentTypeId",
                schema: "common",
                table: "Attachment",
                column: "AttachmentTypeId",
                principalSchema: "common",
                principalTable: "AttachmentType",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_AttachmentType_AttachmentTypeId",
                schema: "common",
                table: "Attachment");

            migrationBuilder.AlterColumn<int>(
                name: "AttachmentTypeId",
                schema: "common",
                table: "Attachment",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_AttachmentType_AttachmentTypeId",
                schema: "common",
                table: "Attachment",
                column: "AttachmentTypeId",
                principalSchema: "common",
                principalTable: "AttachmentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
