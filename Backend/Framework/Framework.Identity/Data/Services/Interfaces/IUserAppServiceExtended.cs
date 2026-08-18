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
        Task<bool> ResetUserPasswordAsync(Guid id, string? newPassword = null);
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