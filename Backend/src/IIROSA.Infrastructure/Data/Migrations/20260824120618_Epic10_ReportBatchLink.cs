using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic10_ReportBatchLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrphanPaymentId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicOrphanReport_OrphanPaymentId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                column: "OrphanPaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicOrphanReport_OrphanPayment_OrphanPaymentId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                column: "OrphanPaymentId",
                principalSchema: "IIROSA",
                principalTable: "OrphanPayment",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicOrphanReport_OrphanPayment_OrphanPaymentId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropIndex(
                name: "IX_PeriodicOrphanReport_OrphanPaymentId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropColumn(
                name: "OrphanPaymentId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");
        }
    }
}
