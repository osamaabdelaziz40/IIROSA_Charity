// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebNotification.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Framework.Core.Data;
using System;
using System.Collections.Generic;

namespace Framework.Core.SharedServices.Entities
{
    public class WebNotification : FullAuditedEntityBase<Guid>
    {
        public WebNotification()
        {
            WebNotificationUsers = new HashSet<WebNotificationUser>();
        }

        public int ApplicationId { get; set; }
        public string MessageContentAr { get; set; }
        public string MessageContentEn { get; set; }
        public string Url { get; set; }

        public virtual ICollection<WebNotificationUser> WebNotificationUsers { get; set; }
    }
}