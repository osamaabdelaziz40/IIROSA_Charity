using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the country NID validation rules (UC-SYS-11, epic 19).
/// Unlike the insert-if-empty catalogues, this fills blanks on EXISTING country rows:
/// a rule is written only where both columns are still NULL, so HQ edits are never
/// overwritten on re-seed. Rows are matched by DialingCode (stable in the seeded set;
/// the legacy IsoCode column holds non-ISO numeric codes). Countries without a
/// well-established, official NID format are deliberately left NULL — an unvalidated
/// field, not a guessed rule.
/// </summary>
public static class CountryNidRuleSeedData
{
    public static async Task SeedNidRulesAsync(ApplicationDbContext context)
    {
        var rules = new (string DialingCode, string Pattern, int? Length)[]
        {
            // Egypt — the 14-digit national number (الرقم القومي).
            ("+20", @"^\d{14}$", 14),
            // Pakistan — CNIC in the official 5-7-1 dashed layout; length left NULL
            // because counting with/without dashes is ambiguous to the user.
            ("+92", @"^\d{5}-\d{7}-\d$", null)
        };

        foreach (var (dialingCode, pattern, length) in rules)
        {
            var country = await context.Set<Country>().FirstOrDefaultAsync(c => c.DialingCode == dialingCode);
            if (country == null || country.NationalIdPattern != null || country.NationalIdLength != null)
                continue; // not seeded here, or already ruled — never overwrite

            country.NationalIdPattern = pattern;
            country.NationalIdLength = length;
        }

        await context.SaveChangesAsync();
    }
}
