using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class removeUnusedColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Roles_ReportingToRoleId",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_ReportingToRoleId",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CommercialNumber",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdentityTypeId",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsExternalUser",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NationalId",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VerificationNumber",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ReportingToRoleId",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "RoleGroup",
                schema: "identity",
                table: "Roles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommercialNumber",
                schema: "identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdentityTypeId",
                schema: "identity",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalUser",
                schema: "identity",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                schema: "identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationNumber",
                schema: "identity",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Code",
                schema: "identity",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                schema: "identity",
                table: "Roles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                schema: "identity",
                table: "Roles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                schema: "identity",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReportingToRoleId",
                schema: "identity",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleGroup",
                schema: "identity",
                table: "Roles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ReportingToRoleId",
                schema: "identity",
                table: "Roles",
                column: "ReportingToRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Roles_ReportingToRoleId",
                schema: "identity",
                table: "Roles",
                column: "ReportingToRoleId",
                principalSchema: "identity",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}