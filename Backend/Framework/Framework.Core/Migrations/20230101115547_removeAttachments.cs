using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class removeAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachment",
                schema: "common");

            migrationBuilder.DropTable(
                name: "AttachmentType",
                schema: "common");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttachmentType",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllowedFilesExtension = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageMaxHeight = table.Column<int>(type: "int", nullable: true),
                    ImageMaxWidth = table.Column<int>(type: "int", nullable: true),
                    IsImage = table.Column<bool>(type: "bit", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    MaxSizeInMegabytes = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((1))"),
                    NameAr = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attachment",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentTypeId = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Thumbnail = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    TitleAr = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TitleEn = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachment_AttachmentType_AttachmentTypeId",
                        column: x => x.AttachmentTypeId,
                        principalSchema: "common",
                        principalTable: "AttachmentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_AttachmentTypeId",
                schema: "common",
                table: "Attachment",
                column: "AttachmentTypeId");
        }
    }
}