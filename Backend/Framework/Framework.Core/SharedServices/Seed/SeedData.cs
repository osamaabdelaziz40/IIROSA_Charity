using Framework.Core.SharedServices.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Framework.Core.SharedServices.Seed
{
    public static class SeedData
    {
        public static void AddSeedData(this ModelBuilder modelBuilder)
        {
            SystemSetting(modelBuilder);
            NotificationTemplate(modelBuilder);
        }
        private static void SystemSetting(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemSetting>().HasData(new List<SystemSetting> {
                new SystemSetting(1, "CamundaUrl", "string", "http://localhost:8080/", "Integration", false, false, true),
                new SystemSetting(2, "SmtpServer", "string", "192.168.1.166", "Notifications", false, false, true),
                new SystemSetting(3, "SmtpUserName", "string", "", "Notifications", false, false, true),
                new SystemSetting(4, "SmtpPassword", "string", "", "Notifications", false, false, true),
                new SystemSetting(5, "SmtpEnableSSL", "bool", "False", "Notifications", false, false, true),
                new SystemSetting(6, "SmtpPort", "int", "25", "Notifications", false, false, true),
                new SystemSetting(7, "EmailFromName", "string", "DGA", "Notifications", false, false, true),
                new SystemSetting(8, "EmailFromAddress", "string", "info@suredemos.com", "Notifications", false, false, true),
                new SystemSetting(9, "AttachmentsPath", "string", "Attachments", "Integration", false, false, true),
                new SystemSetting(10, "AttachmentsServer", "string", "C:\\inetpub\\wwwroot\\DGA-Internal", "Integration", false, false, true)
            });

        }
        private static void NotificationTemplate(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationType>().HasData(new List<NotificationType> {
                new NotificationType(1, "البريد الالكترونى", "Email")
            });

            modelBuilder.Entity<NotificationTemplate>().HasData(new List<NotificationTemplate> {
                new NotificationTemplate(1, "AccessRequest", "تصريح دخول", "Access Request" ," <html dir=\"rtl\" >\r\n \r\n\t\t\t\t\t\t\t   <h3 style=\"font-weight:normal;text-align:right;padding:0 10px;\">\r\n\t\t\t\t\t\t\t\t  <p>\r\nتم اصدار تصريح دخول للزائر {VisitorName}<br/>\r\n صاحب هوية رقم {VisitorId} <br/>\r\nتاريخ الزيارة: {VisitDate} <br/>\r\nوقت الزيارة: {VisitTime}   <br/>\r\nزيارة للموظف: {EmployeeName} <br/>\r\n\r\n\r\n</p>\r\n\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t   </h3>\r\n</html>\r\n\r\n\r\n", " <html dir=\"ltr\" >\r\n \r\n\t\t\t\t\t\t\t   <h3 style=\"font-weight:normal;text-align:left;padding:0 10px;\">\r\n\t\t\t\t\t\t\t\t  <p>\r\nEntry permit has been issued to vistor{VisitorName} <br/>\r\nWith Identity {VisitorId} <br/>\r\nVisit Date: {VisitDate}\t<br/>\r\nVisit time: {VisitTime} <br/>\r\nVisiting employee: {EmployeeName} <br/>\r\n\r\n</p>\r\n\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t   </h3>\r\n</html>\r\n\r\n\r\n", 1, true),

                new NotificationTemplate(2, "AcceptHostingRequest", "قبول طلب الضيافة", "Accept Hosting Request" ,"<html dir=\"rtl\" >\r\n \r\n\t\t\t\t\t\t\t   <h3 style=\"font-weight:normal;text-align:right;padding:0 10px;\">\r\n\t\t\t\t\t\t\t\t  <p>\r\nلقد تمت الموافقة على طلبكم الخاص بطلب الضيافة\r\n</p>\r\n\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t   </h3>\r\n</html>\r\n\r\n\r\n", "<html dir=\"ltr\" >\r\n \r\n\t\t\t\t\t\t\t   <h3 style=\"font-weight:normal;text-align:left;padding:0 10px;\">\r\n\t\t\t\t\t\t\t\t  <p>\r\nYour request for hospitality has been approved.\r\n</p>\r\n\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t   </h3>\r\n</html>\r\n\r\n\r\n", 1, true),

                new NotificationTemplate(3, "RejectHostingRequest", "رفض طلب الضيافة", "Reject Hosting Request" ,"<html dir=\"rtl\" >\r\n \r\n\t\t\t\t\t\t\t   <h3 style=\"font-weight:normal;text-align:right;padding:0 10px;\">\r\n\t\t\t\t\t\t\t\t  <p>\r\nلقد تم رفض طلبكم الخاص بطلب الضيافة\r\n</p>\r\n\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t   </h3>\r\n</html>\r\n\r\n\r\n", "<html dir=\"ltr\" >\r\n \r\n\t\t\t\t\t\t\t   <h3 style=\"font-weight:normal;text-align:left;padding:0 10px;\">\r\n\t\t\t\t\t\t\t\t  <p>\r\nYour request for hospitality has been rejected.\r\n</p>\r\n\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t   </h3>\r\n</html>\r\n\r\n\r\n", 1, true),
            
                new NotificationTemplate(4, "CommonEmailStructure", "هيئة الحكومية الرقمية", "DGA", "<!DOCTYPE HTML\r\n\tPUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" dir=\"rtl\">\r\n\r\n<head>\r\n\t<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\r\n\t<title>هيئة الحكومية الرقمية</title>\r\n</head>\r\n\r\n<body dir=\"rtl\" style=\"padding:0px\">\r\n\t\r\n\t<table width=\"1024\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n\t\t<tr>\r\n\t\t\t<td class=\"email_conts\"\r\n\t\t\t\tstyle=\"padding:10px 20px;  font:11px Segoe UI,tahoma;  line-height:20px;  color:#404040;  text-align:right; border-top:none\">\r\n\t\t\t\t{Body}\r\n\t\t\t\t</br>\r\n\t\t\t\t<h3>وتقبلوا فائق الاحترام</h3>\r\n\t\t\t</td>\r\n\t\t</tr>\r\n\t\t<tr>\r\n\t\t\t<td class=\"email_footer\" width=\"1024\"\r\n\t\t\t\tstyle=\"padding:10px 0;  background-color:#202020;  color:#fff;  font:11px Segoe UI,tahoma;  text-align:center\">\r\n\t\t\t\tلمقترحاتكم وملاحظاتكم راسلونا على البريد الإلكتروني: <a href=\"#\"\r\n\t\t\t\t\tstyle=\"color:#fff;  font:11px Segoe UI,tahoma\">support@pgd.gov.sa</a>\r\n\t\t\t</td>\r\n\t\t</tr>\r\n\t</table>\r\n</body>\r\n\r\n</html>", "<!DOCTYPE HTML\r\n\tPUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" dir=\"rtl\">\r\n\r\n<head>\r\n\t<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\r\n\t<title>DGA</title>\r\n</head>\r\n\r\n<body dir=\"ltr\" style=\"padding:0px\">\r\n\t<table width=\"1024\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n\t\t<tr>\r\n\t\t\t<td class=\"email_conts\"\r\n\t\t\t\tstyle=\"padding:10px 20px;  font:11px Segoe UI,tahoma;  line-height:20px;  color:#404040;  text-align:right; border-top:none\">\r\n\t\t\t\t{Body}\r\n\t\t\t\t</br>\r\n\t\t\t\t<h3 style=\"text-align:justify;font-weight:normal;\">Yours Sincerely </h3>\r\n\t\t\t</td>\r\n\t\t</tr>\r\n\t\t<tr>\r\n\t\t\t<td class=\"email_footer\" width=\"1024\"\r\n\t\t\t\tstyle=\"padding:10px 0;  background-color:#202020;  color:#fff;  font:11px Segoe UI,tahoma;  text-align:center\">\r\n\t\t\t\tFor your suggestions and comments, send us an e-mail: <a href=\"#\"\r\n\t\t\t\t\tstyle=\"color:#fff;  font:11px Segoe UI,tahoma\">support@pgd.gov.sa</a>\r\n\t\t\t</td>\r\n\t\t</tr>\r\n\t</table>\r\n</body>\r\n\r\n</html>", 1, true),

                new NotificationTemplate(5, "LicenseNotification", ": إشعار تغيير حالة الترخيص رقم {LicenseNumber} ", "License Status change notification for License Number : {LicenseNumber} ", "عزيزي  فريق تراخيص، \r\nنفيدكم علما أنه قد تغيير حالة الترخيص {CompanyName} {LicenseType} {LicenseNumber} بنجاح.\r\nاسم الشركة {CompanyName} \r\nنوع الترخيص {LicenseType}\r\nرقم الترخيص {LicenseNumber}\r\nحالة الترخيص {LicenseStatus}\r\nمع تحيات، \r\nهيئة الحكومة الرقمية {CompanyName}،   \r\n نود إبلاغكم بأن رخصتكم التي تحمل الرقم {LicenseNumber} \r\n قد تم تغيير حالتها إلى {LicenseStatus} \r\n للأسباب التالية:  {ActionReason} \r\n رقم الترخيص {LicenseNumber} \r\n حالة الترخيص {LicenseStatus}  \r\n مع تحيات،   هيئة الحكومة الرقمية  ", "Dear {CompanyName},\r\nWe would like to inform you that the status of your License with the Number {LicenseNumber} was changed to {LicenseStatus} for the following reason:\r\n{ActionReason}\r\n\r\nLicense Number {LicenseNumber}\r\nLicense Status {LicenseStatus}.\r\nBest regards,\r\nDigital Government Authority",1,true),
                new NotificationTemplate(6, "LicenseTeamNotification", "تم تغيير حالة الترخيص {CompanyName} {LicenseType} {LicenseNumber} بنجاح", "License status for {CompanyName} {LicenseType} {LicenseNumber} was successfully changed", "عزيزي  فريق تراخيص، \r\nنفيدكم علما أنه قد تغيير حالة الترخيص {CompanyName} {LicenseType} {LicenseNumber} بنجاح.\r\nاسم الشركة {CompanyName} \r\nنوع الترخيص {LicenseType}\r\nرقم الترخيص {LicenseNumber}\r\nحالة الترخيص {LicenseStatus}\r\nمع تحيات، \r\nهيئة الحكومة الرقمية", "Dear Licenses Team,\r\nKindly note that the license status for {CompanyName} {LicenseType} {LicenseNumber} has been change to {LicenseStatus} successfully.\r\nCompany Name {CompanyName} \r\nLicense Type {LicenseType}\r\nLicense Number {LicenseNumber}\r\nLicense Status {LicenseStatus}\r\nBest regards,\r\nDigital Government Authority",1,true),
            });

        }
    }
}
