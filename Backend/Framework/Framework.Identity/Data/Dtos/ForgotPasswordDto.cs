using Framework.Core.Notifications;

namespace Framework.Identity.Data.Dtos
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public NotificationTypes NotificationTypes { get; set; }
    }
}