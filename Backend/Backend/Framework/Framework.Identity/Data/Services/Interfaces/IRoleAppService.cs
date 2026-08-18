using Framework.Identity.Data.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Services.Interfaces
{
    public interface IRoleAppService
    {
        // Existing methods
        Task<RoleDto> FindByRoleNameAsync(string roleName);
        Task<IEnumerable<SelectListItem>> List();
        Task<bool> DeleteAsync(Guid id);
        Task<List<RoleDto>> GetRolesByIds(List<Guid> ids);
        Task<List<RoleDto>> GetAllAsync();
        Task<RoleManagementInsertResultDto> InsertAsync(RoleDto role);
        Task<RoleDto> GetRoleByIdAsync(Guid id);
        Task<Guid> UpdateAsync(RoleDto input);

        // NEW METHODS for IIROSA User Role Management
        Task<RoleDetailDto> GetRoleDetailAsync(Guid id);
        Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleName);
        Task<int> GetUserCountInRoleAsync(string roleName);
        Task<bool> RoleExistsAsync(string roleName);
        Task<RoleManagementInsertResultDto> CreateRoleAsync(CreateRoleDto role);
        Task<bool> UpdateRoleDetailAsync(Guid id, UpdateRoleDto role);
        Task<bool> CanDeleteRoleAsync(Guid id);
    }

    // NEW DTOs for IIROSA User Role Management
    public class RoleDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DisplayNameAr { get; set; }
        public string DisplayNameEn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int UserCount { get; set; }
        public List<UserBasicDto> Users { get; set; }
    }

    public class UserBasicDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateRoleDto
    {
        public string Name { get; set; }
        public string DisplayNameAr { get; set; }
        public string DisplayNameEn { get; set; }
    }

    public class UpdateRoleDto
    {
        public string DisplayNameAr { get; set; }
        public string DisplayNameEn { get; set; }
    }
}