using Framework.Identity.Data;
using Framework.Identity.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IIROSA.Infrastructure.Data.SeedData
{
    /// <summary>
    /// Seed data for IIROSA users as per UC-1.1 specifications
    /// Implements the seed users with default password: P@ssw0rd@2022
    /// </summary>
    public static class IIROSAUserSeedData
    {
        // User GUIDs - consistent across deployments
        public static readonly Guid OsamaSuperUserId = Guid.Parse("A1111111-2222-3333-4444-555555555555");
        public static readonly Guid AdminUserId = Guid.Parse("B2222222-3333-4444-5555-666666666666");
        public static readonly Guid CharityUserId = Guid.Parse("C3333333-4444-5555-6666-777777777777");
        public static readonly Guid AccountantUserId = Guid.Parse("D4444444-5555-6666-7777-888888888888");
        public static readonly Guid FinancialOfficerUserId = Guid.Parse("E5555555-6666-7777-8888-999999999999");

        // Default password for all seed users as per UC-1.1
        private const string DefaultPassword = "P@ssw0rd@2022";

        /// <summary>
        /// Seed IIROSA users to the database
        /// </summary>
        public static async Task SeedUsersAsync(
            AppIdentityDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger logger)
        {
            try
            {
                // Check if users already exist
                var existingUserEmails = new[] { "OsamaSuper@IIROSA.com", "Admin@IIROSA.com", "Charity@IIROSA.com", "Accountant@IIROSA.com", "FinancialOfficer@IIROSA.com" };
                var existingUsers = await context.Users
                    .Where(u => existingUserEmails.Contains(u.Email))
                    .ToListAsync();

                if (existingUsers.Any())
                {
                    logger.LogWarning("IIROSA users already exist. Skipping user seeding. Found users: {Users}",
                        string.Join(", ", existingUsers.Select(u => u.Email)));
                    return;
                }

                // Ensure roles exist first
                await EnsureRolesExistAsync(roleManager, logger);

                // Create IIROSA specific users as per UC-1.1
                var seedUsers = new[]
                {
                    new
                    {
                        Id = OsamaSuperUserId,
                        Email = "OsamaSuper@IIROSA.com",
                        UserName = "OsamaSuper@IIROSA.com",
                        FullName = "Osama Abdelaziz",
                        RoleName = "SuperAdmin",
                        RoleId = IIROSARoleSeedData.SuperAdminRoleId,
                        IsActive = true,
                        AdditionalInfo = "System owner"
                    },
                    new
                    {
                        Id = AdminUserId,
                        Email = "Admin@IIROSA.com",
                        UserName = "Admin@IIROSA.com",
                        FullName = "System Admin",
                        RoleName = "Admin",
                        RoleId = IIROSARoleSeedData.AdminRoleId,
                        IsActive = true,
                        AdditionalInfo = "Administrative user"
                    },
                    new
                    {
                        Id = CharityUserId,
                        Email = "Charity@IIROSA.com",
                        UserName = "Charity@IIROSA.com",
                        FullName = "Charity User",
                        RoleName = "Charity",
                        RoleId = IIROSARoleSeedData.CharityRoleId,
                        IsActive = true,
                        AdditionalInfo = "Charity operations"
                    },
                    new
                    {
                        Id = AccountantUserId,
                        Email = "Accountant@IIROSA.com",
                        UserName = "Accountant@IIROSA.com",
                        FullName = "Accountant User",
                        RoleName = "Accountant",
                        RoleId = IIROSARoleSeedData.AccountantRoleId,
                        IsActive = true,
                        AdditionalInfo = "Financial operations"
                    },
                    new
                    {
                        Id = FinancialOfficerUserId,
                        Email = "FinancialOfficer@IIROSA.com",
                        UserName = "FinancialOfficer@IIROSA.com",
                        FullName = "Financial Officer",
                        RoleName = "FinancialOfficer",
                        RoleId = IIROSARoleSeedData.FinancialOfficerRoleId,
                        IsActive = true,
                        AdditionalInfo = "Financial oversight"
                    }
                };

                int createdUsersCount = 0;
                var errors = new System.Collections.Generic.List<string>();

                foreach (var seedUser in seedUsers)
                {
                    try
                    {
                        // Check if user already exists
                        var existingUser = await userManager.FindByEmailAsync(seedUser.Email);
                        if (existingUser != null)
                        {
                            logger.LogWarning("User {Email} already exists. Skipping creation.", seedUser.Email);
                            continue;
                        }

                        // Create new user
                        var user = new ApplicationUser
                        {
                            Id = seedUser.Id,
                            UserName = seedUser.UserName,
                            Email = seedUser.Email,
                            FullName = seedUser.FullName,
                            EmailConfirmed = true,
                            IsActive = seedUser.IsActive,
                            CreatedBy = "SYSTEM",
                            CreatedOn = DateTime.UtcNow,
                            SecurityStamp = Guid.NewGuid().ToString()
                        };

                        var result = await userManager.CreateAsync(user, DefaultPassword);

                        if (result.Succeeded)
                        {
                            // Assign user to role
                            var roleResult = await userManager.AddToRoleAsync(user, seedUser.RoleName);

                            if (roleResult.Succeeded)
                            {
                                createdUsersCount++;
                                logger.LogInformation("Successfully created user {Email} and assigned to role {RoleName}",
                                    seedUser.Email, seedUser.RoleName);
                            }
                            else
                            {
                                errors.Add($"Failed to assign role {seedUser.RoleName} to user {seedUser.Email}: " +
                                          string.Join(", ", roleResult.Errors.Select(e => e.Description)));

                                // Rollback user creation if role assignment fails
                                await userManager.DeleteAsync(user);
                            }
                        }
                        else
                        {
                            errors.Add($"Failed to create user {seedUser.Email}: " +
                                      string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Exception creating user {seedUser.Email}: {ex.Message}");
                        logger.LogError(ex, "Error creating user {Email}", seedUser.Email);
                    }
                }

                if (createdUsersCount > 0)
                {
                    logger.LogInformation("Successfully seeded {Count} IIROSA users out of {Total}",
                        createdUsersCount, seedUsers.Length);
                }

                if (errors.Any())
                {
                    logger.LogError("Encountered {ErrorCount} errors during user seeding:\n{Errors}",
                        errors.Count, string.Join("\n", errors));
                }

                // Log seed data metadata
                await LogSeedDataAsync(context, "Users", createdUsersCount, "1.0");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error occurred while seeding IIROSA users");
                throw;
            }
        }

        /// <summary>
        /// Ensure all IIROSA roles exist before creating users
        /// </summary>
        private static async Task EnsureRolesExistAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
        {
            var roleNames = new[] { "SuperAdmin", "Admin", "Charity", "Accountant", "FinancialOfficer" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    logger.LogWarning("Role {RoleName} does not exist. Creating it now.", roleName);

                    var roleId = IIROSARoleSeedData.GetRoleIdByRoleName(roleName);
                    var role = new ApplicationRole
                    {
                        Id = roleId,
                        Name = roleName,
                        NormalizedName = roleName.ToUpperInvariant(),
                        DisplayNameEn = roleName,
                        DisplayNameAr = GetArabicDisplayName(roleName),
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        CreatedBy = "SYSTEM",
                        CreatedOn = DateTime.UtcNow
                    };

                    var result = await roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create role {roleName}: " +
                                          string.Join(", ", result.Errors.Select(e => e.Description)));
                    }

                    logger.LogInformation("Successfully created role {RoleName}", roleName);
                }
            }
        }

        /// <summary>
        /// Get Arabic display name for role
        /// </summary>
        private static string GetArabicDisplayName(string roleName)
        {
            return roleName switch
            {
                "SuperAdmin" => "مسؤول النظام",
                "Admin" => "مدير",
                "Charity" => "جمعية",
                "Accountant" => "محاسب",
                "FinancialOfficer" => "موظف مالي",
                _ => roleName
            };
        }

        /// <summary>
        /// Get all IIROSA user IDs
        /// </summary>
        public static Guid[] GetAllUserIds()
        {
            return new[]
            {
                OsamaSuperUserId,
                AdminUserId,
                CharityUserId,
                AccountantUserId,
                FinancialOfficerUserId
            };
        }

        private static async Task LogSeedDataAsync(AppIdentityDbContext context, string entityType, int count, string version)
        {
            // You could create a SeedDataLog entity if you want to track seeding operations
            // For now, we'll just log to the application logs
            await Task.CompletedTask;
        }
    }
}