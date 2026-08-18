using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFamilyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationLevel",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "HealthStatus",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "HousingStatus",
                table: "Families");

            migrationBuilder.AddColumn<string>(
                name: "AcademicPerformance",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChronicDiseases",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Disabilities",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EducationLevelId",
                table: "Orphans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeClass",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HealthStatusId",
                table: "Orphans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hobbies",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrphanType",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PhotoAttachmentId",
                table: "Orphans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolName",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SponsorshipStatus",
                table: "Orphans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityVillage",
                table: "Families",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DistrictArea",
                table: "Families",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HousingTypeId",
                table: "Families",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LivingConditionId",
                table: "Families",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderType",
                table: "Families",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDate",
                table: "Families",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "EducationLevel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_EducationLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_HealthStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HousingType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_HousingType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LivingCondition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_LivingCondition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrphanPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PaymentPeriodFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentPeriodTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GroupDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DontRemoveRate = table.Column<bool>(type: "bit", nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShowOrder = table.Column<int>(type: "int", nullable: false),
                    IsBatchUploaded = table.Column<bool>(type: "bit", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_OrphanPayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Provider",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelationshipToFamily = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Job = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Provider", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Provider_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Father",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EducationLevelId = table.Column<int>(type: "int", nullable: true),
                    Job = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HealthStatusId = table.Column<int>(type: "int", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAlive = table.Column<bool>(type: "bit", nullable: false),
                    IsProvider = table.Column<bool>(type: "bit", nullable: false),
                    DeathDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Father", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Father_EducationLevel_EducationLevelId",
                        column: x => x.EducationLevelId,
                        principalTable: "EducationLevel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Father_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Father_HealthStatus_HealthStatusId",
                        column: x => x.HealthStatusId,
                        principalTable: "HealthStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Mother",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EducationLevelId = table.Column<int>(type: "int", nullable: true),
                    Job = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HealthStatusId = table.Column<int>(type: "int", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAlive = table.Column<bool>(type: "bit", nullable: false),
                    IsProvider = table.Column<bool>(type: "bit", nullable: false),
                    DeathDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Mother", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mother_EducationLevel_EducationLevelId",
                        column: x => x.EducationLevelId,
                        principalTable: "EducationLevel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Mother_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Mother_HealthStatus_HealthStatusId",
                        column: x => x.HealthStatusId,
                        principalTable: "HealthStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrphanPaymentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrphanPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrphanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_OrphanPaymentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrphanPaymentItems_OrphanPayments_OrphanPaymentId",
                        column: x => x.OrphanPaymentId,
                        principalTable: "OrphanPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrphanPaymentItems_Orphans_OrphanId",
                        column: x => x.OrphanId,
                        principalTable: "Orphans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orphans_EducationLevelId",
                table: "Orphans",
                column: "EducationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Orphans_HealthStatusId",
                table: "Orphans",
                column: "HealthStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Families_HousingTypeId",
                table: "Families",
                column: "HousingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Families_LivingConditionId",
                table: "Families",
                column: "LivingConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_Father_EducationLevelId",
                table: "Father",
                column: "EducationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Father_FamilyId",
                table: "Father",
                column: "FamilyId",
                unique: true,
                filter: "[FamilyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Father_HealthStatusId",
                table: "Father",
                column: "HealthStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Mother_EducationLevelId",
                table: "Mother",
                column: "EducationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Mother_FamilyId",
                table: "Mother",
                column: "FamilyId",
                unique: true,
                filter: "[FamilyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Mother_HealthStatusId",
                table: "Mother",
                column: "HealthStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPaymentItems_OrphanId",
                table: "OrphanPaymentItems",
                column: "OrphanId");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPaymentItems_OrphanPaymentId",
                table: "OrphanPaymentItems",
                column: "OrphanPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPaymentItems_OrphanPaymentId_OrphanId",
                table: "OrphanPaymentItems",
                columns: new[] { "OrphanPaymentId", "OrphanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPayments_BatchNo",
                table: "OrphanPayments",
                column: "BatchNo",
                unique: true,
                filter: "[BatchNo] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPayments_GroupDate",
                table: "OrphanPayments",
                column: "GroupDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPayments_GroupName",
                table: "OrphanPayments",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPayments_IsBatchUploaded",
                table: "OrphanPayments",
                column: "IsBatchUploaded");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPayments_PaymentPeriodFrom",
                table: "OrphanPayments",
                column: "PaymentPeriodFrom");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPayments_PaymentPeriodTo",
                table: "OrphanPayments",
                column: "PaymentPeriodTo");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPayments_ShowOrder",
                table: "OrphanPayments",
                column: "ShowOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_FamilyId",
                table: "Provider",
                column: "FamilyId",
                unique: true,
                filter: "[FamilyId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Families_HousingType_HousingTypeId",
                table: "Families",
                column: "HousingTypeId",
                principalTable: "HousingType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Families_LivingCondition_LivingConditionId",
                table: "Families",
                column: "LivingConditionId",
                principalTable: "LivingCondition",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orphans_EducationLevel_EducationLevelId",
                table: "Orphans",
                column: "EducationLevelId",
                principalTable: "EducationLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orphans_HealthStatus_HealthStatusId",
                table: "Orphans",
                column: "HealthStatusId",
                principalTable: "HealthStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Families_HousingType_HousingTypeId",
                table: "Families");

            migrationBuilder.DropForeignKey(
                name: "FK_Families_LivingCondition_LivingConditionId",
                table: "Families");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphans_EducationLevel_EducationLevelId",
                table: "Orphans");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphans_HealthStatus_HealthStatusId",
                table: "Orphans");

            migrationBuilder.DropTable(
                name: "Father");

            migrationBuilder.DropTable(
                name: "HousingType");

            migrationBuilder.DropTable(
                name: "LivingCondition");

            migrationBuilder.DropTable(
                name: "Mother");

            migrationBuilder.DropTable(
                name: "OrphanPaymentItems");

            migrationBuilder.DropTable(
                name: "Provider");

            migrationBuilder.DropTable(
                name: "EducationLevel");

            migrationBuilder.DropTable(
                name: "HealthStatus");

            migrationBuilder.DropTable(
                name: "OrphanPayments");

            migrationBuilder.DropIndex(
                name: "IX_Orphans_EducationLevelId",
                table: "Orphans");

            migrationBuilder.DropIndex(
                name: "IX_Orphans_HealthStatusId",
                table: "Orphans");

            migrationBuilder.DropIndex(
                name: "IX_Families_HousingTypeId",
                table: "Families");

            migrationBuilder.DropIndex(
                name: "IX_Families_LivingConditionId",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "AcademicPerformance",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "ChronicDiseases",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "Disabilities",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "EducationLevelId",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "GradeClass",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "HealthStatusId",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "Hobbies",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "OrphanType",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "PhotoAttachmentId",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "SchoolName",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "SponsorshipStatus",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "CityVillage",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "DistrictArea",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "HousingTypeId",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "LivingConditionId",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "ProviderType",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "Families");

            migrationBuilder.AddColumn<string>(
                name: "EducationLevel",
                table: "Orphans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HealthStatus",
                table: "Orphans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HousingStatus",
                table: "Families",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
