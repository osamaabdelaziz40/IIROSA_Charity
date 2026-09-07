using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the bank catalogue (UC-SYS-08, epic 19). Banks are operational
/// data — an empty catalogue blocks cheque creation — so a starter set of the
/// major banks operating in the platform's primary geographies (Egypt, Saudi
/// Arabia) is seeded once. HQ admins manage the live list from the
/// lookup-management banks screen; this seed only ever runs on an empty table.
/// The catalogue is global by recorded ruling (no CountryId).
/// </summary>
public static class BankSeedData
{
    public static async Task SeedBanksAsync(ApplicationDbContext context)
    {
        if (await context.Set<Bank>().AnyAsync())
            return; // Already seeded

        var banks = new[]
        {
            new Bank { NameEn = "National Bank of Egypt", NameAr = "البنك الأهلي المصري", IsActive = true, SortOrder = 1 },
            new Bank { NameEn = "Banque Misr", NameAr = "بنك مصر", IsActive = true, SortOrder = 2 },
            new Bank { NameEn = "Banque du Caire", NameAr = "بنك القاهرة", IsActive = true, SortOrder = 3 },
            new Bank { NameEn = "Al Rajhi Bank", NameAr = "مصرف الراجحي", IsActive = true, SortOrder = 4 },
            new Bank { NameEn = "Saudi National Bank", NameAr = "البنك الأهلي السعودي", IsActive = true, SortOrder = 5 },
            new Bank { NameEn = "Riyad Bank", NameAr = "بنك الرياض", IsActive = true, SortOrder = 6 }
        };

        await context.Set<Bank>().AddRangeAsync(banks);
        await context.SaveChangesAsync();
    }
}
