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
    public class UserAppServiceExtended : UserAppService, IUserAppServiceExtended
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
            : base(userManager, httpContextAccessor, roleRepository, userRolesRepository,
                  appSettingsService, userRepository, signInManager, userRoleAppService,
                  environment, roleAppService, logger)
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

        #region NEW METHODS for IIROSA User Role Management

        /// <summary>
        /// Get user details with roles
        /// </summary>
        public async Task<UserDetailDto> GetUserDetailAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDetailDto
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

        /// <summary>
        /// Create new user with roles
        /// </summary>
        public async Task<UserManagementInsertResultDto> CreateUserAsync(CreateUserDto userDto)
        {
            var result = new UserManagementInsertResultDto();

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                result.VerificationMSG = "EmailAlreadyExists";
                result.Success = false;
                return result;
            }

            // Create new user
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

            // Use default password if not provided
            var password = userDto.Password ?? "P@ssw0rd@2022";

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                result.VerificationMSG = string.Join(", ", createResult.Errors.Select(e => e.Description));
                result.Success = false;
                return result;
            }

            // Assign roles
            if (userDto.Roles != null && userDto.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, userDto.Roles);
                if (!roleResult.Succeeded)
                {
                    // Rollback user creation if role assignment fails
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

        /// <summary>
        /// Update user details
        /// </summary>
        public async Task<Guid> UpdateUserDetailAsync(Guid id, UpdateUserDto userDto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return Guid.Empty;

            // Update email if changed
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

            // Update other fields
            if (!string.IsNullOrEmpty(userDto.FullName))
            {
                user.FullName = userDto.FullName;
            }

            if (userDto.PhoneNumber != null)
            {
                user.PhoneNumber = userDto.PhoneNumber;
            }

            if (userDto.IsActive.HasValue)
            {
                user.IsActive = userDto.IsActive.Value;
            }

            user.UpdatedBy = CurrentUserName;
            user.UpdatedOn = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Guid.Empty;
            }

            // Update roles if provided
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

        /// <summary>
        /// Set user active status
        /// </summary>
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

        /// <summary>
        /// Reset user password
        /// </summary>
        public async Task<bool> ResetUserPasswordAsync(Guid id, string? newPassword = null)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;

            // Generate new password if not provided
            var password = newPassword ?? GenerateRandomPassword();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, password);

            return result.Succeeded;
        }

        /// <summary>
        /// Assign user to role
        /// </summary>
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

        /// <summary>
        /// Remove user from role
        /// </summary>
        public async Task<bool> RemoveUserFromRoleAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        /// <summary>
        /// Get user roles
        /// </summary>
        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        /// <summary>
        /// Get users with filtering and pagination
        /// </summary>
        public async Task<UserListResponseDto> GetUsersFilteredAsync(UserFilterDto filter)
        {
            var users = _userManager.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                users = users.Where(u =>
                    u.Email.Contains(filter.SearchText) ||
                    u.UserName.Contains(filter.SearchText) ||
                    (u.FullName != null && u.FullName.Contains(filter.SearchText)));
            }

            // Apply role filter
            if (!string.IsNullOrEmpty(filter.Role))
            {
                var roleUsers = await _userManager.GetUsersInRoleAsync(filter.Role);
                var roleUserIds = roleUsers.Select(u => u.Id).ToList();
                users = users.Where(u => roleUserIds.Contains(u.Id));
            }

            // Apply active status filter
            if (filter.IsActive.HasValue)
            {
                users = users.Where(u => u.IsActive == filter.IsActive.Value);
            }

            // Get total count for pagination
            var totalCount = await users.CountAsync();

            // Apply pagination
            var pagedUsers = await users
                .OrderByDescending(u => u.CreatedOn)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Map to DTOs
            var userDtos = new List<UserListItemDto>();
            foreach (var user in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserListItemDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    CreatedOn = user.CreatedOn,
                    LastLogin = null // Can be populated from UserLogins if needed
                });
            }

            return new UserListResponseDto
            {
                Items = userDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
            };
        }

        #endregion

        #region Helper Methods

        private string GenerateRandomPassword()
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "@#$%^&*";

            var random = new Random();
            var password = new char[12];

            // Ensure at least one of each type
            password[0] = lowercase[random.Next(lowercase.Length)];
            password[1] = uppercase[random.Next(uppercase.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = special[random.Next(special.Length)];

            // Fill the rest randomly
            const string allChars = lowercase + uppercase + digits + special;
            for (int i = 4; i < password.Length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            // Shuffle the password
            return new string(password.OrderBy(c => random.Next()).ToArray());
        }

        #endregion
    }
}