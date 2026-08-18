// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmtpEmailService.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Notifications
{
    using Framework.Core.Data.Repositories;
    using Framework.Core.SharedServices.Entities;
    using Microsoft.Extensions.Logging;
    using NLog;
    using System;
    using System.Linq;

    #region usings

    using System.Net.Mail;

    #endregion usings

    /// <summary>
    ///     The smtp email service.
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly ILogger<SmtpEmailService> _logger;

        //private readonly IEmailService emailService;
        private readonly NotificationsLog _notificationsLogRepository;
        public SmtpEmailService(
            ILogger<SmtpEmailService> logger
            /*NotificationsLog notificationsLogRepository*/)
        {
            //this.emailService = emailService;
            //_notificationsLogRepository = notificationsLogRepository;
            _logger= logger;
        }

        /// <summary>
        /// The send email.
        /// </summary>
        /// <param name="emailMessage">
        /// The email message.
        /// </param>
        /// <param name="notificationSettings">
        /// todo: describe notificationSettings parameter on SendEmail
        /// </param>
        //public void SendEmail(EmailMessage emailMessage
        //    , NotificationSettings notificationSettings

        //    )
        //{
        //    using (var smtpClient = new SmtpClient
        //    {
        //        Host = notificationSettings.SmtpServer,
        //        UseDefaultCredentials = false,
        //        Port = notificationSettings.SmtpPort,
        //        EnableSsl = notificationSettings.SmtpEnableSSL,
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        Credentials = new NetworkCredential(
        //                                        notificationSettings.SmtpUserName,
        //                                        notificationSettings.SmtpPassword)
        //    })
        //    {
        //        var mail = emailMessage.ToMailMessage();

        //        ServicePointManager.SecurityProtocol =
        //            SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        //        smtpClient.Send(mail);
        //    }
        //}

        public void SendEmail(EmailMessage emailMessage, NotificationSettings notificationSettings)
        {
            _logger.LogCritical("start send email here :> ");
            using (SmtpClient smtp = new SmtpClient(notificationSettings.SmtpServer, notificationSettings.SmtpPort))
            {
                smtp.EnableSsl = false;
                smtp.UseDefaultCredentials = notificationSettings.IsSmtpAuthenticated;
                smtp.Credentials = new System.Net.NetworkCredential(notificationSettings.SmtpUserName, notificationSettings.SmtpPassword);
                var mail = emailMessage.ToMailMessage();
                try
                {
                    //NotificationsLog data = new NotificationsLog();
                    //data.StatusId = 1;
                    //data.Message = "Success";
                    //data.CreatedOn = DateTime.Now;
                    //data.To = emailMessage.To.FirstOrDefault();
                    
                    //await .InsertAsync(data);
                   
                    smtp.Send(mail);
                }
                catch (Exception ex)
                {
                    //NotificationsLog data = new NotificationsLog();
                    //data.StatusId = 0;
                    //data.Message = "Failed";
                    //data.CreatedOn = DateTime.Now;
                    //data.To = groupOrEmail;
                    //data.Exception = ex.Message;
                    //await _notificationsLogRepository.InsertAsync(data);
                    _logger.LogCritical("critical issue here :> "+ex.Message);
                }
            }
        }
    }
}