using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic19_CountryNidRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NationalIdLength",
                schema: "Lookup",
                table: "Country",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdPattern",
                schema: "Lookup",
                table: "Country",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalIdLength",
                schema: "Lookup",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "NationalIdPattern",
                schema: "Lookup",
                table: "Country");
        }
    }
}
