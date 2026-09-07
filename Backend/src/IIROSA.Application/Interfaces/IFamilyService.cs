using IIROSA.Application.DTOs.Family;
using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Family Service Interface
/// Implements all use cases UC-4.1 through UC-4.15
/// </summary>
public interface IFamilyService
{
    #region UC-4.1: Register Family
    Task<FamilyDto> CreateFamilyAsync(CreateFamilyDto dto);

    /// <summary>
    /// UC-HOU-03 (§11.U.3): register a housing family — forces the Housing discriminator,
    /// resolves the building/flat allocation (flat must belong to the chosen building) and
    /// delegates to the shared create path. Served by POST /api/HousingProjects/projects.
    /// </summary>
    Task<FamilyDto> AddNewHousingFamilyAsync(CreateFamilyDto dto);

    /// <summary>
    /// UC-HOU-04 (§11.U.4): the housing-family aggregate for the view/edit screen — every
    /// §11.S.2 field incl. the guardian block and full child detail. Unknown, non-housing and
    /// foreign rows all answer 404 (a foreign id must not prove the record exists).
    /// Served by GET /api/HousingProjects/projects/{id}.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">Family does not exist, is not a housing
    /// family, or belongs to another charity than the caller's scope.</exception>
    Task<HousingFamilyDetailDto> GetHousingFamilyAsync(Guid id, Guid? userCharityId, string? userRole);

    /// <summary>
    /// UC-HOU-07 (§11.U.7 البحث بالكود): the housing family's beneficiaries — children +
    /// the guardian — for the §11.S.3 picker. A non-blank code resolves a child's
    /// sponsorship code EXACTLY (empty list = explicit not-found); the family itself is
    /// charity-scoped like the 6-4 read, so another charity's code never resolves.
    /// Served by GET /api/HousingProjects/projects/{id}/beneficiaries?code=.
    /// </summary>
    Task<List<HousingBeneficiaryDto>> GetHousingBeneficiariesAsync(Guid familyId, string? code, Guid? userCharityId, string? userRole);

    /// <summary>
    /// UC-HOU-04 (§11.U.4): update a housing family — family fields + allocation
    /// (flat ⊂ building re-validated) + guardian + children sync (id-matched update, id-less
    /// add, absent soft-remove) under the same §11.S.2 mandatory contract as create.
    /// Ownership NEVER moves (any client-sent charity field is ignored) and the register
    /// discriminator is immutable. Served by PUT /api/HousingProjects/projects/{id}.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">Family does not exist, is not a housing
    /// family, or belongs to another charity than the caller's scope.</exception>
    /// <exception cref="Exceptions.BusinessException">Building/flat allocation refused, a
    /// payload child does not belong to the family, or the duplicate-guardian rule
    /// «أحد المعيلين مكرر من قبل أكثر من مرة» fires.</exception>
    Task<HousingFamilyDetailDto> UpdateHousingFamilyAsync(Guid id, CreateFamilyDto dto, Guid? userCharityId, string? userRole, string? userName = null);
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
    Task<ProviderDto> AddProviderToFamilyAsync(Guid familyId, CreateProviderDto dto, Guid? userCharityId, string? userRole);

    /// <summary>
    /// UC-REF-04: edit the family's guardian (معيل) — the per-member PUT behind the §12.S.2
    /// refugee edit screen. Patch-style like UpdateRelativeAsync; the family's derived
    /// HeadOfFamily follows a renamed guardian. Tenancy is enforced here (fail-closed for a
    /// Charity token without a parseable charity claim) — the same rule as
    /// <see cref="GetProviderByFamilyAsync"/>.
    /// </summary>
    Task<ProviderDto> UpdateProviderAsync(Guid familyId, UpdateProviderDto dto, Guid? userCharityId, string? userRole);

    /// <summary>
    /// UC-REF-04: the family's guardian for the §12.S.2 view/edit screens. Charity-role callers
    /// are scoped to their own family (same isolation as GetFamilyByIdAsync).
    /// </summary>
    Task<ProviderDto> GetProviderByFamilyAsync(Guid familyId, Guid? userCharityId, string? userRole);
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

    /// <summary>
    /// Export the family list to Excel — the same query and scoping as
    /// <see cref="GetFamiliesAsync"/> with the page widened to every matching row.
    /// </summary>
    Task<byte[]> ExportFamiliesToExcelAsync(FamilyFilterDto filter, Guid? userCharityId, string? userRole);

    /// <summary>
    /// The follow-up report (UC-FAM-11 متابعة إدخالات الأسر): what was created or updated on the
    /// family files for one calendar day — a pure read over the inherited audit columns, nothing
    /// is stored. A <c>Charity</c>-role caller is scoped server-side to its own charity's
    /// activity; HQ roles see all and may narrow through the filter's charity parameter. Rows
    /// created on the day report kind Created (creation wins over an update); otherwise the last
    /// update on the day reports kind Updated. Ordered newest-first, paged.
    /// </summary>
    /// <param name="maxPageSize">
    /// The page-size clamp ceiling. The interactive report clamps at 100; only the print path
    /// (UC-FAM-14's tracking sheet, which prints the whole day) passes the internal 5000 ceiling.
    /// </param>
    Task<(IEnumerable<FamilyFollowUpListDto> Items, int TotalCount)> GetFollowUpActivityAsync(
        FamilyFollowUpFilterDto filter,
        Guid? userCharityId,
        string? userRole,
        int maxPageSize = 100);

    /// <summary>
    /// UC-RPT-13 (§23.S.10 متابعة إدخلات الأسر والأيتام) — entry tracking off the one
    /// <c>GET /api/Families/{id}/follow-up</c> endpoint: إجماليات (<c>mode=totals</c> →
    /// <see cref="FamilyFollowUpTotalsDto"/>) and تفاصيل (<c>mode=details</c> →
    /// <see cref="ReportPagedResult{T}"/> of <see cref="FamilyFollowUpDetailDto"/>) — families
    /// and orphans ENTERED on/after the filter date (new registrations, not the one-day
    /// activity of UC-FAM-11). Read-only; a Charity-role caller is clamped to its own charity
    /// whatever the route id says; HQ may name any charity (Guid.Empty → all charities).
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Mode outside the whitelist or
    /// out-of-range page bounds.</exception>
    Task<object> GetFamilyFollowUpAsync(
        FamilyEntryTrackingFilterDto filter,
        Guid? userCharityId,
        string? userRole);
    #endregion

    #region UC-4.13: View Family Details
    Task<FamilyDto> GetFamilyByIdAsync(Guid id, Guid? userCharityId, string? userRole);
    #endregion

    #region UC-4.14: Deactivate Family
    Task DeactivateFamilyAsync(Guid id);
    #endregion

    #region UC-FAM-06: Transfer Family to Another Charity

    /// <summary>
    /// UC-FAM-06 نقل الأسرة لجمعية أخرى — move the family and its dependents (orphans) to the
    /// receiving charity in one transaction, and record the movement (who, when, from, to, why).
    /// HQ-only: a charity-role caller is refused here as well as at the endpoint.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">Family does not exist.</exception>
    /// <exception cref="Exceptions.BusinessException">Target charity missing/inactive/same as current, or caller is a charity role.</exception>
    Task TransferFamilyToCharityAsync(Guid familyId, TransferFamilyDto dto, Guid? userCharityId, string? userRole);
    #endregion

    #region UC-FAM-07/08: Member Control (move an orphan / guardian between families)

    /// <summary>
    /// UC-FAM-07 نقل يتيم بين الأسر — the shared member-control endpoint. Legacy action contract:
    /// 0 = detach to a NEW holding family created under the source family's charity (justification
    /// appended to the orphan's Notes), 1 = attach to an existing family by register code.
    /// Within-charity only — cross-charity movement is UC-FAM-06. This story implements the
    /// orphan branch (memberType 1); memberType 2 (guardian) is UC-FAM-08's branch.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">Family or orphan does not exist.</exception>
    /// <exception cref="Exceptions.BusinessException">Mismatched member, unresolvable/same/cross-charity target, unsupported member type, or charity-role caller.</exception>
    Task ControlFamilyMemberAsync(Guid familyId, Guid memberId, MemberControlDto dto, Guid? userCharityId, string? userRole, string? userName = null);
    #endregion

    #region UC-FAM-13: Remove Guardian Sponsorship Link (حذف كفالة العائل)

    /// <summary>
    /// UC-FAM-13 حذف كفالة العائل — removes the family's guardian link: the provider row is
    /// soft-deleted (never hard-deleted) once no orphan of the family is still referenced by an
    /// ACTIVE sponsorship (SponsorId set + status "Sponsored"; Pending/Unsponsored do not block).
    /// The التعليق lands in the provider's Notes (member-control stamp format) before the delete.
    /// When the provider row was the acting guardian (ProviderType "Other"), the seat is vacated.
    /// HQ-only: a charity-role caller is refused here as well as on the endpoint.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">Family does not exist or has no guardian of record.</exception>
    /// <exception cref="Exceptions.BusinessException">A live sponsorship still references the family, or a charity-role caller.</exception>
    Task RemoveProviderSponsorLinkAsync(Guid familyId, RemoveProviderSponsorLinkDto dto, Guid? userCharityId, string? userRole, string? userName = null);
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

    #region Orphan Register & Coding (UC-ORP-01 through UC-ORP-10)

    /// <summary>
    /// UC-ORP-02/03/07 — the shared orphan read: search by name or code (all roles) and, when
    /// <c>CodingStatus</c> is supplied, the HQ-only coding worklist (uncoded = empty
    /// <c>Orphan.Code</c>). Charity callers are pinned to their own charity; HQ may narrow by
    /// <c>CharityId</c>.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">A charity caller supplied <c>CodingStatus</c> — the coding worklist is a head-office function.</exception>
    Task<(IEnumerable<OrphanLookupDto> Items, int TotalCount)> SearchOrphansAsync(OrphanSearchFilterDto filter, Guid? userCharityId, string? userRole);

    /// <summary>
    /// UC-ORP-01 — may an orphan be added? Verdict over the charity's write state (locked /
    /// add-disabled / inactive, mirroring <c>ICharityWriteGuard</c> but as a verdict, not a throw)
    /// and prior registration of the national ID within the caller's scope.
    /// </summary>
    Task<OrphanEligibilityDto> CheckOrphanCanBeAddedAsync(OrphanEligibilityCheckDto check, Guid? userCharityId, string? userRole);

    /// <summary>
    /// UC-SYS-12 — family/guardian-level national-id uniqueness check: is the id held by any
    /// person (father/mother/provider/relative/orphan) on another in-scope family? A clash
    /// names the holder; soft-deleted rows never block.
    /// </summary>
    Task<FamilyNationalIdCheckResultDto> CheckFamilyNationalIdAsync(CheckFamilyNationalIdDto check, Guid? userCharityId, string? userRole);

    /// <summary>
    /// UC-ORP-05 — verify a sponsorship code is not already used. BR-07: uniqueness is judged
    /// within the resolved charity's register.
    /// </summary>
    Task<OrphanCodeCheckDto> CheckOrphanCodeUniqueAsync(OrphanCodeCheckFilterDto filter, Guid? userCharityId, string? userRole);

    /// <summary>
    /// UC-ORP-06 — assign a sponsorship code. Re-checks uniqueness inside the save (race-safe
    /// BR-07); the first code assignment flips SponsorshipStatus null/"Pending" to "Unsponsored".
    /// A charity caller may only code its own orphans.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">The orphan belongs to another charity.</exception>
    /// <exception cref="InvalidOperationException">The code is already used within the charity (BR-07).</exception>
    Task<OrphanDto> AssignOrphanCodeAsync(AssignOrphanCodeDto dto, Guid? userCharityId, string? userRole);

    /// <summary>
    /// UC-ORP-10 — check a phone number is not duplicated across the five phone holders of every
    /// other family in scope (Family, Father, Mother, Provider, Orphan). The given family's own
    /// records never clash with themselves. Type-agnostic (no phone column carries a type today).
    /// </summary>
    Task<PhoneCheckDto> CheckPhoneNumberDuplicateAsync(Guid familyId, PhoneCheckFilterDto filter, Guid? userCharityId, string? userRole);
    #endregion
}
