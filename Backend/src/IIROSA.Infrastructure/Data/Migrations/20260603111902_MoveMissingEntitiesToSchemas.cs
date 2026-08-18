using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveMissingEntitiesToSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Check_Bank_BankId",
                table: "Check");

            migrationBuilder.DropForeignKey(
                name: "FK_Check_ChequeBeneficiary_ChequeBeneficiaryId",
                table: "Check");

            migrationBuilder.DropForeignKey(
                name: "FK_ChequeBeneficiary_Bank_BankId",
                table: "ChequeBeneficiary");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildOutGoing_UploadedFile_UploadedFileId",
                table: "ChildOutGoing");

            migrationBuilder.DropForeignKey(
                name: "FK_Father_EducationLevel_EducationLevelId",
                table: "Father");

            migrationBuilder.DropForeignKey(
                name: "FK_Father_Family_FamilyId",
                table: "Father");

            migrationBuilder.DropForeignKey(
                name: "FK_Father_HealthStatus_HealthStatusId",
                table: "Father");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Center_CenterId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Charity_CharityId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Country_CountryId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Family_FamilyId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Region_RegionId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_Mother_EducationLevel_EducationLevelId",
                table: "Mother");

            migrationBuilder.DropForeignKey(
                name: "FK_Mother_Family_FamilyId",
                table: "Mother");

            migrationBuilder.DropForeignKey(
                name: "FK_Mother_HealthStatus_HealthStatusId",
                table: "Mother");

            migrationBuilder.DropForeignKey(
                name: "FK_Provider_Family_FamilyId",
                table: "Provider");

            migrationBuilder.DropForeignKey(
                name: "FK_Relative_EducationLevel_EducationLevelId",
                table: "Relative");

            migrationBuilder.DropForeignKey(
                name: "FK_Relative_Family_FamilyId",
                table: "Relative");

            migrationBuilder.DropForeignKey(
                name: "FK_Relative_HealthStatus_HealthStatusId",
                table: "Relative");

            migrationBuilder.DropIndex(
                name: "IX_ChequeBeneficiary_BankId",
                table: "ChequeBeneficiary");

            migrationBuilder.DropIndex(
                name: "IX_Check_BankId",
                table: "Check");

            migrationBuilder.DropIndex(
                name: "IX_Check_ChequeBeneficiaryId",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "ChequeBeneficiary");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "ChequeBeneficiaryId",
                table: "Check");

            migrationBuilder.RenameTable(
                name: "Relative",
                newName: "Relative",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "Provider",
                newName: "Provider",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "OutgoingCategory",
                newName: "OutgoingCategory",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "Mother",
                newName: "Mother",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "HousingProject",
                newName: "HousingProject",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "Father",
                newName: "Father",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "ChildOutGoing",
                newName: "ChildOutGoing",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "ChequeBeneficiary",
                newName: "ChequeBeneficiary",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "Check",
                newName: "Check",
                newSchema: "IIROSA");

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipType",
                schema: "IIROSA",
                table: "Relative",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PlaceOfBirth",
                schema: "IIROSA",
                table: "Relative",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "IIROSA",
                table: "Relative",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "IIROSA",
                table: "Relative",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                schema: "IIROSA",
                table: "Relative",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Job",
                schema: "IIROSA",
                table: "Relative",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsLivingWithFamily",
                schema: "IIROSA",
                table: "Relative",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAlive",
                schema: "IIROSA",
                table: "Relative",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                schema: "IIROSA",
                table: "Relative",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "IIROSA",
                table: "Relative",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "IIROSA",
                table: "Relative",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipToFamily",
                schema: "IIROSA",
                table: "Provider",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "IIROSA",
                table: "Provider",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "IIROSA",
                table: "Provider",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                schema: "IIROSA",
                table: "Provider",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Job",
                schema: "IIROSA",
                table: "Provider",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "IIROSA",
                table: "Provider",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "IIROSA",
                table: "Provider",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Lookup",
                table: "OutgoingCategory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlaceOfBirth",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Job",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsProvider",
                schema: "IIROSA",
                table: "Mother",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAlive",
                schema: "IIROSA",
                table: "Mother",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Village",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectType",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectStatus",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Planning",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ProgressNotes",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HousingType",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "GPSCoordinates",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DonorName",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStage",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CompletionPercentage",
                schema: "IIROSA",
                table: "HousingProject",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CompletionNotes",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BudgetCurrency",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "EGP",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "IIROSA",
                table: "HousingProject",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlaceOfBirth",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Job",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsProvider",
                schema: "IIROSA",
                table: "Father",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAlive",
                schema: "IIROSA",
                table: "Father",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                schema: "IIROSA",
                table: "ChildOutGoing",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                schema: "IIROSA",
                table: "ChildOutGoing",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OutgoingId1",
                schema: "IIROSA",
                table: "ChildOutGoing",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UploadedFileId1",
                schema: "IIROSA",
                table: "ChildOutGoing",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "Lookup",
                table: "ChequeBeneficiary",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                schema: "Lookup",
                table: "ChequeBeneficiary",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "Lookup",
                table: "ChequeBeneficiary",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryType",
                schema: "Lookup",
                table: "ChequeBeneficiary",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "Lookup",
                table: "ChequeBeneficiary",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                schema: "Lookup",
                table: "ChequeBeneficiary",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VoidReason",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VoidNotes",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentReason",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentDescription",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "EGP",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ClearanceNotes",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CheckStatus",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CheckNumber",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryType",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryPhone",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryName",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryIdNumber",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryEmail",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryAddress",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankReference",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankBranch",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AmountInWords",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Relative_IsAlive",
                schema: "IIROSA",
                table: "Relative",
                column: "IsAlive");

            migrationBuilder.CreateIndex(
                name: "IX_Relative_IsLivingWithFamily",
                schema: "IIROSA",
                table: "Relative",
                column: "IsLivingWithFamily");

            migrationBuilder.CreateIndex(
                name: "IX_Relative_NationalId",
                schema: "IIROSA",
                table: "Relative",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Relative_RelationshipType",
                schema: "IIROSA",
                table: "Relative",
                column: "RelationshipType");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_NationalId",
                schema: "IIROSA",
                table: "Provider",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Mother_IsAlive",
                schema: "IIROSA",
                table: "Mother",
                column: "IsAlive");

            migrationBuilder.CreateIndex(
                name: "IX_Mother_IsProvider",
                schema: "IIROSA",
                table: "Mother",
                column: "IsProvider");

            migrationBuilder.CreateIndex(
                name: "IX_Mother_NationalId",
                schema: "IIROSA",
                table: "Mother",
                column: "NationalId");

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
                name: "IX_HousingProject_StartDate",
                schema: "IIROSA",
                table: "HousingProject",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Father_IsAlive",
                schema: "IIROSA",
                table: "Father",
                column: "IsAlive");

            migrationBuilder.CreateIndex(
                name: "IX_Father_IsProvider",
                schema: "IIROSA",
                table: "Father",
                column: "IsProvider");

            migrationBuilder.CreateIndex(
                name: "IX_Father_NationalId",
                schema: "IIROSA",
                table: "Father",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildOutGoing_Date",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ChildOutGoing_OutgoingId1",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "OutgoingId1");

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

            migrationBuilder.CreateIndex(
                name: "IX_ChequeBeneficiary_BeneficiaryType",
                schema: "Lookup",
                table: "ChequeBeneficiary",
                column: "BeneficiaryType");

            migrationBuilder.CreateIndex(
                name: "IX_ChequeBeneficiary_FK_BankId",
                schema: "Lookup",
                table: "ChequeBeneficiary",
                column: "FK_BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Check_CheckDate",
                schema: "IIROSA",
                table: "Check",
                column: "CheckDate");

            migrationBuilder.CreateIndex(
                name: "IX_Check_CheckNumber",
                schema: "IIROSA",
                table: "Check",
                column: "CheckNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Check_CheckStatus",
                schema: "IIROSA",
                table: "Check",
                column: "CheckStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Check_DueDate",
                schema: "IIROSA",
                table: "Check",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Check_FK_BankId",
                schema: "IIROSA",
                table: "Check",
                column: "FK_BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Check_FK_ChequeBeneficiaryId",
                schema: "IIROSA",
                table: "Check",
                column: "FK_ChequeBeneficiaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Check_Bank_FK_BankId",
                schema: "IIROSA",
                table: "Check",
                column: "FK_BankId",
                principalSchema: "Lookup",
                principalTable: "Bank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Check_ChequeBeneficiary_FK_ChequeBeneficiaryId",
                schema: "IIROSA",
                table: "Check",
                column: "FK_ChequeBeneficiaryId",
                principalSchema: "Lookup",
                principalTable: "ChequeBeneficiary",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChequeBeneficiary_Bank_FK_BankId",
                schema: "Lookup",
                table: "ChequeBeneficiary",
                column: "FK_BankId",
                principalSchema: "Lookup",
                principalTable: "Bank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildOutGoing_Outgoing_OutgoingId1",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "OutgoingId1",
                principalSchema: "IIROSA",
                principalTable: "Outgoing",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildOutGoing_UploadedFile_UploadedFileId",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "UploadedFileId",
                principalTable: "UploadedFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildOutGoing_UploadedFile_UploadedFileId1",
                schema: "IIROSA",
                table: "ChildOutGoing",
                column: "UploadedFileId1",
                principalTable: "UploadedFile",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Father_EducationLevel_EducationLevelId",
                schema: "IIROSA",
                table: "Father",
                column: "EducationLevelId",
                principalSchema: "Lookup",
                principalTable: "EducationLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Father_Family_FamilyId",
                schema: "IIROSA",
                table: "Father",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Father_HealthStatus_HealthStatusId",
                schema: "IIROSA",
                table: "Father",
                column: "HealthStatusId",
                principalSchema: "Lookup",
                principalTable: "HealthStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Center_CenterId",
                schema: "IIROSA",
                table: "HousingProject",
                column: "CenterId",
                principalSchema: "Lookup",
                principalTable: "Center",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Charity_CharityId",
                schema: "IIROSA",
                table: "HousingProject",
                column: "CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Country_CountryId",
                schema: "IIROSA",
                table: "HousingProject",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Family_FamilyId",
                schema: "IIROSA",
                table: "HousingProject",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Region_RegionId",
                schema: "IIROSA",
                table: "HousingProject",
                column: "RegionId",
                principalSchema: "Lookup",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mother_EducationLevel_EducationLevelId",
                schema: "IIROSA",
                table: "Mother",
                column: "EducationLevelId",
                principalSchema: "Lookup",
                principalTable: "EducationLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mother_Family_FamilyId",
                schema: "IIROSA",
                table: "Mother",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mother_HealthStatus_HealthStatusId",
                schema: "IIROSA",
                table: "Mother",
                column: "HealthStatusId",
                principalSchema: "Lookup",
                principalTable: "HealthStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_Family_FamilyId",
                schema: "IIROSA",
                table: "Provider",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Relative_EducationLevel_EducationLevelId",
                schema: "IIROSA",
                table: "Relative",
                column: "EducationLevelId",
                principalSchema: "Lookup",
                principalTable: "EducationLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Relative_Family_FamilyId",
                schema: "IIROSA",
                table: "Relative",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Relative_HealthStatus_HealthStatusId",
                schema: "IIROSA",
                table: "Relative",
                column: "HealthStatusId",
                principalSchema: "Lookup",
                principalTable: "HealthStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Check_Bank_FK_BankId",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropForeignKey(
                name: "FK_Check_ChequeBeneficiary_FK_ChequeBeneficiaryId",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropForeignKey(
                name: "FK_ChequeBeneficiary_Bank_FK_BankId",
                schema: "Lookup",
                table: "ChequeBeneficiary");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildOutGoing_Outgoing_OutgoingId1",
                schema: "IIROSA",
                table: "ChildOutGoing");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildOutGoing_UploadedFile_UploadedFileId",
                schema: "IIROSA",
                table: "ChildOutGoing");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildOutGoing_UploadedFile_UploadedFileId1",
                schema: "IIROSA",
                table: "ChildOutGoing");

            migrationBuilder.DropForeignKey(
                name: "FK_Father_EducationLevel_EducationLevelId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropForeignKey(
                name: "FK_Father_Family_FamilyId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropForeignKey(
                name: "FK_Father_HealthStatus_HealthStatusId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Center_CenterId",
                schema: "IIROSA",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Charity_CharityId",
                schema: "IIROSA",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Country_CountryId",
                schema: "IIROSA",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Family_FamilyId",
                schema: "IIROSA",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Region_RegionId",
                schema: "IIROSA",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_Mother_EducationLevel_EducationLevelId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropForeignKey(
                name: "FK_Mother_Family_FamilyId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropForeignKey(
                name: "FK_Mother_HealthStatus_HealthStatusId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropForeignKey(
                name: "FK_Provider_Family_FamilyId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropForeignKey(
                name: "FK_Relative_EducationLevel_EducationLevelId",
                schema: "IIROSA",
                table: "Relative");

            migrationBuilder.DropForeignKey(
                name: "FK_Relative_Family_FamilyId",
                schema: "IIROSA",
                table: "Relative");

            migrationBuilder.DropForeignKey(
                name: "FK_Relative_HealthStatus_HealthStatusId",
                schema: "IIROSA",
                table: "Relative");

            migrationBuilder.DropIndex(
                name: "IX_Relative_IsAlive",
                schema: "IIROSA",
                table: "Relative");

            migrationBuilder.DropIndex(
                name: "IX_Relative_IsLivingWithFamily",
                schema: "IIROSA",
                table: "Relative");

            migrationBuilder.DropIndex(
                name: "IX_Relative_NationalId",
                schema: "IIROSA",
                table: "Relative");

            migrationBuilder.DropIndex(
                name: "IX_Relative_RelationshipType",
                schema: "IIROSA",
                table: "Relative");

            migrationBuilder.DropIndex(
                name: "IX_Provider_NationalId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.DropIndex(
                name: "IX_Mother_IsAlive",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropIndex(
                name: "IX_Mother_IsProvider",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropIndex(
                name: "IX_Mother_NationalId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropIndex(
                name: "IX_HousingProject_Name",
                schema: "IIROSA",
                table: "HousingProject");

            migrationBuilder.DropIndex(
                name: "IX_HousingProject_ProjectStatus",
                schema: "IIROSA",
                table: "HousingProject");

            migrationBuilder.DropIndex(
                name: "IX_HousingProject_StartDate",
                schema: "IIROSA",
                table: "HousingProject");

            migrationBuilder.DropIndex(
                name: "IX_Father_IsAlive",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropIndex(
                name: "IX_Father_IsProvider",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropIndex(
                name: "IX_Father_NationalId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropIndex(
                name: "IX_ChildOutGoing_Date",
                schema: "IIROSA",
                table: "ChildOutGoing");

            migrationBuilder.DropIndex(
                name: "IX_ChildOutGoing_OutgoingId1",
                schema: "IIROSA",
                table: "ChildOutGoing");

            migrationBuilder.DropIndex(
                name: "IX_ChildOutGoing_UploadedFileId1",
                schema: "IIROSA",
                table: "ChildOutGoing");

            migrationBuilder.DropIndex(
                name: "IX_ChildOutGoing_Year",
                schema: "IIROSA",
                table: "ChildOutGoing");

            migrationBuilder.DropIndex(
                name: "IX_ChequeBeneficiary_BeneficiaryType",
                schema: "Lookup",
                table: "ChequeBeneficiary");

            migrationBuilder.DropIndex(
                name: "IX_ChequeBeneficiary_FK_BankId",
                schema: "Lookup",
                table: "ChequeBeneficiary");

            migrationBuilder.DropIndex(
                name: "IX_Check_CheckDate",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropIndex(
                name: "IX_Check_CheckNumber",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropIndex(
                name: "IX_Check_CheckStatus",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropIndex(
                name: "IX_Check_DueDate",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropIndex(
                name: "IX_Check_FK_BankId",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropIndex(
                name: "IX_Check_FK_ChequeBeneficiaryId",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "OutgoingId1",
                schema: "IIROSA",
                table: "ChildOutGoing");

            migrationBuilder.DropColumn(
                name: "UploadedFileId1",
                schema: "IIROSA",
                table: "ChildOutGoing");

            migrationBuilder.RenameTable(
                name: "Relative",
                schema: "IIROSA",
                newName: "Relative");

            migrationBuilder.RenameTable(
                name: "Provider",
                schema: "IIROSA",
                newName: "Provider");

            migrationBuilder.RenameTable(
                name: "OutgoingCategory",
                schema: "Lookup",
                newName: "OutgoingCategory");

            migrationBuilder.RenameTable(
                name: "Mother",
                schema: "IIROSA",
                newName: "Mother");

            migrationBuilder.RenameTable(
                name: "HousingProject",
                schema: "IIROSA",
                newName: "HousingProject");

            migrationBuilder.RenameTable(
                name: "Father",
                schema: "IIROSA",
                newName: "Father");

            migrationBuilder.RenameTable(
                name: "ChildOutGoing",
                schema: "IIROSA",
                newName: "ChildOutGoing");

            migrationBuilder.RenameTable(
                name: "ChequeBeneficiary",
                schema: "Lookup",
                newName: "ChequeBeneficiary");

            migrationBuilder.RenameTable(
                name: "Check",
                schema: "IIROSA",
                newName: "Check");

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipType",
                table: "Relative",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "PlaceOfBirth",
                table: "Relative",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Relative",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Relative",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Relative",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Job",
                table: "Relative",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsLivingWithFamily",
                table: "Relative",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAlive",
                table: "Relative",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Relative",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Relative",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Relative",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipToFamily",
                table: "Provider",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Provider",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Provider",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Provider",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Job",
                table: "Provider",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Provider",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Provider",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "OutgoingCategory",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlaceOfBirth",
                table: "Mother",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Mother",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Mother",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Mother",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Job",
                table: "Mother",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsProvider",
                table: "Mother",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAlive",
                table: "Mother",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Mother",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Village",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectType",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectStatus",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Planning");

            migrationBuilder.AlterColumn<string>(
                name: "ProgressNotes",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "HousingType",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "GPSCoordinates",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DonorName",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStage",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CompletionPercentage",
                table: "HousingProject",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CompletionNotes",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BudgetCurrency",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "EGP");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "HousingProject",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlaceOfBirth",
                table: "Father",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Father",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Father",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Father",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Job",
                table: "Father",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsProvider",
                table: "Father",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAlive",
                table: "Father",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Father",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "ChildOutGoing",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "ChildOutGoing",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "ChequeBeneficiary",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "ChequeBeneficiary",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ChequeBeneficiary",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryType",
                table: "ChequeBeneficiary",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "ChequeBeneficiary",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "ChequeBeneficiary",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BankId",
                table: "ChequeBeneficiary",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VoidReason",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VoidNotes",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentReason",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentDescription",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Check",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "EGP");

            migrationBuilder.AlterColumn<string>(
                name: "ClearanceNotes",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CheckStatus",
                table: "Check",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "CheckNumber",
                table: "Check",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryType",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryPhone",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryName",
                table: "Check",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryIdNumber",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryEmail",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BeneficiaryAddress",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankReference",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankBranch",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AmountInWords",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "Check",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BankId",
                table: "Check",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChequeBeneficiaryId",
                table: "Check",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChequeBeneficiary_BankId",
                table: "ChequeBeneficiary",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Check_BankId",
                table: "Check",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Check_ChequeBeneficiaryId",
                table: "Check",
                column: "ChequeBeneficiaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Check_Bank_BankId",
                table: "Check",
                column: "BankId",
                principalSchema: "Lookup",
                principalTable: "Bank",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Check_ChequeBeneficiary_ChequeBeneficiaryId",
                table: "Check",
                column: "ChequeBeneficiaryId",
                principalTable: "ChequeBeneficiary",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChequeBeneficiary_Bank_BankId",
                table: "ChequeBeneficiary",
                column: "BankId",
                principalSchema: "Lookup",
                principalTable: "Bank",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildOutGoing_UploadedFile_UploadedFileId",
                table: "ChildOutGoing",
                column: "UploadedFileId",
                principalTable: "UploadedFile",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Father_EducationLevel_EducationLevelId",
                table: "Father",
                column: "EducationLevelId",
                principalSchema: "Lookup",
                principalTable: "EducationLevel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Father_Family_FamilyId",
                table: "Father",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Father_HealthStatus_HealthStatusId",
                table: "Father",
                column: "HealthStatusId",
                principalSchema: "Lookup",
                principalTable: "HealthStatus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Center_CenterId",
                table: "HousingProject",
                column: "CenterId",
                principalSchema: "Lookup",
                principalTable: "Center",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Charity_CharityId",
                table: "HousingProject",
                column: "CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Country_CountryId",
                table: "HousingProject",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Family_FamilyId",
                table: "HousingProject",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Region_RegionId",
                table: "HousingProject",
                column: "RegionId",
                principalSchema: "Lookup",
                principalTable: "Region",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Mother_EducationLevel_EducationLevelId",
                table: "Mother",
                column: "EducationLevelId",
                principalSchema: "Lookup",
                principalTable: "EducationLevel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Mother_Family_FamilyId",
                table: "Mother",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Mother_HealthStatus_HealthStatusId",
                table: "Mother",
                column: "HealthStatusId",
                principalSchema: "Lookup",
                principalTable: "HealthStatus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_Family_FamilyId",
                table: "Provider",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Relative_EducationLevel_EducationLevelId",
                table: "Relative",
                column: "EducationLevelId",
                principalSchema: "Lookup",
                principalTable: "EducationLevel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Relative_Family_FamilyId",
                table: "Relative",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Relative_HealthStatus_HealthStatusId",
                table: "Relative",
                column: "HealthStatusId",
                principalSchema: "Lookup",
                principalTable: "HealthStatus",
                principalColumn: "Id");
        }
    }
}
