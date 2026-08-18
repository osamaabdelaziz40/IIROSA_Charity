using Framework.Core.AutoMapper;
using Framework.Core.Caching;
using Framework.Core.Data.Repositories;
using Framework.Core.Globalization;
using Framework.Core.SharedServices.Dto;
using Framework.Core.SharedServices.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Core.SharedServices.Services
{
    public class AppSettingsService
    {
        private readonly IRepositoryBase<ICommonsDbContext, SystemSetting> _settingRepository;
        private readonly ICacheManager _cacheManager;

        public AppSettingsService(
            IRepositoryBase<ICommonsDbContext, SystemSetting> settingRepository,
            ICacheManager cacheManager)
        {
            _settingRepository = settingRepository;
            _cacheManager = cacheManager;
            LoadSettings();
        }

        protected IDictionary<string, IList<SettingsDto>> GetAllSettingsCached()
        {
            //cache
            return _cacheManager.Get(CachingDefaults.SettingsAllCacheKey, () =>
            {
                //we use no tracking here for performance optimization
                //anyway records are loaded only for read-only operations
                var query = from s in _settingRepository.TableNoTracking
                            orderby s.Name
                            select s;
                var settings = query.ToList();
                var dictionary = new Dictionary<string, IList<SettingsDto>>();
                foreach (var s in settings)
                {
                    var resourceName = s.Name.ToLowerInvariant();

                    var settingForCaching = new SettingsDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Value = s.Value,
                        GroupName = s.GroupName,
                        ValueType = s.ValueType
                    };

                    if (!dictionary.ContainsKey(resourceName))
                    {
                        //first setting
                        dictionary.Add(resourceName, new List<SettingsDto>
                        {
                            settingForCaching
                        });
                    }
                    else
                    {
                        //already added
                        //most probably it's the setting with the same name but for some certain store (storeId > 0)
                        dictionary[resourceName].Add(settingForCaching);
                    }
                }

                return dictionary;
            });
        }

        public List<SettingsDto> GetSettingsByGroup(string groupName)
        {
            var list = new List<SettingsDto>();

            foreach (var setting in GetAllSettingsCached())
            {
                var item = setting.Value.Where(a => a.GroupName == groupName);
                list.AddRange(item.ToList());
            }

            return list;
        }

        public SettingsDto GetSetting(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            var settings = GetAllSettingsCached();
            key = key.Trim().ToLowerInvariant();
            if (!settings.ContainsKey(key))
                return null;

            var settingsByKey = settings[key];
            var setting = settingsByKey.FirstOrDefault(x => x.Name.ToLowerInvariant() == key);

            return setting;
        }

        public virtual void UpdateSetting(SettingsDto settingsDto, bool clearCache = true)
        {
            if (settingsDto == null)
                throw new ArgumentNullException(nameof(settingsDto));

            var setting = _settingRepository.GetById(settingsDto.Id);
            setting.UpdateValue( settingsDto.Value);
            _settingRepository.Update(setting, true);

            //cache
            if (clearCache)
                _cacheManager.Remove(CachingDefaults.SettingsAllCacheKey);
        }

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            var settings = GetAllSettingsCached();
            key = key.Trim().ToLowerInvariant();
            if (!settings.ContainsKey(key))
                return defaultValue;

            var settingsByKey = settings[key];
            var setting = settingsByKey.FirstOrDefault(x => x.Name.ToLowerInvariant() == key);

            return setting != null ? CommonHelper.To<T>(setting.Value) : defaultValue;
        }

        public async Task<SettingsDto> GetByName(string id)
        {
            try
            {
                var filters = new List<Expression<Func<SystemSetting, bool>>>();

                filters.Add(e => e.Name == id);
                Func<IQueryable<SystemSetting>, IOrderedQueryable<SystemSetting>> orderBy = a => a.OrderBy(b => b.CreatedOn);

                var entity = _settingRepository.SearchWithFilters(orderBy, filters).FirstOrDefault();
                var SettingsDto = entity.MapTo<SettingsDto>();

                return SettingsDto;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void LoadSettings()
        {
            foreach (var prop in this.GetType().GetProperties())
            {
                // get properties we can read and write to
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                var key = prop.Name;

                var setting = GetValue<string>(key);
                if (setting == null)
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).IsValid(setting))
                    continue;

                var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(setting);

                //set property
                prop.SetValue(this, value);
            }
        }

        #region Props

        #region General Settings

        public string EmailLanguage { get; set; }
        public string DevApplicationUrl { get; set; }
        public string ApplicationUrl { get; set; }
        public string PortalUrl { get; set; }
        public string DateFormat { get; set; }

        public string DateTimeFormat => $"{this.DateFormat} {this.TimeFormat}";

        public int DefaultPagerPageSize { get; set; } = 10;

        public string LoanOfferLoginApi { get; set; }

        public string LoanOfferApi { get; set; }

        public string LoanOfferApiUsername { get; set; }
        public string LoanOfferApiPassword { get; set; }

        public string PagerSizeDefaultValues { get; set; }

        public string DownloadFileUrl => "/Files/Download/?attId="; // CommonsSettings.ApplicationRootUrl +

        public int ExportNoOfItems { get; set; }
        public string TimeFormat { get; set; }

        public string RequestDetailsPageUrl { get; set; }

        public bool IsSimulation { get; set; }
        public bool IsDevelopment { get; set; }
        public string MinPeriodToCreateAuction { get; set; }
        public string ImageTermsAr { get; set; }
        public string ImageTermsEn { get; set; }
        public string TermsAndConditionsAr { get; set; }
        public string TermsAndConditionsEn { get; set; }

        public int PeriodBeforeRenewLicenseInDays { get; set; }
        #endregion General Settings

        #region Attachment Settings

        public int AttachmentsAllowedHeight { get; set; }

        public string AttachmentsAllowedTypes { get; set; }

        public int AttachmentsAllowedWidth { get; set; }

        public int AttachmentsMaxSize { get; set; }

        public string AttachmentsPath { get; set; }

        public bool SaveFilesToDatabase { get; set; }

        #endregion Attachment Settings

        #region Notification Settings

        public bool DisableSMSNotifications { get; set; }

        public bool DisableEmailNotifications { get; set; }

        public string ContactUsEmail { get; set; }

        public string EmailSubject => CultureHelper.IsArabic ? this.EmailSubjectAr : this.EmailSubjectEn;

        public string EmailSubjectAr { get; set; }

        public string EmailSubjectEn { get; set; }

        public string EmailFromAddress { get; set; }

        public string EmailFromName { get; set; }

        public string GoogleFCMSenderId { get; set; }

        public string GoogleFCMServerKey { get; set; }

        public bool IsSmtpAuthenticated { get; set; }

        public string SenderId { get; set; }

        public string ServerKey { get; set; }

        public bool SmtpEnableSSL { get; set; }

        public string SmtpPassword { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpServer { get; set; }

        public string SmtpUserName { get; set; }

        public string SmsAppId { get; set; }
        public string SmsClientId { get; set; }
        public string SmsAPIUrl { get; set; }
        public string ExternalUrl { get; set; }
        public string InternalUrl { get; set; }
        public bool IgnoreSDIFIntegration { get; set; }
        public bool IsSmsSimulation { get; set; }

        #endregion Notification Settings

        #region Users Management Settings

        public string ActiveDirectoryDomainName { get; set; }
        public int VerificationCodeMaxAttempts { get; set; }

        public int IdentityTokenLifespan { get; set; }

        #endregion Users Management Settings

        #region External Services Settings

        public string K2WebApiUrl { get; set; }
        public string K2SecurityLabel { get; set; }

        #endregion External Services Settings

        #region EFile Settings

        public string EFileClientId { get; set; }
        public string EFileClientSecret { get; set; }
        public string EFileServiceUrl { get; set; }
        public string EFileWrapperApiUrl { get; set; }
        public string EFileRegisterUrl { get; set; }

        public bool SkipCrCheck { get; set; }

        #endregion EFile Settings

        #region Escalation Settings

        public int GateKeeperDays { get; set; }
        public int AnalyticalDirectorDays { get; set; }
        public int BiDirectorDays { get; set; }
        public int ExecutionDirectorDays { get; set; }
        public int AccountManagerDays { get; set; }
        public int ComplianceDirectorDays { get; set; }
        public int CommunicationDirectorDays { get; set; }
        public int EntityDays { get; set; }
        public int SecondLevelEscalationDays { get; set; }
        public int WorkingDayEndTime { get; set; }

        #endregion Escalation Settings

        #region Integration

        public string IntegrationMobileApiKey { get; set; }
        public string MerchantStatementService { get; set; }
        public string CompanyCode { get; set; }
        public string SurePullAccount { get; set; }
        public string CompanyStatementService { get; set; }
        public string AnbEodService { get; set; }
        public string RiyadhBankSignKeyPath { get; set; }
        public string RiyadhBankPaymentUrl { get; set; }
        public bool IsProduction { get; set; }
        public string AnbPaymentServiceUrl { get; set; }
        public string AnbOmniBusAccount { get; set; }
        public string AnbBankKeyPath { get; set; }
        public double? MinimumAmountToTransfer { get; set; }
        public int MaxmumAttachmentFiles { get; set; }

        public double? VatPercentage { get; set; }
        public double? TransferFees { get; set; }
        public int SettlementAccountsNumber { get; set; }
        public int MerchantTrxServiceDaysNumber { get; set; }

        #endregion Integration

        #region LDAP

        public bool IsActiveDirectorySimulation { get; set; }
        public string LDAP_Client_Secret { get; set; }
        public string LDAP_Client_Id { get; set; }
        public string Notification_API_Url { get; set; }
        public bool IsNotificationSimulation { get; set; }
        public string LDAP_API_Url { get; set; }

        #endregion LDAP

        #region Camunda
        public string CamundaUrl { get { return GetValue<string>("CamundaUrl"); } }
        #endregion
        #endregion Props
    }
}