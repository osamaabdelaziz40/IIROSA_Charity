using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Epic 6 / UC-HOU-05 scope: Lookup.HousingBuilding + Lookup.HousingFlat tables and
    /// Family.FK_HousingBuildingId / FK_HousingFlatId allocation columns.
    ///
    /// PARALLEL-SESSION SWEEP (same situation as Epic06_HousingFamilyType): the shared model
    /// snapshot had pending epic-5/9 changes when this migration was cut — Provider
    /// (DateOfBirth, DeathDate, DeathReason, IsAlive, NationalityCountryId, ReasonOfRelationId,
    /// SocialStatusId), Orphan.CenterId, Family (CenterId, HouseOwnershipId, HouseStatusId,
    /// IncomeTypeId, NearBy, RegionId, RentAmount, Street) and seven lookup tables
    /// (HouseOwnership, HouseStatus, IncomeType, ReasonOfRel, RefuseReason, Relation,
    /// SocialStatus). Trimming them would strand those sessions' Designer snapshot claims, so
    /// they ride along. Everything in Up() is additive — no drops, no destructive alters.
    /// NOT yet applied to the database — run `dotnet ef database update` when the API can be
    /// restarted (live process holds the build/database; never kill it).
    /// </remarks>
    public partial class Epic06_HousingBuildingsFlats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                schema: "IIROSA",
                table: "Provider",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeathDate",
                schema: "IIROSA",
                table: "Provider",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeathReason",
                schema: "IIROSA",
                table: "Provider",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAlive",
                schema: "IIROSA",
                table: "Provider",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NationalityCountryId",
                schema: "IIROSA",
                table: "Provider",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReasonOfRelationId",
                schema: "IIROSA",
                table: "Provider",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SocialStatusId",
                schema: "IIROSA",
                table: "Orphan",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CenterId",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FK_HousingBuildingId",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FK_HousingFlatId",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HouseOwnershipId",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HouseStatusId",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IncomeTypeId",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NearBy",
                schema: "IIROSA",
                table: "Family",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegionId",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RentAmount",
                schema: "IIROSA",
                table: "Family",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                schema: "IIROSA",
                table: "Family",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HouseOwnership",
                schema: "Lookup",
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
                    table.PrimaryKey("PK_HouseOwnership", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HouseStatus",
                schema: "Lookup",
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
                    table.PrimaryKey("PK_HouseStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HousingBuilding",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousingBuilding", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncomeType",
                schema: "Lookup",
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
                    table.PrimaryKey("PK_IncomeType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReasonOfRel",
                schema: "Lookup",
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
                    table.PrimaryKey("PK_ReasonOfRel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefuseReason",
                schema: "Lookup",
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
                    table.PrimaryKey("PK_RefuseReason", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Relation",
                schema: "Lookup",
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
                    table.PrimaryKey("PK_Relation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialStatus",
                schema: "Lookup",
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
                    table.PrimaryKey("PK_SocialStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HousingFlat",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousingFlat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HousingFlat_HousingBuilding_BuildingId",
                        column: x => x.BuildingId,
                        principalSchema: "Lookup",
                        principalTable: "HousingBuilding",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Provider_NationalityCountryId",
                schema: "IIROSA",
                table: "Provider",
                column: "NationalityCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_ReasonOfRelationId",
                schema: "IIROSA",
                table: "Provider",
                column: "ReasonOfRelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Orphan_SocialStatusId",
                schema: "IIROSA",
                table: "Orphan",
                column: "SocialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Family_CenterId",
                schema: "IIROSA",
                table: "Family",
                column: "CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Family_FK_HousingBuildingId",
                schema: "IIROSA",
                table: "Family",
                column: "FK_HousingBuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Family_FK_HousingFlatId",
                schema: "IIROSA",
                table: "Family",
                column: "FK_HousingFlatId");

            migrationBuilder.CreateIndex(
                name: "IX_Family_HouseOwnershipId",
                schema: "IIROSA",
                table: "Family",
                column: "HouseOwnershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Family_HouseStatusId",
                schema: "IIROSA",
                table: "Family",
                column: "HouseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Family_IncomeTypeId",
                schema: "IIROSA",
                table: "Family",
                column: "IncomeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Family_RegionId",
                schema: "IIROSA",
                table: "Family",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingBuilding_IsActive",
                schema: "Lookup",
                table: "HousingBuilding",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HousingFlat_BuildingId",
                schema: "Lookup",
                table: "HousingFlat",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingFlat_IsActive",
                schema: "Lookup",
                table: "HousingFlat",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_Family_Center_CenterId",
                schema: "IIROSA",
                table: "Family",
                column: "CenterId",
                principalSchema: "Lookup",
                principalTable: "Center",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Family_HouseOwnership_HouseOwnershipId",
                schema: "IIROSA",
                table: "Family",
                column: "HouseOwnershipId",
                principalSchema: "Lookup",
                principalTable: "HouseOwnership",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Family_HouseStatus_HouseStatusId",
                schema: "IIROSA",
                table: "Family",
                column: "HouseStatusId",
                principalSchema: "Lookup",
                principalTable: "HouseStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Family_HousingBuilding_FK_HousingBuildingId",
                schema: "IIROSA",
                table: "Family",
                column: "FK_HousingBuildingId",
                principalSchema: "Lookup",
                principalTable: "HousingBuilding",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Family_HousingFlat_FK_HousingFlatId",
                schema: "IIROSA",
                table: "Family",
                column: "FK_HousingFlatId",
                principalSchema: "Lookup",
                principalTable: "HousingFlat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Family_IncomeType_IncomeTypeId",
                schema: "IIROSA",
                table: "Family",
                column: "IncomeTypeId",
                principalSchema: "Lookup",
                principalTable: "IncomeType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Family_Region_RegionId",
                schema: "IIROSA",
                table: "Family",
                column: "RegionId",
                principalSchema: "Lookup",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orphan_SocialStatus_SocialStatusId",
                schema: "IIROSA",
                table: "Orphan",
                column: "SocialStatusId",
                principalSchema: "Lookup",
                principalTable: "SocialStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_Country_NationalityCountryId",
                schema: "IIROSA",
                table: "Provider",
                column: "NationalityCountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_ReasonOfRel_ReasonOfRelationId",
                schema: "IIROSA",
                table: "Provider",
                column: "ReasonOfRelationId",
                principalSchema: "Lookup",
                principalTable: "ReasonOfRel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Family_Center_CenterId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_HouseOwnership_HouseOwnershipId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_HouseStatus_HouseStatusId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_HousingBuilding_FK_HousingBuildingId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_HousingFlat_FK_HousingFlatId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_IncomeType_IncomeTypeId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_Region_RegionId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphan_SocialStatus_SocialStatusId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropForeignKey(
                name: "FK_Provider_Country_NationalityCountryId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropForeignKey(
                name: "FK_Provider_ReasonOfRel_ReasonOfRelationId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropTable(
                name: "HouseOwnership",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "HouseStatus",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "HousingFlat",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "IncomeType",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "ReasonOfRel",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "RefuseReason",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "Relation",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "SocialStatus",
                schema: "Lookup");

            migrationBuilder.DropTable(
                name: "HousingBuilding",
                schema: "Lookup");

            migrationBuilder.DropIndex(
                name: "IX_Provider_NationalityCountryId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropIndex(
                name: "IX_Provider_ReasonOfRelationId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropIndex(
                name: "IX_Orphan_SocialStatusId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropIndex(
                name: "IX_Family_CenterId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropIndex(
                name: "IX_Family_FK_HousingBuildingId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropIndex(
                name: "IX_Family_FK_HousingFlatId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropIndex(
                name: "IX_Family_HouseOwnershipId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropIndex(
                name: "IX_Family_HouseStatusId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropIndex(
                name: "IX_Family_IncomeTypeId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropIndex(
                name: "IX_Family_RegionId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "DeathDate",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "DeathReason",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "IsAlive",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "NationalityCountryId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "ReasonOfRelationId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropColumn(
                name: "SocialStatusId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropColumn(
                name: "CenterId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "FK_HousingBuildingId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "FK_HousingFlatId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "HouseOwnershipId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "HouseStatusId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "IncomeTypeId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "NearBy",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "RegionId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "RentAmount",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "Street",
                schema: "IIROSA",
                table: "Family");
        }
    }
}
