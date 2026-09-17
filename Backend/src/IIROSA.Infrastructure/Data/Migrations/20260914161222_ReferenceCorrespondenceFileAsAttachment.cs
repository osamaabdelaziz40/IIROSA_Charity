using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReferenceCorrespondenceFileAsAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incoming_UploadedFile_UploadedFileId",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.DropForeignKey(
                name: "FK_Outgoing_UploadedFile_UploadedFileId",
                schema: "IIROSA",
                table: "Outgoing");

            migrationBuilder.DropTable(
                name: "UploadedFile");

            migrationBuilder.DropIndex(
                name: "IX_Outgoing_UploadedFileId",
                schema: "IIROSA",
                table: "Outgoing");

            migrationBuilder.DropIndex(
                name: "IX_Incoming_UploadedFileId",
                schema: "IIROSA",
                table: "Incoming");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedFile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFile", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Outgoing_UploadedFileId",
                schema: "IIROSA",
                table: "Outgoing",
                column: "UploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_UploadedFileId",
                schema: "IIROSA",
                table: "Incoming",
                column: "UploadedFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incoming_UploadedFile_UploadedFileId",
                schema: "IIROSA",
                table: "Incoming",
                column: "UploadedFileId",
                principalTable: "UploadedFile",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Outgoing_UploadedFile_UploadedFileId",
                schema: "IIROSA",
                table: "Outgoing",
                column: "UploadedFileId",
                principalTable: "UploadedFile",
                principalColumn: "Id");
        }
    }
}
