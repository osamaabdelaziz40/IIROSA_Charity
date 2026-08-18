// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebNotification.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Notifications
{
    #region usings

    using System;
    using System.Collections.Generic;

    #endregion usings

    /// <summary>
    ///     The web notification.
    /// </summary>
    public class WebNotification : NotificationMessageBase
    {
        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the groups names.
        /// </summary>
        public List<string> GroupsNames { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the users ids.
        /// </summary>
        public List<Guid> UsersIds { get; set; }

        /// <summary>
        ///     Gets or sets the body.
        /// </summary>
        public string Body { get; set; }
    }
}