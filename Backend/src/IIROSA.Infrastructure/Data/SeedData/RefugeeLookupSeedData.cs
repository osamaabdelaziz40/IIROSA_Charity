using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the Refugee register lookups (epic 7 · UC-REF-03 §12.S.2).
/// Six catalogues feed the refugee form's drop-downs; option lists follow the legacy
/// WAR.IIROSA form (§12.S.2): ownership ملك/إيجار/أخرى, contents status, income types,
/// social statuses, relations الاب/الام/علاقة أخرى, reasons-of-relation.
/// Cause of death (طبيعية/مرض/حادث) is a STATIC list on the form — it is stored on
/// Provider.DeathReason as a string, not a lookup.
/// </summary>
public static class RefugeeLookupSeedData
{
    /// <summary>
    /// Seed all six catalogues (existence-guarded per catalogue — re-runs are no-ops)
    /// </summary>
    public static async Task SeedAllAsync(ApplicationDbContext context)
    {
        await SeedAsync<HouseOwnership>(context, rows =>
        {
            rows.Add(new HouseOwnership { NameAr = "ملك", NameEn = "Owned", IsActive = true, SortOrder = 1 });
            rows.Add(new HouseOwnership { NameAr = "إيجار", NameEn = "Rented", IsActive = true, SortOrder = 2 });
            rows.Add(new HouseOwnership { NameAr = "أخرى", NameEn = "Other", IsActive = true, SortOrder = 3 });
        });

        await SeedAsync<HouseStatus>(context, rows =>
        {
            rows.Add(new HouseStatus { NameAr = "جيدة", NameEn = "Good", IsActive = true, SortOrder = 1 });
            rows.Add(new HouseStatus { NameAr = "متوسطة", NameEn = "Fair", IsActive = true, SortOrder = 2 });
            rows.Add(new HouseStatus { NameAr = "ضعيفة", NameEn = "Poor", IsActive = true, SortOrder = 3 });
            rows.Add(new HouseStatus { NameAr = "منعدمة", NameEn = "None", IsActive = true, SortOrder = 4 });
        });

        await SeedAsync<IncomeType>(context, rows =>
        {
            rows.Add(new IncomeType { NameAr = "راتب", NameEn = "Salary", IsActive = true, SortOrder = 1 });
            rows.Add(new IncomeType { NameAr = "عمل حر", NameEn = "Freelance work", IsActive = true, SortOrder = 2 });
            rows.Add(new IncomeType { NameAr = "مساعدات", NameEn = "Aid", IsActive = true, SortOrder = 3 });
            rows.Add(new IncomeType { NameAr = "بدون دخل", NameEn = "No income", IsActive = true, SortOrder = 4 });
        });

        await SeedAsync<SocialStatus>(context, rows =>
        {
            rows.Add(new SocialStatus { NameAr = "عادي", NameEn = "Normal", IsActive = true, SortOrder = 1 });
            rows.Add(new SocialStatus { NameAr = "يتيم الأب", NameEn = "Father deceased", IsActive = true, SortOrder = 2 });
            rows.Add(new SocialStatus { NameAr = "يتيم الأم", NameEn = "Mother deceased", IsActive = true, SortOrder = 3 });
            rows.Add(new SocialStatus { NameAr = "يتيم الأبوين", NameEn = "Both parents deceased", IsActive = true, SortOrder = 4 });
        });

        await SeedAsync<Relation>(context, rows =>
        {
            rows.Add(new Relation { NameAr = "الاب", NameEn = "Father", IsActive = true, SortOrder = 1 });
            rows.Add(new Relation { NameAr = "الام", NameEn = "Mother", IsActive = true, SortOrder = 2 });
            rows.Add(new Relation { NameAr = "علاقة أخرى", NameEn = "Other relation", IsActive = true, SortOrder = 3 });
        });

        await SeedAsync<ReasonOfRel>(context, rows =>
        {
            rows.Add(new ReasonOfRel { NameAr = "قريب بعيد", NameEn = "Distant relative", IsActive = true, SortOrder = 1 });
            rows.Add(new ReasonOfRel { NameAr = "جار", NameEn = "Neighbour", IsActive = true, SortOrder = 2 });
            rows.Add(new ReasonOfRel { NameAr = "وصي شرعي", NameEn = "Legal guardian", IsActive = true, SortOrder = 3 });
            rows.Add(new ReasonOfRel { NameAr = "أخرى", NameEn = "Other", IsActive = true, SortOrder = 4 });
        });
    }

    /// <summary>
    /// Per-catalogue existence guard — an admin's later edits to one catalogue never block
    /// seeding (or reseeding) the others.
    /// </summary>
    private static async Task SeedAsync<TLookup>(ApplicationDbContext context, Action<List<TLookup>> addRows)
        where TLookup : LookupEntity
    {
        if (await context.Set<TLookup>().AnyAsync())
            return; // Already seeded

        var rows = new List<TLookup>();
        addRows(rows);

        await context.Set<TLookup>().AddRangeAsync(rows);
        await context.SaveChangesAsync();
    }
}
