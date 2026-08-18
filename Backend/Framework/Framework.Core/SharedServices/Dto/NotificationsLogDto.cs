using Framework.Core.Globalization;
using Framework.Core.SharedServices.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ZXing.QrCode.Internal.Mode;

namespace Framework.Core.SharedServices.Dto
{
    public class NotificationsLogDto
    {
        public Guid? Id { get; set; }
        public string To { get; set; }
        public DateTime Date { get; set; }
        public int StatusId { get; set; }
        public string MessageAr { get; set; }
        public string MessageEn { get; set; }
        [NotMapped]
        public string Message { get { return CultureHelper.IsArabic ? MessageAr : MessageEn; } }

        public string Exception { get; set; }
        public string NotificationTypeAr { get; set; }
        public string NotificationTypeEn { get; set; }
        [NotMapped]
        public string NotificationTypeName { get { return CultureHelper.IsArabic ? NotificationTypeAr : NotificationTypeEn; } }

        public Guid? CompanyProfileId { get; set; }
        public Guid? EmployeeProfileId { get; set; }


        public int NotificationTypeId { get; set; }
        public virtual NotificationType NotificationType { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
    }
}
