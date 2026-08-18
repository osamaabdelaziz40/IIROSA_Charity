using DGA.Raqmi.Application.Services.Users.Dtos;
using Framework.Core;
using Framework.Core.AutoMapper;
using Framework.Core.Data;
using Framework.Core.Extensions;
using Framework.Core.Globalization;
using Framework.Core.SharedServices.Services;
using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Extensions;
using Framework.Identity.Data.Repositories;
using Framework.Identity.Data.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Services
{
    public class UserAppService : IUserAppService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RoleRepository _roleRepository;
        private readonly UserRepository _userRepository;
        private readonly UserRolesRepository _userRolesRepository;
        private readonly AppSettingsService _appSettingsService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserRoleAppService _userRoleAppService;
        private readonly IWebHostEnvironment _environment;
        private readonly IRoleAppService _roleAppService;
        private readonly ILogger<UserAppService> _logger;
        public UserAppService(
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,

            RoleRepository roleRepository,
            UserRolesRepository userRolesRepository,
            AppSettingsService appSettingsService,
            UserRepository userRepository,
            SignInManager<ApplicationUser> signInManager,
            UserRoleAppService userRoleAppService,
            IWebHostEnvironment environment,
            IRoleAppService roleAppService,
            ILogger<UserAppService> logger)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _signInManager = signInManager;
            _roleRepository = roleRepository;
            _userRolesRepository = userRolesRepository;
            _appSettingsService = appSettingsService;
            _userRoleAppService = userRoleAppService;
            _environment = environment;
            _roleAppService = roleAppService;
            _logger = logger;
        }

        public string CurrentUserName => _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(s => s.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value?.To<string>();
        public string CurrentUserEmail => GetClaimValueByKey("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
        public bool IsAdmin => CurrentUserRoleName == Roles.Admin.ToString();

        public async Task<List<UserDto>> GetUsersInRoles(List<string> roleNames)
        {
            var users = new List<UserDto>();
            foreach (var role in roleNames)
            {
                users.AddRange(await FindAllByRoleNameAsync(role, true));
            }

            return users;
        }
        public async Task<List<UserDto>> GetUsersNotInRoles(List<string> exceptedRoleNames)
        {
            var users = new List<UserDto>();
            var myRoles = await _roleRepository.GetAsync(a =>! exceptedRoleNames.Contains(a.Name));
            foreach (var role in myRoles)
            {
                users.AddRange(await FindAllByRoleNameAsync(role.Name, true));
            }

            return users;
        }
        public async Task<UserGridSearchDto> GetGridList(UserGridSearchDto model)
        {
            var filters = new List<Expression<Func<ApplicationUser, bool>>>();
            if (!string.IsNullOrEmpty(model.UserTxtSearch))
            {
                filters.Add(u => u.NormalizedEmail.Contains(model.UserTxtSearch) || u.UserName.Contains(model.UserTxtSearch) || u.FullName.Contains(model.UserTxtSearch));
            }
            if (model.IsActive.HasValue)
            {
                filters.Add(u => u.IsActive == model.IsActive.Value);
            }
            //if (!string.IsNullOrEmpty(model.RoleName))
            //{
            //    var usersList = await FindAllByRoleNameAsync(model.RoleName);
            //    var userIDs = usersList.Select(u => u.Id).ToList();
            //    filters.Add(u => userIDs.Contains(u.Id));
            //}
            if (model.Role != null && model.Role.Count != 0)
            {
                var usersList = await FindAllByRoleNameAsync(model.Role[0].Value);
                var userIDs = usersList.Select(u => u.Id).ToList();
                filters.Add(u => userIDs.Contains(u.Id));
            }

            model.PageSize ??= _appSettingsService.DefaultPagerPageSize;
            var result = _userRepository.SearchAndSelectWithFilters
                (
                model.PageNumber,
                model.IsExport ? _appSettingsService.ExportNoOfItems : model.PageSize.Value,
                b => b.OrderByDescending(a => a.CreatedOn),
                 a => new ApplicationUser(a.UserName, a.FullName, a.Email, a.IsActive)
                 {
                     Id = a.Id,
                     FullName = a.FullName,
                     //FullNameAr = a.FullNameAr,
                     Email = a.Email,
                     UserName = a.UserName,
                     //JobTitle = a.JobTitle,
                     //JobTitleAr = a.JobTitleAr,
                     //Department = a.Department,
                     //DepartmentAr = a.DepartmentAr,
                     //EmployeeNumber = a.EmployeeNumber,
                     PhoneNumber = a.PhoneNumber,
                     IsActive = a.IsActive
                 },
                filters
                );
            model.Items =
                new StaticPagedList<ApplicationUser>(
                    result,
                    result.PageNumber,
                    result.PageSize,
                    result.TotalItemCount);
            model.TotalItemsCount = model.Items.TotalItemCount;
            return await Task.FromResult(model);

        }

        public async Task<UserDto> FindByIdAsync(Guid? id)
        {
            var result = await _userRepository.TableNoTracking.Where(x => x.Id == id).FirstOrDefaultAsync();
            return result.MapTo<UserDto>();
        }
        public int GetUsersCount()
        {
            return _userRepository.TableNoTracking.Count();
        }
        public async Task<UserDto> GetUser(Guid id)
        {
            var result = await _userRepository.GetFirstOrDefault(x => x.Id == id);
            var user = result.MapTo<UserDto>();
            var roles = await _userRepository.GetRolesAsync(id);
            user.Roles = roles.Select(x => new Framework.Core.Data.LookupBaseDto<string> { Id = x.Name, Name = CultureHelper.IsArabic ? x.DisplayNameAr : x.DisplayNameEn }).ToList();
            return user;
        }
        public async Task<Guid> UpdateAsync(UserDto input)
        {
            var user = await _userRepository.TableNoTracking.Where(x => x.Id == input.Id).FirstOrDefaultAsync();
            if (!user.Id.ToString().IsNotNullOrEmpty())
            {
                return Guid.Empty;
            }
            if (input.UserTypeId.HasValue)
            {
                user.UserTypeId = input.UserTypeId.Value;
            }
            user.PhoneNumber = input.PhoneNumber;

            await _userRepository.UpdateAsync(user, true);
            await UpdateUserRoles(input);

            return user.Id;
        }

        private async Task UpdateUserRoles(UserDto input)
        {
            _userRoleAppService.DeleteByUserId(input.Id.Value);

            foreach (var userRole in input.Roles)
            {
                var role = await _roleAppService.FindByRoleNameAsync(userRole.Id);
                await _userRoleAppService.InsertAsync(new UserRolesDto() { IsActive = true, RoleId = role.Id, UserId = input.Id.Value }, true);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _userRepository.TableNoTracking.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (user != null)
                return await _userRepository.DeleteAsync(u => u.Id == id, true);
            else
                return false;
        }

        public async Task<UserDto> FindByUsernameAsync(string username)
        {
            if (username != null)
            {
                var user = await _userRepository.TableNoTracking.Where(x => x.UserName == username).FirstOrDefaultAsync();
                return user.MapTo<UserDto>();
            }
            return null;
        }
        public async Task<UserDto> GetUser(Guid? id)
        {
            var result = await _userRepository.GetFirstOrDefault(x => x.Id == id);
            var user = result.MapTo<UserDto>();
            var roles = await _userRepository.GetRolesByUsernameAsync(user.UserName);
            user.RoleNames = roles.Select(x => x.Name).ToList();
            return user;
        }
        public async Task<UserDto> FindByEmailAsync(string email)
        {
            var user = await _userRepository.FindByEmailAsync(email);
            //if (user != null)
            //user.JobTitle = CurrentUserRoleName;
            return user?.MapTo<UserDto>();
        }

        public async Task<List<UserDto>> FindAllByRoleNameAsync(string roleName, bool activeOnly = false)
        {
            var role = await _roleRepository.FindByNameAsync(roleName);
            var userRoles = await _userRolesRepository.TableNoTracking.Where(x => x.RoleId == role.Id).ToListAsync();
            var users = userRoles.Select(x => x.UserId).ToList();
            var userList = await _userRepository.TableNoTracking.Where(x => users.Contains(x.Id)).ToListAsync();
            return userList.MapTo<List<UserDto>>();
        }

        public async Task<UserDto> CreateAsync(UserCreateDto input)
        {

            var user = new ApplicationUser(input.UserName.ToLower(), input.FullName.Trim(), input.Email.Trim().ToLower(), input.IsActive)
            {
                CreatedBy = CurrentUserName ?? input.UserName,
                EmailConfirmed = true,
                UserTypeId = input.UserTypeId
            };
            user.CreatedBy = CurrentUserName ?? input.UserName;
            user.EmailConfirmed = true;
            user.PhoneNumber = input.PhoneNumber;
            var createUserResult = await _userManager.CreateAsync(user, input.Password);
            if (!createUserResult.Succeeded)
            {
                // Handle the case where user creation fails
                var error = createUserResult.Errors.FirstOrDefault();
                return null;
            }
            //var res = await _userManager.CreateAsync(user, input.Password);
            //await _userRepository.InsertAsync(user, true);

            if (input.RoleNames != null && input.RoleNames.Length > 0)
            {
                List<UserRolesDto> userRoles = new List<UserRolesDto>();
                foreach (var roleName in input.RoleNames)
                {
                    var role = await _roleRepository.FindByNameAsync(roleName);
                    userRoles.Add(new UserRolesDto
                    {
                        RoleId = role.Id,
                        UserId = user.Id,
                        IsActive = true
                    });
                }
                await _userRoleAppService.InsertRangeAsync(userRoles, true);
            }

            return user.MapTo<UserDto>();
        }

        public async Task<ApiResponse<UserDto>> Register(UserRegister input)
        {

            var email = input.Email.Trim().ToLower();
            var user = new ApplicationUser(email, email, email)
            {
                CreatedBy = email,
                SecurityStamp = null
            };


            var createUserResult = await _userManager.CreateAsync(user, input.Password);
            if (!createUserResult.Succeeded)
            {
                // Handle the case where user creation fails
                var error = createUserResult.Errors.FirstOrDefault();
                return new ApiResponse<UserDto> { Value = new UserDto { Id = null }, Message = error.Description, Success = false };
            }

            // Assign roles to the user if provided
            if (input.RoleNames != null && input.RoleNames.Length > 0)
            {
                var userRoles = new List<UserRolesDto>();
                foreach (var roleName in input.RoleNames)
                {
                    var role = await _roleRepository.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        userRoles.Add(new UserRolesDto
                        {
                            RoleId = role.Id,
                            UserId = user.Id,
                            IsActive = true
                        });
                    }
                    else
                    {
                        // Handle the case where the role is not found
                        throw new Exception($"Role '{roleName}' not found.");
                    }
                }
                await _userRoleAppService.InsertRangeAsync(userRoles, true);
            }
            return new ApiResponse<UserDto> { Value = user.MapTo<UserDto>(), Success = true };


        }

        public string CurrentUserRoleName =>
            _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(s => s.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value?.To<string>();

        public string GetClaimValueByKey(string Key)
        {
            if (!string.IsNullOrEmpty(Key))
            {
                return _httpContextAccessor?.HttpContext?.User?.Claims?.Where(q => q.Type == Key).Select(q => q.Value).FirstOrDefault();
            }
            return null;
        }

        public async Task<bool> ChangeStatus(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                var isLockedOut = await _userManager.IsLockedOutAsync(user);
                if (isLockedOut)
                {
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow); // Unlock account immediately
                }
                user.IsActive = !user.IsActive;
                await _userRepository.UpdateAsync(user, true);
                return true;
            }
            return false;
        }
        public async Task<bool> UpdatePreferredNotificationLanguage(Guid id, string langKey)
        {
            var user = await _userRepository.TableNoTracking.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (user != null)
            {
                user.PreferredNotificationLanguage = langKey;
                await _userRepository.UpdateAsync(user, true);
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateProfile(UpdateProfileDto model)
        {
            var user = await _userRepository.TableNoTracking.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
            if (user != null)
            {
                user.PreferredNotificationLanguage = model.LangKey;
                user.PhoneNumber = model.PhoneNumber;
                user.Email = model.Email;
                await _userRepository.UpdateAsync(user, true);
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateUserAgency(Guid UserId, int AgencyId)
        {
            _logger.LogError("Start Update ");
            var user = await _userRepository.TableNoTracking.FirstOrDefaultAsync(x => x.Id == UserId);
            _logger.LogError("Full Name" + user.FullName);
            if (user != null)
            {
                user.AgancyId = AgencyId;
                await _userRepository.UpdateAsync(user, true);
                return true;
            }
            return false;
        }

        public async Task<ApiResponse<UserDto>> Login(LoginDto model)
        {
            // Attempt to find the user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Attempt to sign in the user using SignInManager
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded && user.IsActive)
                {
                    // Map user to DTO
                    var userDto = user.MapTo<UserDto>();

                    // Load user roles from database and add to DTO
                    var roles = await _userRepository.GetRolesAsync(user.Id);
                    userDto.RoleNames = roles.Select(r => r.Name).ToList();
                    userDto.Roles = roles.Select(r => new LookupBaseDto<string>
                    {
                        Id = r.Name,
                        Name = r.DisplayNameEn ?? r.Name
                    }).ToList();

                    // User successfully signed in
                    return new ApiResponse<UserDto> { Value = userDto, Success = true };
                }
                else
                {
                    // Map user to DTO
                    var userDto = user.MapTo<UserDto>();

                    // Load user roles from database and add to DTO
                    var roles = await _userRepository.GetRolesAsync(user.Id);
                    userDto.RoleNames = roles.Select(r => r.Name).ToList();
                    userDto.Roles = roles.Select(r => new LookupBaseDto<string>
                    {
                        Id = r.Name,
                        Name = r.DisplayNameEn ?? r.Name
                    }).ToList();

                    _logger.LogCritical("login in userappservice" + result.Succeeded);
                    return new ApiResponse<UserDto> { Value = userDto, Success = false , Message = "Pass Error"};
                }
            }
            return new ApiResponse<UserDto> { Value = null, Success = false , Message = "user Not Exist" };

        }

        //public async Task<bool> VerifyOtp(VerifyOtpDto model,string username)
        //{
        //    var user = await _userManager.FindByNameAsync(username);
        //    if (user.Otp.Equals(model.Otp))
        //    {
        //        return true;
        //    }
        //    else return false;

        //}

        public async Task<bool> ChangePassword(string username, ChangePasswordDto changePassword)
        {

            var resuluser = await _userManager.FindByNameAsync(username);
            var result = await _userManager.ChangePasswordAsync(resuluser, changePassword.OldPassword,
                changePassword.NewPassword);
            return result.Succeeded;

        }

        public async Task<bool> ChangePassword(Guid? id, ChangePasswordDto changePassword)
        {
            var resuluser = await _userManager.FindByIdAsync(id.ToString());
            var result = await _userManager.ChangePasswordAsync(resuluser, changePassword.OldPassword,
                changePassword.NewPassword);
            return result.Succeeded;
        }

        public async Task<bool> UpdateEmailConfirmed(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            user.EmailConfirmed = true;
            var resultupdate = await _userRepository.UpdateAsync(user, true);
            return true;
        }
        public async Task<UserDto> IsUserNameExistAsync(string userName)
        {
            UserManagementInsertResultDto objResult = new UserManagementInsertResultDto();

            return await FindByUsernameAsync(userName);

        }

        public async Task<bool> IncreaseFailedLoginCount_LockIfExceed(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                await _userManager.AccessFailedAsync(user);
                if(user.AccessFailedCount >= 4)
                {
                    user.IsActive = false;
                    await _userRepository.UpdateAsync(user, true);
                }
                
                return false;
            }
            return false;
        }

        public async Task<bool> CheckIfAccountIsLocked(string userName)
        {
            if (userName != null)
            {
                var user = await _userRepository.TableNoTracking.Where(x => x.UserName == userName).FirstOrDefaultAsync();
                var isLockedOut = await _userManager.IsLockedOutAsync(user);
                if (isLockedOut)
                {
                    //user.IsActive = false;
                    //await _userRepository.UpdateAsync(user, true);
                    return true;
                }
            }
            return false;
        }
    }
}
