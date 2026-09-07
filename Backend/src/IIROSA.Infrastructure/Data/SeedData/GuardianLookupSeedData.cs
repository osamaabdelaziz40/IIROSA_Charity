using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the guardian reference catalogues (UC-SYS-05 / UC-SYS-09, epic 19).
/// An empty marital-status or job catalogue would leave the guardian form's mandatory
/// drop-downs unusable, so the standard values are seeded once.
/// </summary>
public static class GuardianLookupSeedData
{
    public static async Task SeedAllAsync(ApplicationDbContext context)
    {
        await SeedMaritalStatusesAsync(context);
        await SeedJobsAsync(context);
    }

    private static async Task SeedMaritalStatusesAsync(ApplicationDbContext context)
    {
        if (await context.Set<MaritalStatus>().AnyAsync())
            return; // Already seeded

        var statuses = new[]
        {
            new MaritalStatus { NameEn = "Single", NameAr = "أعزب", IsActive = true, SortOrder = 1 },
            new MaritalStatus { NameEn = "Married", NameAr = "متزوج", IsActive = true, SortOrder = 2 },
            new MaritalStatus { NameEn = "Divorced", NameAr = "مطلق", IsActive = true, SortOrder = 3 },
            new MaritalStatus { NameEn = "Widowed", NameAr = "أرمل", IsActive = true, SortOrder = 4 }
        };

        await context.Set<MaritalStatus>().AddRangeAsync(statuses);
        await context.SaveChangesAsync();
    }

    private static async Task SeedJobsAsync(ApplicationDbContext context)
    {
        if (await context.Set<Job>().AnyAsync())
            return; // Already seeded

        var jobs = new[]
        {
            new Job { NameEn = "Government Employee", NameAr = "موظف حكومي", IsActive = true, SortOrder = 1 },
            new Job { NameEn = "Private Sector Employee", NameAr = "موظف قطاع خاص", IsActive = true, SortOrder = 2 },
            new Job { NameEn = "Labourer", NameAr = "عامل", IsActive = true, SortOrder = 3 },
            new Job { NameEn = "Merchant", NameAr = "تاجر", IsActive = true, SortOrder = 4 },
            new Job { NameEn = "Farmer", NameAr = "مزارع", IsActive = true, SortOrder = 5 },
            new Job { NameEn = "Housewife", NameAr = "ربة منزل", IsActive = true, SortOrder = 6 },
            new Job { NameEn = "Retired", NameAr = "متقاعد", IsActive = true, SortOrder = 7 },
            new Job { NameEn = "Unemployed", NameAr = "بدون عمل", IsActive = true, SortOrder = 8 }
        };

        await context.Set<Job>().AddRangeAsync(jobs);
        await context.SaveChangesAsync();
    }
}
