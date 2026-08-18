using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class EditNotificationLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyProfileId",
                schema: "common",
                table: "NotificationsLog",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeProfileId",
                schema: "common",
                table: "NotificationsLog",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                schema: "common",
                table: "NotificationsLog",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NotificationTypeId",
                schema: "common",
                table: "NotificationsLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadDate",
                schema: "common",
                table: "NotificationsLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "common",
                table: "NotificationTemplate",
                keyColumn: "Id",
                keyValue: 5,
                column: "BodyAr",
                value: "عزيزي  فريق تراخيص، \r\nنفيدكم علما أنه قد تغيير حالة الترخيص {CompanyName} {LicenseType} {LicenseNumber} بنجاح.\r\nاسم الشركة {CompanyName} \r\nنوع الترخيص {LicenseType}\r\nرقم الترخيص {LicenseNumber}\r\nحالة الترخيص {LicenseStatus}\r\nمع تحيات، \r\nهيئة الحكومة الرقمية {CompanyName}،   \r\n نود إبلاغكم بأن رخصتكم التي تحمل الرقم {LicenseNumber} \r\n قد تم تغيير حالتها إلى {LicenseStatus} \r\n للأسباب التالية:  {ActionReason} \r\n رقم الترخيص {LicenseNumber} \r\n حالة الترخيص {LicenseStatus}  \r\n مع تحيات،   هيئة الحكومة الرقمية  ");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationsLog_NotificationTypeId",
                schema: "common",
                table: "NotificationsLog",
                column: "NotificationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationsLog_NotificationType_NotificationTypeId",
                schema: "common",
                table: "NotificationsLog",
                column: "NotificationTypeId",
                principalSchema: "common",
                principalTable: "NotificationType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationsLog_NotificationType_NotificationTypeId",
                schema: "common",
                table: "NotificationsLog");

            migrationBuilder.DropIndex(
                name: "IX_NotificationsLog_NotificationTypeId",
                schema: "common",
                table: "NotificationsLog");

            migrationBuilder.DropColumn(
                name: "CompanyProfileId",
                schema: "common",
                table: "NotificationsLog");

            migrationBuilder.DropColumn(
                name: "EmployeeProfileId",
                schema: "common",
                table: "NotificationsLog");

            migrationBuilder.DropColumn(
                name: "IsRead",
                schema: "common",
                table: "NotificationsLog");

            migrationBuilder.DropColumn(
                name: "NotificationTypeId",
                schema: "common",
                table: "NotificationsLog");

            migrationBuilder.DropColumn(
                name: "ReadDate",
                schema: "common",
                table: "NotificationsLog");

            migrationBuilder.UpdateData(
                schema: "common",
                table: "NotificationTemplate",
                keyColumn: "Id",
                keyValue: 5,
                column: "BodyAr",
                value: "عزيزي {CompanyName}،\r\n\r\nنود إبلاغكم بأن رخصتكم التي تحمل الرقم {LicenseNumber} قد تم تغيير حالتها إلى {LicenseStatus} للأسباب التالية:\r\n{ActionReason}\r\n\r\nرقم الترخيص {LicenseNumber}\r\nحالة الترخيص {LicenseStatus}\r\nمع تحيات، \r\nهيئة الحكومة الرقمية\r\n");
        }
    }
}
