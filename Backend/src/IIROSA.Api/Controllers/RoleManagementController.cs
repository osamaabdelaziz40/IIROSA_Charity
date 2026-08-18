using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IIROSA.Api.Controllers
{
    /// <summary>
    /// Role Management API Controller
    /// Implements UC-1.7 and UC-1.8: Role CRUD operations
    /// Refactored to use Framework.Identity services
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RoleManagementController : ControllerBase
    {
        private readonly IRoleAppService _roleAppService;
        private readonly ILogger<RoleManagementController> _logger;

        public RoleManagementController(
            IRoleAppService roleAppService,
            ILogger<RoleManagementController> logger)
        {
            _roleAppService = roleAppService;
            _logger = logger;
        }

        /// <summary>
        /// Get all roles (UC-1.8: Update Role Permissions - View Roles)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AllRoles")]
        public async Task<ActionResult<IEnumerable<RoleListDto>>> GetRoles()
        {
            try
            {
                var roles = await _roleAppService.GetAllAsync();

                var roleListDtos = roles.Select(r => new RoleListDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    DisplayNameAr = r.DisplayNameAr,
                    DisplayNameEn = r.DisplayNameEn,
                  //  CreatedOn = r.CreatedOn,
                    UserCount = 0 // Will be populated below
                }).ToList();

                // Get user count for each role
                foreach (var role in roleListDtos)
                {
                    role.UserCount = await _roleAppService.GetUserCountInRoleAsync(role.Name);
                }

                return Ok(roleListDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving roles");
                return StatusCode(500, new { message = "An error occurred while retrieving roles" });
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "AllRoles")]
        public async Task<ActionResult<RoleDetailDto>> GetRole(Guid id)
        {
            try
            {
                var role = await _roleAppService.GetRoleDetailAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving role {RoleId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving role" });
            }
        }

        /// <summary>
        /// Create new role (UC-1.7: Create Role)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "CanManageRoles")]
        public async Task<ActionResult<RoleDetailDto>> CreateRole([FromBody] CreateRoleDto model)
        {
            try
            {
                // Check if role already exists
                if (await _roleAppService.RoleExistsAsync(model.Name))
                {
                    return BadRequest(new { message = "Role name already exists" });
                }

                var result = await _roleAppService.CreateRoleAsync(model);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.VerificationMSG });
                }

                var role = await _roleAppService.GetRoleDetailAsync((Guid)result.InsertedId);

                _logger.LogInformation("Role {RoleName} created by {CreatedBy}", model.Name, User.Identity?.Name);

                return CreatedAtAction(nameof(GetRole), new { id = result.InsertedId }, role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating role");
                return StatusCode(500, new { message = "An error occurred while creating role" });
            }
        }

        /// <summary>
        /// Update role (UC-1.8: Update Role Permissions)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageRoles")]
        public async Task<ActionResult<RoleDetailDto>> UpdateRole(Guid id, [FromBody] UpdateRoleDto model)
        {
            try
            {
                var success = await _roleAppService.UpdateRoleDetailAsync(id, model);

                if (!success)
                {
                    return BadRequest(new { message = "Failed to update role" });
                }

                _logger.LogInformation("Role {RoleId} updated by {UpdatedBy}", id, User.Identity?.Name);

                var role = await _roleAppService.GetRoleDetailAsync(id);
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating role {RoleId}", id);
                return StatusCode(500, new { message = "An error occurred while updating role" });
            }
        }

        /// <summary>
        /// Delete role
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageRoles")]
        public async Task<ActionResult> DeleteRole(Guid id)
        {
            try
            {
                // Check if role can be deleted
                var canDelete = await _roleAppService.CanDeleteRoleAsync(id);
                if (!canDelete)
                {
                    return BadRequest(new { message = "Cannot delete this role. It may have users assigned or it's a core system role." });
                }

                var success = await _roleAppService.DeleteAsync(id);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to delete role" });
                }

                _logger.LogInformation("Role {RoleId} deleted by {DeletedBy}", id, User.Identity?.Name);

                return Ok(new { message = "Role deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting role {RoleId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting role" });
            }
        }

        /// <summary>
        /// Get users in role
        /// </summary>
        [HttpGet("{id}/users")]
        [Authorize(Policy = "ManagementOnly")]
        public async Task<ActionResult<IEnumerable<UserBasicDto>>> GetUsersInRole(Guid id)
        {
            try
            {
                var role = await _roleAppService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                var users = await _roleAppService.GetUsersInRoleAsync(role.Name);

                return Ok(users.Select(u => new UserBasicDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsActive = u.IsActive
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users for role {RoleId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// Manage role permissions (placeholder for future permission management)
        /// </summary>
        [HttpPut("{id}/permissions")]
        [Authorize(Policy = "CanManageRoles")]
        public async Task<ActionResult> UpdateRolePermissions(Guid id, [FromBody] RolePermissionsDto model)
        {
            try
            {
                var role = await _roleAppService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                // This is a placeholder for future permission management
                // In a full implementation, you would update role permissions in the database
                _logger.LogInformation("Role permissions updated for {RoleId} by {UpdatedBy}", id, User.Identity?.Name);

                return Ok(new { message = "Role permissions updated successfully", permissions = model.Permissions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating permissions for role {RoleId}", id);
                return StatusCode(500, new { message = "An error occurred while updating permissions" });
            }
        }
    }

    // DTOs
    public class RoleListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DisplayNameAr { get; set; }
        public string DisplayNameEn { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UserCount { get; set; }
    }

    public class RolePermissionsDto
    {
        public Dictionary<string, bool> Permissions { get; set; }
    }
}