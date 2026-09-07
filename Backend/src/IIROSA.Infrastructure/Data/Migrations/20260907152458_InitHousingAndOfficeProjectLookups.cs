using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitHousingAndOfficeProjectLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                schema: "Lookup",
                table: "HousingBuilding",
                newName: "BuildingAddress");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Lookup",
                table: "HousingFlat",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                schema: "Lookup",
                table: "HousingFlat",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SizeInMtr",
                schema: "Lookup",
                table: "HousingFlat",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingDescription",
                schema: "Lookup",
                table: "HousingBuilding",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuildingNumber",
                schema: "Lookup",
                table: "HousingBuilding",
                type: "int",
                nullable: true);

            // ==================== initiation data (user-specified catalogue) ====================
            // Upsert-by-id: rows already occupying an id are updated in place, missing ids are
            // inserted with explicit identity values. This keeps the FK references of existing
            // housing families (IIROSA.Family.FK_HousingBuildingId / FK_HousingFlatId) valid
            // while converging the tables on the user's exact catalogue:
            // 4 buildings (العمارات 7/8/13/14 — مدينه نصر) × 12 flats of 100 m² each.
            migrationBuilder.Sql(BuildHousingBuildingDataSql());
            migrationBuilder.Sql(BuildHousingFlatDataSql());
        }

        /// <summary>
        /// The initiation catalogue — (id, title, number, address, description) per building.
        /// Flats derive from this: 12 per building, ids assigned building-major (1..12 →
        /// building 1, 13..24 → building 2, …), matching the user's explicit id scheme.
        /// </summary>
        private static readonly (int Id, string Title, int Number, string Address, string Description)[] HousingBuildings =
        {
            (1, "العماره رقم 7", 7, "مدينه نصر", "العماره رقم 7"),
            (2, "العماره رقم 8", 8, "مدينه نصر", "العماره رقم 8"),
            (3, "العماره رقم 13", 13, "مدينه نصر", "العماره رقم 13"),
            (4, "العماره رقم 14", 14, "مدينه نصر", "العماره رقم 14")
        };

        private static string BuildHousingBuildingDataSql()
        {
            var sql = new StringBuilder();
            sql.AppendLine("SET IDENTITY_INSERT [Lookup].[HousingBuilding] ON;");
            foreach (var b in HousingBuildings)
            {
                sql.AppendLine($"IF EXISTS (SELECT 1 FROM [Lookup].[HousingBuilding] WHERE Id = {b.Id})");
                sql.AppendLine($"    UPDATE [Lookup].[HousingBuilding] SET NameAr = N'{b.Title}', NameEn = N'Building No. {b.Number}', BuildingNumber = {b.Number}, BuildingAddress = N'{b.Address}', BuildingDescription = N'{b.Description}', IsActive = 1, SortOrder = {b.Id}, UpdatedOn = GETUTCDATE() WHERE Id = {b.Id};");
                sql.AppendLine("ELSE");
                sql.AppendLine($"    INSERT INTO [Lookup].[HousingBuilding] (Id, NameAr, NameEn, BuildingNumber, BuildingAddress, BuildingDescription, IsActive, SortOrder, CreatedOn) VALUES ({b.Id}, N'{b.Title}', N'Building No. {b.Number}', {b.Number}, N'{b.Address}', N'{b.Description}', 1, {b.Id}, GETUTCDATE());");
            }
            sql.AppendLine("SET IDENTITY_INSERT [Lookup].[HousingBuilding] OFF;");
            return sql.ToString();
        }

        private static string BuildHousingFlatDataSql()
        {
            var sql = new StringBuilder();
            sql.AppendLine("SET IDENTITY_INSERT [Lookup].[HousingFlat] ON;");
            var id = 1;
            foreach (var b in HousingBuildings)
            {
                for (var number = 1; number <= 12; number++)
                {
                    sql.AppendLine($"IF EXISTS (SELECT 1 FROM [Lookup].[HousingFlat] WHERE Id = {id})");
                    sql.AppendLine($"    UPDATE [Lookup].[HousingFlat] SET BuildingId = {b.Id}, NameAr = N'{number}', NameEn = N'{number}', Number = {number}, SizeInMtr = 100, [Description] = NULL, IsActive = 1, SortOrder = {number}, UpdatedOn = GETUTCDATE() WHERE Id = {id};");
                    sql.AppendLine("ELSE");
                    sql.AppendLine($"    INSERT INTO [Lookup].[HousingFlat] (Id, BuildingId, NameAr, NameEn, Number, SizeInMtr, [Description], IsActive, SortOrder, CreatedOn) VALUES ({id}, {b.Id}, N'{number}', N'{number}', {number}, 100, NULL, 1, {number}, GETUTCDATE());");
                    id++;
                }
            }
            sql.AppendLine("SET IDENTITY_INSERT [Lookup].[HousingFlat] OFF;");
            return sql.ToString();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Lookup",
                table: "HousingFlat");

            migrationBuilder.DropColumn(
                name: "Number",
                schema: "Lookup",
                table: "HousingFlat");

            migrationBuilder.DropColumn(
                name: "SizeInMtr",
                schema: "Lookup",
                table: "HousingFlat");

            migrationBuilder.DropColumn(
                name: "BuildingDescription",
                schema: "Lookup",
                table: "HousingBuilding");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                schema: "Lookup",
                table: "HousingBuilding");

            migrationBuilder.RenameColumn(
                name: "BuildingAddress",
                schema: "Lookup",
                table: "HousingBuilding",
                newName: "Location");
        }
    }
}
