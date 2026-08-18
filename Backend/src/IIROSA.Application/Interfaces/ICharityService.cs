using IIROSA.Application.DTOs.Charity;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Charity Service Interface
/// Implements all use cases UC-3.1 through UC-3.14
/// </summary>
public interface ICharityService
{
    // UC-3.1: Register Charity
    Task<CharityDto> CreateCharityAsync(CreateCharityDto dto);

    // UC-3.2: Update Charity Details
    Task<CharityDto> UpdateCharityAsync(UpdateCharityDto dto);

    // UC-3.3: Activate Charity
    Task ActivateCharityAsync(Guid id);

    // UC-3.4: Deactivate Charity
    Task DeactivateCharityAsync(Guid id);

    // UC-3.5: Change Charity Password
    Task<string> ResetPasswordAsync(Guid id);

    // UC-3.6: Enable/Disable Add Rights
    Task SetAddRightsAsync(Guid id, bool isEnabled);

    // UC-3.7: Enable/Disable Update Rights
    Task SetUpdateRightsAsync(Guid id, bool isEnabled);

    // UC-3.8: Lock Charity
    Task LockCharityAsync(Guid id);

    // UC-3.8 (continued): Unlock Charity
    Task UnlockCharityAsync(Guid id);

    // UC-3.9: Set Bank Account Details
    Task SetBankDetailsAsync(CharityBankDetailsDto dto);

    // UC-3.10: View All Charities
    Task<(IEnumerable<CharityListDto> Items, int TotalCount)> GetCharitiesAsync(CharityFilterDto filter);

    // UC-3.11: View Charity Profile
    Task<CharityProfileDto> GetCharityProfileAsync(Guid id);
    Task<CharityProfileDto> GetMyProfileAsync(string userId);

    // UC-3.12: Assign Charity to Center
    Task SetLocationAsync(CharityLocationDto dto);

    // UC-3.13: Manage Charity Contacts
    Task UpdateManagementContactsAsync(CharityManagementContactsDto dto);

    // UC-3.14: Set Map Location
    Task SetMapLocationAsync(Guid id, string mapLocation);

    // Additional helper methods
    Task<CharityDto?> GetByIdAsync(Guid id);
    Task<CharityDto?> GetByCodeAsync(string code);
    Task<CharityDto?> GetByUserIdAsync(string userId);
    Task<IEnumerable<CharityListDto>> GetActiveCharitiesAsync();
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    Task DeleteCharityAsync(Guid id);
}
