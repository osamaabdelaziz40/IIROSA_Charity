using Framework.Core.SharedServices.Services;
using System.Threading.Tasks;

namespace Framework.Core.Notifications
{
    #region usings

    using Framework.Core.Extensions;
    using Hangfire;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    #endregion usings

    public class NotificationsManager : INotificationsManager
    {
        private readonly NotificationSettings _notificationSettings;
        private readonly NotificationTemplateService _notificationTemplateService;
        private readonly AppSettingsService _appSettingsService;

        public NotificationsManager(
            NotificationTemplateService notificationTemplateService,
            AppSettingsService appSettingsService
            )
        {
            _notificationTemplateService = notificationTemplateService;
            _appSettingsService = appSettingsService;
            ;
            this._notificationSettings = this.LoadNotificationsSettings();
        }

        public async Task EnqueueEmailAsync(EmailMessage message, NotificationLanguage? preferredLanguage = null)
        {
            var notification = await this.BuildNotification<EmailMessage>(message, NotificationTypes.Email, preferredLanguage);
            notification.From = _notificationSettings.EmailFromAddress;
            if (notification.DisplayName == null || notification.DisplayName == string.Empty)
            {
                notification.DisplayName = _notificationSettings.EmailFromName;
            }

            BackgroundJob.Enqueue<IEmailService>(s => s.SendEmail(notification, _notificationSettings));
        }

        public async Task EnqueueMobileNotificationAsync(
            MobileNotification message,
            NotificationLanguage? preferredLanguage = null)
        {
            var obj = await this.BuildNotification<MobileNotification>(
                message,
                NotificationTypes.WebNotification,
                preferredLanguage);
            BackgroundJob.Enqueue<IMobileNotificationService>(
                s => s.SendMobileNotification(obj, this._notificationSettings));
        }

        public async Task EnqueueSmsAsync(SmsMessage message, NotificationLanguage? preferredLanguage = null)
        {
            var notification = await this.BuildNotification<SmsMessage>(message, NotificationTypes.Sms, preferredLanguage);
            BackgroundJob.Enqueue<ISmsService>(s => s.SendSms(notification));
        }

        public async Task EnqueueWebNotificationAsync(WebNotification message, string redirectUrl, NotificationLanguage? preferredLanguage = null)
        {
            // TODO:(Ahmed Gaduo)Add the other actions send to groups / send to group / send to users
            var messageAr = new WebNotification
            {
                UserId = message.UserId,
                Body = message.Body,
                ApplicationId = message.ApplicationId,
                GroupName = message.GroupName,
                GroupsNames = message.GroupsNames,
                TemplateData = message.TemplateData,
                TemplateName = message.TemplateName,
                CreatedBy = message.CreatedBy,
                UsersIds = message.UsersIds
            };
            var messageEn = new WebNotification
            {
                UserId = message.UserId,
                Body = message.Body,
                ApplicationId = message.ApplicationId,
                GroupName = message.GroupName,
                GroupsNames = message.GroupsNames,
                TemplateData = message.TemplateData,
                TemplateName = message.TemplateName,
                CreatedBy = message.CreatedBy,
                UsersIds = message.UsersIds
            };

            var notificationAr = await this.BuildNotification<WebNotification>(
                messageAr,
                NotificationTypes.WebNotification,
                NotificationLanguage.Ar);
            var notificationEn = await this.BuildNotification<WebNotification>(
                messageEn,
                NotificationTypes.WebNotification,
                NotificationLanguage.En);

            //if (message.UserId != null)
            //{
            //    // just one user
            //    await _useHub.SendMessageToUser(message.UserId, notificationAr.Body, notificationEn.Body, redirectUrl);
            //}
            //else if (message.GroupName != null)
            //{
            //    // just one group/role
            //    await _useHub.SendMessageToGroup(message.GroupName, message.UsersIds, notificationAr.Body, notificationEn.Body, redirectUrl);
            //}
            //else if (message.GroupsNames != null && message.GroupsNames.Count > 1)
            //{
            //    // multiple group/role
            //    await _useHub.SendMessageToGroups(message.GroupsNames, message.UsersIds, notificationAr.Body, notificationEn.Body, redirectUrl);
            //}
            //else
            //{
            //    // here some users without groups or roles
            //    await _useHub.SendMessageToUsers(message.UsersIds.Select(a => a.ToString()).ToList(), notificationAr.Body, notificationEn.Body, redirectUrl);
            //}
        }

        public async Task<T> BuildNotification<T>(
            NotificationMessageBase message,
            NotificationTypes notificationType,
            NotificationLanguage? preferredLanguage = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var isArabic = preferredLanguage == NotificationLanguage.Ar;

            if (!(message is EmailMessage) && !(message is SmsMessage) && !(message is MobileNotification)
                && !(message is WebNotification))
            {
                throw new ArgumentException(
                    "message type must be of any: (EmailMessage, SmsMessage, FireBaseMessage, SignalRWebNotification)");
            }

            var mainContainerTemplateName = $"Common{notificationType}Structure";

            var templateName = message.TemplateName;
            message.TemplateData = message.TemplateData ?? new Dictionary<string, string>();

            //if (!message.TemplateData.ContainsKey("RootUrl"))
            //    message.TemplateData.Add("RootUrl", _appSettingsService.ApplicationUrl);

            //if (!message.TemplateData.ContainsKey("ContactUsEmail"))
            //    message.TemplateData.Add("ContactUsEmail", _appSettingsService.ContactUsEmail);

            var mainContainerTemplateContent = string.Empty;
            var subject = string.Empty;

            if (notificationType == NotificationTypes.Email)
            {
                var mainTemplate = await _notificationTemplateService.GetTemplateAsync(mainContainerTemplateName, NotificationTypes.Email);
                mainContainerTemplateContent = isArabic ? mainTemplate.BodyAr : mainTemplate.BodyEn;
                subject = isArabic ? mainTemplate.Subject.Trim() : mainTemplate.SubjectEn.Trim();
                // Replace subject placeholders with values from message.TemplateData
                subject = message.TemplateData.Keys.Aggregate(subject, (current, key) =>
                    current.Replace($"{{" + key + "}}", message.TemplateData[key]));
            }

            var template = await _notificationTemplateService.GetTemplateAsync(templateName, notificationType);

            if (mainContainerTemplateContent.IsNullOrEmpty() && notificationType != NotificationTypes.WebNotification)
            {
                throw new NotificationException(
                    $"The template {mainContainerTemplateName} is not available, Check table common.NotificationTemplate");
            }

            // replace values in design template
            ////TODO : Ali
            var templateContent = isArabic ? template.BodyAr : template.BodyEn;
            subject = isArabic ? $"{subject} - {template.SubjectAr.Trim()}" : $"{subject} - {template.SubjectEn.Trim()}";

            if (!message.TemplateData.ContainsKey("MailTitle"))
                message.TemplateData.Add("MailTitle", isArabic ? template.SubjectAr.Trim() : template.SubjectEn.Trim());
            templateContent =
                message.TemplateData.Keys.Aggregate(templateContent, (current, key) =>
                     current.Replace($"{"{" + key + "}"}", message.TemplateData[key]));

            // replace values in common template
            mainContainerTemplateContent =
                message.TemplateData.Keys.Aggregate(mainContainerTemplateContent, (current, key) =>
                     current.Replace($"{"{" + key + "}"}", message.TemplateData[key]));

            subject += "< {RequestType} - {RequestNumber} > ";
            var subjectContent =
                message.TemplateData.Keys.Aggregate(subject, (current, key) =>
                     current.Replace($"{"{" + key + "}"}", message.TemplateData[key]));

            //TODO : Ali
            var messageContent = (notificationType == NotificationTypes.WebNotification)
                                 ? templateContent
                                 : mainContainerTemplateContent.Replace("{Body}", templateContent);

            switch (notificationType)
            {
                case NotificationTypes.Email:
                    var email = (EmailMessage)message;
                    email.Body = messageContent;
                    email.Subject = subjectContent;
                    email.From = email.From ?? this._notificationSettings.EmailFromAddress;
                    email.DisplayName = email.DisplayName ?? this._notificationSettings.EmailFromName;
                    return (T)Convert.ChangeType(email, typeof(T));

                // break;
                case NotificationTypes.Sms:
                    var sms = (SmsMessage)message;
                    sms.Text = messageContent;
                    return (T)Convert.ChangeType(sms, typeof(T));

                // break;
                case NotificationTypes.WebNotification:
                    var webNotification = (WebNotification)message;
                    webNotification.Body = messageContent;
                    return (T)Convert.ChangeType(webNotification, typeof(T));

                // break;
                case NotificationTypes.MobileNotification:
                    var mobileNotification = (MobileNotification)message;
                    return (T)Convert.ChangeType(mobileNotification, typeof(T));

                // break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(notificationType), notificationType, null);
            }
        }

        public NotificationSettings LoadNotificationsSettings()
        {
            return new NotificationSettings
            {
                EmailFromName = _appSettingsService.GetValue<string>("EmailFromName"),
                EmailFromAddress = _appSettingsService.GetValue<string>("EmailFromAddress"),
                GoogleFCMSenderId = _appSettingsService.GetValue<string>("GoogleFCMSenderId"),
                IsSmtpAuthenticated = _appSettingsService.GetValue<bool>("IsSmtpAuthenticated"),
                SenderId = _appSettingsService.GetValue<string>("SenderId"),
                ServerKey = _appSettingsService.GetValue<string>("ServerKey"),
                SmtpEnableSSL = _appSettingsService.GetValue<bool>("SmtpEnableSSL"),
                SmtpPassword = _appSettingsService.GetValue<string>("SmtpPassword"),
                SmtpPort = _appSettingsService.GetValue<int>("SmtpPort"),
                SmtpUserName = _appSettingsService.GetValue<string>("SmtpUserName"),
                SmtpServer = _appSettingsService.GetValue<string>("SmtpServer"),
                GoogleFCMServerKey = _appSettingsService.GetValue<string>("GoogleFCMServerKey"),
                EmailSubjectAr = _appSettingsService.GetValue<string>("EmailSubjectAr"),
                EmailSubjectEn = _appSettingsService.GetValue<string>("EmailSubjectEn")
            };
        }
    }
}