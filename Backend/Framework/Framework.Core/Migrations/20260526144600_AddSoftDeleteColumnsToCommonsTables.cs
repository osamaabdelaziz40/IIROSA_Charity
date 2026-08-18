using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteColumnsToCommonsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add DeletedBy column to Attachment table
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "common",
                table: "Attachment",
                type: "nvarchar(max)",
                nullable: true);

            // Add DeletedOn column to Attachment table
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "common",
                table: "Attachment",
                type: "datetime2",
                nullable: true);

            // Add IsDeleted column to Attachment table
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "common",
                table: "Attachment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Add DeletedBy column to AttachmentType table
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "common",
                table: "AttachmentType",
                type: "nvarchar(max)",
                nullable: true);

            // Add DeletedOn column to AttachmentType table
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "common",
                table: "AttachmentType",
                type: "datetime2",
                nullable: true);

            // Add IsDeleted column to AttachmentType table
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "common",
                table: "AttachmentType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Add DeletedBy column to NotificationType table
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "common",
                table: "NotificationType",
                type: "nvarchar(max)",
                nullable: true);

            // Add DeletedOn column to NotificationType table
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "common",
                table: "NotificationType",
                type: "datetime2",
                nullable: true);

            // Add IsDeleted column to NotificationType table
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "common",
                table: "NotificationType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Add DeletedBy column to NotificationTemplate table
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "common",
                table: "NotificationTemplate",
                type: "nvarchar(max)",
                nullable: true);

            // Add DeletedOn column to NotificationTemplate table
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "common",
                table: "NotificationTemplate",
                type: "datetime2",
                nullable: true);

            // Add IsDeleted column to NotificationTemplate table
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "common",
                table: "NotificationTemplate",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback NotificationTemplate
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "common",
                table: "NotificationTemplate");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "common",
                table: "NotificationTemplate");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "common",
                table: "NotificationTemplate");

            // Rollback NotificationType
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "common",
                table: "NotificationType");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "common",
                table: "NotificationType");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "common",
                table: "NotificationType");

            // Rollback AttachmentType
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "common",
                table: "AttachmentType");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "common",
                table: "AttachmentType");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "common",
                table: "AttachmentType");

            // Rollback Attachment
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "common",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "common",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "common",
                table: "Attachment");
        }
    }
}
