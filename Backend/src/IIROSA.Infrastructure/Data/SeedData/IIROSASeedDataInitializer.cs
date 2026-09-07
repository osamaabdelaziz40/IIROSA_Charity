using Framework.Identity.Data;
using Framework.Identity.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IIROSA.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace IIROSA.Infrastructure.Data.SeedData
{
    /// <summary>
    /// Main seed data initializer for IIROSA system
    /// Implements UC-1.1: Seed Roles and Users (System Initialization)
    /// </summary>
    public class IIROSASeedDataInitializer
    {
        private readonly AppIdentityDbContext _identityContext;
        private readonly ApplicationDbContext _appContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<IIROSASeedDataInitializer> _logger;

        public IIROSASeedDataInitializer(
            AppIdentityDbContext identityContext,
            ApplicationDbContext appContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<IIROSASeedDataInitializer> _logger)
        {
            _identityContext = identityContext;
            _appContext = appContext;
            _userManager = userManager;
            _roleManager = roleManager;
            this._logger = _logger;
        }

        /// <summary>
        /// Initialize seed data for IIROSA system
        /// This implements UC-1.1: Seed Roles and Users (System Initialization)
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Starting IIROSA seed data initialization...");
                _logger.LogInformation("Database state: {DatabaseState}",
                    await _identityContext.Database.CanConnectAsync() ? "Connected" : "Disconnected");

                // Check if this is first run or re-seeding
                var existingRoles = await _identityContext.Roles.CountAsync();
                var existingUsers = await _identityContext.Users.CountAsync();

                _logger.LogInformation("Current database state: {RoleCount} roles, {UserCount} users",
                    existingRoles, existingUsers);

                // Step 1: Seed roles first (users depend on roles)
                _logger.LogInformation("Step 1: Seeding IIROSA roles...");
                await IIROSARoleSeedData.SeedRolesAsync(_identityContext, _logger);
                _logger.LogInformation("Roles seeding completed successfully");

                // Step 2: Seed users (depends on roles)
                _logger.LogInformation("Step 2: Seeding IIROSA users...");
                await IIROSAUserSeedData.SeedUsersAsync(_identityContext, _userManager, _roleManager, _logger);
                _logger.LogInformation("Users seeding completed successfully");

                // Step 3: Seed Technical Support lookup data
                _logger.LogInformation("Step 3: Seeding Technical Support lookup data...");
                await TechnicalSupportSeedData.SeedAllAsync(_appContext);
                _logger.LogInformation("Technical Support lookup data seeding completed successfully");

                // Step 4: Seed Missions module lookup data (epic 15)
                _logger.LogInformation("Step 4: Seeding Missions lookup data...");
                await MissionLookupSeedData.SeedAllAsync(_appContext);
                _logger.LogInformation("Missions lookup data seeding completed successfully");

                // Step 5: Seed Correspondence module lookup data (epic 16)
                _logger.LogInformation("Step 5: Seeding Correspondence lookup data...");
                await CorrespondenceLookupSeedData.SeedAllAsync(_appContext);
                _logger.LogInformation("Correspondence lookup data seeding completed successfully");

                // Step 6: Seed Periodic Orphan Reports refuse-reason catalogue (epic 9, UC-ORR-08)
                _logger.LogInformation("Step 6: Seeding Periodic Reports refuse reasons...");
                await RefuseReasonSeedData.SeedRefuseReasonsAsync(_appContext);
                _logger.LogInformation("Periodic Reports refuse reasons seeding completed successfully");

                // Step 7: Seed Housing module catalogue (epic 6, UC-HOU-05)
                _logger.LogInformation("Step 7: Seeding Housing buildings and flats...");
                await HousingLookupSeedData.SeedAllAsync(_appContext);
                _logger.LogInformation("Housing catalogue seeding completed successfully");

                // Step 8: Seed Refugee register lookups (epic 7, UC-REF-03)
                _logger.LogInformation("Step 8: Seeding Refugee register lookups...");
                await RefugeeLookupSeedData.SeedAllAsync(_appContext);
                _logger.LogInformation("Refugee register lookup seeding completed successfully");

                // Step 9: Seed guardian reference catalogue (epic 19, UC-SYS-05)
                _logger.LogInformation("Step 9: Seeding guardian marital statuses...");
                await GuardianLookupSeedData.SeedAllAsync(_appContext);
                _logger.LogInformation("Guardian reference catalogue seeding completed successfully");

                // Step 10: Seed the bank catalogue starter set (epic 19, UC-SYS-08)
                _logger.LogInformation("Step 10: Seeding starter bank catalogue...");
                await BankSeedData.SeedBanksAsync(_appContext);
                _logger.LogInformation("Bank catalogue seeding completed successfully");

                // Step 11: Fill country NID validation rules (epic 19, UC-SYS-11)
                _logger.LogInformation("Step 11: Seeding country NID rules...");
                await CountryNidRuleSeedData.SeedNidRulesAsync(_appContext);
                _logger.LogInformation("Country NID rules seeding completed successfully");

                // Step 12: Seed Office Development Projects type catalogue (UC-7.1)
                _logger.LogInformation("Step 12: Seeding office project types...");
                await OfficeProjectTypeSeedData.SeedOfficeProjectTypesAsync(_appContext);
                _logger.LogInformation("Office project type seeding completed successfully");

                // Final state
                var finalRoles = await _identityContext.Roles.CountAsync();
                var finalUsers = await _identityContext.Users.CountAsync();

                _logger.LogInformation("Seed data initialization completed successfully");
                _logger.LogInformation("Final database state: {RoleCount} roles, {UserCount} users",
                    finalRoles, finalUsers);

                // Display summary
                await DisplaySeedDataSummaryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during seed data initialization");
                throw;
            }
        }

        /// <summary>
        /// Display summary of seeded data
        /// </summary>
        private async Task DisplaySeedDataSummaryAsync()
        {
            try
            {
                var roles = await _identityContext.Roles.ToListAsync();
                var users = await _identityContext.Users.Include(u => u.UserRoles).ToListAsync();

                _logger.LogInformation("=== IIROSA Seed Data Summary ===");
                _logger.LogInformation("Seeded Roles ({Count}):", roles.Count);
                foreach (var role in roles)
                {
                    var userCount = await _userManager.GetUsersInRoleAsync(role.Name);
                    _logger.LogInformation("  - {RoleName} ({DisplayNameEn}): {UserCount} users",
                        role.Name, role.DisplayNameEn, userCount.Count);
                }

                _logger.LogInformation("Seeded Users ({Count}):", users.Count);
                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    _logger.LogInformation("  - {Email} ({FullName}): Roles = {Roles}",
                        user.Email, user.FullName, string.Join(", ", userRoles));
                }

                _logger.LogInformation("=== End of Summary ===");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not generate seed data summary");
            }
        }

        /// <summary>
        /// Verify seed data integrity
        /// </summary>
        public async Task<bool> VerifySeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Verifying IIROSA seed data integrity...");

                // Verify all required roles exist
                var requiredRoles = new[] { "SuperAdmin", "Admin", "Charity", "Accountant", "FinancialOfficer" };
                foreach (var roleName in requiredRoles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role == null)
                    {
                        _logger.LogError("Required role not found: {RoleName}", roleName);
                        return false;
                    }
                }

                // Verify all required users exist
                var requiredUsers = new[]
                {
                    "OsamaSuper@IIROSA.com",
                    "Admin@IIROSA.com",
                    "Charity@IIROSA.com",
                    "Accountant@IIROSA.com",
                    "FinancialOfficer@IIROSA.com"
                };

                foreach (var userEmail in requiredUsers)
                {
                    var user = await _userManager.FindByEmailAsync(userEmail);
                    if (user == null)
                    {
                        _logger.LogError("Required user not found: {UserEmail}", userEmail);
                        return false;
                    }

                    // Verify user has at least one role
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (userRoles.Count == 0)
                    {
                        _logger.LogError("User {UserEmail} has no roles assigned", userEmail);
                        return false;
                    }
                }

                _logger.LogInformation("Seed data verification passed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during seed data verification");
                return false;
            }
        }

        /// <summary>
        /// Reset seed data (use with caution - deletes all seeded data)
        /// </summary>
        public async Task ResetSeedDataAsync()
        {
            try
            {
                _logger.LogWarning("Starting seed data reset - this will delete all seeded data");

                // Delete seeded users first (due to foreign key constraints)
                var userIds = IIROSAUserSeedData.GetAllUserIds();
                foreach (var userId in userIds)
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("Deleted user: {Email}", user.Email);
                        }
                        else
                        {
                            _logger.LogError("Failed to delete user {Email}: {Errors}",
                                user.Email,
                                string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }

                // Delete seeded roles
                var roleIds = IIROSARoleSeedData.GetAllRoleIds();
                foreach (var roleId in roleIds)
                {
                    var role = await _roleManager.FindByIdAsync(roleId.ToString());
                    if (role != null)
                    {
                        var result = await _roleManager.DeleteAsync(role);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("Deleted role: {Name}", role.Name);
                        }
                        else
                        {
                            _logger.LogError("Failed to delete role {Name}: {Errors}",
                                role.Name,
                                string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }

                _logger.LogWarning("Seed data reset completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during seed data reset");
                throw;
            }
        }
    }
}