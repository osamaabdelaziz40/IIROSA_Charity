using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCharityIconSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfficeIcon",
                table: "Charities");

            migrationBuilder.AddColumn<Guid>(
                name: "IconId",
                table: "Charities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OutgoingCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutgoingCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UploadedFile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChildOutGoing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutgoingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Fk_DepartmentId = table.Column<int>(type: "int", nullable: true),
                    UploadedFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildOutGoing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildOutGoing_UploadedFile_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFile",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Incoming",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Serial = table.Column<int>(type: "int", nullable: true),
                    Serial_Txt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IncomingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncomingId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LetterNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LetterDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LetterDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FK_DepartmentId = table.Column<int>(type: "int", nullable: true),
                    FK_UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutgoingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UploadedFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incoming", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incoming_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Incoming_UploadedFile_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFile",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Outgoing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Serial = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OutGoingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutGoingId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Fk_DepartmentId = table.Column<int>(type: "int", nullable: true),
                    UploadedFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutgoingCategoryId = table.Column<int>(type: "int", nullable: true),
                    IncomingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outgoing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Outgoing_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Outgoing_Incoming_IncomingId",
                        column: x => x.IncomingId,
                        principalTable: "Incoming",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Outgoing_OutgoingCategory_OutgoingCategoryId",
                        column: x => x.OutgoingCategoryId,
                        principalTable: "OutgoingCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Outgoing_UploadedFile_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFile",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChildOutGoing_OutgoingId",
                table: "ChildOutGoing",
                column: "OutgoingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildOutGoing_UploadedFileId",
                table: "ChildOutGoing",
                column: "UploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_DepartmentId",
                table: "Incoming",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_OutgoingId",
                table: "Incoming",
                column: "OutgoingId");

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_UploadedFileId",
                table: "Incoming",
                column: "UploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Outgoing_DepartmentId",
                table: "Outgoing",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Outgoing_IncomingId",
                table: "Outgoing",
                column: "IncomingId");

            migrationBuilder.CreateIndex(
                name: "IX_Outgoing_OutgoingCategoryId",
                table: "Outgoing",
                column: "OutgoingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Outgoing_UploadedFileId",
                table: "Outgoing",
                column: "UploadedFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildOutGoing_Outgoing_OutgoingId",
                table: "ChildOutGoing",
                column: "OutgoingId",
                principalTable: "Outgoing",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incoming_Outgoing_OutgoingId",
                table: "Incoming",
                column: "OutgoingId",
                principalTable: "Outgoing",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incoming_Outgoing_OutgoingId",
                table: "Incoming");

            migrationBuilder.DropTable(
                name: "ChildOutGoing");

            migrationBuilder.DropTable(
                name: "Outgoing");

            migrationBuilder.DropTable(
                name: "Incoming");

            migrationBuilder.DropTable(
                name: "OutgoingCategory");

            migrationBuilder.DropTable(
                name: "UploadedFile");

            migrationBuilder.DropColumn(
                name: "IconId",
                table: "Charities");

            migrationBuilder.AddColumn<string>(
                name: "OfficeIcon",
                table: "Charities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
