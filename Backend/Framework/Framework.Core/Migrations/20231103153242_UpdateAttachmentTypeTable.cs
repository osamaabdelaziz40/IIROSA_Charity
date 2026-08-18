using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class UpdateAttachmentTypeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FileNumber",
                schema: "common",
                table: "AttachmentType",
                type: "int",
                nullable: true,
                defaultValueSql: "((1))");

            migrationBuilder.AddColumn<int>(
                name: "RequestType",
                schema: "common",
                table: "AttachmentType",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileNumber",
                schema: "common",
                table: "AttachmentType");

            migrationBuilder.DropColumn(
                name: "RequestType",
                schema: "common",
                table: "AttachmentType");
        }
    }
}
