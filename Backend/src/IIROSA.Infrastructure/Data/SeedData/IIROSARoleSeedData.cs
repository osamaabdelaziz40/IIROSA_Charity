using Framework.Identity.Data;
using Framework.Identity.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IIROSA.Infrastructure.Data.SeedData
{
    /// <summary>
    /// Seed data for IIROSA roles as per UC-1.1 specifications
    /// Implements the seed roles: Super Admin, Admin, Charity, Accountant, FinancialOfficer
    /// </summary>
    public static class IIROSARoleSeedData
    {
        // Role GUIDs - consistent across deployments
        public static readonly Guid SuperAdminRoleId = Guid.Parse("8B22698B-42A2-4115-9631-1C2D1E2AC5F7");
        public static readonly Guid AdminRoleId = Guid.Parse("AA224BA6-94D9-4395-BB1C-9D54939E70EF");
        public static readonly Guid CharityRoleId = Guid.Parse("ED6D642F-50ED-4E28-BDE5-03B4C88F9387");
        public static readonly Guid AccountantRoleId = Guid.Parse("DB262BE8-1A1A-4910-99BA-65D89D8FD90E");
        public static readonly Guid FinancialOfficerRoleId = Guid.Parse("F1F088FC-E2CB-4AC4-BF04-12A0800415A6");

        /// <summary>
        /// Seed IIROSA roles to the database
        /// </summary>
        public static async Task SeedRolesAsync(AppIdentityDbContext context, ILogger logger)
        {
            try
            {
                // Check if roles already exist by checking for any of our specific roles
                var roleNames = new[] { "SuperAdmin", "Admin", "Charity", "Accountant", "FinancialOfficer" };
                var existingRoles = await context.Roles
                    .Where(r => roleNames.Contains(r.Name))
                    .ToListAsync();

                if (existingRoles.Any())
                {
                    logger.LogWarning("IIROSA roles already exist. Skipping role seeding. Found roles: {Roles}",
                        string.Join(", ", existingRoles.Select(r => r.Name)));
                    return;
                }

                // Create IIROSA specific roles as per UC-1.1
                var roles = new[]
                {
                    new ApplicationRole
                    {
                        Id = SuperAdminRoleId,
                        Name = "SuperAdmin",
                        NormalizedName = "SUPERADMIN",
                        DisplayNameAr = "مسؤول النظام",
                        DisplayNameEn = "Super Admin",
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        CreatedBy = "SYSTEM",
                        CreatedOn = DateTime.UtcNow,
                        Email = "superadmin@iirosa.org"
                    },
                    new ApplicationRole
                    {
                        Id = AdminRoleId,
                        Name = "Admin",
                        NormalizedName = "ADMIN",
                        DisplayNameAr = "مدير",
                        DisplayNameEn = "Administrator",
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        CreatedBy = "SYSTEM",
                        CreatedOn = DateTime.UtcNow,
                        Email = "admin@iirosa.org"
                    },
                    new ApplicationRole
                    {
                        Id = CharityRoleId,
                        Name = "Charity",
                        NormalizedName = "CHARITY",
                        DisplayNameAr = "جمعية",
                        DisplayNameEn = "Charity User",
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        CreatedBy = "SYSTEM",
                        CreatedOn = DateTime.UtcNow,
                        Email = "charity@iirosa.org"
                    },
                    new ApplicationRole
                    {
                        Id = AccountantRoleId,
                        Name = "Accountant",
                        NormalizedName = "ACCOUNTANT",
                        DisplayNameAr = "محاسب",
                        DisplayNameEn = "Accountant",
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        CreatedBy = "SYSTEM",
                        CreatedOn = DateTime.UtcNow,
                        Email = "accountant@iirosa.org"
                    },
                    new ApplicationRole
                    {
                        Id = FinancialOfficerRoleId,
                        Name = "FinancialOfficer",
                        NormalizedName = "FINANCIALOFFICER",
                        DisplayNameAr = "موظف مالي",
                        DisplayNameEn = "Financial Officer",
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        CreatedBy = "SYSTEM",
                        CreatedOn = DateTime.UtcNow,
                        Email = "financialofficer@iirosa.org"
                    }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully seeded {Count} IIROSA roles: {Roles}",
                    roles.Length,
                    string.Join(", ", roles.Select(r => r.Name)));

                // Log seed data metadata
                await LogSeedDataAsync(context, "Roles", 5, "1.0");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while seeding IIROSA roles");
                throw;
            }
        }

        /// <summary>
        /// Get all IIROSA role IDs
        /// </summary>
        public static Guid[] GetAllRoleIds()
        {
            return new[]
            {
                SuperAdminRoleId,
                AdminRoleId,
                CharityRoleId,
                AccountantRoleId,
                FinancialOfficerRoleId
            };
        }

        /// <summary>
        /// Get role ID by role name
        /// </summary>
        public static Guid GetRoleIdByRoleName(string roleName)
        {
            return roleName switch
            {
                "SuperAdmin" => SuperAdminRoleId,
                "Admin" => AdminRoleId,
                "Charity" => CharityRoleId,
                "Accountant" => AccountantRoleId,
                "FinancialOfficer" => FinancialOfficerRoleId,
                _ => throw new ArgumentException($"Unknown role name: {roleName}")
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