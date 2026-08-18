using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CheckForPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "TicketResponses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SupportTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Sponsors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SeasonalAidDistribution",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SeasonalAidCampaign",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SeasonalAidBeneficiary",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "PeriodicOrphanReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "OfficeProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Missions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Families",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Charities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChequeBeneficiary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeneficiaryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FK_BankId = table.Column<int>(type: "int", nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ChequeBeneficiary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChequeBeneficiary_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HousingProject",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    CenterId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Village = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GPSCoordinates = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HousingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfUnits = table.Column<int>(type: "int", nullable: true),
                    AreaPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalArea = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BudgetCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletionPercentage = table.Column<int>(type: "int", nullable: false),
                    CurrentStage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgressNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HandoverDocumentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_HousingProject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HousingProject_Center_CenterId",
                        column: x => x.CenterId,
                        principalTable: "Center",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HousingProject_Charities_CharityId",
                        column: x => x.CharityId,
                        principalTable: "Charities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HousingProject_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HousingProject_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HousingProject_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Region",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Check",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CheckNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BeneficiaryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeneficiaryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FK_ChequeBeneficiaryId = table.Column<int>(type: "int", nullable: true),
                    BeneficiaryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeneficiaryPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeneficiaryEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeneficiaryIdNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountInWords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FK_BankId = table.Column<int>(type: "int", nullable: true),
                    BankBranch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClearanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BankReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClearanceNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VoidReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoidNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FK_CheckImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeBeneficiaryId = table.Column<int>(type: "int", nullable: true),
                    BankId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Check", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Check_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Check_ChequeBeneficiary_ChequeBeneficiaryId",
                        column: x => x.ChequeBeneficiaryId,
                        principalTable: "ChequeBeneficiary",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Check_BankId",
                table: "Check",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Check_ChequeBeneficiaryId",
                table: "Check",
                column: "ChequeBeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_ChequeBeneficiary_BankId",
                table: "ChequeBeneficiary",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_CenterId",
                table: "HousingProject",
                column: "CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_CharityId",
                table: "HousingProject",
                column: "CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_CountryId",
                table: "HousingProject",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_FamilyId",
                table: "HousingProject",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingProject_RegionId",
                table: "HousingProject",
                column: "RegionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Check");

            migrationBuilder.DropTable(
                name: "HousingProject");

            migrationBuilder.DropTable(
                name: "ChequeBeneficiary");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TicketResponses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SeasonalAidDistribution");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SeasonalAidCampaign");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SeasonalAidBeneficiary");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "PeriodicOrphanReports");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "OfficeProjects");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Charities");
        }
    }
}
