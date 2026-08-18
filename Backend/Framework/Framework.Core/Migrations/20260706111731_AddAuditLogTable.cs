using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Framework.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "common");

            migrationBuilder.CreateTable(
                name: "AttachmentType",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowedFilesExtension = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImageMaxHeight = table.Column<int>(type: "int", nullable: true),
                    ImageMaxWidth = table.Column<int>(type: "int", nullable: true),
                    IsImage = table.Column<bool>(type: "bit", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    MaxSizeInMegabytes = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((1))"),
                    RequestType = table.Column<int>(type: "int", nullable: true),
                    FileNumber = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((1))"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentType", x => x.Id);
                });

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
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FieldChanges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalContext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.AuditLogId);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Thread = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    LogLevel = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Logger = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    UserName = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Message = table.Column<string>(type: "varchar(4000)", unicode: false, maxLength: 4000, nullable: true),
                    Exception = table.Column<string>(type: "varchar(6000)", unicode: false, maxLength: 6000, nullable: true),
                    CallSite = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationType",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSetting",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValueType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSecure = table.Column<bool>(type: "bit", nullable: false),
                    IsSticky = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attachment",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TitleAr = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TitleEn = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AttachmentTypeId = table.Column<int>(type: "int", nullable: true),
                    Thumbnail = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachment_AttachmentType_AttachmentTypeId",
                        column: x => x.AttachmentTypeId,
                        principalSchema: "common",
                        principalTable: "AttachmentType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotificationsLog",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    MessageAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationsLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationsLog_NotificationType_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalSchema: "common",
                        principalTable: "NotificationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplate",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubjectAr = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SubjectEn = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BodyAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplate_NotificationType_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalSchema: "common",
                        principalTable: "NotificationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentContent",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttachmentContent_Attachment_AttachmentId",
                        column: x => x.AttachmentId,
                        principalSchema: "common",
                        principalTable: "Attachment",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                schema: "common",
                table: "NotificationType",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "IsDeleted", "NameAr", "NameEn", "UpdatedBy", "UpdatedOn" },
                values: new object[] { 1, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, "البريد الالكترونى", "Email", null, null });

            migrationBuilder.InsertData(
                schema: "common",
                table: "SystemSetting",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "GroupName", "IsActive", "IsDeleted", "IsSecure", "IsSticky", "Name", "UpdatedBy", "UpdatedOn", "Value", "ValueType" },
                values: new object[,]
                {
                    { 1, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Integration", true, false, false, false, "CamundaUrl", null, null, "http://localhost:8080/", "string" },
                    { 2, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Notifications", true, false, false, false, "SmtpServer", null, null, "192.168.1.166", "string" },
                    { 3, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Notifications", true, false, false, false, "SmtpUserName", null, null, "", "string" },
                    { 4, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Notifications", true, false, false, false, "SmtpPassword", null, null, "", "string" },
                    { 5, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Notifications", true, false, false, false, "SmtpEnableSSL", null, null, "False", "bool" },
                    { 6, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Notifications", true, false, false, false, "SmtpPort", null, null, "25", "int" },
                    { 7, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Notifications", true, false, false, false, "EmailFromName", null, null, "DGA", "string" },
                    { 8, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Notifications", true, false, false, false, "EmailFromAddress", null, null, "info@suredemos.com", "string" },
                    { 9, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Integration", true, false, false, false, "AttachmentsPath", null, null, "Attachments", "string" },
                    { 10, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Integration", true, false, false, false, "AttachmentsServer", null, null, "C:\\inetpub\\wwwroot\\DGA-Internal", "string" }
                });

            migrationBuilder.InsertData(
                schema: "common",
                table: "NotificationTemplate",
                columns: new[] { "Id", "BodyAr", "BodyEn", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "IsActive", "IsDeleted", "Name", "NotificationTypeId", "SubjectAr", "SubjectEn", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, " <html dir=\"rtl\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:right;padding:0 10px;\">\r\n								  <p>\r\nتم اصدار تصريح دخول للزائر {VisitorName}<br/>\r\n صاحب هوية رقم {VisitorId} <br/>\r\nتاريخ الزيارة: {VisitDate} <br/>\r\nوقت الزيارة: {VisitTime}   <br/>\r\nزيارة للموظف: {EmployeeName} <br/>\r\n\r\n\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", " <html dir=\"ltr\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:left;padding:0 10px;\">\r\n								  <p>\r\nEntry permit has been issued to vistor{VisitorName} <br/>\r\nWith Identity {VisitorId} <br/>\r\nVisit Date: {VisitDate}	<br/>\r\nVisit time: {VisitTime} <br/>\r\nVisiting employee: {EmployeeName} <br/>\r\n\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "AccessRequest", 1, "تصريح دخول", "Access Request", null, null },
                    { 2, "<html dir=\"rtl\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:right;padding:0 10px;\">\r\n								  <p>\r\nلقد تمت الموافقة على طلبكم الخاص بطلب الضيافة\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", "<html dir=\"ltr\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:left;padding:0 10px;\">\r\n								  <p>\r\nYour request for hospitality has been approved.\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "AcceptHostingRequest", 1, "قبول طلب الضيافة", "Accept Hosting Request", null, null },
                    { 3, "<html dir=\"rtl\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:right;padding:0 10px;\">\r\n								  <p>\r\nلقد تم رفض طلبكم الخاص بطلب الضيافة\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", "<html dir=\"ltr\" >\r\n \r\n							   <h3 style=\"font-weight:normal;text-align:left;padding:0 10px;\">\r\n								  <p>\r\nYour request for hospitality has been rejected.\r\n</p>\r\n								\r\n							   </h3>\r\n</html>\r\n\r\n\r\n", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "RejectHostingRequest", 1, "رفض طلب الضيافة", "Reject Hosting Request", null, null },
                    { 4, "<!DOCTYPE HTML\r\n	PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" dir=\"rtl\">\r\n\r\n<head>\r\n	<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\r\n	<title>هيئة الحكومية الرقمية</title>\r\n</head>\r\n\r\n<body dir=\"rtl\" style=\"padding:0px\">\r\n	\r\n	<table width=\"1024\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n		<tr>\r\n			<td class=\"email_conts\"\r\n				style=\"padding:10px 20px;  font:11px Segoe UI,tahoma;  line-height:20px;  color:#404040;  text-align:right; border-top:none\">\r\n				{Body}\r\n				</br>\r\n				<h3>وتقبلوا فائق الاحترام</h3>\r\n			</td>\r\n		</tr>\r\n		<tr>\r\n			<td class=\"email_footer\" width=\"1024\"\r\n				style=\"padding:10px 0;  background-color:#202020;  color:#fff;  font:11px Segoe UI,tahoma;  text-align:center\">\r\n				لمقترحاتكم وملاحظاتكم راسلونا على البريد الإلكتروني: <a href=\"#\"\r\n					style=\"color:#fff;  font:11px Segoe UI,tahoma\">support@pgd.gov.sa</a>\r\n			</td>\r\n		</tr>\r\n	</table>\r\n</body>\r\n\r\n</html>", "<!DOCTYPE HTML\r\n	PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" dir=\"rtl\">\r\n\r\n<head>\r\n	<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\r\n	<title>DGA</title>\r\n</head>\r\n\r\n<body dir=\"ltr\" style=\"padding:0px\">\r\n	<table width=\"1024\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n		<tr>\r\n			<td class=\"email_conts\"\r\n				style=\"padding:10px 20px;  font:11px Segoe UI,tahoma;  line-height:20px;  color:#404040;  text-align:right; border-top:none\">\r\n				{Body}\r\n				</br>\r\n				<h3 style=\"text-align:justify;font-weight:normal;\">Yours Sincerely </h3>\r\n			</td>\r\n		</tr>\r\n		<tr>\r\n			<td class=\"email_footer\" width=\"1024\"\r\n				style=\"padding:10px 0;  background-color:#202020;  color:#fff;  font:11px Segoe UI,tahoma;  text-align:center\">\r\n				For your suggestions and comments, send us an e-mail: <a href=\"#\"\r\n					style=\"color:#fff;  font:11px Segoe UI,tahoma\">support@pgd.gov.sa</a>\r\n			</td>\r\n		</tr>\r\n	</table>\r\n</body>\r\n\r\n</html>", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "CommonEmailStructure", 1, "هيئة الحكومية الرقمية", "DGA", null, null },
                    { 5, "عزيزي  فريق تراخيص، \r\nنفيدكم علما أنه قد تغيير حالة الترخيص {CompanyName} {LicenseType} {LicenseNumber} بنجاح.\r\nاسم الشركة {CompanyName} \r\nنوع الترخيص {LicenseType}\r\nرقم الترخيص {LicenseNumber}\r\nحالة الترخيص {LicenseStatus}\r\nمع تحيات، \r\nهيئة الحكومة الرقمية {CompanyName}،   \r\n نود إبلاغكم بأن رخصتكم التي تحمل الرقم {LicenseNumber} \r\n قد تم تغيير حالتها إلى {LicenseStatus} \r\n للأسباب التالية:  {ActionReason} \r\n رقم الترخيص {LicenseNumber} \r\n حالة الترخيص {LicenseStatus}  \r\n مع تحيات،   هيئة الحكومة الرقمية  ", "Dear {CompanyName},\r\nWe would like to inform you that the status of your License with the Number {LicenseNumber} was changed to {LicenseStatus} for the following reason:\r\n{ActionReason}\r\n\r\nLicense Number {LicenseNumber}\r\nLicense Status {LicenseStatus}.\r\nBest regards,\r\nDigital Government Authority", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "LicenseNotification", 1, ": إشعار تغيير حالة الترخيص رقم {LicenseNumber} ", "License Status change notification for License Number : {LicenseNumber} ", null, null },
                    { 6, "عزيزي  فريق تراخيص، \r\nنفيدكم علما أنه قد تغيير حالة الترخيص {CompanyName} {LicenseType} {LicenseNumber} بنجاح.\r\nاسم الشركة {CompanyName} \r\nنوع الترخيص {LicenseType}\r\nرقم الترخيص {LicenseNumber}\r\nحالة الترخيص {LicenseStatus}\r\nمع تحيات، \r\nهيئة الحكومة الرقمية", "Dear Licenses Team,\r\nKindly note that the license status for {CompanyName} {LicenseType} {LicenseNumber} has been change to {LicenseStatus} successfully.\r\nCompany Name {CompanyName} \r\nLicense Type {LicenseType}\r\nLicense Number {LicenseNumber}\r\nLicense Status {LicenseStatus}\r\nBest regards,\r\nDigital Government Authority", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, false, "LicenseTeamNotification", 1, "تم تغيير حالة الترخيص {CompanyName} {LicenseType} {LicenseNumber} بنجاح", "License status for {CompanyName} {LicenseType} {LicenseNumber} was successfully changed", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_AttachmentTypeId",
                schema: "common",
                table: "Attachment",
                column: "AttachmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentContent_AttachmentId",
                schema: "common",
                table: "AttachmentContent",
                column: "AttachmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CorrelationId",
                schema: "common",
                table: "AuditLog",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Entity_EntityId_Timestamp",
                schema: "common",
                table: "AuditLog",
                columns: new[] { "EntityType", "EntityId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityId",
                schema: "common",
                table: "AuditLog",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityType",
                schema: "common",
                table: "AuditLog",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Operation",
                schema: "common",
                table: "AuditLog",
                column: "Operation");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Timestamp",
                schema: "common",
                table: "AuditLog",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId",
                schema: "common",
                table: "AuditLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationsLog_NotificationTypeId",
                schema: "common",
                table: "NotificationsLog",
                column: "NotificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_NotificationTypeId",
                schema: "common",
                table: "NotificationTemplate",
                column: "NotificationTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttachmentContent",
                schema: "common");

            migrationBuilder.DropTable(
                name: "AuditLog",
                schema: "common");

            migrationBuilder.DropTable(
                name: "Log",
                schema: "common");

            migrationBuilder.DropTable(
                name: "NotificationsLog",
                schema: "common");

            migrationBuilder.DropTable(
                name: "NotificationTemplate",
                schema: "common");

            migrationBuilder.DropTable(
                name: "SystemSetting",
                schema: "common");

            migrationBuilder.DropTable(
                name: "Attachment",
                schema: "common");

            migrationBuilder.DropTable(
                name: "NotificationType",
                schema: "common");

            migrationBuilder.DropTable(
                name: "AttachmentType",
                schema: "common");
        }
    }
}
