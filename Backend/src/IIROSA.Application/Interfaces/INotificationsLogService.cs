using IIROSA.Application.DTOs.Notifications;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// NotificationsLog service contract (UC-NTF notifications epic).
/// The service owns persistence, validation and audience resolution; the API
/// layer performs the live SignalR delivery using the returned audience list.
/// </summary>
public interface INotificationsLogService
{
    /// <summary>Admin register read — every pushed notification, paged (UC-NTF list).</summary>
    Task<NotificationsLogPagedResult> GetFilteredAsync(NotificationsLogFilterDto filter);

    /// <summary>
    /// Register statistics for the band above the admin notifications grid (UC-NTF list) —
    /// the whole register, matching GetFilteredAsync's unscoped admin semantics.
    /// </summary>
    Task<NotificationsLogStatisticsDto> GetStatisticsAsync();

    /// <summary>Detail read for the edit screen (UC-NTF edit).</summary>
    Task<NotificationsLogListDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// The recipient's read (UC-NTF my notifications): every notification whose
    /// audience contains the user directly or through their charity.
    /// </summary>
    Task<NotificationsLogPagedResult> GetForUserAsync(Guid userId, Guid? charityId, NotificationsLogFilterDto filter);

    /// <summary>
    /// Create and stage the first push (UC-NTF push): validates, persists the
    /// log and resolves the audience. Returns the stored row with
    /// DeliveredToUserIds ready for the hub.
    /// </summary>
    Task<NotificationsLogListDto> CreateAsync(CreateNotificationsLogDto dto);

    /// <summary>Update a stored notification's content/audience (UC-NTF edit).</summary>
    Task<NotificationsLogListDto> UpdateAsync(UpdateNotificationsLogDto dto);

    /// <summary>
    /// Resend (UC-NTF resend): re-resolves the audience from the stored
    /// selection and bumps SentCount/LastSentOn. Returns the row with the fresh
    /// DeliveredToUserIds for the hub.
    /// </summary>
    Task<NotificationsLogListDto> ResendAsync(Guid id);
}
