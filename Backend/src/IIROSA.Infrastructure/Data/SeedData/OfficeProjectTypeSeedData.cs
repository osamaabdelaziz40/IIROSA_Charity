using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the Office Development Projects type catalogue (UC-7.1).
/// The three starter types feed the create form's نوع مشروع المكتب drop-down
/// (GET api/LookupManagement/office-project-types) and the lookup-management screen.
/// </summary>
public static class OfficeProjectTypeSeedData
{
    /// <summary>
    /// Seed the catalogue (per-value existence guard on NameAr — re-runs are no-ops and
    /// admin edits persist). Values that pre-date this catalogue (e.g. the legacy دواجن /
    /// خياطه rows) are left in place: خياطه is referenced by OfficeProject rows, and any
    /// removal belongs to an admin action through lookup management, not the seeder.
    /// </summary>
    public static async Task SeedOfficeProjectTypesAsync(ApplicationDbContext context)
    {
        var rows = new (string NameAr, string NameEn)[]
        {
            ("محطه تنقيه المياه", "Water Purification Station"),
            ("مشروع أسر منتجه", "Productive Families Project"),
            ("وصلات مياه شرب", "Drinking Water Connections")
        };

        var sortOrder = await context.Set<OfficeProjectType>().MaxAsync(x => (int?)x.SortOrder) ?? 0;
        foreach (var row in rows)
        {
            if (await context.Set<OfficeProjectType>().AnyAsync(x => x.NameAr == row.NameAr))
                continue; // Already present

            context.Set<OfficeProjectType>().Add(new OfficeProjectType
            {
                NameAr = row.NameAr,
                NameEn = row.NameEn,
                IsActive = true,
                SortOrder = ++sortOrder
            });
        }

        await context.SaveChangesAsync();
    }
}
