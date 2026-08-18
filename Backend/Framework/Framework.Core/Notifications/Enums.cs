// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Enums.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Notifications
{
    /// <summary>
    ///     The notification types.
    /// </summary>
    public enum NotificationTypes
    {
        /// <summary>
        ///     The email.
        /// </summary>
        Email = 1,

        /// <summary>
        ///     The sms.
        /// </summary>
        Sms = 2,

        /// <summary>
        ///     The mobile notification
        /// </summary>
        MobileNotification = 3,

        /// <summary>
        ///     The web notification.
        /// </summary>
        WebNotification = 4
    }

    /// <summary>
    /// The notification language.
    /// </summary>
    public enum NotificationLanguage
    {
        /// <summary>
        /// The ar.
        /// </summary>
        Ar = 1,

        /// <summary>
        /// The en.
        /// </summary>
        En = 2
    }
}