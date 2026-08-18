using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Framework.Core.Migrations
{
    public partial class InitCommons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "common");

            migrationBuilder.CreateTable(
                name: "AttachmentContent",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileContent = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentType",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(maxLength: 256, nullable: false),
                    NameEn = table.Column<string>(maxLength: 256, nullable: false),
                    Code = table.Column<string>(nullable: false),
                    AllowedFilesExtension = table.Column<string>(maxLength: 200, nullable: true),
                    ImageMaxHeight = table.Column<int>(nullable: true),
                    ImageMaxWidth = table.Column<int>(nullable: true),
                    IsImage = table.Column<bool>(nullable: false),
                    IsMandatory = table.Column<bool>(nullable: false),
                    MaxSizeInMegabytes = table.Column<int>(nullable: false, defaultValueSql: "((1))"),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
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
                    Id = table.Column<Guid>(nullable: false),
                    Host = table.Column<string>(maxLength: 256, nullable: true),
                    MachineName = table.Column<string>(nullable: true),
                    Url = table.Column<string>(unicode: false, maxLength: 500, nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Thread = table.Column<string>(unicode: false, maxLength: 256, nullable: true),
                    LogLevel = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Logger = table.Column<string>(unicode: false, maxLength: 256, nullable: true),
                    UserAgent = table.Column<string>(unicode: false, maxLength: 256, nullable: true),
                    UserName = table.Column<string>(unicode: false, maxLength: 256, nullable: true),
                    Message = table.Column<string>(unicode: false, maxLength: 4000, nullable: true),
                    Exception = table.Column<string>(unicode: false, maxLength: 6000, nullable: true),
                    CallSite = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationType",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(maxLength: 100, nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSetting",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    ValueType = table.Column<string>(unicode: false, maxLength: 30, nullable: false),
                    Value = table.Column<string>(maxLength: 255, nullable: false),
                    GroupName = table.Column<string>(maxLength: 50, nullable: false),
                    IsSecure = table.Column<bool>(nullable: false),
                    IsSticky = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attachment",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(maxLength: 255, nullable: false),
                    Extension = table.Column<string>(maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(maxLength: 100, nullable: true),
                    TitleAr = table.Column<string>(maxLength: 255, nullable: true),
                    TitleEn = table.Column<string>(maxLength: 255, nullable: true),
                    DescriptionAr = table.Column<string>(maxLength: 500, nullable: true),
                    DescriptionEn = table.Column<string>(maxLength: 500, nullable: true),
                    AttachmentTypeId = table.Column<int>(nullable: false),
                    Thumbnail = table.Column<byte[]>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Attachment_AttachmentContent_Id",
                        column: x => x.Id,
                        principalSchema: "common",
                        principalTable: "AttachmentContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplate",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    SubjectAr = table.Column<string>(maxLength: 256, nullable: false),
                    SubjectEn = table.Column<string>(maxLength: 256, nullable: true),
                    BodyAr = table.Column<string>(maxLength: 4000, nullable: false),
                    BodyEn = table.Column<string>(maxLength: 4000, nullable: true),
                    NotificationTypeId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplate_NotificationType_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalSchema: "common",
                        principalTable: "NotificationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_AttachmentTypeId",
                schema: "common",
                table: "Attachment",
                column: "AttachmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_NotificationTypeId",
                schema: "common",
                table: "NotificationTemplate",
                column: "NotificationTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachment",
                schema: "common");

            migrationBuilder.DropTable(
                name: "Log",
                schema: "common");

            migrationBuilder.DropTable(
                name: "NotificationTemplate",
                schema: "common");

            migrationBuilder.DropTable(
                name: "SystemSetting",
                schema: "common");

            migrationBuilder.DropTable(
                name: "AttachmentType",
                schema: "common");

            migrationBuilder.DropTable(
                name: "AttachmentContent",
                schema: "common");

            migrationBuilder.DropTable(
                name: "NotificationType",
                schema: "common");
        }
    }
}