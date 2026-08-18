using Framework.Core.SharedServices.Services;
using Framework.Identity.Data.Dtos;
using System.DirectoryServices.AccountManagement;

namespace Framework.Identity
{
    public class ActiveDirectoryHelperAppService
    {
        private readonly AppSettingsService _appSettingsService;

        public ActiveDirectoryHelperAppService(AppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService;
        }

        public ADUserCreateDto FindUser(string userName)
        {
            ADUserCreateDto user = GetUserFromActiveDirectory(userName);

            return user;
        }

        public ADUserCreateDto GetUserFromActiveDirectory(string userName)
        {
            using (var pc = new PrincipalContext(ContextType.Domain, _appSettingsService.ActiveDirectoryDomainName))
            {
                var user = UserPrincipal.FindByIdentity(pc, userName);

                return user == null
                    ? null
                    : new ADUserCreateDto
                    {
                        SurName = user.Surname,
                        Description = user.Description,
                        GivenName = user.GivenName,
                        DisplayName = user.DisplayName,
                        Name = user.Name,
                        UserPrincipalName = user.UserPrincipalName,
                        Email = user.EmailAddress ?? user.UserPrincipalName,
                        FullName = user.DisplayName,
                        NationalId = user.EmployeeId,
                        UserName = user.SamAccountName,
                        PhoneNumber = user.VoiceTelephoneNumber
                    };
            }
        }
    }
}