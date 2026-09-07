using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the Housing module catalogue (epic 6 · UC-HOU-05).
/// Seeds organisation-owned buildings and their flats — the §11.S.2 رقم العماره /
/// رقم الشقه drop-downs would be unusable empty.
///
/// Buildings are an HQ catalogue, NOT per-charity rows (decision recorded in story 6-5).
/// The initial catalogue (4 buildings × 12 flats of 100 m²) is initiation data the
/// migration inserts with explicit ids; this seeder is the empty-table fallback and
/// must stay in sync with it.
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

        // (title, number, address, description) — العمارات 7/8/13/14 بمدينه نصر
        var buildingSpecs = new (string Title, int Number, string Address, string Description)[]
        {
            ("العماره رقم 7", 7, "مدينه نصر", "العماره رقم 7"),
            ("العماره رقم 8", 8, "مدينه نصر", "العماره رقم 8"),
            ("العماره رقم 13", 13, "مدينه نصر", "العماره رقم 13"),
            ("العماره رقم 14", 14, "مدينه نصر", "العماره رقم 14")
        };

        var buildings = buildingSpecs
            .Select((spec, index) => new HousingBuilding
            {
                NameAr = spec.Title,
                NameEn = $"Building No. {spec.Number}",
                BuildingNumber = spec.Number,
                BuildingAddress = spec.Address,
                BuildingDescription = spec.Description,
                IsActive = true,
                SortOrder = index + 1
            })
            .ToList();

        await context.Set<HousingBuilding>().AddRangeAsync(buildings);
        await context.SaveChangesAsync();

        // 12 flats of 100 m² per building, numbered 1..12
        var flats = new List<HousingFlat>();
        foreach (var building in buildings)
        {
            for (var i = 1; i <= 12; i++)
            {
                flats.Add(new HousingFlat
                {
                    NameAr = i.ToString(),
                    NameEn = i.ToString(),
                    Number = i,
                    SizeInMtr = 100,
                    BuildingId = building.Id,
                    IsActive = true,
                    SortOrder = i
                });
            }
        }

        await context.Set<HousingFlat>().AddRangeAsync(flats);
        await context.SaveChangesAsync();
    }
}
