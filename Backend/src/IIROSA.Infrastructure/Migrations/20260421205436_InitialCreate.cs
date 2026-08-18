using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsoCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PhoneCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttachmentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconData = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    CountryId1 = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryId1",
                        column: x => x.CountryId1,
                        principalTable: "Countries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Charities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EstablishmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Charities_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sponsors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    SponsorshipType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sponsors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sponsors_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sponsors_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HeadOfFamily = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    CharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FamilyStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FinancialStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HousingStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FamilyMembersCount = table.Column<int>(type: "int", nullable: false),
                    OrphansCount = table.Column<int>(type: "int", nullable: false),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyAssistance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Families_Charities_CharityId",
                        column: x => x.CharityId,
                        principalTable: "Charities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Families_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Families_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orphans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EducationLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HealthStatus = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SponsorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SponsorshipStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orphans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orphans_Charities_CharityId",
                        column: x => x.CharityId,
                        principalTable: "Charities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orphans_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orphans_Sponsors_SponsorId",
                        column: x => x.SponsorId,
                        principalTable: "Sponsors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicOrphanReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrphanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportYear = table.Column<int>(type: "int", nullable: false),
                    ReportMonth = table.Column<int>(type: "int", nullable: false),
                    CharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubmittedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PrayerStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QuranMemorization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QuranParts = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MannersStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EducationLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EducationStage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AcademicPerformance = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SchoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HealthStatus = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChronicDiseases = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MedicalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Skills = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Hobbies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PersonalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MajorEvents = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Achievements = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Challenges = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicOrphanReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodicOrphanReports_Charities_CharityId",
                        column: x => x.CharityId,
                        principalTable: "Charities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodicOrphanReports_Orphans_OrphanId",
                        column: x => x.OrphanId,
                        principalTable: "Orphans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Charities_CityId",
                table: "Charities",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Charities_Code",
                table: "Charities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Charities_CountryId",
                table: "Charities",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Charities_Name",
                table: "Charities",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryId",
                table: "Cities",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryId1",
                table: "Cities",
                column: "CountryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Code",
                table: "Employees",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_NationalId",
                table: "Employees",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Families_CharityId",
                table: "Families",
                column: "CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_Families_CityId",
                table: "Families",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Families_Code",
                table: "Families",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Families_CountryId",
                table: "Families",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_IsRead",
                table: "NotificationLogs",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_Type",
                table: "NotificationLogs",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_UserId",
                table: "NotificationLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orphans_CharityId",
                table: "Orphans",
                column: "CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_Orphans_Code",
                table: "Orphans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orphans_FamilyId",
                table: "Orphans",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Orphans_SponsorId",
                table: "Orphans",
                column: "SponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReports_CharityId",
                table: "PeriodicOrphanReports",
                column: "CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReports_OrphanId",
                table: "PeriodicOrphanReports",
                column: "OrphanId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReports_OrphanId_ReportMonth_ReportYear",
                table: "PeriodicOrphanReports",
                columns: new[] { "OrphanId", "ReportMonth", "ReportYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReports_ReportMonth",
                table: "PeriodicOrphanReports",
                column: "ReportMonth");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReports_ReportYear",
                table: "PeriodicOrphanReports",
                column: "ReportYear");

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_CityId",
                table: "Sponsors",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_Code",
                table: "Sponsors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_CountryId",
                table: "Sponsors",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_Email",
                table: "Sponsors",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Category",
                table: "SystemSettings",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "PeriodicOrphanReports");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Orphans");

            migrationBuilder.DropTable(
                name: "Families");

            migrationBuilder.DropTable(
                name: "Sponsors");

            migrationBuilder.DropTable(
                name: "Charities");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
