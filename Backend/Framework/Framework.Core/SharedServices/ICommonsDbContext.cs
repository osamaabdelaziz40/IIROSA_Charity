using Framework.Core.Data;
using Framework.Core.SharedServices.Entities;
using Microsoft.EntityFrameworkCore;

namespace Framework.Core.SharedServices
{
    public interface ICommonsDbContext : IBaseDbContext
    {
        //DbSet<Attachment> Attachments { get; set; }
        //DbSet<AttachmentType> AttachmentTypes { get; set; }
        //DbSet<Log> Logs { get; set; }

        DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        DbSet<SystemSetting> SystemSettings { get; set; }
        DbSet<NotificationType> NotificationTypes { get; set; }
    }
}