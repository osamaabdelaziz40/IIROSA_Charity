using Framework.Core.Data.Repositories;
using Framework.Core.Extensions;
using Framework.Core.SharedServices.Dto;
using Framework.Core.SharedServices.Entities;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Core.SharedServices.Services
{
    public class LogAppService
    {
        private readonly IRepositoryBase<ICommonsDbContext, Log> _logRepository;
        private readonly ICommonsDbContext _dbContext;
        private readonly AppSettingsService _appSettingsService;

        public LogAppService(IRepositoryBase<ICommonsDbContext, Log> logRepository,
            ICommonsDbContext dbContext,
            AppSettingsService appSettingsService)
        {
            _logRepository = logRepository;
            _dbContext = dbContext;
            _appSettingsService = appSettingsService;
        }

        public LogSearchDto GetLogs(LogSearchDto model)
        {
            var filters = new List<Expression<Func<Log, bool>>>();

            if (model.DateFrom.HasValue && model.DateTo.HasValue)
            {
                Expression<Func<Log, bool>> dateFilter = r =>
                    (r.Date.Date >= model.DateFrom.Value.Date && r.Date.Date <= model.DateTo.Value.Date);
                filters.Add(dateFilter);

            }
            else if (model.DateFrom.HasValue)
            {
                Expression<Func<Log, bool>> dateFilter = r =>
                    (r.Date.Date >= model.DateFrom.Value.Date);
                filters.Add(dateFilter);

            }
            else if (model.DateTo.HasValue)
            {
                Expression<Func<Log, bool>> dateFilter = r =>
                    (r.Date.Date <= model.DateTo.Value.Date);
                filters.Add(dateFilter);

            }

            if (!model.UserName.IsNullOrEmpty())
            {
                Expression<Func<Log, bool>> userNameFilter = r => r.UserName.Contains(model.UserName);
                filters.Add(userNameFilter);
            }

            if (!model.LogLevel.IsNullOrEmpty())
            {
                Expression<Func<Log, bool>> logLevelFilter = r => r.LogLevel == model.LogLevel;
                filters.Add(logLevelFilter);
            }


            var result = _logRepository.SearchWithFilters
            (
                model.PageNumber,
                model.PageSize ?? _appSettingsService.DefaultPagerPageSize,
                a => a.OrderByDescending(b => b.Date),
                filters
            );

            model.Items =
                new StaticPagedList<Log>(
                    result,
                    result.PageNumber,
                    result.PageSize,
                    result.TotalItemCount);

            return model;

        }

        public async Task<Log> GetLogById(Guid id)
        {
            return await _logRepository.GetByIdAsync(id);
        }

        //public void ClearLog()
        //{
        //    _dbContext.ExecuteSqlCommand($"TRUNCATE TABLE common.Log");
        //}

        public async Task DeleteLogOlderThan5Days()
        {

            await _logRepository.DeleteAsync(l => l.Date <= DateTime.Now.AddDays(-5));
        }
    }
}
