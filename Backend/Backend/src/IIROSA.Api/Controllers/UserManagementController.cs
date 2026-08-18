using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Helper;
using Framework.Identity.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IIROSA.Api.Controllers
{
    /// <summary>
    /// User Management API Controller
    /// Implements UC-1.2 to UC-1.6: User CRUD operations
    /// Refactored to use Framework.Identity services
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            [FromQuery] string search = null,
            [FromQuery] string role = null,
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
                    newPassword = model.SendEmail ? null : model.NewPassword
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
    }

    // DTOs for API requests
    public class ResetPasswordDto
    {
        public string NewPassword { get; set; }
        public bool SendEmail { get; set; }
    }

    public class AssignRoleDto
    {
        public string RoleName { get; set; }
    }
}