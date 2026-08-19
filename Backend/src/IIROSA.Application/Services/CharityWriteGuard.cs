using IIROSA.Application.Interfaces;
using IIROSA.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <inheritdoc />
public class CharityWriteGuard : ICharityWriteGuard
{
    private readonly ICharityRepository _charityRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CharityWriteGuard> _logger;

    public CharityWriteGuard(
        ICharityRepository charityRepository,
        ICurrentUserService currentUser,
        ILogger<CharityWriteGuard> logger)
    {
        _charityRepository = charityRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public Task EnsureCanAddAsync(CancellationToken cancellationToken = default) =>
        EnsureAsync(
            charity => charity.IsAddEnabled,
            "adding records is disabled for this charity",
            cancellationToken);

    public Task EnsureCanUpdateAsync(CancellationToken cancellationToken = default) =>
        EnsureAsync(
            charity => charity.IsUpdateEnabled,
            "editing records is disabled for this charity",
            cancellationToken);

    private async Task EnsureAsync(
        Func<Domain.Entities.Charity, bool> isPermitted,
        string deniedReason,
        CancellationToken cancellationToken)
    {
        var charityId = _currentUser.CharityId;

        // No charity claim means head office. The flags exist for head office to restrain a
        // charity, so they are not applied to head office itself.
        if (!charityId.HasValue)
        {
            return;
        }

        var charity = await _charityRepository.GetByIdAsync(charityId.Value);

        if (charity == null)
        {
            // The token names a charity that no longer exists. Refuse rather than fall through:
            // a dangling claim must not be treated as unrestricted.
            _logger.LogWarning(
                "User {UserId} carries charity claim {CharityId}, which does not resolve to a charity",
                _currentUser.UserId, charityId);

            throw new CharityWriteForbiddenException(
                "Your account is not linked to an active charity");
        }

        // Locked outranks the individual permissions: a locked charity may not write at all,
        // whatever its add/update flags say.
        if (charity.IsLocked)
        {
            _logger.LogInformation(
                "Write refused for charity {CharityId}: the account is locked", charityId);

            throw new CharityWriteForbiddenException("This charity's account is locked");
        }

        if (!isPermitted(charity))
        {
            _logger.LogInformation(
                "Write refused for charity {CharityId}: {Reason}", charityId, deniedReason);

            throw new CharityWriteForbiddenException($"Not permitted — {deniedReason}");
        }
    }
}
