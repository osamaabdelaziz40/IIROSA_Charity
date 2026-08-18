// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MobileNotification.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Notifications
{
    /// <summary>
    ///     The mobile notification.
    /// </summary>
    public class MobileNotification : NotificationMessageBase
    {
        /// <summary>
        ///     Gets or sets the badge.
        /// </summary>
        public int Badge { get; set; }

        /// <summary>
        ///     Gets or sets the body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        ///     Gets or sets the data.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        ///     Gets or sets the priority.
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     Gets or sets the to mobile token.
        /// </summary>
        public string ToMobileToken { get; set; }
    }
}