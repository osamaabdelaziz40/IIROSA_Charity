namespace IIROSA.Application.Interfaces;

/// <summary>
/// Enforces the per-charity write permissions head office controls (UC-CHR-07, UC-CHR-08,
/// UC-CHR-09).
/// </summary>
/// <remarks>
/// <c>Charity.IsLocked</c>, <c>IsAddEnabled</c> and <c>IsUpdateEnabled</c> were stored, filtered on
/// and rendered as toggles in the UI, but no service anywhere consulted them — head office could
/// flip them and nothing changed. This is the single place that reads them, so every caller gets
/// the same answer.
///
/// Head-office callers are not subject to these flags: the flags exist for head office to restrain
/// a charity, so applying them to head office itself would let a charity lock out its supervisor.
/// </remarks>
public interface ICharityWriteGuard
{
    /// <summary>
    /// Throws when the caller's charity may not create records.
    /// </summary>
    /// <exception cref="CharityWriteForbiddenException">
    /// The charity is locked, or its add permission is disabled.
    /// </exception>
    Task EnsureCanAddAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Throws when the caller's charity may not amend records.
    /// </summary>
    /// <exception cref="CharityWriteForbiddenException">
    /// The charity is locked, or its update permission is disabled.
    /// </exception>
    Task EnsureCanUpdateAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Raised when a charity's own permissions forbid the write it attempted.
/// </summary>
/// <remarks>
/// Distinct from <see cref="UnauthorizedAccessException"/>: the caller is authenticated and holds
/// the right role, so this is a 403 about the charity's current state, not about identity. A
/// dedicated type keeps it from being mapped to 404 by the middleware's
/// <c>InvalidOperationException</c> case.
/// </remarks>
public class CharityWriteForbiddenException : Exception
{
    public CharityWriteForbiddenException(string message) : base(message)
    {
    }
}
