using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Epic 6 / UC-HOU-01: adds the Family.FamilyType register discriminator (backfilled Regular = 1).
    /// NOTE: this migration also carries the PENDING correspondence-module model sync that was sitting
    /// unmigrated in the working tree when it was scaffolded (Incoming/Outgoing FK_CharityId,
    /// FamilyCharityTransfer, IncomingEmployee, OutgoingOrphanReport, ChildOutGoing retirement).
    /// The parallel epic-16/17 session removed its own duplicate migrations after this snapshot landed —
    /// see the story Dev Agent Record before regenerating anything here.
    /// </remarks>
    public partial class Epic06_HousingFamilyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildOutGoing",
                schema: "IIROSA");

            migrationBuilder.AddColumn<Guid>(
                name: "FK_CharityId",
                schema: "IIROSA",
                table: "Outgoing",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FK_CharityId",
                schema: "IIROSA",
                table: "Incoming",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FamilyType",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "FamilyCharityTransfer",
                schema: "IIROSA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromCharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToCharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FamilyCharityTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyCharityTransfer_Charity_FromCharityId",
                        column: x => x.FromCharityId,
                        principalSchema: "IIROSA",
                        principalTable: "Charity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyCharityTransfer_Charity_ToCharityId",
                        column: x => x.ToCharityId,
                        principalSchema: "IIROSA",
                        principalTable: "Charity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyCharityTransfer_Family_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "IIROSA",
                        principalTable: "Family",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncomingEmployee",
                schema: "IIROSA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncomingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_IncomingEmployee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingEmployee_Incoming_IncomingId",
                        column: x => x.IncomingId,
                        principalSchema: "IIROSA",
                        principalTable: "Incoming",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncomingEmployee_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutgoingOrphanReport",
                schema: "IIROSA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutgoingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrphanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_OutgoingOrphanReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutgoingOrphanReport_Orphan_OrphanId",
                        column: x => x.OrphanId,
                        principalSchema: "IIROSA",
                        principalTable: "Orphan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OutgoingOrphanReport_Outgoing_OutgoingId",
                        column: x => x.OutgoingId,
                        principalSchema: "IIROSA",
                        principalTable: "Outgoing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Outgoing_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Outgoing",
                columns: new[] { "FK_CharityId", "Year", "Serial" });

            migrationBuilder.CreateIndex(
                name: "IX_Outgoing_Fk_DepartmentId",
                schema: "IIROSA",
                table: "Outgoing",
                column: "Fk_DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Incoming",
                columns: new[] { "FK_CharityId", "Year", "Serial" });

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_FK_DepartmentId",
                schema: "IIROSA",
                table: "Incoming",
                column: "FK_DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_FK_UserId",
                schema: "IIROSA",
                table: "Incoming",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyCharityTransfer_FamilyId",
                schema: "IIROSA",
                table: "FamilyCharityTransfer",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyCharityTransfer_FromCharityId",
                schema: "IIROSA",
                table: "FamilyCharityTransfer",
                column: "FromCharityId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyCharityTransfer_ToCharityId",
                schema: "IIROSA",
                table: "FamilyCharityTransfer",
                column: "ToCharityId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingEmployee_IncomingId_UserId",
                schema: "IIROSA",
                table: "IncomingEmployee",
                columns: new[] { "IncomingId", "UserId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingEmployee_UserId",
                schema: "IIROSA",
                table: "IncomingEmployee",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OutgoingOrphanReport_OrphanId",
                schema: "IIROSA",
                table: "OutgoingOrphanReport",
                column: "OrphanId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_OutgoingOrphanReport_OutgoingId",
                schema: "IIROSA",
                table: "OutgoingOrphanReport",
                column: "OutgoingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incoming_Charity_FK_CharityId",
                schema: "IIROSA",
                table: "Incoming",
                column: "FK_CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Incoming_Users_FK_UserId",
                schema: "IIROSA",
                table: "Incoming",
                column: "FK_UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Outgoing_Charity_FK_CharityId",
                schema: "IIROSA",
                table: "Outgoing",
                column: "FK_CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incoming_Charity_FK_CharityId",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.DropForeignKey(
                name: "FK_Incoming_Users_FK_UserId",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.DropForeignKey(
                name: "FK_Outgoing_Charity_FK_CharityId",
                schema: "IIROSA",
                table: "Outgoing");

            migrationBuilder.DropTable(
                name: "FamilyCharityTransfer",
                schema: "IIROSA");

            migrationBuilder.DropTable(
                name: "IncomingEmployee",
                schema: "IIROSA");

            migrationBuilder.DropTable(
                name: "OutgoingOrphanReport",
                schema: "IIROSA");

            migrationBuilder.DropIndex(
                name: "IX_Outgoing_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Outgoing");

            migrationBuilder.DropIndex(
                name: "IX_Outgoing_Fk_DepartmentId",
                schema: "IIROSA",
                table: "Outgoing");

            migrationBuilder.DropIndex(
                name: "IX_Incoming_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.DropIndex(
                name: "IX_Incoming_FK_DepartmentId",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.DropIndex(
                name: "IX_Incoming_FK_UserId",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.DropColumn(
                name: "FK_CharityId",
                schema: "IIROSA",
                table: "Outgoing");

            migrationBuilder.DropColumn(
                name: "FK_CharityId",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.DropColumn(
                name: "FamilyType",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.CreateTable(
                name: "ChildOutGoing",
                schema: "IIROSA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutgoingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Fk_DepartmentId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OutgoingId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedFileId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildOutGoing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildOutGoing_Outgoing_OutgoingId",
                        column: x => x.OutgoingId,
                        principalSchema: "IIROSA",
                        principalTable: "Outgoing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChildOutGoing_Outgoing_OutgoingId1",
                        column: x => x.OutgoingId1,
                        principalSchema: "IIROSA",
                        principalTable: "Outgoing",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChildOutGoing_UploadedFile_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChildOutGoing_UploadedFile_UploadedFileId1",
                        column: x => x.UploadedFileId1,
                        principalTable: "UploadedFile",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChildOutGoing_Date",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ChildOutGoing_OutgoingId",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "OutgoingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildOutGoing_OutgoingId1",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "OutgoingId1");

            migrationBuilder.CreateIndex(
                name: "IX_ChildOutGoing_UploadedFileId",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "UploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildOutGoing_UploadedFileId1",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "UploadedFileId1");

            migrationBuilder.CreateIndex(
                name: "IX_ChildOutGoing_Year",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "Year");
        }
    }
}
