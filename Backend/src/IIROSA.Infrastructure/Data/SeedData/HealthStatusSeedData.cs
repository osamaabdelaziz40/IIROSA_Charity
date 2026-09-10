using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the HealthStatus lookup — the father/mother/relative health-state
/// drop-down (excellent / good / fair / poor). Existence-guarded: re-runs are no-ops.
/// </summary>
public static class HealthStatusSeedData
{
    /// <summary>
    /// Seed the four base states; an admin can extend the catalogue afterwards.
    /// </summary>
    public static async Task SeedHealthStatusesAsync(ApplicationDbContext context)
    {
        if (await context.Set<HealthStatus>().AnyAsync())
        {
            return; // Already seeded
        }

        var statuses = new List<HealthStatus>
        {
            new HealthStatus { NameAr = "ممتازة", NameEn = "Excellent", IsActive = true, SortOrder = 1 },
            new HealthStatus { NameAr = "جيدة", NameEn = "Good", IsActive = true, SortOrder = 2 },
            new HealthStatus { NameAr = "متوسطة", NameEn = "Fair", IsActive = true, SortOrder = 3 },
            new HealthStatus { NameAr = "ضعيفة", NameEn = "Poor", IsActive = true, SortOrder = 4 }
        };

        await context.Set<HealthStatus>().AddRangeAsync(statuses);
        await context.SaveChangesAsync();
    }
}
