using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class AddHostingRequestTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "common",
                table: "NotificationTemplate",
                columns: new[] { "Id", "BodyAr", "BodyEn", "CreatedBy", "CreatedOn", "IsActive", "Name", "NotificationTypeId", "SubjectAr", "SubjectEn", "UpdatedBy", "UpdatedOn" },
                values: new object[] { 2, "<html dir=\"rtl\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:right;padding:0 10px;\">\r\n								  <p>\r\nلقد تمت الموافقة على طلبكم الخاص بطلب الضيافة\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", "<html dir=\"ltr\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:left;padding:0 10px;\">\r\n								  <p>\r\nYour request for hospitality has been approved.\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "AcceptHostingRequest", 1, "قبول طلب الضيافة", "Accept Hosting Request", null, null });

            migrationBuilder.InsertData(
                schema: "common",
                table: "NotificationTemplate",
                columns: new[] { "Id", "BodyAr", "BodyEn", "CreatedBy", "CreatedOn", "IsActive", "Name", "NotificationTypeId", "SubjectAr", "SubjectEn", "UpdatedBy", "UpdatedOn" },
                values: new object[] { 3, "<html dir=\"rtl\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:right;padding:0 10px;\">\r\n								  <p>\r\nلقد تم رفض طلبكم الخاص بطلب الضيافة\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", "<html dir=\"ltr\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:left;padding:0 10px;\">\r\n								  <p>\r\nYour request for hospitality has been rejected.\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "RejectHostingRequest", 1, "رفض طلب الضيافة", "Reject Hosting Request", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "common",
                table: "NotificationTemplate",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "common",
                table: "NotificationTemplate",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
