using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic15_Missions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FK_CharityId",
                schema: "IIROSA",
                table: "Mission",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FK_MissionInterviewTypeId",
                schema: "IIROSA",
                table: "Mission",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MissionInterviewType",
                schema: "Lookup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_MissionInterviewType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mission_FK_CharityId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_Mission_FK_MissionInterviewTypeId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_MissionInterviewTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionInterviewType_TypeCode",
                schema: "Lookup",
                table: "MissionInterviewType",
                column: "TypeCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_Charity_FK_CharityId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_MissionInterviewType_FK_MissionInterviewTypeId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_MissionInterviewTypeId",
                principalSchema: "Lookup",
                principalTable: "MissionInterviewType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mission_Charity_FK_CharityId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropForeignKey(
                name: "FK_Mission_MissionInterviewType_FK_MissionInterviewTypeId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropTable(
                name: "MissionInterviewType",
                schema: "Lookup");

            migrationBuilder.DropIndex(
                name: "IX_Mission_FK_CharityId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropIndex(
                name: "IX_Mission_FK_MissionInterviewTypeId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropColumn(
                name: "FK_CharityId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropColumn(
                name: "FK_MissionInterviewTypeId",
                schema: "IIROSA",
                table: "Mission");
        }
    }
}
