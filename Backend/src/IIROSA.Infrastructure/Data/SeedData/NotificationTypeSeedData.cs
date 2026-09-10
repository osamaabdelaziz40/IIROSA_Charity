using Framework.Core.SharedServices;
using Framework.Core.SharedServices.Entities;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.SeedData;

/// <summary>
/// Seed data for the common.NotificationType catalogue (UC-NTF web notifications).
/// The framework ships only "Email"; the web-push feature stamps every NotificationsLog
/// row with the "Web" type, so it must exist before the first push. Runtime seed —
/// no explicit Id: the column is identity, and a fixed HasData id would collide with
/// rows already grown past it.
/// </summary>
public static class NotificationTypeSeedData
{
    public static async Task SeedWebNotificationTypeAsync(ICommonsDbContext commonsContext)
    {
        var exists = await commonsContext.Set<NotificationType>()
            .AnyAsync(t => t.NameEn == "Web");
        if (exists)
            return; // Already seeded

        await commonsContext.Set<NotificationType>()
            .AddAsync(new NotificationType { NameAr = "ويب", NameEn = "Web" });
        await commonsContext.SaveChangesAsync();
    }
}
