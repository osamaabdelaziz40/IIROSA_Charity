using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.TechnicalSupport.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for Technical Support lookup entities
/// Provides default categories, priorities, and statuses for support tickets
/// </summary>
public static class TechnicalSupportSeedData
{
    /// <summary>
    /// Seed default support ticket categories
    /// </summary>
    public static async Task SeedSupportTicketCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.SupportTicketCategories.AnyAsync())
            return; // Already seeded

        var categories = new[]
        {
            new SupportTicketCategory
            {
                NameEn = "Technical",
                NameAr = "تقني",
                IsActive = true,
                SortOrder = 1,
                Description = "Technical issues and problems"
            },
            new SupportTicketCategory
            {
                NameEn = "Access",
                NameAr = "صلاحيات",
                IsActive = true,
                SortOrder = 2,
                Description = "Access and permission issues"
            },
            new SupportTicketCategory
            {
                NameEn = "Data",
                NameAr = "بيانات",
                IsActive = true,
                SortOrder = 3,
                Description = "Data-related issues"
            },
            new SupportTicketCategory
            {
                NameEn = "Feature Request",
                NameAr = "طلب ميزة",
                IsActive = true,
                SortOrder = 4,
                Description = "New feature requests"
            },
            new SupportTicketCategory
            {
                NameEn = "Bug",
                NameAr = "خطأ",
                IsActive = true,
                SortOrder = 5,
                Description = "Bug reports"
            },
            new SupportTicketCategory
            {
                NameEn = "Other",
                NameAr = "أخرى",
                IsActive = true,
                SortOrder = 6,
                Description = "Other issues not covered by specific categories"
            }
        };

        await context.SupportTicketCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed default support ticket priorities
    /// </summary>
    public static async Task SeedSupportTicketPrioritiesAsync(ApplicationDbContext context)
    {
        if (await context.SupportTicketPriorities.AnyAsync())
            return; // Already seeded

        var priorities = new[]
        {
            new SupportTicketPriority
            {
                NameEn = "Low",
                NameAr = "منخفض",
                IsActive = true,
                SeverityLevel = 0,
                ResponseTimeHours = 48,
                ColorCode = "#28a745", // Green
                Description = "Low priority issues"
            },
            new SupportTicketPriority
            {
                NameEn = "Medium",
                NameAr = "متوسط",
                IsActive = true,
                SeverityLevel = 1,
                ResponseTimeHours = 24,
                ColorCode = "#ffc107", // Yellow
                Description = "Medium priority issues"
            },
            new SupportTicketPriority
            {
                NameEn = "High",
                NameAr = "عالي",
                IsActive = true,
                SeverityLevel = 2,
                ResponseTimeHours = 12,
                ColorCode = "#fd7e14", // Orange
                Description = "High priority issues"
            },
            new SupportTicketPriority
            {
                NameEn = "Urgent",
                NameAr = "عاجل",
                IsActive = true,
                SeverityLevel = 3,
                ResponseTimeHours = 4,
                ColorCode = "#dc3545", // Red
                Description = "Urgent issues requiring immediate attention"
            }
        };

        await context.SupportTicketPriorities.AddRangeAsync(priorities);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed default support ticket statuses
    /// </summary>
    public static async Task SeedSupportTicketStatusesAsync(ApplicationDbContext context)
    {
        if (await context.SupportTicketStatuses.AnyAsync())
            return; // Already seeded

        var statuses = new[]
        {
            new SupportTicketStatus
            {
                NameEn = "Open",
                NameAr = "مفتوح",
                IsActive = true,
                SortOrder = 1,
                IsTerminalStatus = false,
                ColorCode = "#007bff", // Blue
                Description = "Ticket is open and awaiting response"
            },
            new SupportTicketStatus
            {
                NameEn = "In Progress",
                NameAr = "قيد التنفيذ",
                IsActive = true,
                SortOrder = 2,
                IsTerminalStatus = false,
                ColorCode = "#17a2b8", // Cyan
                Description = "Ticket is being worked on"
            },
            new SupportTicketStatus
            {
                NameEn = "Resolved",
                NameAr = "تم الحل",
                IsActive = true,
                SortOrder = 3,
                IsTerminalStatus = false,
                ColorCode = "#28a745", // Green
                Description = "Ticket has been resolved"
            },
            new SupportTicketStatus
            {
                NameEn = "Closed",
                NameAr = "مغلق",
                IsActive = true,
                SortOrder = 4,
                IsTerminalStatus = true,
                ColorCode = "#6c757d", // Gray
                Description = "Ticket is closed"
            }
        };

        await context.SupportTicketStatuses.AddRangeAsync(statuses);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed all Technical Support lookup data
    /// </summary>
    public static async Task SeedAllAsync(ApplicationDbContext context)
    {
        await SeedSupportTicketCategoriesAsync(context);
        await SeedSupportTicketPrioritiesAsync(context);
        await SeedSupportTicketStatusesAsync(context);
    }
}
