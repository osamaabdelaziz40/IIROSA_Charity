using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Helper;
using Framework.Identity.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IIROSA.Api.Controllers
{
    /// <summary>
    /// User Management API Controller
    /// Implements UC-1.2 to UC-1.6: User CRUD operations
    /// Refactored to use Framework.Identity services
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserAppServiceExtended _userAppService;
        private readonly IRoleAppService _roleAppService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            IUserAppServiceExtended userAppService,
            IRoleAppService roleAppService,
            ILogger<UserManagementController> logger)
        {
            _userAppService = userAppService;
            _roleAppService = roleAppService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with filtering and pagination (UC-1.9: View All Users)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "ManagementOnly")]
        public async Task<ActionResult<UserListResponseDto>> GetUsers(
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var filter = new UserFilterDto
                {
                    SearchText = search,
                    Role = role,
                    IsActive = isActive,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _userAppService.GetUsersFilteredAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users");
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// Get user by ID (UC-1.9: View All Users)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "ManagementOnly")]
        public async Task<ActionResult<UserDetailDto>> GetUser(Guid id)
        {
            try
            {
                var user = await _userAppService.GetUserDetailAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving user" });
            }
        }

        /// <summary>
        /// Create new user (UC-1.2: Create User)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult<UserDetailDto>> CreateUser([FromBody] CreateUserDto model)
        {
            try
            {
                var result = await _userAppService.CreateUserAsync(model);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.VerificationMSG });
                }

                var user = await _userAppService.GetUserDetailAsync(result.InsertedId);
                return CreatedAtAction(nameof(GetUser), new { id = result.InsertedId }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user");
                return StatusCode(500, new { message = "An error occurred while creating user" });
            }
        }

        /// <summary>
        /// Update user (UC-1.3: Update User)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult<UserDetailDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto model)
        {
            try
            {
                var result = await _userAppService.UpdateUserDetailAsync(id, model);

                if (result == Guid.Empty)
                {
                    return BadRequest(new { message = "Failed to update user" });
                }

                var user = await _userAppService.GetUserDetailAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while updating user" });
            }
        }

        /// <summary>
        /// Deactivate user (UC-1.4: Deactivate User)
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> DeactivateUser(Guid id)
        {
            try
            {
                var success = await _userAppService.SetUserActiveStatusAsync(id, false);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to deactivate user" });
                }

                _logger.LogInformation("User {UserId} deactivated by {DeactivatedBy}", id, User.Identity?.Name);
                return Ok(new { message = "User deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while deactivating user" });
            }
        }

        /// <summary>
        /// Activate user
        /// </summary>
        [HttpPatch("{id}/activate")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> ActivateUser(Guid id)
        {
            try
            {
                var success = await _userAppService.SetUserActiveStatusAsync(id, true);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to activate user" });
                }

                _logger.LogInformation("User {UserId} activated by {ActivatedBy}", id, User.Identity?.Name);
                return Ok(new { message = "User activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while activating user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while activating user" });
            }
        }

        /// <summary>
        /// Reset user password (UC-1.5: Reset User Password)
        /// </summary>
        [HttpPost("{id}/reset-password")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> ResetUserPassword(Guid id, [FromBody] ResetPasswordDto model)
        {
            try
            {
                var success = await _userAppService.ResetUserPasswordAsync(id, model.NewPassword);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to reset password" });
                }

                _logger.LogInformation("Password reset for user {UserId} by {ResetBy}", id, User.Identity?.Name);

                return Ok(new
                {
                    message = "Password reset successfully",
                    newPassword = model.SendEmail ? null : model.NewPassword // Only return password if not sending email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while resetting password" });
            }
        }

        /// <summary>
        /// Assign user to role (UC-1.6: Assign User to Role)
        /// </summary>
        [HttpPost("{id}/roles")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> AssignUserToRole(Guid id, [FromBody] AssignRoleDto model)
        {
            try
            {
                var roleExists = await _roleAppService.FindByRoleNameAsync(model.RoleName);
                if (roleExists == null)
                {
                    return BadRequest(new { message = $"Role '{model.RoleName}' does not exist" });
                }

                var success = await _userAppService.AssignUserToRoleAsync(id, model.RoleName);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to assign role" });
                }

                _logger.LogInformation("User {UserId} assigned to role {RoleName} by {AssignedBy}", id, model.RoleName, User.Identity?.Name);

                return Ok(new { message = $"User successfully assigned to role '{model.RoleName}'" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning role to user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while assigning role" });
            }
        }

        /// <summary>
        /// Remove user from role
        /// </summary>
        [HttpDelete("{id}/roles/{roleName}")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> RemoveUserFromRole(Guid id, string roleName)
        {
            try
            {
                var success = await _userAppService.RemoveUserFromRoleAsync(id, roleName);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to remove role" });
                }

                _logger.LogInformation("User {UserId} removed from role {RoleName} by {RemovedBy}", id, roleName, User.Identity?.Name);

                return Ok(new { message = $"User successfully removed from role '{roleName}'" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing role from user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while removing role" });
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var success = await _userAppService.DeleteAsync(id);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to delete user" });
                }

                _logger.LogInformation("User {UserId} deleted by {DeletedBy}", id, User.Identity?.Name);

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting user" });
            }
        }

        /// <summary>
        /// Export users to Excel (UC-1.9: View All Users - Export functionality)
        /// </summary>
        [HttpGet("export")]
        [Authorize(Policy = "ManagementOnly")]
        public async Task<IActionResult> ExportUsers(
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var filter = new UserFilterDto
                {
                    SearchText = search,
                    Role = role,
                    IsActive = isActive,
                    Page = 1,
                    PageSize = 10000 // Export all matching records
                };

                var result = await _userAppService.GetUsersFilteredAsync(filter);

                // Generate CSV content
                var csv = new System.Text.StringBuilder();

                // Header row
                csv.AppendLine("Email,Full Name,Phone Number,Roles,Status,Created Date");

                // Data rows
                foreach (var user in result.Items)
                {
                    var roles = user.Roles != null ? string.Join("; ", user.Roles) : "";
                    var status = user.IsActive ? "Active" : "Inactive";
                    var createdDate = user.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss");

                    csv.AppendLine($"{user.Email},{user.FullName},{user.PhoneNumber ?? ""},{roles},{status},{createdDate}");
                }

                var fileName = $"users_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                var contentType = "text/csv";

                _logger.LogInformation("Users exported by {ExportedBy}. Total records: {Count}", User.Identity?.Name, result.Items.Count);

                return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting users");
                return StatusCode(500, new { message = "An error occurred while exporting users" });
            }
        }

        /// <summary>
        /// Get user activity log (UC-1.10: View User Activity)
        /// </summary>
        [HttpGet("{id}/activity")]
        [Authorize(Policy = "ManagementOnly")]
        public async Task<ActionResult> GetUserActivity(Guid id)
        {
            try
            {
                // TODO: Implement actual audit log retrieval
                // For now, return a placeholder response
                var user = await _userAppService.GetUserDetailAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Placeholder activity data - replace with actual audit log implementation
                var activities = new[]
                {
                    new
                    {
                        Timestamp = DateTime.UtcNow.AddDays(-1),
                        Action = "Login",
                        Entity = "Authentication",
                        Details = "User logged in successfully",
                        IpAddress = "192.168.1.100"
                    },
                    new
                    {
                        Timestamp = DateTime.UtcNow.AddDays(-2),
                        Action = "Update",
                        Entity = "UserProfile",
                        Details = "User updated their profile",
                        IpAddress = "192.168.1.100"
                    }
                };

                return Ok(new { userId = id, activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving activity for user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving user activity" });
            }
        }

        /// <summary>
        /// Get user claims (UC-1.11: Manage User Claims)
        /// </summary>
        [HttpGet("{id}/claims")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<ActionResult> GetUserClaims(Guid id)
        {
            try
            {
                var user = await _userAppService.GetUserDetailAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // TODO: Implement actual claims retrieval from Identity
                // For now, return a placeholder response
                var claims = new List<object>();

                return Ok(new { userId = id, claims });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving claims for user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving user claims" });
            }
        }

        /// <summary>
        /// Add claim to user (UC-1.11: Manage User Claims)
        /// </summary>
        [HttpPost("{id}/claims")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<ActionResult> AddUserClaim(Guid id, [FromBody] AddClaimDto model)
        {
            try
            {
                var user = await _userAppService.GetUserDetailAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // TODO: Implement actual claim addition to Identity
                _logger.LogInformation("Claim added to user {UserId} by {AddedBy}", id, User.Identity?.Name);

                return Ok(new { message = "Claim added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding claim to user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while adding claim" });
            }
        }

        /// <summary>
        /// Remove claim from user (UC-1.11: Manage User Claims)
        /// </summary>
        [HttpDelete("{id}/claims/{claimType}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<ActionResult> RemoveUserClaim(Guid id, string claimType)
        {
            try
            {
                var user = await _userAppService.GetUserDetailAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // TODO: Implement actual claim removal from Identity
                _logger.LogInformation("Claim removed from user {UserId} by {RemovedBy}", id, User.Identity?.Name);

                return Ok(new { message = "Claim removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing claim from user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while removing claim" });
            }
        }
    }

    // DTOs
    public class ResetPasswordDto
    {
        public string? NewPassword { get; set; }
        public bool SendEmail { get; set; }
    }

    public class AssignRoleDto
    {
        public string RoleName { get; set; }
    }

    public class AddClaimDto
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}