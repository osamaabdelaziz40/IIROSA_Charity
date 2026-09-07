using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic16_SerialUniqueBackstop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Review P6 — the serial sequence's storage backstop: the per-charity + year
            // composite becomes a unique filtered index (soft-deleted rows leave the index
            // so their serial is reusable; legacy NULL-year rows stay outside it).
            migrationBuilder.DropIndex(
                name: "IX_Outgoing_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Outgoing");

            migrationBuilder.DropIndex(
                name: "IX_Incoming_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.CreateIndex(
                name: "IX_Outgoing_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Outgoing",
                columns: new[] { "FK_CharityId", "Year", "Serial" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [Year] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Incoming",
                columns: new[] { "FK_CharityId", "Year", "Serial" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [Year] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Outgoing_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Outgoing");

            migrationBuilder.DropIndex(
                name: "IX_Incoming_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.CreateIndex(
                name: "IX_Outgoing_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Outgoing",
                columns: new[] { "FK_CharityId", "Year", "Serial" });

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_FK_CharityId_Year_Serial",
                schema: "IIROSA",
                table: "Incoming",
                columns: new[] { "FK_CharityId", "Year", "Serial" });
        }
    }
}
