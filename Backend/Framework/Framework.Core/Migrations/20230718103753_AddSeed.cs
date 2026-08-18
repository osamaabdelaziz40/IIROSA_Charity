using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class AddSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "common",
                table: "NotificationType",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "NameAr", "NameEn", "UpdatedBy", "UpdatedOn" },
                values: new object[] { 1, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "البريد الالكترونى", "Email", null, null });

            migrationBuilder.InsertData(
                schema: "common",
                table: "SystemSetting",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "GroupName", "IsActive", "IsSecure", "IsSticky", "Name", "UpdatedBy", "UpdatedOn", "Value", "ValueType" },
                values: new object[] { 1, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Integration", true, false, false, "CamundaUrl", null, null, "http://192.168.1.136:8080/", "string" });

            migrationBuilder.InsertData(
                schema: "common",
                table: "NotificationTemplate",
                columns: new[] { "Id", "BodyAr", "BodyEn", "CreatedBy", "CreatedOn", "IsActive", "Name", "NotificationTypeId", "SubjectAr", "SubjectEn", "UpdatedBy", "UpdatedOn" },
                values: new object[] { 1, " <html dir=\"rtl\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:right;padding:0 10px;\">\r\n								  <p>\r\nتم اصدار تصريح دخول للزائر {VisitorName}<br/>\r\n صاحب هوية رقم {VisitorId} <br/>\r\nتاريخ الزيارة: {VisitDate} <br/>\r\nوقت الزيارة: {VisitTime}   <br/>\r\nزيارة للموظف: {EmployeeName} <br/>\r\n\r\n\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", " <html dir=\"ltr\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:left;padding:0 10px;\">\r\n								  <p>\r\nEntry permit has been issued to vistor{VisitorName} <br/>\r\nWith Identity {VisitorId} <br/>\r\nVisit Date: {VisitDate}	<br/>\r\nVisit time: {VisitTime} <br/>\r\nVisiting employee: {EmployeeName} <br/>\r\n\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "AccessRequest", 1, "تصريح دخول", "Access Request", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "common",
                table: "NotificationTemplate",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "SystemSetting",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "NotificationType",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
