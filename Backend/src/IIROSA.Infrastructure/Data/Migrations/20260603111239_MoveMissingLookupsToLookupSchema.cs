using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveMissingLookupsToLookupSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Center_Country_CountryId",
                table: "Center");

            migrationBuilder.DropForeignKey(
                name: "FK_Center_Region_RegionId",
                table: "Center");

            migrationBuilder.DropForeignKey(
                name: "FK_Region_Country_CountryId",
                table: "Region");

            migrationBuilder.RenameTable(
                name: "Region",
                newName: "Region",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "ProjectType",
                newName: "ProjectType",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "NGOType",
                newName: "NGOType",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "MissionType",
                newName: "MissionType",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "LivingCondition",
                newName: "LivingCondition",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "HousingType",
                newName: "HousingType",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "HealthStatus",
                newName: "HealthStatus",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "EducationLevel",
                newName: "EducationLevel",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "Center",
                newName: "Center",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "Bank",
                newName: "Bank",
                newSchema: "Lookup");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "Lookup",
                table: "Region",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                schema: "Lookup",
                table: "Region",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId1",
                schema: "Lookup",
                table: "Region",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeDescription",
                schema: "Lookup",
                table: "ProjectType",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeCode",
                schema: "Lookup",
                table: "ProjectType",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeDescription",
                schema: "Lookup",
                table: "NGOType",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeCode",
                schema: "Lookup",
                table: "NGOType",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeDescription",
                schema: "Lookup",
                table: "MissionType",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeCode",
                schema: "Lookup",
                table: "MissionType",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "Lookup",
                table: "Center",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CenterCode",
                schema: "Lookup",
                table: "Center",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId1",
                schema: "Lookup",
                table: "Center",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SwiftCode",
                schema: "Lookup",
                table: "Bank",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "Lookup",
                table: "Bank",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankCode",
                schema: "Lookup",
                table: "Bank",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "Lookup",
                table: "Bank",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Region_CountryId1",
                schema: "Lookup",
                table: "Region",
                column: "CountryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Region_RegionCode",
                schema: "Lookup",
                table: "Region",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_Region_SortOrder",
                schema: "Lookup",
                table: "Region",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectType_TypeCode",
                schema: "Lookup",
                table: "ProjectType",
                column: "TypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_NGOType_TypeCode",
                schema: "Lookup",
                table: "NGOType",
                column: "TypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_MissionType_TypeCode",
                schema: "Lookup",
                table: "MissionType",
                column: "TypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_Center_CenterCode",
                schema: "Lookup",
                table: "Center",
                column: "CenterCode");

            migrationBuilder.CreateIndex(
                name: "IX_Center_CountryId1",
                schema: "Lookup",
                table: "Center",
                column: "CountryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Center_SortOrder",
                schema: "Lookup",
                table: "Center",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Bank_BankCode",
                schema: "Lookup",
                table: "Bank",
                column: "BankCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Center_Country_CountryId",
                schema: "Lookup",
                table: "Center",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Center_Country_CountryId1",
                schema: "Lookup",
                table: "Center",
                column: "CountryId1",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Center_Region_RegionId",
                schema: "Lookup",
                table: "Center",
                column: "RegionId",
                principalSchema: "Lookup",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Region_Country_CountryId",
                schema: "Lookup",
                table: "Region",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Region_Country_CountryId1",
                schema: "Lookup",
                table: "Region",
                column: "CountryId1",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Center_Country_CountryId",
                schema: "Lookup",
                table: "Center");

            migrationBuilder.DropForeignKey(
                name: "FK_Center_Country_CountryId1",
                schema: "Lookup",
                table: "Center");

            migrationBuilder.DropForeignKey(
                name: "FK_Center_Region_RegionId",
                schema: "Lookup",
                table: "Center");

            migrationBuilder.DropForeignKey(
                name: "FK_Region_Country_CountryId",
                schema: "Lookup",
                table: "Region");

            migrationBuilder.DropForeignKey(
                name: "FK_Region_Country_CountryId1",
                schema: "Lookup",
                table: "Region");

            migrationBuilder.DropIndex(
                name: "IX_Region_CountryId1",
                schema: "Lookup",
                table: "Region");

            migrationBuilder.DropIndex(
                name: "IX_Region_RegionCode",
                schema: "Lookup",
                table: "Region");

            migrationBuilder.DropIndex(
                name: "IX_Region_SortOrder",
                schema: "Lookup",
                table: "Region");

            migrationBuilder.DropIndex(
                name: "IX_ProjectType_TypeCode",
                schema: "Lookup",
                table: "ProjectType");

            migrationBuilder.DropIndex(
                name: "IX_NGOType_TypeCode",
                schema: "Lookup",
                table: "NGOType");

            migrationBuilder.DropIndex(
                name: "IX_MissionType_TypeCode",
                schema: "Lookup",
                table: "MissionType");

            migrationBuilder.DropIndex(
                name: "IX_Center_CenterCode",
                schema: "Lookup",
                table: "Center");

            migrationBuilder.DropIndex(
                name: "IX_Center_CountryId1",
                schema: "Lookup",
                table: "Center");

            migrationBuilder.DropIndex(
                name: "IX_Center_SortOrder",
                schema: "Lookup",
                table: "Center");

            migrationBuilder.DropIndex(
                name: "IX_Bank_BankCode",
                schema: "Lookup",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "CountryId1",
                schema: "Lookup",
                table: "Region");

            migrationBuilder.DropColumn(
                name: "CountryId1",
                schema: "Lookup",
                table: "Center");

            migrationBuilder.RenameTable(
                name: "Region",
                schema: "Lookup",
                newName: "Region");

            migrationBuilder.RenameTable(
                name: "ProjectType",
                schema: "Lookup",
                newName: "ProjectType");

            migrationBuilder.RenameTable(
                name: "NGOType",
                schema: "Lookup",
                newName: "NGOType");

            migrationBuilder.RenameTable(
                name: "MissionType",
                schema: "Lookup",
                newName: "MissionType");

            migrationBuilder.RenameTable(
                name: "LivingCondition",
                schema: "Lookup",
                newName: "LivingCondition");

            migrationBuilder.RenameTable(
                name: "HousingType",
                schema: "Lookup",
                newName: "HousingType");

            migrationBuilder.RenameTable(
                name: "HealthStatus",
                schema: "Lookup",
                newName: "HealthStatus");

            migrationBuilder.RenameTable(
                name: "EducationLevel",
                schema: "Lookup",
                newName: "EducationLevel");

            migrationBuilder.RenameTable(
                name: "Center",
                schema: "Lookup",
                newName: "Center");

            migrationBuilder.RenameTable(
                name: "Bank",
                schema: "Lookup",
                newName: "Bank");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "Region",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                table: "Region",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeDescription",
                table: "ProjectType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeCode",
                table: "ProjectType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeDescription",
                table: "NGOType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeCode",
                table: "NGOType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeDescription",
                table: "MissionType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeCode",
                table: "MissionType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "Center",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CenterCode",
                table: "Center",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SwiftCode",
                table: "Bank",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Bank",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankCode",
                table: "Bank",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Bank",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Center_Country_CountryId",
                table: "Center",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Center_Region_RegionId",
                table: "Center",
                column: "RegionId",
                principalTable: "Region",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Region_Country_CountryId",
                table: "Region",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id");
        }
    }
}
