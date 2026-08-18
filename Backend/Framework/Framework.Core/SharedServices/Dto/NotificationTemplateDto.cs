using Framework.Core.Globalization;

namespace Framework.Core.SharedServices.Dto
{
    /// <summary>
    ///     The notification template.
    /// </summary>
    public class NotificationTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string SubjectAr { get; set; }
        public string SubjectEn { get; set; }
        public string Subject => CultureHelper.IsArabic ? SubjectAr : SubjectEn;
        public string BodyAr { get; set; }
        public string BodyEn { get; set; }
        public string Body => CultureHelper.IsArabic ? BodyAr : BodyEn;
        public int NotificationTypeId { get; set; }
        public bool IsActive { get; set; }
        public NotificationTypeDto NotificationType { get; set; }
    }
}