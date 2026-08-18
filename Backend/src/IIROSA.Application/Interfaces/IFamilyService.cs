using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Family Service Interface
/// Implements all use cases UC-4.1 through UC-4.15
/// </summary>
public interface IFamilyService
{
    #region UC-4.1: Register Family
    Task<FamilyDto> CreateFamilyAsync(CreateFamilyDto dto);
    #endregion

    #region UC-4.2: Add Family Father
    Task<FatherDto> AddFatherToFamilyAsync(Guid familyId, CreateFatherDto dto);
    #endregion

    #region UC-4.3: Add Family Mother
    Task<MotherDto> AddMotherToFamilyAsync(Guid familyId, CreateMotherDto dto);
    #endregion

    #region UC-4.4: Add Orphan to Family
    Task<OrphanDto> AddOrphanToFamilyAsync(Guid familyId, CreateOrphanDto dto);
    #endregion

    #region UC-4.5: Specify Provider Type
    Task SetProviderTypeAsync(Guid familyId, string providerType);
    #endregion

    #region UC-4.6: Add Non-Parent Provider
    Task<ProviderDto> AddProviderToFamilyAsync(Guid familyId, CreateProviderDto dto);
    #endregion

    #region UC-4.7: Verify Parent as Provider
    Task VerifyParentProviderAsync(Guid familyId, bool fatherIsProvider, bool motherIsProvider, string? notes);
    #endregion

    #region UC-4.8: Update Family Information
    Task<FamilyDto> UpdateFamilyAsync(UpdateFamilyDto dto);
    #endregion

    #region UC-4.9: Update Father Details
    Task<FatherDto> UpdateFatherAsync(UpdateFatherDto dto);
    #endregion

    #region UC-4.10: Update Mother Details
    Task<MotherDto> UpdateMotherAsync(UpdateMotherDto dto);
    #endregion

    #region UC-4.11: Update Orphan Details
    Task<OrphanDto> UpdateOrphanAsync(UpdateOrphanDto dto);
    #endregion

    #region UC-4.12: View Family List
    Task<(IEnumerable<FamilyListDto> Items, int TotalCount)> GetFamiliesAsync(FamilyFilterDto filter, Guid? userCharityId, string? userRole);
    #endregion

    #region UC-4.13: View Family Details
    Task<FamilyDto> GetFamilyByIdAsync(Guid id, Guid? userCharityId, string? userRole);
    #endregion

    #region UC-4.14: Deactivate Family
    Task DeactivateFamilyAsync(Guid id);
    #endregion

    #region UC-4.15: Attach Family Documents
    Task<Guid> AttachDocumentAsync(Guid familyId, string fileName, string contentType, byte[] fileData, string documentType, string? description);
    #endregion

    #region Additional Helper Methods
    Task<FamilyDto?> GetByIdAsync(Guid id);
    Task<FamilyDto?> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    Task<IEnumerable<OrphanListDto>> GetFamilyOrphansAsync(Guid familyId);
    Task<FatherDto?> GetFamilyFatherAsync(Guid familyId);
    Task<MotherDto?> GetFamilyMotherAsync(Guid familyId);
    Task<ProviderDto?> GetFamilyProviderAsync(Guid familyId);
    #endregion

    #region UC-4.7: Add Family Relative
    Task<RelativeDto> AddRelativeToFamilyAsync(Guid familyId, CreateRelativeDto dto);
    #endregion

    #region UC-4.8: Update Relative Details
    Task<RelativeDto> UpdateRelativeAsync(UpdateRelativeDto dto);
    #endregion

    #region UC-4.9: Remove Relative from Family
    Task RemoveRelativeAsync(Guid familyId, Guid relativeId);
    #endregion

    #region Relative Helper Methods
    Task<IEnumerable<RelativeListDto>> GetFamilyRelativesAsync(Guid familyId);
    Task<RelativeDto?> GetRelativeByIdAsync(Guid relativeId);
    #endregion
}
