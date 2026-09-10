using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the cause-of-death catalogue (سبب الوفاة) used by the father/mother
/// death-reason drop-downs. Values follow the legacy WAR.IIROSA closed set
/// (§10 اضافة معيل): طبيعية / مرض / حادث. The refugee/housing Provider keeps its
/// static string list — this catalogue backs the Father/Mother FK (DeathReasonId).
/// </summary>
public static class DeathReasonSeedData
{
    /// <summary>
    /// Seed the catalogue (existence-guarded — re-runs are no-ops, admin edits survive)
    /// </summary>
    public static async Task SeedDeathReasonsAsync(ApplicationDbContext context)
    {
        if (await context.Set<DeathReason>().AnyAsync())
            return; // Already seeded

        var reasons = new List<DeathReason>
        {
            new DeathReason { NameAr = "طبيعية", NameEn = "Natural", IsActive = true, SortOrder = 1 },
            new DeathReason { NameAr = "مرض", NameEn = "Illness", IsActive = true, SortOrder = 2 },
            new DeathReason { NameAr = "حادث", NameEn = "Accident", IsActive = true, SortOrder = 3 }
        };

        await context.Set<DeathReason>().AddRangeAsync(reasons);
        await context.SaveChangesAsync();
    }
}
