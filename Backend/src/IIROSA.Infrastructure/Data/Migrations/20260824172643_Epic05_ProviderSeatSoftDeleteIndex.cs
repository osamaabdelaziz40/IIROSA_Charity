using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic05_ProviderSeatSoftDeleteIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Provider_FamilyId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_FamilyId",
                schema: "IIROSA",
                table: "Provider",
                column: "FamilyId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Provider_FamilyId",
                schema: "IIROSA",
                table: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_FamilyId",
                schema: "IIROSA",
                table: "Provider",
                column: "FamilyId",
                unique: true,
                filter: "[FamilyId] IS NOT NULL");
        }
    }
}
