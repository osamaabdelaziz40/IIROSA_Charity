using IIROSA.Domain.Entities.Lookups;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the Correspondence module lookups (epic 16)
/// Seeds the routing department catalogue (UC-COR-08) and the outgoing category catalogue
/// (UC-COR-17) — empty catalogues would leave the mandatory letter-form drop-downs unusable.
/// </summary>
public static class CorrespondenceLookupSeedData
{
    /// <summary>
    /// Seed the routing department catalogue (UC-COR-08 — اختيار الإدارة المختصة)
    /// </summary>
    public static async Task SeedDepartmentsAsync(ApplicationDbContext context)
    {
        if (await context.Set<Department>().AnyAsync())
            return; // Already seeded

        var departments = new[]
        {
            new Department { NameAr = "رئاسة المكتب", NameEn = "Head Office", IsActive = true, SortOrder = 1 },
            new Department { NameAr = "رعاية الأيتام", NameEn = "Orphan Care", IsActive = true, SortOrder = 2 },
            new Department { NameAr = "المشاريع", NameEn = "Projects", IsActive = true, SortOrder = 3 },
            new Department { NameAr = "الشؤون المالية", NameEn = "Finance", IsActive = true, SortOrder = 4 },
            new Department { NameAr = "شؤون الجمعيات", NameEn = "Charity Affairs", IsActive = true, SortOrder = 5 },
            new Department { NameAr = "الشؤون الإدارية", NameEn = "Administrative Affairs", IsActive = true, SortOrder = 6 }
        };

        await context.Set<Department>().AddRangeAsync(departments);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed the outgoing category catalogue (UC-COR-17 — اختيار تصنيف الصادر)
    /// </summary>
    public static async Task SeedOutgoingCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Set<OutgoingCategory>().AnyAsync())
            return; // Already seeded

        var categories = new[]
        {
            new OutgoingCategory { NameAr = "رسمي", NameEn = "Official", IsActive = true, SortOrder = 1 },
            new OutgoingCategory { NameAr = "داخلي", NameEn = "Internal", IsActive = true, SortOrder = 2 },
            new OutgoingCategory { NameAr = "خارجي", NameEn = "External", IsActive = true, SortOrder = 3 },
            new OutgoingCategory { NameAr = "تعميم", NameEn = "Circular", IsActive = true, SortOrder = 4 },
            new OutgoingCategory { NameAr = "أخرى", NameEn = "Other", IsActive = true, SortOrder = 5 }
        };

        await context.Set<OutgoingCategory>().AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed all correspondence lookups
    /// </summary>
    public static async Task SeedAllAsync(ApplicationDbContext context)
    {
        await SeedDepartmentsAsync(context);
        await SeedOutgoingCategoriesAsync(context);
    }
}
