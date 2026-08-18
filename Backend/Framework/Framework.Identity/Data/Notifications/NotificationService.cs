//using Framework.Core.Extensions;
//using Framework.Core.Notifications;
//using Framework.Core.SharedServices.Services;
//using Framework.Identity.Data.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Framework.Identity
//{
//    public class NotificationService
//    {
//        private readonly INotificationsManager _notificationsManager;
//        private readonly UserAppService _userAppService;
//        public NotificationService(INotificationsManager notificationsManager,
//            UserAppService userAppService)
//        {
//            _notificationsManager = notificationsManager;
//            _userAppService = userAppService;
//        }

//        public async Task AccountActivation(Guid userId, string callbackUrl)
//        {
//            var user = await this._userAppService.FindByIdAsync(userId);
//            var mail = new EmailMessage
//            {
//                To = new List<string> { user.Email },
//                TemplateName = EmailTemplateNames.Users_AccountRegistration.ToString(),
//                TemplateData = new Dictionary<string, string> {
//                    { "PageUrl", callbackUrl },
//                    { "FullName", user.FullName??user.Email }
//                }
//            };

//            await _notificationsManager.EnqueueEmailAsync(mail);
//        }

//        public async Task AccountRegistrationCreatedByAdmin(Guid userId, string UserName, string Password, string callbackUrl)
//        {
//            var user = await this._userAppService.FindByIdAsync(userId);
//            var mail = new EmailMessage
//            {
//                To = new List<string> { user.Email },
//                TemplateName = EmailTemplateNames.AccountRegistrationCreatedByAdmin.ToString(),
//                TemplateData = new Dictionary<string, string> {
//                    { "PageUrl", callbackUrl },
//                    { "FullName", user.FullName??user.FullName },
//                    { "UserName", UserName??UserName },
//                     { "Password", Password??Password },
//                }
//            };

//            await _notificationsManager.EnqueueEmailAsync(mail);
//        }

//        public async Task ChangeEmail(Guid userId, string callbackUrl)
//        {
//            var user = await this._userAppService.GetUserAsync(userId);
//            var mail = new EmailMessage
//            {
//                To = new List<string> { user.TempEmail },
//                TemplateName = EmailTemplateNames.Users_ChangeEmail.ToString(),
//                TemplateData = new Dictionary<string, string> {
//                    { "PageUrl", callbackUrl },
//                    { "FullName", user.FullName??user.Email }
//                }
//            };

//            await _notificationsManager.EnqueueEmailAsync(mail);
//        }

//        public async Task ChangePhoneNumber_SMS(Guid userId, string code)
//        {
//            var user = await this._userAppService.GetUserAsync(userId);
//            var sms = new SmsMessage
//            {
//                PhoneNumber = FormatPhoneNumberForSms(user.TempPhoneNumber),
//                TemplateName = SmsTemplateNames.PhoneNumberActivation_SMS.ToString(),
//                TemplateData = new Dictionary<string, string> {
//                    { "code", code }
//                }
//            };
//            await _notificationsManager.EnqueueSmsAsync(sms);
//        }

//        public async Task ResetPassword(Guid userId, string callbackUrl)
//        {
//            var user = await this._userAppService.FindByIdAsync(userId);
//            var mail = new EmailMessage
//            {
//                To = new List<string> { user.Email },
//                TemplateName = EmailTemplateNames.Users_ForgotPassword.ToString(),
//                TemplateData = new Dictionary<string, string> {
//                    { "PageUrl", callbackUrl },
//                    { "FullName", user.FullName??user.Email }
//                }
//            };
//            await _notificationsManager.EnqueueEmailAsync(mail);
//        }

//        public async Task ResetPasswordConfirmation(Guid userId)
//        {
//            var user = await this._userAppService.FindByIdAsync(userId);
//            var mail = new EmailMessage
//            {
//                To = new List<string> { user.Email },
//                TemplateName = EmailTemplateNames.Users_ResetPasswordConfirmation.ToString(),
//                TemplateData = new Dictionary<string, string> {
//                    { "FullName", user.FullName??user.Email  }
//                }
//            };
//            await _notificationsManager.EnqueueEmailAsync(mail);
//        }

//        //public async Task ResetPassword_SMS(Guid userId, string code)
//        //{
//        //    var user = await this._userAppService.FindByIdAsync(userId);
//        //    var sms = new SmsMessage
//        //    {
//        //        PhoneNumber = FormatPhoneNumberForSms(user.PhoneNumber),
//        //        TemplateName = SmsTemplateNames.User_SmsForgetPassword.ToString(),
//        //        TemplateData = new Dictionary<string, string> {
//        //            { "code", code }
//        //        }
//        //    };
//        //    await _notificationsManager.EnqueueSmsAsync(sms);
//        //}

//        private string FormatPhoneNumberForSms(string phone)
//        {
//            if (phone.StartsWith("+"))
//            {
//                return phone.Replace("+", "");
//            }
//            if (phone.StartsWith("00"))
//            {
//                return phone.Replace("00", "");
//            }

//            return phone;
//        }
//    }
//}