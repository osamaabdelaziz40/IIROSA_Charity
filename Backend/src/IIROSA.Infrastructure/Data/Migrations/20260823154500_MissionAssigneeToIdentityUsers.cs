using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Repoints IIROSA.Mission.FK_UserId at the live users table.
    ///
    /// The FK was created against dbo.ApplicationUser — an empty duplicate of the users table
    /// that the application context produced by convention before ApplicationUser was remapped
    /// to identity.Users (see ApplicationDbContext.OnModelCreating). Referencing an empty table
    /// meant no real assignee could ever satisfy the constraint; any mission created with an
    /// assigned user would violate it. The runtime model now joins identity.Users, and this
    /// migration aligns the physical constraint with it.
    ///
    /// Hand-written (no Designer): the [DbContext]/[Migration] attributes below are what the
    /// designer partial class normally supplies, which is all Database.Migrate() needs to
    /// discover and apply it.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260823154500_MissionAssigneeToIdentityUsers")]
    public partial class MissionAssigneeToIdentityUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mission_ApplicationUser_FK_UserId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_ApplicationUser_FK_UserId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mission_ApplicationUser_FK_UserId",
                schema: "IIROSA",
                table: "Mission");

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
