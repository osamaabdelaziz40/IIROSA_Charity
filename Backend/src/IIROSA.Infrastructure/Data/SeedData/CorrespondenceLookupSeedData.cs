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
    /// Seed the outgoing category catalogue (UC-COR-17 — اختيار تصنيف الصادر).
    /// The office's working classification of outgoing letters.
    /// </summary>
    public static async Task SeedOutgoingCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Set<OutgoingCategory>().AnyAsync())
            return; // Already seeded

        var categories = new[]
        {
            new OutgoingCategory { NameAr = "دراسة حالة", NameEn = "Case Study", IsActive = true, SortOrder = 1 },
            new OutgoingCategory { NameAr = "مخاطبات داخل مصر", NameEn = "Correspondence Within Egypt", IsActive = true, SortOrder = 2 },
            new OutgoingCategory { NameAr = "بنكية", NameEn = "Banking", IsActive = true, SortOrder = 3 },
            new OutgoingCategory { NameAr = "تقارير إنجاز", NameEn = "Progress Reports", IsActive = true, SortOrder = 4 },
            new OutgoingCategory { NameAr = "سداد عهد مالية", NameEn = "Financial Custody Settlement", IsActive = true, SortOrder = 5 },
            new OutgoingCategory { NameAr = "تقارير مالية", NameEn = "Financial Reports", IsActive = true, SortOrder = 6 },
            new OutgoingCategory { NameAr = "شؤون قانونية", NameEn = "Legal Affairs", IsActive = true, SortOrder = 7 },
            new OutgoingCategory { NameAr = "تقارير دورية للأيتام", NameEn = "Periodic Orphan Reports", IsActive = true, SortOrder = 8 },
            new OutgoingCategory { NameAr = "شؤون إدارية", NameEn = "Administrative Affairs", IsActive = true, SortOrder = 9 },
            new OutgoingCategory { NameAr = "دراسات للمشروعات", NameEn = "Project Studies", IsActive = true, SortOrder = 10 },
            new OutgoingCategory { NameAr = "موارد بشرية", NameEn = "Human Resources", IsActive = true, SortOrder = 11 }
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
