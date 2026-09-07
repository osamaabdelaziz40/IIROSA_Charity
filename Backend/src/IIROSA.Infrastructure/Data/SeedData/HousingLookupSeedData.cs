using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the Housing module catalogue (epic 6 · UC-HOU-05).
/// Seeds organisation-owned buildings and their flats — the §11.S.2 رقم العماره /
/// رقم الشقه drop-downs would be unusable empty.
///
/// Buildings are an HQ catalogue, NOT per-charity rows (decision recorded in story 6-5):
/// chapter 11 has no building-maintenance screen, so the catalogue is maintained via
/// seeds/admin, never by charity users.
/// </summary>
public static class HousingLookupSeedData
{
    /// <summary>
    /// Seed buildings + flats (existence-guarded — re-runs are no-ops)
    /// </summary>
    public static async Task SeedAllAsync(ApplicationDbContext context)
    {
        await SeedBuildingsAndFlatsAsync(context);
    }

    private static async Task SeedBuildingsAndFlatsAsync(ApplicationDbContext context)
    {
        if (await context.Set<HousingBuilding>().AnyAsync())
            return; // Already seeded

        var building1 = new HousingBuilding { NameAr = "مبنى القاهرة ١", NameEn = "Cairo Building 1", Location = "القاهرة - مدينة نصر", IsActive = true, SortOrder = 1 };
        var building2 = new HousingBuilding { NameAr = "مبنى القاهرة ٢", NameEn = "Cairo Building 2", Location = "القاهرة - مدينة نصر", IsActive = true, SortOrder = 2 };
        var building3 = new HousingBuilding { NameAr = "مبنى الجيزة", NameEn = "Giza Building", Location = "الجيزة - الدقي", IsActive = true, SortOrder = 3 };

        await context.Set<HousingBuilding>().AddRangeAsync(building1, building2, building3);
        await context.SaveChangesAsync();

        var flats = new List<HousingFlat>();
        foreach (var building in new[] { building1, building2, building3 })
        {
            for (var i = 1; i <= 8; i++)
            {
                flats.Add(new HousingFlat
                {
                    NameAr = $"شقة {ToArabicDigits(i)}",
                    NameEn = $"Flat {i}",
                    BuildingId = building.Id,
                    IsActive = true,
                    SortOrder = i
                });
            }
        }

        await context.Set<HousingFlat>().AddRangeAsync(flats);
        await context.SaveChangesAsync();
    }

    private static string ToArabicDigits(int value)
    {
        var arabicDigits = "٠١٢٣٤٥٦٧٨٩";
        return string.Concat(value.ToString().Select(d => arabicDigits[d - '0']));
    }
}
