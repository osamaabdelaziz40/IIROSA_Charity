using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class addrelationuserRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoleId1",
                schema: "identity",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                schema: "identity",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId1",
                schema: "identity",
                table: "UserRoles",
                column: "RoleId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId1",
                schema: "identity",
                table: "UserRoles",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId1",
                schema: "identity",
                table: "UserRoles",
                column: "RoleId1",
                principalSchema: "identity",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId1",
                schema: "identity",
                table: "UserRoles",
                column: "UserId1",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId1",
                schema: "identity",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId1",
                schema: "identity",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_RoleId1",
                schema: "identity",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId1",
                schema: "identity",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                schema: "identity",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "UserId1",
                schema: "identity",
                table: "UserRoles");
        }
    }
}
