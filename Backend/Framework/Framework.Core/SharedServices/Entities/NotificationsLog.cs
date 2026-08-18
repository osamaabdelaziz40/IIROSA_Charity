
using Framework.Core.Data;
using System;

namespace Framework.Core.SharedServices.Entities
{
    public class NotificationsLog : FullAuditedEntityBase<Guid>
    {
        public string To { get; set; }
        public DateTime Date { get; set; }
        public int StatusId { get; set; }
        public string MessageAr { get; set; }
        public string MessageEn { get; set; }
        public string Exception { get; set; }

        public Guid? CompanyProfileId { get; set; }
        public Guid? EmployeeProfileId { get; set; }


        public int NotificationTypeId { get; set; }
        
        public NotificationType NotificationType { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }

    }
}