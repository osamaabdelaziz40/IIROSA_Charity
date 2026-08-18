using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class AddLicenseNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "common",
                table: "NotificationTemplate",
                columns: new[] { "Id", "BodyAr", "BodyEn", "CreatedBy", "CreatedOn", "IsActive", "Name", "NotificationTypeId", "SubjectAr", "SubjectEn", "UpdatedBy", "UpdatedOn" },
                values: new object[] { 5, "عزيزي {CompanyName}،\r\n\r\nنود إبلاغكم بأن رخصتكم التي تحمل الرقم {LicenseNumber} قد تم تغيير حالتها إلى {LicenseStatus} للأسباب التالية:\r\n{ActionReason}\r\n\r\nرقم الترخيص {LicenseNumber}\r\nحالة الترخيص {LicenseStatus}\r\nمع تحيات، \r\nهيئة الحكومة الرقمية\r\n", "Dear {CompanyName},\r\nWe would like to inform you that the status of your License with the Number {LicenseNumber} was changed to {LicenseStatus} for the following reason:\r\n{ActionReason}\r\n\r\nLicense Number {LicenseNumber}\r\nLicense Status {LicenseStatus}.\r\nBest regards,\r\nDigital Government Authority", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "LicenseNotification", 1, ": إشعار تغيير حالة الترخيص رقم {LicenseNumber} ", "License Status change notification for License Number : {LicenseNumber} ", null, null });

            migrationBuilder.InsertData(
                schema: "common",
                table: "NotificationTemplate",
                columns: new[] { "Id", "BodyAr", "BodyEn", "CreatedBy", "CreatedOn", "IsActive", "Name", "NotificationTypeId", "SubjectAr", "SubjectEn", "UpdatedBy", "UpdatedOn" },
                values: new object[] { 6, "عزيزي  فريق تراخيص، \r\nنفيدكم علما أنه قد تغيير حالة الترخيص {CompanyName} {LicenseType} {LicenseNumber} بنجاح.\r\nاسم الشركة {CompanyName} \r\nنوع الترخيص {LicenseType}\r\nرقم الترخيص {LicenseNumber}\r\nحالة الترخيص {LicenseStatus}\r\nمع تحيات، \r\nهيئة الحكومة الرقمية", "Dear Licenses Team,\r\nKindly note that the license status for {CompanyName} {LicenseType} {LicenseNumber} has been change to {LicenseStatus} successfully.\r\nCompany Name {CompanyName} \r\nLicense Type {LicenseType}\r\nLicense Number {LicenseNumber}\r\nLicense Status {LicenseStatus}\r\nBest regards,\r\nDigital Government Authority", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "LicenseTeamNotification", 1, "تم تغيير حالة الترخيص {CompanyName} {LicenseType} {LicenseNumber} بنجاح", "License status for {CompanyName} {LicenseType} {LicenseNumber} was successfully changed", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "common",
                table: "NotificationTemplate",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "NotificationTemplate",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
