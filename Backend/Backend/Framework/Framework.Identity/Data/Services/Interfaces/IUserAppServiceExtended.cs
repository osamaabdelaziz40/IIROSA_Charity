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
        Task<bool> ResetUserPasswordAsync(Guid id, string newPassword = null);
        Task<bool> AssignUserToRoleAsync(Guid userId, string roleName);
        Task<bool> RemoveUserFromRoleAsync(Guid userId, string roleName);
        Task<List<string>> GetUserRolesAsync(Guid userId);
        Task<UserListResponseDto> GetUsersFilteredAsync(UserFilterDto filter);
    }
}
