// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FakeSmsService.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Notifications
{
    /// <summary>
    ///     The default sms service.
    /// </summary>
    public class FakeSmsService : ISmsService
    {
        /// <summary>
        /// The send sms.
        /// </summary>
        /// <param name="smsMessage">
        /// The sms message.
        /// </param>
        /// <param name="notificationSettings">
        /// The notification settings.
        /// </param>
        public void SendSms(SmsMessage smsMessage)
        {
        }
    }
}