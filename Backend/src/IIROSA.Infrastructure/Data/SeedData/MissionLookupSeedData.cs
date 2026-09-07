using Microsoft.EntityFrameworkCore;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the Missions module lookups (epic 15)
/// Seeds the mission type, mission interview type and mission time type catalogues
/// (UC-MSN-03 / UC-MSN-04 / UC-MSN-05) — an empty catalogue would leave the
/// mandatory mission-form drop-downs unusable.
/// </summary>
public static class MissionLookupSeedData
{
    /// <summary>
    /// Seed the mission type catalogue (UC-MSN-03)
    /// </summary>
    public static async Task SeedMissionTypesAsync(ApplicationDbContext context)
    {
        if (await context.Set<MissionType>().AnyAsync())
            return; // Already seeded

        var types = new[]
        {
            new MissionType { NameEn = "Fieldwork", NameAr = "عمل ميداني", TypeCode = "FIELD", IsActive = true, SortOrder = 1 },
            new MissionType { NameEn = "Conference", NameAr = "مؤتمر", TypeCode = "CONF", IsActive = true, SortOrder = 2 },
            new MissionType { NameEn = "Training", NameAr = "تدريب", TypeCode = "TRAIN", IsActive = true, SortOrder = 3 },
            new MissionType { NameEn = "Meeting", NameAr = "اجتماع", TypeCode = "MEET", IsActive = true, SortOrder = 4 },
            new MissionType { NameEn = "Inspection", NameAr = "تفتيش", TypeCode = "INSP", IsActive = true, SortOrder = 5 },
            new MissionType { NameEn = "Other", NameAr = "أخرى", TypeCode = "OTHER", IsActive = true, SortOrder = 6 }
        };

        await context.Set<MissionType>().AddRangeAsync(types);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed the mission interview type catalogue (UC-MSN-04)
    /// </summary>
    public static async Task SeedMissionInterviewTypesAsync(ApplicationDbContext context)
    {
        if (await context.Set<MissionInterviewType>().AnyAsync())
            return; // Already seeded

        var types = new[]
        {
            new MissionInterviewType { NameEn = "Field Interview", NameAr = "مقابلة ميدانية", TypeCode = "FIELD", IsActive = true, SortOrder = 1 },
            new MissionInterviewType { NameEn = "Office Interview", NameAr = "مقابلة مكتبية", TypeCode = "OFFICE", IsActive = true, SortOrder = 2 },
            new MissionInterviewType { NameEn = "Phone Interview", NameAr = "مقابلة هاتفية", TypeCode = "PHONE", IsActive = true, SortOrder = 3 }
        };

        await context.Set<MissionInterviewType>().AddRangeAsync(types);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed the mission time type catalogue (UC-MSN-05)
    /// The spec's classification is time-of-day (§20 catalogue summary), not frequency.
    /// </summary>
    public static async Task SeedMissionTimeTypesAsync(ApplicationDbContext context)
    {
        if (await context.Set<MissionTimeType>().AnyAsync())
            return; // Already seeded

        var types = new[]
        {
            new MissionTimeType { NameEn = "Morning", NameAr = "صباحي", TimeTypeCode = "MORNING", IsActive = true, SortOrder = 1 },
            new MissionTimeType { NameEn = "Evening", NameAr = "مسائي", TimeTypeCode = "EVENING", IsActive = true, SortOrder = 2 },
            new MissionTimeType { NameEn = "Full Day", NameAr = "يوم كامل", TimeTypeCode = "FULLDAY", IsActive = true, SortOrder = 3 }
        };

        await context.Set<MissionTimeType>().AddRangeAsync(types);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed all mission lookups
    /// </summary>
    public static async Task SeedAllAsync(ApplicationDbContext context)
    {
        await SeedMissionTypesAsync(context);
        await SeedMissionInterviewTypesAsync(context);
        await SeedMissionTimeTypesAsync(context);
    }
}
