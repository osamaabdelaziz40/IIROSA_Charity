// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationSettings.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Notifications
{
    /// <summary>
    ///     The notification settings.
    /// </summary>
    public class NotificationSettings
    {
        /// <summary>
        ///     Gets the email from address.
        /// </summary>
        public string EmailFromAddress { get; set; }

        /// <summary>
        ///     Gets the email from name.
        /// </summary>
        public string EmailFromName { get; set; }

        /// <summary>
        ///     Gets the email subject.
        /// </summary>
        public string EmailSubjectAr { get; set; }

        public string EmailSubjectEn { get; set; }

        /// <summary>
        ///     Gets the google fcm sender id.
        /// </summary>
        public string GoogleFCMSenderId { get; set; }

        /// <summary>
        ///     Gets the google fcm server key.
        /// </summary>
        public string GoogleFCMServerKey { get; set; }

        /// <summary>
        ///     Gets a value indicating whether is smtp authenticated.
        /// </summary>
        public bool IsSmtpAuthenticated { get; set; }

        /// <summary>
        ///     Gets the sender id.
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        ///     Gets the server key.
        /// </summary>
        public string ServerKey { get; set; }

        /// <summary>
        ///     Gets a value indicating whether smtp enable ssl.
        /// </summary>
        public bool SmtpEnableSSL { get; set; }

        /// <summary>
        ///     Gets the smtp password.
        /// </summary>
        public string SmtpPassword { get; set; }

        /// <summary>
        ///     Gets the smtp port.
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        ///     Gets the smtp server.
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        ///     Gets the smtp user name.
        /// </summary>
        public string SmtpUserName { get; set; }
    }
}