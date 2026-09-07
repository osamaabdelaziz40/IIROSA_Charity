using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic11_GeneralCheques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "BankReference",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "CheckStatus",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "ClearanceDate",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "ClearanceNotes",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "PaymentDescription",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "PaymentReason",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "VoidDate",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "VoidNotes",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "VoidReason",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.RenameColumn(
                name: "RequiresApproval",
                schema: "IIROSA",
                table: "Check",
                newName: "IsReturned");

            migrationBuilder.RenameColumn(
                name: "FK_CheckImageId",
                schema: "IIROSA",
                table: "Check",
                newName: "FK_CharityId");

            migrationBuilder.AddColumn<string>(
                name: "ChequeType",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Individuals");

            migrationBuilder.AddColumn<bool>(
                name: "IsDamaged",
                schema: "IIROSA",
                table: "Check",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDispensed",
                schema: "IIROSA",
                table: "Check",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDone",
                schema: "IIROSA",
                table: "Check",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountWordsX",
                schema: "Lookup",
                table: "Bank",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountWordsY",
                schema: "Lookup",
                table: "Bank",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountX",
                schema: "Lookup",
                table: "Bank",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountY",
                schema: "Lookup",
                table: "Bank",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ChequeDateX",
                schema: "Lookup",
                table: "Bank",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ChequeDateY",
                schema: "Lookup",
                table: "Bank",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PayeeX",
                schema: "Lookup",
                table: "Bank",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PayeeY",
                schema: "Lookup",
                table: "Bank",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Check_CheckNumber_FK_BankId_FK_CharityId",
                schema: "IIROSA",
                table: "Check",
                columns: new[] { "CheckNumber", "FK_BankId", "FK_CharityId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Check_FK_CharityId",
                schema: "IIROSA",
                table: "Check",
                column: "FK_CharityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Check_Charity_FK_CharityId",
                schema: "IIROSA",
                table: "Check",
                column: "FK_CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Check_Charity_FK_CharityId",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropIndex(
                name: "IX_Check_CheckNumber_FK_BankId_FK_CharityId",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropIndex(
                name: "IX_Check_FK_CharityId",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "ChequeType",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "IsDamaged",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "IsDispensed",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "IsDone",
                schema: "IIROSA",
                table: "Check");

            migrationBuilder.DropColumn(
                name: "AmountWordsX",
                schema: "Lookup",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "AmountWordsY",
                schema: "Lookup",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "AmountX",
                schema: "Lookup",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "AmountY",
                schema: "Lookup",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "ChequeDateX",
                schema: "Lookup",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "ChequeDateY",
                schema: "Lookup",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "PayeeX",
                schema: "Lookup",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "PayeeY",
                schema: "Lookup",
                table: "Bank");

            migrationBuilder.RenameColumn(
                name: "IsReturned",
                schema: "IIROSA",
                table: "Check",
                newName: "RequiresApproval");

            migrationBuilder.RenameColumn(
                name: "FK_CharityId",
                schema: "IIROSA",
                table: "Check",
                newName: "FK_CheckImageId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                schema: "IIROSA",
                table: "Check",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                schema: "IIROSA",
                table: "Check",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankReference",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckStatus",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClearanceDate",
                schema: "IIROSA",
                table: "Check",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClearanceNotes",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                schema: "IIROSA",
                table: "Check",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssueDate",
                schema: "IIROSA",
                table: "Check",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentDescription",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReason",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidDate",
                schema: "IIROSA",
                table: "Check",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidNotes",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                schema: "IIROSA",
                table: "Check",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

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
        }
    }
}
