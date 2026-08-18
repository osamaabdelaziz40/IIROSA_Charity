using Framework.Core.AutoMapper;
using Framework.Core.Data.Repositories;
using Framework.Core.Notifications;
using Framework.Core.SharedServices.Dto;
using Framework.Core.SharedServices.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Framework.Core.SharedServices.Services
{
    public class NotificationTemplateService
    {
        private readonly IRepositoryBase<ICommonsDbContext, NotificationTemplate> _notificationTemplateRepository;

        public NotificationTemplateService(IRepositoryBase<ICommonsDbContext, NotificationTemplate> notificationTemplateRepository)
        {
            _notificationTemplateRepository = notificationTemplateRepository;
        }

        public async Task<NotificationTemplate> GetTemplateAsync(string key, NotificationTypes notificationType)
        {
            try
            {

            var template = await _notificationTemplateRepository.TableNoTracking.FirstOrDefaultAsync(n => n.Name.Trim() == key && n.NotificationTypeId == (int)notificationType);
            if (template == null)
            {
                throw new NotificationException(
                    $"The template '{key}' is not available, Check table common.NotificationTemplate");
            }

            return template;

            }
            catch (System.Exception ec)
            {

                throw;
            }
        }

        public async Task<List<NotificationTemplateDto>> GetTemplatesAsync()
        {
            var templates = await _notificationTemplateRepository.TableNoTracking.ToListAsync();
            return templates.MapTo<List<NotificationTemplateDto>>();
        }

        public async Task<bool> UpdateTemplateAsync(NotificationTemplateDto notificationTemplateDto)
        {
            var template = await _notificationTemplateRepository.TableNoTracking.Where(o => o.Id == notificationTemplateDto.Id).FirstOrDefaultAsync();
            template.BodyAr = notificationTemplateDto.BodyAr;
            template.BodyEn = notificationTemplateDto.BodyEn;
            template.IsActive = notificationTemplateDto.IsActive;
            await _notificationTemplateRepository.UpdateAsync(template, true);
            return true;
        }
    }
}