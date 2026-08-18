// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsMessage.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Notifications
{
    /// <summary>
    ///     The sms message.
    /// </summary>
    public class SmsMessage : NotificationMessageBase
    {
        /// <summary>
        ///     Gets or sets the phone number.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        ///     Gets or sets the text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        public string Title { get; set; }
    }
}