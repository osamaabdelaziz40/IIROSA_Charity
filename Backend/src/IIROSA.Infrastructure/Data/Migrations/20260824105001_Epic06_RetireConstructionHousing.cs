using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Epic 6 / UC-HOU-03 scope: retires the invented construction tracker — DROPS the
    /// IIROSA.HousingProject table (dev seed/test data only; the epic's open question asked the
    /// user to confirm before the migration is APPLIED — file this cut, apply on restart) — and
    /// adds the §11.S.2 register columns: Provider (RelationId, MainRelation, SocialStatusId,
    /// HealthStatusId, EducationLevelId, WidowSponsorship, AnotherSponsor, MotherIsMar, IsCaring)
    /// and Orphan (Profession, DepartmentName, FacultyName, BirthCertificateAttachmentId,
    /// EnrollmentAttachmentId).
    ///
    /// PARALLEL-SESSION SWEEP (shared model snapshot, same situation as the two earlier epic-6
    /// migrations): also carries epic-5's GuardianChangeRequest table, epic-17's HqTransferDetail
    /// table and Country.MaxTransferAmount. Those sessions' entities exist and compile — trimming
    /// would strand their Designer snapshot claims. Do not apply this migration until the HousingProject
    /// drop is confirmed with the user; NOT yet applied to the database (live API holds it).
    /// </remarks>
    public partial class Epic06_RetireConstructionHousing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HousingProject",
                schema: "IIROSA");

            migrationBuilder.AddColumn<bool>(
                name: "AnotherSponsor",
                schema: "IIROSA",
                table: "Provider",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EducationLevelId",
                schema: "IIROSA",
                table: "Provider",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HealthStatusId",
                schema: "IIROSA",
                table: "Provider",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCaring",
                schema: "IIROSA",
                table: "Provider",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainRelation",
                schema: "IIROSA",
                table: "Provider",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MotherIsMar",
                schema: "IIROSA",
                table: "Provider",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelationId",
                schema: "IIROSA",
                table: "Provider",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SocialStatusId",
                schema: "IIROSA",
                table: "Provider",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WidowSponsorship",
                schema: "IIROSA",
                table: "Provider",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BirthCertificateAttachmentId",
                schema: "IIROSA",
                table: "Orphan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                schema: "IIROSA",
                table: "Orphan",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EnrollmentAttachmentId",
                schema: "IIROSA",
                table: "Orphan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacultyName",
                schema: "IIROSA",
                table: "Orphan",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profession",
                schema: "IIROSA",
                table: "Orphan",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxTransferAmount",
                schema: "Lookup",
                table: "Country",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GuardianChangeRequest",
                schema: "IIROSA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrphanCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrphanName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MotherName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OldGuardianName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OldGuardianNationalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NewGuardianName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NewGuardianNationalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestedByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DecidedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DecidedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_GuardianChangeRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuardianChangeRequest_Charity_CharityId",
                        column: x => x.CharityId,
                        principalSchema: "IIROSA",
                        principalTable: "Charity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuardianChangeRequest_Family_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "IIROSA",
                        principalTable: "Family",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HqTransferDetail",
                schema: "IIROSA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_HqTransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedTransferDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsExecuted = table.Column<bool>(type: "bit", nullable: true),
                    ExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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
                    table.PrimaryKey("PK_HqTransferDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HqTransferDetail_HqTransfer_FK_HqTransferId",
                        column: x => x.FK_HqTransferId,
                        principalSchema: "IIROSA",
                        principalTable: "HqTransfer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Provider_EducationLevelId",
                schema: "IIROSA",
                table: "Provider",
                column: "EducationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_HealthStatusId",
                schema: "IIROSA",
                table: "Provider",
                column: "HealthStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_RelationId",
                schema: "IIROSA",
                table: "Provider",
                column: "RelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_SocialStatusId",
                schema: "IIROSA",
                table: "Provider",
                column: "SocialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_GuardianChangeRequest_CharityId",
                schema: "IIROSA",
                table: "GuardianChangeRequest",
                column: "CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_GuardianChangeRequest_FamilyId",
                schema: "IIROSA",
                table: "GuardianChangeRequest",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_GuardianChangeRequest_Status_CharityId",
                schema: "IIROSA",
                table: "GuardianChangeRequest",
                columns: new[] { "Status", "CharityId" });

            migrationBuilder.CreateIndex(
                name: "IX_HqTransferDetail_FK_HqTransferId",
                schema: "IIROSA",
                table: "HqTransferDetail",
                column: "FK_HqTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_HqTransferDetail_TransferNumber",
                schema: "IIROSA",
                table: "HqTransferDetail",
                column: "TransferNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_EducationLevel_EducationLevelId",
                schema: "IIROSA",
                table: "Provider",
                column: "EducationLevelId",
                principalSchema: "Lookup",
                principalTable: "EducationLevel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_HealthStatus_HealthStatusId",
                schema: "IIROSA",
                table: "Provider",
                column: "HealthStatusId",
                principalSchema: "Lookup",
                principalTable: "HealthStatus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_Relation_RelationId",
                schema: "IIROSA",
                table: "Provider",
                column: "RelationId",
                principalSchema: "Lookup",
                principalTable: "Relation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_SocialStatus_SocialStatusId",
                schema: "IIROSA",
                table: "Provider",
                column: "SocialStatusId",
                principalSchema: "Lookup",
                principalTable: "SocialStatus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Provider_EducationLevel_EducationLevelId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropForeignKey(
                name: "FK_Provider_HealthStatus_HealthStatusId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropForeignKey(
                name: "FK_Provider_Relation_RelationId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropForeignKey(
                name: "FK_Provider_SocialStatus_SocialStatusId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropTable(
                name: "GuardianChangeRequest",
                schema: "IIROSA");

            migrationBuilder.DropTable(
                name: "HqTransferDetail",
                schema: "IIROSA");

            migrationBuilder.DropIndex(
                name: "IX_Provider_EducationLevelId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropIndex(
                name: "IX_Provider_HealthStatusId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropIndex(
                name: "IX_Provider_RelationId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropIndex(
                name: "IX_Provider_SocialStatusId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "AnotherSponsor",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "EducationLevelId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "HealthStatusId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "IsCaring",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "MainRelation",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "MotherIsMar",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "RelationId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "SocialStatusId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "WidowSponsorship",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "BirthCertificateAttachmentId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropColumn(
                name: "EnrollmentAttachmentId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropColumn(
                name: "FacultyName",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropColumn(
                name: "Profession",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropColumn(
                name: "MaxTransferAmount",
                schema: "Lookup",
                table: "Country");

            migrationBuilder.CreateTable(
                name: "HousingProject",
                schema: "IIROSA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CenterId = table.Column<int>(type: "int", nullable: true),
                    CharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AreaPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BudgetCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "EGP"),
                    CompletionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CompletionPercentage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentStage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DonorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExpectedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GPSCoordinates = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HandoverDocumentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HousingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumberOfUnits = table.Column<int>(type: "int", nullable: true),
                    ProgressNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProjectStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Planning"),
                    ProjectType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalArea = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Village = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousingProject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HousingProject_Center_CenterId",
                        column: x => x.CenterId,
                        principalSchema: "Lookup",
                        principalTable: "Center",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HousingProject_Charity_CharityId",
                        column: x => x.CharityId,
                        principalSchema: "IIROSA",
                        principalTable: "Charity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HousingProject_Country_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "Lookup",
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HousingProject_Family_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "IIROSA",
                        principalTable: "Family",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HousingProject_Region_RegionId",
                        column: x => x.RegionId,
                        principalSchema: "Lookup",
                        principalTable: "Region",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_CenterId",
                schema: "IIROSA",
                table: "HousingProject",
                column: "CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_CharityId",
                schema: "IIROSA",
                table: "HousingProject",
                column: "CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_CountryId",
                schema: "IIROSA",
                table: "HousingProject",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_FamilyId",
                schema: "IIROSA",
                table: "HousingProject",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_Name",
                schema: "IIROSA",
                table: "HousingProject",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_ProjectStatus",
                schema: "IIROSA",
                table: "HousingProject",
                column: "ProjectStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_RegionId",
                schema: "IIROSA",
                table: "HousingProject",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_StartDate",
                schema: "IIROSA",
                table: "HousingProject",
                column: "StartDate");
        }
    }
}
