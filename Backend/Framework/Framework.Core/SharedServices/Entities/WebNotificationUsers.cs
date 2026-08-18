// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebNotificationUsers.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Framework.Core.Data;
using System;

namespace Framework.Core.SharedServices.Entities
{
    public class WebNotificationUser : FullAuditedEntityBase<Guid>
    {
        public int ApplicationId { get; set; }
        public Guid WebNotificationId { get; set; }
        public Guid? UserId { get; set; }
        public bool IsNotified { get; set; }
        public DateTime? NotifiedDateTime { get; set; }
        public bool IsSeen { get; set; }
        public DateTime? SeenDateTime { get; set; }

        public virtual WebNotification WebNotification { get; set; }
    }
}