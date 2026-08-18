using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Framework.Core.Migrations
{
    /// <summary>
    /// Migration to add AuditLog table for comprehensive audit logging
    /// UC-17: Audit Logging Module
    /// </summary>
    public partial class AuditLogging_AddAuditLogTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLog",
                schema: "common",
                columns: table => new
                {
                    AuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    EntityType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Operation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FieldChanges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalContext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.AuditLogId);
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityType",
                schema: "common",
                table: "AuditLog",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityId",
                schema: "common",
                table: "AuditLog",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Operation",
                schema: "common",
                table: "AuditLog",
                column: "Operation");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId",
                schema: "common",
                table: "AuditLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Timestamp",
                schema: "common",
                table: "AuditLog",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CorrelationId",
                schema: "common",
                table: "AuditLog",
                column: "CorrelationId");

            // Composite index for entity history queries
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Entity_EntityId_Timestamp",
                schema: "common",
                table: "AuditLog",
                columns: new[] { "EntityType", "EntityId", "Timestamp" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog",
                schema: "common");
        }
    }
}
