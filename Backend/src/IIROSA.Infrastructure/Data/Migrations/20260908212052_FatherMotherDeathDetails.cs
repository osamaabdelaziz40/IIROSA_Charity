using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FatherMotherDeathDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeathCertificateAttachmentId",
                schema: "IIROSA",
                table: "Mother",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeathReasonId",
                schema: "IIROSA",
                table: "Mother",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyName",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MezaCard",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MezaCardExpirationDate",
                schema: "IIROSA",
                table: "Mother",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NationalityCountryId",
                schema: "IIROSA",
                table: "Mother",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondName",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdName",
                schema: "IIROSA",
                table: "Mother",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeathCertificateAttachmentId",
                schema: "IIROSA",
                table: "Father",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeathReasonId",
                schema: "IIROSA",
                table: "Father",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyName",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MezaCard",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MezaCardExpirationDate",
                schema: "IIROSA",
                table: "Father",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NationalityCountryId",
                schema: "IIROSA",
                table: "Father",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondName",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdName",
                schema: "IIROSA",
                table: "Father",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeathReason",
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
                    table.PrimaryKey("PK_DeathReason", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mother_DeathReasonId",
                schema: "IIROSA",
                table: "Mother",
                column: "DeathReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Mother_NationalityCountryId",
                schema: "IIROSA",
                table: "Mother",
                column: "NationalityCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Father_DeathReasonId",
                schema: "IIROSA",
                table: "Father",
                column: "DeathReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Father_NationalityCountryId",
                schema: "IIROSA",
                table: "Father",
                column: "NationalityCountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Father_Country_NationalityCountryId",
                schema: "IIROSA",
                table: "Father",
                column: "NationalityCountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Father_DeathReason_DeathReasonId",
                schema: "IIROSA",
                table: "Father",
                column: "DeathReasonId",
                principalSchema: "Lookup",
                principalTable: "DeathReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mother_Country_NationalityCountryId",
                schema: "IIROSA",
                table: "Mother",
                column: "NationalityCountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mother_DeathReason_DeathReasonId",
                schema: "IIROSA",
                table: "Mother",
                column: "DeathReasonId",
                principalSchema: "Lookup",
                principalTable: "DeathReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Father_Country_NationalityCountryId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropForeignKey(
                name: "FK_Father_DeathReason_DeathReasonId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropForeignKey(
                name: "FK_Mother_Country_NationalityCountryId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropForeignKey(
                name: "FK_Mother_DeathReason_DeathReasonId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropTable(
                name: "DeathReason",
                schema: "Lookup");

            migrationBuilder.DropIndex(
                name: "IX_Mother_DeathReasonId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropIndex(
                name: "IX_Mother_NationalityCountryId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropIndex(
                name: "IX_Father_DeathReasonId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropIndex(
                name: "IX_Father_NationalityCountryId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropColumn(
                name: "DeathCertificateAttachmentId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropColumn(
                name: "DeathReasonId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropColumn(
                name: "FamilyName",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropColumn(
                name: "MezaCard",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropColumn(
                name: "MezaCardExpirationDate",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropColumn(
                name: "NationalityCountryId",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropColumn(
                name: "SecondName",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropColumn(
                name: "ThirdName",
                schema: "IIROSA",
                table: "Mother");

            migrationBuilder.DropColumn(
                name: "DeathCertificateAttachmentId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropColumn(
                name: "DeathReasonId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropColumn(
                name: "FamilyName",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropColumn(
                name: "MezaCard",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropColumn(
                name: "MezaCardExpirationDate",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropColumn(
                name: "NationalityCountryId",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropColumn(
                name: "SecondName",
                schema: "IIROSA",
                table: "Father");

            migrationBuilder.DropColumn(
                name: "ThirdName",
                schema: "IIROSA",
                table: "Father");
        }
    }
}
