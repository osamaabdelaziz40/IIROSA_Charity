using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class AddAttach : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttachmentType",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowedFilesExtension = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImageMaxHeight = table.Column<int>(type: "int", nullable: true),
                    ImageMaxWidth = table.Column<int>(type: "int", nullable: true),
                    IsImage = table.Column<bool>(type: "bit", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    MaxSizeInMegabytes = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((1))"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Thread = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    LogLevel = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Logger = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    UserName = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Message = table.Column<string>(type: "varchar(4000)", unicode: false, maxLength: 4000, nullable: true),
                    Exception = table.Column<string>(type: "varchar(6000)", unicode: false, maxLength: 6000, nullable: true),
                    CallSite = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attachment",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TitleAr = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TitleEn = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AttachmentTypeId = table.Column<int>(type: "int", nullable: false),
                    Thumbnail = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    IsTransferred = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
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

            migrationBuilder.CreateTable(
                name: "AttachmentContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttachmentContents_Attachment_AttachmentId",
                        column: x => x.AttachmentId,
                        principalSchema: "common",
                        principalTable: "Attachment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_AttachmentTypeId",
                schema: "common",
                table: "Attachment",
                column: "AttachmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentContents_AttachmentId",
                table: "AttachmentContents",
                column: "AttachmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttachmentContents");

            migrationBuilder.DropTable(
                name: "Log",
                schema: "common");

            migrationBuilder.DropTable(
                name: "Attachment",
                schema: "common");

            migrationBuilder.DropTable(
                name: "AttachmentType",
                schema: "common");
        }
    }
}
