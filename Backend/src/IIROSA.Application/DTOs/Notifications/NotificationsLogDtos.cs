namespace IIROSA.Application.DTOs.Notifications;

/// <summary>
/// NotificationsLog list/detail row (UC-NTF). Recipient ids travel as lists —
/// the CSV is a storage detail hidden behind the profile mapping.
/// </summary>
public class NotificationsLogListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public bool IsCharity { get; set; }
    public List<Guid> RecipientUserIds { get; set; } = new();
    public List<Guid> RecipientCharityIds { get; set; } = new();
    public int NotificationTypeId { get; set; }
    public int SentCount { get; set; }
    public DateTime? LastSentOn { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Resolved push audience (direct users + every user of the selected charities)
    /// for the most recent send. Consumed by the controller to address the hub;
    /// harmless on the wire otherwise.
    /// </summary>
    public List<string> DeliveredToUserIds { get; set; } = new();
}

/// <summary>
/// Create payload (UC-NTF push): Title + Message plus the audience flags.
/// Exactly the two multi-selects the screen shows — isUser shows the user
/// picker, isCharity the charity picker; at least one recipient list must
/// accompany each raised flag (validator).
/// </summary>
public class CreateNotificationsLogDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public bool IsCharity { get; set; }
    public List<Guid> RecipientUserIds { get; set; } = new();
    public List<Guid> RecipientCharityIds { get; set; } = new();
}

/// <summary>Update payload (UC-NTF edit) — id comes from the route.</summary>
public class UpdateNotificationsLogDto : CreateNotificationsLogDto
{
    public Guid Id { get; set; }
}

/// <summary>Filter for the admin register read (UC-NTF list).</summary>
public class NotificationsLogFilterDto
{
    /// <summary>Free-text match on Title or Message.</summary>
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>Paged result wrapper (mission-register shape).</summary>
public class NotificationsLogPagedResult
{
    public List<NotificationsLogListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
