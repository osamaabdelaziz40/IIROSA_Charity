namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// UC-3.5 — the outcome of a charity password reset.
///
/// The password is generated server-side, applied to the linked Identity account and returned here
/// exactly once. It is not stored anywhere and cannot be read back, so the screen that receives this
/// response is the operator's only chance to hand it to the charity — which is why the client shows
/// it in a popup rather than in a transient toast.
/// </summary>
public class CharityPasswordResetResultDto
{
    /// <summary>The login the new password belongs to (the charity's linked account user name).</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>The password the account was actually set to.</summary>
    public string NewPassword { get; set; } = string.Empty;
}
