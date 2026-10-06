using DGA.Raqmi.Application.Services.Users.Dtos;
using Framework.Core;
using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Helper;
using Framework.Identity.Data.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Services.Interfaces
{
    // Extended interface for IIROSA User Role Management
    public interface IUserAppServiceExtended : IUserAppService
    {
        // NEW METHODS for IIROSA User Role Management
        Task<UserDetailDto> GetUserDetailAsync(Guid id);
        Task<UserManagementInsertResultDto> CreateUserAsync(CreateUserDto user);
        Task<Guid> UpdateUserDetailAsync(Guid id, UpdateUserDto user);
        Task<bool> SetUserActiveStatusAsync(Guid id, bool isActive);

        /// <summary>
        /// Resets the user's password and returns the password that was applied — the value the
        /// caller must display to the operator, since it is generated server-side whenever
        /// <paramref name="newPassword"/> is null and is unrecoverable afterwards. Returns null
        /// when the reset failed (unknown user, locked account, password policy violation).
        /// </summary>
        Task<string?> ResetUserPasswordAsync(Guid id, string? newPassword = null);

        /// <summary>
        /// Finds the login account bound to a charity through its CharityId tenancy column.
        /// Used to heal charities whose Charity.UserId back-reference was never written.
        /// </summary>
        Task<UserDto?> FindByCharityIdAsync(Guid charityId);
        Task<bool> AssignUserToRoleAsync(Guid userId, string roleName);
        Task<bool> RemoveUserFromRoleAsync(Guid userId, string roleName);
        Task<List<string>> GetUserRolesAsync(Guid userId);
        Task<UserListResponseDto> GetUsersFilteredAsync(UserFilterDto filter);
    }

    // NEW DTOs for IIROSA User Role Management
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? PhoneNumber { get; set; }
        public string PreferredNotificationLanguage { get; set; }
    }

    public class UserManagementInsertResultDto
    {
        public Guid InsertedId { get; set; }
        public string VerificationMSG { get; set; }
        public bool Success { get; set; }
    }

    public class CreateUserDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
        public string? Password { get; set; }

        /// <summary>
        /// The charity this user belongs to. Required for any account holding the Charity role:
        /// without it the account has no tenancy claim and the application layer, which fails
        /// closed on an unscopeable caller, will show them nothing.
        /// </summary>
        public Guid? CharityId { get; set; }

        /// <summary>The country this user operates in.</summary>
        public int? CountryId { get; set; }
    }

    public class UpdateUserDto
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class UserFilterDto
    {
        public string? SearchText { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UserListResponseDto
    {
        public List<UserListItemDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserListItemDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}