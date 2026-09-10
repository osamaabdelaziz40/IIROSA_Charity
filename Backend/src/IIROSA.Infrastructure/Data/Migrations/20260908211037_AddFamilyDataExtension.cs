using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyDataExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ==================== initiation data (user-specified catalogues) ====================
            // Upsert-by-id (same convention as InitHousingAndOfficeProjectLookups): rows already
            // occupying an id are updated in place, missing ids are inserted with explicit
            // identity values, keeping any FK references valid while converging the tables on
            // the user's exact catalogues (§4 معلومات الأسرة legacy WAR.IIROSA select values).
            migrationBuilder.Sql(BuildHouseOwnershipDataSql());
            migrationBuilder.Sql(BuildIncomeTypeDataSql());

            migrationBuilder.AddColumn<int>(
                name: "ChildrenCount",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FamilyProjectStatusId",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasProject",
                schema: "IIROSA",
                table: "Family",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "IncomeValue",
                schema: "IIROSA",
                table: "Family",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalIncome",
                schema: "IIROSA",
                table: "Family",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FamilyPhone",
                schema: "IIROSA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_FamilyPhone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyPhone_Family_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "IIROSA",
                        principalTable: "Family",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyProjectStatus",
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
                    table.PrimaryKey("PK_FamilyProjectStatus", x => x.Id);
                });

            // Seed AFTER the table exists (this migration creates it)
            migrationBuilder.Sql(BuildFamilyProjectStatusDataSql());

            migrationBuilder.CreateIndex(
                name: "IX_Family_FamilyProjectStatusId",
                schema: "IIROSA",
                table: "Family",
                column: "FamilyProjectStatusId");

            migrationBuilder.CreateIndex(
                name: "UX_FamilyPhone_OneDefaultPerFamily",
                schema: "IIROSA",
                table: "FamilyPhone",
                column: "FamilyId",
                unique: true,
                filter: "[IsDefault] = 1 AND [IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Family_FamilyProjectStatus_FamilyProjectStatusId",
                schema: "IIROSA",
                table: "Family",
                column: "FamilyProjectStatusId",
                principalSchema: "Lookup",
                principalTable: "FamilyProjectStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <summary>ملكية السكن — ملك / إيجار</summary>
        private static string BuildHouseOwnershipDataSql()
        {
            var sql = new System.Text.StringBuilder();
            sql.AppendLine("SET IDENTITY_INSERT [Lookup].[HouseOwnership] ON;");
            var rows = new (int Id, string NameAr, string NameEn)[]
            {
                (1, "ملك", "Owned"),
                (2, "إيجار", "Rented")
            };
            foreach (var r in rows)
            {
                sql.AppendLine($"IF EXISTS (SELECT 1 FROM [Lookup].[HouseOwnership] WHERE Id = {r.Id})");
                sql.AppendLine($"    UPDATE [Lookup].[HouseOwnership] SET NameAr = N'{r.NameAr}', NameEn = N'{r.NameEn}', IsActive = 1, SortOrder = {r.Id}, UpdatedOn = GETUTCDATE() WHERE Id = {r.Id};");
                sql.AppendLine("ELSE");
                sql.AppendLine($"    INSERT INTO [Lookup].[HouseOwnership] (Id, NameAr, NameEn, IsActive, SortOrder, CreatedOn) VALUES ({r.Id}, N'{r.NameAr}', N'{r.NameEn}', 1, {r.Id}, GETUTCDATE());");
            }
            sql.AppendLine("SET IDENTITY_INSERT [Lookup].[HouseOwnership] OFF;");
            return sql.ToString();
        }

        /// <summary>نوع الدخل — معاش / راتب / ضمان إجتماعى / أخرى / منحه / مساعدات</summary>
        private static string BuildIncomeTypeDataSql()
        {
            var sql = new System.Text.StringBuilder();
            sql.AppendLine("SET IDENTITY_INSERT [Lookup].[IncomeType] ON;");
            var rows = new (int Id, string NameAr, string NameEn)[]
            {
                (1, "معاش", "Pension"),
                (2, "راتب", "Salary"),
                (3, "ضمان إجتماعى", "Social Security"),
                (4, "أخرى", "Other"),
                (5, "منحه", "Grant"),
                (6, "مساعدات", "Aid")
            };
            foreach (var r in rows)
            {
                sql.AppendLine($"IF EXISTS (SELECT 1 FROM [Lookup].[IncomeType] WHERE Id = {r.Id})");
                sql.AppendLine($"    UPDATE [Lookup].[IncomeType] SET NameAr = N'{r.NameAr}', NameEn = N'{r.NameEn}', IsActive = 1, SortOrder = {r.Id}, UpdatedOn = GETUTCDATE() WHERE Id = {r.Id};");
                sql.AppendLine("ELSE");
                sql.AppendLine($"    INSERT INTO [Lookup].[IncomeType] (Id, NameAr, NameEn, IsActive, SortOrder, CreatedOn) VALUES ({r.Id}, N'{r.NameAr}', N'{r.NameEn}', 1, {r.Id}, GETUTCDATE());");
            }
            sql.AppendLine("SET IDENTITY_INSERT [Lookup].[IncomeType] OFF;");
            return sql.ToString();
        }

        /// <summary>حالة المشروع — يوجد مشروع قائم (1) / مشروع جديد (2), per the legacy select values</summary>
        private static string BuildFamilyProjectStatusDataSql()
        {
            var sql = new System.Text.StringBuilder();
            sql.AppendLine("SET IDENTITY_INSERT [Lookup].[FamilyProjectStatus] ON;");
            var rows = new (int Id, string NameAr, string NameEn)[]
            {
                (1, "يوجد مشروع قائم", "Existing Project"),
                (2, "مشروع جديد", "New Project")
            };
            foreach (var r in rows)
            {
                sql.AppendLine($"IF EXISTS (SELECT 1 FROM [Lookup].[FamilyProjectStatus] WHERE Id = {r.Id})");
                sql.AppendLine($"    UPDATE [Lookup].[FamilyProjectStatus] SET NameAr = N'{r.NameAr}', NameEn = N'{r.NameEn}', IsActive = 1, SortOrder = {r.Id}, UpdatedOn = GETUTCDATE() WHERE Id = {r.Id};");
                sql.AppendLine("ELSE");
                sql.AppendLine($"    INSERT INTO [Lookup].[FamilyProjectStatus] (Id, NameAr, NameEn, IsActive, SortOrder, CreatedOn) VALUES ({r.Id}, N'{r.NameAr}', N'{r.NameEn}', 1, {r.Id}, GETUTCDATE());");
            }
            sql.AppendLine("SET IDENTITY_INSERT [Lookup].[FamilyProjectStatus] OFF;");
            return sql.ToString();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Family_FamilyProjectStatus_FamilyProjectStatusId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropTable(
                name: "FamilyPhone",
                schema: "IIROSA");

            migrationBuilder.DropTable(
                name: "FamilyProjectStatus",
                schema: "Lookup");

            migrationBuilder.DropIndex(
                name: "IX_Family_FamilyProjectStatusId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "ChildrenCount",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "FamilyProjectStatusId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "HasProject",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "IncomeValue",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropColumn(
                name: "TotalIncome",
                schema: "IIROSA",
                table: "Family");

            // Seeded catalogues keep their rows on Down — another module's data may reference them
        }
    }
}
