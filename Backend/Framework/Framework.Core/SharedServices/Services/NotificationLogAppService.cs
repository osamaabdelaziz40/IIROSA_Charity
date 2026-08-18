using Framework.Core.AutoMapper;
using Framework.Core.Data.Repositories;
using Framework.Core.Globalization;
using Framework.Core.Notifications;
using Framework.Core.Notifications.Dtos;
using Framework.Core.SharedServices.Dto;
using Framework.Core.SharedServices.Entities;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.SharedServices.Services
{
    public class NotificationLogAppService
    {
        private readonly IRepositoryBase<ICommonsDbContext, NotificationsLog> _notificationLogRepository;
        private readonly AppSettingsService _appSettingsService;

        public NotificationLogAppService(IRepositoryBase<ICommonsDbContext, NotificationsLog> notificationLogRepository , 
            AppSettingsService appSettingsService)
        {
            _notificationLogRepository = notificationLogRepository;
            _appSettingsService = appSettingsService;
        }

        public async Task<List<NotificationsLog>> GetNotificationLogsByTypeAsync(string key, NotificationTypes notificationType)
        {
            try
            {
                var template = await _notificationLogRepository.TableNoTracking.Where(n => n.NotificationTypeId == (int)notificationType).ToListAsync();
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
        public async Task<List<NotificationsLog>> GetNotReadedNotificationAsync()
        {
            try
            {
                var template = await _notificationLogRepository.TableNoTracking.Where(n => n.IsRead == false).ToListAsync();
                if (template == null)
                {
                    throw new NotificationException(
                        $"The template is not available, Check table common.NotificationTemplate");
                }
                return template;
            }
            catch (System.Exception ec)
            {
                throw;
            }
        }
        public async Task<NotificationSearchDto> GetNotificationsAsync(NotificationSearchDto model)
        {

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            model.PageSize = model.IsExport ? _appSettingsService.ExportNoOfItems : model.PageSize.Value;

            //var myApprovedCompanyIds = await GetApprovedCompanyRequestIds(_identityUser.CurrentUserID);

            var query = _notificationLogRepository.TableNoTracking
                .Include(x => x.NotificationType).Where(a=>a.NotificationTypeId!=null);

            if (model.Filter.IsRead !=null)
            {
                query = query.Where(r => r.IsRead==model.Filter.IsRead);
            }

            

            var requests = await query
                .Select(request => new NotificationsLogDto()
                {
                    Id = request.Id,
                    MessageAr = request.MessageAr,
                    Date = request.Date,
                     NotificationTypeId = request.NotificationTypeId,
                    CompanyProfileId = request.CompanyProfileId,
                    EmployeeProfileId = request.EmployeeProfileId,
                    MessageEn = request.MessageEn,
                    NotificationTypeAr = request.NotificationType.NameAr,
                    NotificationTypeEn = request.NotificationType.NameEn,
                })
                .OrderByDescending(x => x.Date)
                //.ThenByDescending(a=>a.LatestRequestDate)
                .ToListAsync();

            // Perform pagination
            var pagedRequests = requests
                .Skip((model.PageNumber - 1) * model.PageSize ?? 0)
                .Take(model.PageSize ?? 0)
                .ToList();

            model.Items = new StaticPagedList<NotificationsLogDto>(
                pagedRequests,
                model.PageNumber,
                model.PageSize ?? 0,
                requests.Count);

            model.TotalItemsCount = requests.Count;
            return model;
        }

        public async Task<ReturnResult<List<NotificationsLogDto>>> GetNotificationLogsAsync()
        {
            var result = new ReturnResult<List<NotificationsLogDto>>();

            var templates = await _notificationLogRepository.TableNoTracking
                .Include(a => a.NotificationType)
                .OrderByDescending(a=>a.Date).ToListAsync();
            if (result.IsValid)
            {
                result.Value = templates.MapTo<List<NotificationsLogDto>>();
            }
            return result;
        }
        public async Task<ReturnResult<List<NotificationsLogDto>>> GetNewNotificationLogsAsync()
        {
            var result = new ReturnResult<List<NotificationsLogDto>>();

            var templates = await _notificationLogRepository.TableNoTracking
                .Include(a=>a.NotificationType)
                .Where(a=>a.IsRead == false)
                .OrderByDescending(a => a.Date).ToListAsync();
            if (result.IsValid)
            {
                result.Value = templates.MapTo<List<NotificationsLogDto>>();
            }
            return result;
        }


        public async Task<ReturnResult<int>> GetNotificationLogsCountAsync()
        {
            var result = new ReturnResult<int>();

            var templates = await _notificationLogRepository.TableNoTracking.Where(a => a.IsRead == false).CountAsync();
            if (result.IsValid)
            {
                result.Value = templates;//.MapTo<int>();
            }
            return result;
        }


        public async Task<bool> AddNotificationLogAsync(NotificationsLogDto notificationLogDto)
        {
            var template = notificationLogDto.MapTo<NotificationsLog>();
            await _notificationLogRepository.InsertAsync(template, true);
            return true;
        }
    }
}