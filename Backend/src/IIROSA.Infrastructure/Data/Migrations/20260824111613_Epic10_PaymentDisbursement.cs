using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic10_PaymentDisbursement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrphanPayment_BatchNo",
                schema: "IIROSA",
                table: "OrphanPayment");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BenificiaryName",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChiqueNum",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExchangeStatus",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGotIt",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrinted",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStopped",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Printdate",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrintedOn",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedOn",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StoppedOn",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferNo",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                schema: "IIROSA",
                table: "OrphanPayment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPayment_BatchNo",
                schema: "IIROSA",
                table: "OrphanPayment",
                column: "BatchNo",
                unique: true,
                filter: "[BatchNo] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrphanPayment_BatchNo",
                schema: "IIROSA",
                table: "OrphanPayment");

            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "BenificiaryName",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "ChiqueNum",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "ExchangeStatus",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "IsGotIt",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "IsPrinted",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "IsStopped",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "Printdate",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "PrintedOn",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "ReceivedOn",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "StoppedOn",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "TransferNo",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                schema: "IIROSA",
                table: "OrphanPayment");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanPayment_BatchNo",
                schema: "IIROSA",
                table: "OrphanPayment",
                column: "BatchNo",
                unique: true,
                filter: "\"BatchNo\" IS NOT NULL AND \"IsDeleted\" = false");
        }
    }
}
