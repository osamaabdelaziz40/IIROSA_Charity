// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationTemplate.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Framework.Core.Data;
using Framework.Core.Globalization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Framework.Core.SharedServices.Entities
{
    /// <summary>
    ///     The notification template.
    /// </summary>
    public class NotificationTemplate : FullAuditedEntityBase<int>
    {
        public string Name { get; set; }

        public string SubjectAr { get; set; }
        public string SubjectEn { get; set; }
        [NotMapped] public string Subject => CultureHelper.IsArabic ? SubjectAr : SubjectEn;
        public string BodyAr { get; set; }
        public string BodyEn { get; set; }
        [NotMapped] public string Body => CultureHelper.IsArabic ? BodyAr : BodyEn;
        public int NotificationTypeId { get; set; }
        public bool IsActive { get; set; }
        public NotificationType NotificationType { get; set; }
        private NotificationTemplate() { }
        public NotificationTemplate(int id,
                                    string name,
                                    string subjectAr,
                                    string subjectEn,
                                    string bodyAr,
                                    string bodyEn,
                                    int notificationTypeId,
                                    bool isActive)
        {
            Id = id;
            Name = name;
            SubjectAr = subjectAr;
            SubjectEn = subjectEn;
            BodyAr = bodyAr;
            BodyEn = bodyEn;
            NotificationTypeId = notificationTypeId;
            IsActive = isActive;
        }
    }
}