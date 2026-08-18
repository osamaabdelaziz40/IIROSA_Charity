using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class AddCommonEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "common",
                table: "NotificationTemplate",
                columns: new[] { "Id", "BodyAr", "BodyEn", "CreatedBy", "CreatedOn", "IsActive", "Name", "NotificationTypeId", "SubjectAr", "SubjectEn", "UpdatedBy", "UpdatedOn" },
                values: new object[] { 4, "<!DOCTYPE HTML\r\n	PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" dir=\"rtl\">\r\n\r\n<head>\r\n	<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\r\n	<title>هيئة الحكومية الرقمية</title>\r\n</head>\r\n\r\n<body dir=\"rtl\" style=\"padding:0px\">\r\n	\r\n	<table width=\"1024\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n		<tr>\r\n			<td class=\"email_conts\"\r\n				style=\"padding:10px 20px;  font:11px Segoe UI,tahoma;  line-height:20px;  color:#404040;  text-align:right; border-top:none\">\r\n				{Body}\r\n				</br>\r\n				<h3>وتقبلوا فائق الاحترام</h3>\r\n			</td>\r\n		</tr>\r\n		<tr>\r\n			<td class=\"email_footer\" width=\"1024\"\r\n				style=\"padding:10px 0;  background-color:#202020;  color:#fff;  font:11px Segoe UI,tahoma;  text-align:center\">\r\n				لمقترحاتكم وملاحظاتكم راسلونا على البريد الإلكتروني: <a href=\"#\"\r\n					style=\"color:#fff;  font:11px Segoe UI,tahoma\">support@pgd.gov.sa</a>\r\n			</td>\r\n		</tr>\r\n	</table>\r\n</body>\r\n\r\n</html>", "<!DOCTYPE HTML\r\n	PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" dir=\"rtl\">\r\n\r\n<head>\r\n	<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\r\n	<title>DGA</title>\r\n</head>\r\n\r\n<body dir=\"ltr\" style=\"padding:0px\">\r\n	<table width=\"1024\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n		<tr>\r\n			<td class=\"email_conts\"\r\n				style=\"padding:10px 20px;  font:11px Segoe UI,tahoma;  line-height:20px;  color:#404040;  text-align:right; border-top:none\">\r\n				{Body}\r\n				</br>\r\n				<h3 style=\"text-align:justify;font-weight:normal;\">Yours Sincerely </h3>\r\n			</td>\r\n		</tr>\r\n		<tr>\r\n			<td class=\"email_footer\" width=\"1024\"\r\n				style=\"padding:10px 0;  background-color:#202020;  color:#fff;  font:11px Segoe UI,tahoma;  text-align:center\">\r\n				For your suggestions and comments, send us an e-mail: <a href=\"#\"\r\n					style=\"color:#fff;  font:11px Segoe UI,tahoma\">support@pgd.gov.sa</a>\r\n			</td>\r\n		</tr>\r\n	</table>\r\n</body>\r\n\r\n</html>", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "CommonEmailStructure", 1, "هيئة الحكومية الرقمية", "DGA", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "common",
                table: "NotificationTemplate",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
