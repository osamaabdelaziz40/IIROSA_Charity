using DGA.Raqmi.Application.Services.Users.Dtos;
using Framework.Core;
using Framework.Core.AutoMapper;
using Framework.Core.Extensions;
using Framework.Core.Globalization;
using Framework.Core.SharedServices.Services;
using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Extensions;
using Framework.Identity.Data.Repositories;
using Framework.Identity.Data.Services.Interfaces;
using IIROSAUserManagementDtos = Framework.Identity.Data.Services.Interfaces;
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
    /// <summary>
    /// Extended User App Service for IIROSA User Role Management
    /// Implements additional methods beyond the base IUserAppService
    /// </summary>
    public class UserAppServiceExtended : IUserAppServiceExtended
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

        public UserAppServiceExtended(
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
            _roleRepository = roleRepository;
            _userRolesRepository = userRolesRepository;
            _appSettingsService = appSettingsService;
            _signInManager = signInManager;
            _userRoleAppService = userRoleAppService;
            _environment = environment;
            _roleAppService = roleAppService;
            _logger = logger;
        }

        public string CurrentUserName => _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(s => s.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value?.To<string>();
        public string CurrentUserEmail => GetClaimValueByKey("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

        // Implement existing IUserAppService methods
        public async Task<List<UserDto>> GetUsersInRoles(List<string> roleNames)
        {
            var users = new List<UserDto>();
            foreach (var role in roleNames)
            {
                users.AddRange(await FindAllByRoleNameAsync(role, true));
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
                     Email = a.Email,
                     UserName = a.UserName,
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

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _userRepository.TableNoTracking.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (user != null)
                return await _userRepository.DeleteAsync(u => u.Id == id, true);
            return false;
        }

        public async Task<UserDto> FindByEmailAsync(string email)
        {
            var result = await _userManager.FindByEmailAsync(email);
            return result.MapTo<UserDto>();
        }

        public async Task<bool> UpdateEmailConfirmed(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<ApiResponse<UserDto>> Register(UserRegister input)
        {
            throw new NotImplementedException("Use CreateUserAsync instead");
        }

        public async Task<ApiResponse<UserDto>> Login(LoginDto model)
        {
            throw new NotImplementedException("Use existing authentication methods");
        }

        public async Task<bool> ChangePassword(string username, ChangePasswordDto changePassword)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, changePassword.NewPassword);
            return result.Succeeded;
        }

        // Helper methods
        private async Task<List<UserDto>> FindAllByRoleNameAsync(string roleName, bool activeOnly = false)
        {
            var role = await _roleRepository.TableNoTracking.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return new List<UserDto>();

            var userIds = await _userRolesRepository.TableNoTracking
                .Where(ur => ur.RoleId == role.Id && (!activeOnly || ur.IsActive))
                .Select(ur => ur.UserId)
                .ToListAsync();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            return users.MapTo<List<UserDto>>();
        }

        private string GetClaimValueByKey(string key)
        {
            return _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(s => s.Type == key)?.Value;
        }

        #region NEW METHODS for IIROSA User Role Management

        public async Task<IIROSAUserManagementDtos.UserDetailDto> GetUserDetailAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new IIROSAUserManagementDtos.UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedOn = user.CreatedOn,
                UpdatedOn = user.UpdatedOn,
                PhoneNumber = user.PhoneNumber,
                PreferredNotificationLanguage = user.PreferredNotificationLanguage ?? "ar"
            };
        }

        public async Task<UserManagementInsertResultDto> CreateUserAsync(IIROSAUserManagementDtos.CreateUserDto userDto)
        {
            var result = new UserManagementInsertResultDto();

            var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                result.VerificationMSG = "EmailAlreadyExists";
                result.Success = false;
                return result;
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = userDto.Email,
                Email = userDto.Email,
                FullName = userDto.FullName,
                PhoneNumber = userDto.PhoneNumber,
                EmailConfirmed = true,
                IsActive = true,
                CreatedBy = CurrentUserName,
                CreatedOn = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var password = userDto.Password ?? "P@ssw0rd@2022";

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                result.VerificationMSG = string.Join(", ", createResult.Errors.Select(e => e.Description));
                result.Success = false;
                return result;
            }

            if (userDto.Roles != null && userDto.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, userDto.Roles);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    result.VerificationMSG = "RoleAssignmentFailed";
                    result.Success = false;
                    return result;
                }
            }

            result.InsertedId = user.Id;
            result.Success = true;
            return result;
        }

        public async Task<Guid> UpdateUserDetailAsync(Guid id, IIROSAUserManagementDtos.UpdateUserDto userDto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return Guid.Empty;

            if (!string.IsNullOrEmpty(userDto.Email) && userDto.Email != user.Email)
            {
                var emailExists = await _userManager.FindByEmailAsync(userDto.Email);
                if (emailExists != null)
                {
                    return Guid.Empty;
                }

                user.Email = userDto.Email;
                user.UserName = userDto.Email;
                user.NormalizedEmail = userDto.Email.ToUpperInvariant();
                user.NormalizedUserName = userDto.Email.ToUpperInvariant();
            }

            if (!string.IsNullOrEmpty(userDto.FullName))
            {
                user.FullName = userDto.FullName;
            }

            if (userDto.PhoneNumber != null)
            {
                user.PhoneNumber = userDto.PhoneNumber;
            }

            user.UpdatedBy = CurrentUserName;
            user.UpdatedOn = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Guid.Empty;
            }

            if (userDto.Roles != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(userDto.Roles).ToList();
                var rolesToAdd = userDto.Roles.Except(currentRoles).ToList();

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }
            }

            return user.Id;
        }

        public async Task<bool> SetUserActiveStatusAsync(Guid id, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;

            user.IsActive = isActive;
            user.UpdatedBy = CurrentUserName;
            user.UpdatedOn = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ResetUserPasswordAsync(Guid id, string newPassword = null)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;

            var password = newPassword ?? GenerateRandomPassword();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, password);

            return result.Succeeded;
        }

        public async Task<bool> AssignUserToRoleAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            if (string.IsNullOrEmpty(roleName)) return false;

            var roleExists = await _roleAppService.FindByRoleNameAsync(roleName);
            if (roleExists == null) return false;

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<bool> RemoveUserFromRoleAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<IIROSAUserManagementDtos.UserListResponseDto> GetUsersFilteredAsync(IIROSAUserManagementDtos.UserFilterDto filter)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                users = users.Where(u =>
                    u.Email.Contains(filter.SearchText) ||
                    u.UserName.Contains(filter.SearchText) ||
                    (u.FullName != null && u.FullName.Contains(filter.SearchText)));
            }

            if (!string.IsNullOrEmpty(filter.Role))
            {
                var roleUsers = await _userManager.GetUsersInRoleAsync(filter.Role);
                var roleUserIds = roleUsers.Select(u => u.Id).ToList();
                users = users.Where(u => roleUserIds.Contains(u.Id));
            }

            if (filter.IsActive.HasValue)
            {
                users = users.Where(u => u.IsActive == filter.IsActive.Value);
            }

            var totalCount = await users.CountAsync();

            var pagedUsers = await users
                .OrderByDescending(u => u.CreatedOn)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var userDtos = new List<IIROSAUserManagementDtos.UserListItemDto>();
            foreach (var user in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new IIROSAUserManagementDtos.UserListItemDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    CreatedOn = user.CreatedOn,
                    LastLogin = null
                });
            }

            return new IIROSAUserManagementDtos.UserListResponseDto
            {
                Items = userDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
            };
        }

        private string GenerateRandomPassword()
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "@#$%^&*";

            var random = new Random();
            var password = new char[12];

            password[0] = lowercase[random.Next(lowercase.Length)];
            password[1] = uppercase[random.Next(uppercase.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = special[random.Next(special.Length)];

            const string allChars = lowercase + uppercase + digits + special;
            for (int i = 4; i < password.Length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            return new string(password.OrderBy(c => random.Next()).ToArray());
        }

        #endregion
    }
}
