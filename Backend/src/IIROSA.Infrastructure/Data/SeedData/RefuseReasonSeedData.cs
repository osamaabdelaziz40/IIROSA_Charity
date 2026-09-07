using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the periodic report refuse-reason catalogue (UC-ORR-08, epic 9).
/// An empty catalogue would leave the mandatory rejection drop-down unusable,
/// so the standard WAR.IIROSA rejection reasons are seeded once.
/// </summary>
public static class RefuseReasonSeedData
{
    public static async Task SeedRefuseReasonsAsync(ApplicationDbContext context)
    {
        if (await context.Set<RefuseReason>().AnyAsync())
            return; // Already seeded

        var reasons = new[]
        {
            new RefuseReason { NameEn = "Photo does not match the orphan", NameAr = "الصورة لا تطابق اليتيم", IsActive = true, SortOrder = 1 },
            new RefuseReason { NameEn = "Report data is incomplete", NameAr = "بيانات التقرير غير مكتملة", IsActive = true, SortOrder = 2 },
            new RefuseReason { NameEn = "Report period is incorrect", NameAr = "فترة التقرير غير صحيحة", IsActive = true, SortOrder = 3 },
            new RefuseReason { NameEn = "Duplicate report for the same period", NameAr = "تقرير مكرر لنفس الفترة", IsActive = true, SortOrder = 4 },
            new RefuseReason { NameEn = "Orphan no longer sponsored", NameAr = "اليتيم لم يعد مكفولاً", IsActive = true, SortOrder = 5 },
            new RefuseReason { NameEn = "Other", NameAr = "أخرى", IsActive = true, SortOrder = 6 }
        };

        await context.Set<RefuseReason>().AddRangeAsync(reasons);
        await context.SaveChangesAsync();
    }
}
