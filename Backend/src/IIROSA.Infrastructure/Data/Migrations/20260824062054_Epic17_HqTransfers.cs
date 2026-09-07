using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic17_HqTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserRoles_ApplicationUser_UserId",
                table: "ApplicationUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Mission_ApplicationUser_FK_UserId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.CreateTable(
                name: "HqTransfer",
                schema: "IIROSA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_CountryId = table.Column<int>(type: "int", nullable: false),
                    FK_DepartmentId = table.Column<int>(type: "int", nullable: false),
                    OperationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FinYear = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentNumber = table.Column<int>(type: "int", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountOfPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statement = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BeneficiariesNumber = table.Column<int>(type: "int", nullable: false),
                    TransactionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HqTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HqTransfer_Country_FK_CountryId",
                        column: x => x.FK_CountryId,
                        principalSchema: "Lookup",
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HqTransfer_Department_FK_DepartmentId",
                        column: x => x.FK_DepartmentId,
                        principalSchema: "Lookup",
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HqTransfer_FK_CountryId",
                schema: "IIROSA",
                table: "HqTransfer",
                column: "FK_CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_HqTransfer_FK_DepartmentId",
                schema: "IIROSA",
                table: "HqTransfer",
                column: "FK_DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HqTransfer_OperationNumber",
                schema: "IIROSA",
                table: "HqTransfer",
                column: "OperationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_HqTransfer_TransactionDate",
                schema: "IIROSA",
                table: "HqTransfer",
                column: "TransactionDate");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserRoles_Users_UserId",
                table: "ApplicationUserRoles",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_Users_FK_UserId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserRoles_Users_UserId",
                table: "ApplicationUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Mission_Users_FK_UserId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropTable(
                name: "HqTransfer",
                schema: "IIROSA");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserRoles_ApplicationUser_UserId",
                table: "ApplicationUserRoles",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_ApplicationUser_FK_UserId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
