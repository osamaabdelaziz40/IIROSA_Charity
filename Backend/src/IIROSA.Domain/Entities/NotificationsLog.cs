using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// A pushed web notification (UC-NTF, notifications epic). One row per composed
/// notification — the recipient selection is stored as CSV id lists so the edit
/// screen can restore it and resend can resolve the same audience. Delivered
/// live over SignalR (NotificationHub) by the API layer; SentCount/LastSentOn
/// track resend history beyond the CreatedOn audit.
/// Cross-context note: NotificationTypeId references the framework's
/// common.NotificationType lookup (seeded with "Web") — no FK navigation is
/// declared because the commons context owns that table.
/// </summary>
public class NotificationsLog : FullAuditedEntity
{
    /// <summary>Notification headline shown in the bell toast and the list.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Notification body typed once by the sender — one language, shown as-is.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Target kind flag 1/2: notification is addressed to specific users.</summary>
    public bool IsUser { get; set; }

    /// <summary>Target kind flag 2/2: notification is addressed to whole charities (all their users).</summary>
    public bool IsCharity { get; set; }

    /// <summary>CSV of ApplicationUser ids selected as direct recipients (empty when IsUser is false).</summary>
    public string RecipientUserIds { get; set; } = string.Empty;

    /// <summary>CSV of Charity ids selected as recipients (empty when IsCharity is false).</summary>
    public string RecipientCharityIds { get; set; } = string.Empty;

    /// <summary>The common.NotificationType row (Web) in force when this was sent.</summary>
    public int NotificationTypeId { get; set; }

    /// <summary>How many times this notification has been pushed (create = 1, each resend +1).</summary>
    public int SentCount { get; set; } = 1;

    /// <summary>UTC timestamp of the most recent push (CreatedOn is the first).</summary>
    public DateTime? LastSentOn { get; set; }
}
