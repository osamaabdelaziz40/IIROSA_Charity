using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCharityAndCountryToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CharityId",
                schema: "identity",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                schema: "identity",
                table: "Users",
                type: "int",
                nullable: true);

            // This column exists to answer "which users belong to this charity", which every
            // tenancy-related admin screen asks. Without an index that is a table scan.
            migrationBuilder.CreateIndex(
                name: "IX_Users_CharityId",
                schema: "identity",
                table: "Users",
                column: "CharityId");

            // Backfill existing accounts from the link the Charity row already holds.
            //
            // Without this, every account that predates these columns has no tenancy claim. The
            // application layer now fails closed on an unscopeable caller, so those users would
            // silently see nothing rather than everything — safe, but broken.
            //
            // Charity.UserId is the authoritative link and stores the user's Guid as text, so the
            // join casts rather than comparing types directly. Guarded on the table existing
            // because on a fresh database the identity migrations may run before the IIROSA
            // schema is created, and only touching rows that are still NULL keeps it re-runnable.
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[IIROSA].[Charity]', N'U') IS NOT NULL
                BEGIN
                    UPDATE u
                    SET u.CharityId = c.Id,
                        u.CountryId  = c.CountryId
                    FROM [identity].[Users] AS u
                    INNER JOIN [IIROSA].[Charity] AS c
                        ON LOWER(CONVERT(nvarchar(36), u.Id)) = LOWER(c.UserId)
                    WHERE u.CharityId IS NULL
                      AND c.IsDeleted = 0
                      -- Never bind a head-office account to a charity. Charity.UserId is not
                      -- unique and legacy data may point at an admin; pinning an admin to one
                      -- charity would silently shrink their register to a single row.
                      AND NOT EXISTS (
                          SELECT 1
                          FROM [identity].[UserRoles] ur
                          INNER JOIN [identity].[Roles] r ON r.Id = ur.RoleId
                          WHERE ur.UserId = u.Id
                            AND r.Name IN (N'SuperAdmin', N'Admin')
                      )
                      -- A user reachable from two Charity rows would be assigned arbitrarily by
                      -- UPDATE ... FROM. Leave those for manual resolution rather than guessing.
                      AND (
                          SELECT COUNT(*)
                          FROM [IIROSA].[Charity] c2
                          WHERE LOWER(c2.UserId) = LOWER(CONVERT(nvarchar(36), u.Id))
                            AND c2.IsDeleted = 0
                      ) = 1;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CharityId",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CharityId",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CountryId",
                schema: "identity",
                table: "Users");
        }
    }
}
