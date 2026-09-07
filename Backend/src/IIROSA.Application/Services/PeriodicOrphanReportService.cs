using System.Linq.Expressions;
using FluentValidation;
using Framework.Identity.Data.Entities;
using IIROSA.Application.DTOs.PeriodicOrphanReport;
using IIROSA.Application.Exceptions;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Periodic Orphan Report service — epic 9 (UC-ORR-01 … UC-ORR-17, from WAR.IIROSA UC-6.11–6.17).
///
/// Platform invariants honoured here:
/// - Every read/write is scoped server-side to the caller's charity/country
///   (claims pin, never widen; out-of-scope records read as NotFound).
/// - Paged reads return <see cref="PeriodicOrphanReportPagedResult{T}"/> — the platform envelope.
/// - Only <see cref="IUnitOfWork"/> saves; repositories never call SaveChanges.
/// - Deletes are soft (audit interceptor sets IsDeleted via the global query filter).
/// - Validation runs in this layer via FluentValidation, not in the controller.
/// </summary>
public class PeriodicOrphanReportService : IPeriodicOrphanReportService
{
    private readonly IRepository<PeriodicOrphanReport> _reportRepository;
    private readonly IRepository<Orphan> _orphanRepository;
    private readonly IRepository<Charity> _charityRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IRepository<EducationLevel> _educationLevelRepository;
    private readonly IRepository<RefuseReason> _refuseReasonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreatePeriodicOrphanReportDto> _createValidator;
    private readonly IValidator<UpdatePeriodicOrphanReportDto> _updateValidator;
    private readonly IValidator<ReviewPeriodicReportDto> _reviewValidator;
    private readonly ILogger<PeriodicOrphanReportService> _logger;
    // UC-HOU-06 (6-6): the guardian branch resolves Provider → housing Family before reading
    private readonly IRepository<Provider> _providerRepository;
    private readonly IRepository<Family> _familyRepository;
    private readonly IValidator<PeriodicOrphanReportFilterDto> _filterValidator;

    public PeriodicOrphanReportService(
        IRepository<PeriodicOrphanReport> reportRepository,
        IRepository<Orphan> orphanRepository,
        IRepository<Charity> charityRepository,
        IRepository<ApplicationUser> userRepository,
        IRepository<EducationLevel> educationLevelRepository,
        IRepository<RefuseReason> refuseReasonRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<CreatePeriodicOrphanReportDto> createValidator,
        IValidator<UpdatePeriodicOrphanReportDto> updateValidator,
        IValidator<ReviewPeriodicReportDto> reviewValidator,
        IValidator<PeriodicOrphanReportFilterDto> filterValidator,
        ILogger<PeriodicOrphanReportService> logger,
        IRepository<Provider> providerRepository,
        IRepository<Family> familyRepository)
    {
        _reportRepository = reportRepository;
        _orphanRepository = orphanRepository;
        _charityRepository = charityRepository;
        _userRepository = userRepository;
        _educationLevelRepository = educationLevelRepository;
        _refuseReasonRepository = refuseReasonRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _reviewValidator = reviewValidator;
        _filterValidator = filterValidator;
        _logger = logger;
        _providerRepository = providerRepository;
        _familyRepository = familyRepository;
    }

    #region CRUD Operations

    /// <summary>
    /// Create a periodic report (UC-ORR-03; UC-HOU-08 through the §11.U.6 discriminator).
    /// Absent housing fields ⇒ the epic-9 regular orphan report, byte-identical.
    /// </summary>
    public async Task<PeriodicOrphanReportDto> CreateReportAsync(CreatePeriodicOrphanReportDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        // §11.U.6 discriminator (6-8): Child ⇒ the orphan rules as before; Parent ⇒ the
        // §11.S.4 guardian report — the branch resolves who is reported on and stamps the
        // housing link; the orphan identity never comes straight from the client payload.
        var beneficiaryType = ParseBeneficiaryType(dto.ChildOrParent);

        var reportYear = dto.ReportDate.Year;
        var reportMonth = dto.ReportDate.Month;

        Guid orphanId;
        Guid? charityId;
        Guid? housingFamilyId = null;

        if (beneficiaryType == Domain.Enums.ReportBeneficiaryType.Parent)
        {
            (orphanId, charityId, housingFamilyId) =
                await ResolveGuardianReportTargetAsync(dto, reportYear, reportMonth);
        }
        else
        {
            // Review 2026-08-24: GetByIdAsync is unfiltered FindAsync — a soft-deleted
            // orphan must read as not-found, never as a reportable beneficiary.
            var orphan = await _orphanRepository.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == dto.OrphanId && !o.IsDeleted)
                ?? throw new NotFoundException(nameof(Orphan), dto.OrphanId);

            // Scope: a charity user can only report on their own orphans (pin, never widen).
            if (!await IsOrphanInCallerScopeAsync(orphan))
                throw new NotFoundException(nameof(Orphan), dto.OrphanId);

            // Review P14 2026-08-24: the §14.S.2 pre-condition enforced server-side —
            // the reporting cycle starts from the 9-2 code lookup, and an uncoded orphan
            // can never be its answer, so it can never be reported on either.
            if (string.IsNullOrWhiteSpace(orphan.Code))
                throw new BusinessException(
                    "This orphan has no sponsorship code on record; periodic reports are filed for coded orphans only.");

            await EnsureNoDuplicateAsync(orphan.Id, reportYear, reportMonth, excludeReportId: null);

            orphanId = orphan.Id;
            charityId = orphan.FK_CharityId;

            // §11.S.4 child path: a housing family context stamps the link (validated Housing
            // type + caller scope + the child belongs to it). The 6-6 Child read filters by
            // orphan id, so the stamp is register context, not a read dependency.
            if (dto.HousingFamilyId.HasValue)
            {
                var family = await GetScopedHousingFamilyAsync(dto.HousingFamilyId.Value);
                if (orphan.FamilyId != family.Id)
                    throw new NotFoundException(nameof(Family), family.Id);
                housingFamilyId = family.Id;
            }
        }

        // Review P4 2026-08-24: a report stamped with a null CharityId is invisible to
        // every charity-scoped reader (ApplyCallerScope pins on CharityId) and its create
        // response would map to a null body — refuse the create at the source instead.
        if (!charityId.HasValue)
            throw new BusinessException(
                "The beneficiary has no charity on record, so a periodic report cannot be filed for them.");

        // Review 2026-08-24: restore the charity feature flag the story's reality check
        // said to preserve — it gates both branches (the flag is charity-level, never
        // beneficiary-level). No-op until epic 19 wires a real configuration source.
        if (!await IsPeriodicReportsEnabledForCharityAsync(charityId.Value))
            throw new BusinessException("Periodic reports are not enabled for this charity.");

        var report = new PeriodicOrphanReport
        {
            Id = Guid.NewGuid(),
            OrphanId = orphanId,
            OrphanPaymentId = dto.OrphanPaymentId,
            CharityId = charityId,
            ChildOrParent = beneficiaryType,
            FK_HousingFamilyId = housingFamilyId,
            ReportDate = dto.ReportDate,
            ReportPeriodFrom = dto.ReportPeriodFrom,
            ReportPeriodTo = dto.ReportPeriodTo,
            // Review 2026-08-24 (binding decision 3 enforced): the number is server-owned —
            // a client-sent ReportNo is ignored on create AND update, so direct API callers
            // can neither forge nor collide numbers.
            ReportNo = await GenerateReportNumberAsync(reportYear),
            ReportYear = reportYear,
            ReportMonth = reportMonth,
            ReviewStatus = "Pending",
            Reviewed = false,
            IsAccepted = false,
            IsRefused = false,
            Locked = false,
            Active = true,
            ActiveDate = DateTime.UtcNow,
            PrayerStatus = dto.PrayerStatus,
            MannersStatus = dto.MannersStatus,
            HadeethStatus = dto.HadeethStatus,
            QuranParts = dto.QuranParts,
            QuranVerses = dto.QuranVerses,
            MedicalStatus = dto.MedicalStatus,
            Disease = dto.Disease,
            Disability = dto.Disability,
            DisabilityDescription = dto.DisabilityDescription,
            DiseaseDescription = dto.DiseaseDescription,
            MedicalReportImageId = dto.MedicalReportImageId,
            Hobby = dto.Hobby,
            Course = dto.Course,
            CourseName = dto.CourseName,
            SportName = dto.SportName,
            ProfessionName = dto.ProfessionName,
            Achievement = dto.Achievement,
            AchievementArr = dto.AchievementArr,
            Wish = dto.Wish,
            WishArr = dto.WishArr,
            OrphanMessage = dto.OrphanMessage,
            EducationalStageId = dto.EducationalStageId,
            EducationalLevelId = dto.EducationalLevelId,
            Grade = dto.Grade,
            School = dto.School,
            SchoolType = dto.SchoolType,
            EducationDegree = dto.EducationDegree,
            HighestEducationalLevel = dto.HighestEducationalLevel,
            HighestEducationalLevelYear = dto.HighestEducationalLevelYear,
            IsOrphanStudent = dto.IsOrphanStudent,
            EducationalYear = dto.EducationalYear,
            AnnualFeeForStudy = dto.AnnualFeeForStudy,
            StudyingYears = dto.StudyingYears,
            RestStudyingYears = dto.RestStudyingYears,
            GraduationYear = dto.GraduationYear,
            DropOut = dto.DropOut,
            DropOutYear = dto.DropOutYear,
            DropOutStageId = dto.DropOutStageId,
            Faculty = dto.Faculty,
            Department = dto.Department,
            Specialization = dto.Specialization,
            Married = dto.Married,
            MarriageDate = dto.MarriageDate,
            OrphanMarriageImageId = dto.OrphanMarriageImageId,
            Dead = dto.Dead,
            DeathDate = dto.DeathDate,
            OrphanDeadImageId = dto.OrphanDeadImageId,
            OrphanCertificateImageId = dto.OrphanCertificateImageId,
            OrphanImageId = dto.OrphanImageId,
            MissingDocuments = dto.MissingDocuments,
            MissingDocumentsName = dto.MissingDocumentsName
        };

        // §11.S.4 review flags are DATA on create (the workflow endpoints are 9-7/9-8): an
        // unticked form stays Pending exactly like the epic-9 create; a ticked decision is
        // stored coherently — reviewed, dated, reasoned — mirroring ReviewReportAsync.
        await ApplyCreateReviewFlagsAsync(report, dto);

        await _reportRepository.InsertAsync(report);
        // Review P9 2026-08-24: the duplicate pre-checks above can lose a race with a
        // concurrent create; the filtered unique indexes are the hard guard — surface
        // their refusal as the same friendly 400 instead of a raw 500.
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicateReportSlotViolation(ex, out var violation))
        {
            throw new BusinessException(violation);
        }

        _logger.LogInformation("Periodic report {ReportNo} created for orphan {OrphanId} by {UserId}",
            report.ReportNo, report.OrphanId, _currentUser.UserId);

        return (await GetByIdAsync(report.Id))!;
    }

    /// <summary>
    /// Update a periodic report (UC-ORR-05). Locked and accepted reports are immutable;
    /// updating a refused report resets it to Pending (resubmission).
    /// </summary>
    public async Task<PeriodicOrphanReportDto> UpdateReportAsync(UpdatePeriodicOrphanReportDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var report = await GetScopedReportAsync(dto.Id);

        // §14.U.5 exception flow — locked or accepted reports are "old" and immutable.
        // Legacy wording preserved verbatim; the SPA maps it to its translated key.
        if (report.Locked || report.IsAccepted)
            throw new BusinessException("You can not update old report");

        // Period keys follow the (possibly new) report date.
        if (dto.ReportDate.HasValue && dto.ReportDate.Value != report.ReportDate)
        {
            report.ReportDate = dto.ReportDate.Value;
            report.ReportYear = dto.ReportDate.Value.Year;
            report.ReportMonth = dto.ReportDate.Value.Month;
            await EnsureNoDuplicateAsync(report.OrphanId, report.ReportYear!.Value, report.ReportMonth!.Value,
                excludeReportId: report.Id);

            // Review P11 2026-08-24: a re-dated guardian report must respect the
            // one-guardian-report-per-family-month rule too — its unique index spans the
            // whole family, not just this carrier orphan, so the orphan-level check
            // above cannot see the clash.
            if (report.ChildOrParent == Domain.Enums.ReportBeneficiaryType.Parent && report.FK_HousingFamilyId.HasValue)
                await EnsureNoFamilyMonthDuplicateAsync(report.FK_HousingFamilyId.Value,
                    report.ReportYear!.Value, report.ReportMonth!.Value, excludeReportId: report.Id);
        }

        // Full-replace contract (9-5): the SPA loads the report into the §14.S.2 form
        // and PUTs every control, so sent values — including explicit nulls — are
        // stored as-is; a cleared optional field actually clears. The update validator
        // enforces the mandatory subset so an incomplete payload cannot wipe a record.
        // ReportNo stays server-owned and the orphan identity is immutable on update.
        report.OrphanPaymentId = dto.OrphanPaymentId;
        report.ReportPeriodFrom = dto.ReportPeriodFrom;
        report.ReportPeriodTo = dto.ReportPeriodTo;
        // Review 2026-08-24 (binding decision 3 enforced): ReportNo is server-owned —
        // the number is assigned once on create; a client-sent value is ignored here too.
        report.PrayerStatus = dto.PrayerStatus;
        report.MannersStatus = dto.MannersStatus;
        report.HadeethStatus = dto.HadeethStatus;
        report.QuranParts = dto.QuranParts;
        report.QuranVerses = dto.QuranVerses;
        report.MedicalStatus = dto.MedicalStatus;
        report.Disease = dto.Disease;
        report.Disability = dto.Disability;
        report.DisabilityDescription = dto.DisabilityDescription;
        report.DiseaseDescription = dto.DiseaseDescription;
        report.MedicalReportImageId = dto.MedicalReportImageId;
        report.Hobby = dto.Hobby;
        report.Course = dto.Course;
        report.CourseName = dto.CourseName;
        report.SportName = dto.SportName;
        report.ProfessionName = dto.ProfessionName;
        report.Achievement = dto.Achievement;
        report.AchievementArr = dto.AchievementArr;
        report.Wish = dto.Wish;
        report.WishArr = dto.WishArr;
        report.OrphanMessage = dto.OrphanMessage;
        report.EducationalStageId = dto.EducationalStageId;
        report.EducationalLevelId = dto.EducationalLevelId;
        report.Grade = dto.Grade;
        report.School = dto.School;
        report.SchoolType = dto.SchoolType;
        report.EducationDegree = dto.EducationDegree;
        report.HighestEducationalLevel = dto.HighestEducationalLevel;
        report.HighestEducationalLevelYear = dto.HighestEducationalLevelYear;
        report.IsOrphanStudent = dto.IsOrphanStudent;
        report.EducationalYear = dto.EducationalYear;
        report.AnnualFeeForStudy = dto.AnnualFeeForStudy;
        report.StudyingYears = dto.StudyingYears;
        report.RestStudyingYears = dto.RestStudyingYears;
        report.GraduationYear = dto.GraduationYear;
        report.DropOut = dto.DropOut;
        report.DropOutYear = dto.DropOutYear;
        report.DropOutStageId = dto.DropOutStageId;
        report.Faculty = dto.Faculty;
        report.Department = dto.Department;
        report.Specialization = dto.Specialization;
        report.Married = dto.Married;
        report.MarriageDate = dto.MarriageDate;
        report.OrphanMarriageImageId = dto.OrphanMarriageImageId;
        report.Dead = dto.Dead;
        report.DeathDate = dto.DeathDate;
        report.OrphanDeadImageId = dto.OrphanDeadImageId;
        report.OrphanCertificateImageId = dto.OrphanCertificateImageId;
        report.OrphanImageId = dto.OrphanImageId;
        report.MissingDocuments = dto.MissingDocuments;
        report.MissingDocumentsName = dto.MissingDocumentsName;

        // Resubmission: correcting a refused report sends it back to the review queue.
        if (report.IsRefused)
        {
            report.Reviewed = false;
            report.IsAccepted = false;
            report.IsRefused = false;
            report.ReviewStatus = "Pending";
            report.ReviewedDate = null;
            report.ReviewerId = null;
            report.RefuseReason = null;
            report.RefuseReasonId = null;
            report.ReviewComments = null;
        }

        await _reportRepository.UpdateAsync(report);
        // Review P9 2026-08-24: same race guard as create — a re-dated report can lose
        // the pre-check race with a concurrent create on the same slot.
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicateReportSlotViolation(ex, out var violation))
        {
            throw new BusinessException(violation);
        }

        _logger.LogInformation("Periodic report {ReportId} updated by {UserId}", report.Id, _currentUser.UserId);

        return (await GetByIdAsync(report.Id))!;
    }

    /// <summary>Fetch a single report with its navigations, enforcing caller scope.</summary>
    public async Task<PeriodicOrphanReportDto?> GetByIdAsync(Guid id)
    {
        var report = await _reportRepository.AsQueryable()
            .Include(r => r.Orphan)
            .Include(r => r.Charity)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (report == null || !await IsWithinCallerScopeAsync(report))
            return null;

        return await MapToDetailDtoAsync(report);
    }

    /// <summary>
    /// UC-ORR-17 (§14.U.17 طباعة التقرير الدوري) — the print payload: caller-scoped detail
    /// data + the resolved layout variant + the attachment slots present. Read-only; the SPA
    /// renders the form (client-side print ruling). Null = nothing to produce (AC 4/5).
    /// </summary>
    public async Task<OrphanReportFormPrintDto?> GetPrintFormAsync(Guid id)
    {
        // Review P17 2026-08-26: the id-required rule lives with the query, not the
        // controller (was a controller-side null-check) — an empty id is a validation
        // failure, not a 404.
        if (id == Guid.Empty)
        {
            throw new FluentValidation.ValidationException("Report id is required");
        }

        var report = await _reportRepository.AsQueryable()
            .Include(r => r.Orphan)
            .Include(r => r.Charity)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (report == null || !await IsWithinCallerScopeAsync(report))
            return null;

        var attachments = new List<OrphanReportFormAttachmentSlotDto>(5);
        if (report.OrphanImageId.HasValue)
            attachments.Add(new OrphanReportFormAttachmentSlotDto { Slot = "orphanPhoto", Id = report.OrphanImageId.Value });
        if (report.OrphanCertificateImageId.HasValue)
            attachments.Add(new OrphanReportFormAttachmentSlotDto { Slot = "certificate", Id = report.OrphanCertificateImageId.Value });
        if (report.MedicalReportImageId.HasValue)
            attachments.Add(new OrphanReportFormAttachmentSlotDto { Slot = "medicalReport", Id = report.MedicalReportImageId.Value });
        if (report.OrphanDeadImageId.HasValue)
            attachments.Add(new OrphanReportFormAttachmentSlotDto { Slot = "deathCertificate", Id = report.OrphanDeadImageId.Value });
        if (report.OrphanMarriageImageId.HasValue)
            attachments.Add(new OrphanReportFormAttachmentSlotDto { Slot = "marriageContract", Id = report.OrphanMarriageImageId.Value });

        return new OrphanReportFormPrintDto
        {
            Variant = OrphanReportFormVariantResolver.Resolve(report),
            Report = await MapToDetailDtoAsync(report),
            Attachments = attachments
        };
    }

    /// <summary>
    /// Soft-delete a periodic report (UC-ORR-06). Only a locked report is protected —
    /// HQ deletes erroneous records including reviewed ones (§14.D-25.5 alternate A2).
    /// </summary>
    public async Task DeleteReportAsync(Guid id)
    {
        var report = await GetScopedReportAsync(id);

        if (report.Locked)
            throw new BusinessException("This report is locked and cannot be deleted. Unlock it first.");

        // Review 2026-08-24: actually soft-delete — RepositoryBase.Delete is a hard
        // DbSet.Remove, which contradicted both the log line and the platform convention.
        // Reads filter IsDeleted; the (OrphanId, year, month) slot checks stay unfiltered
        // by design — a soft-deleted row still holds its duplicate slot.
        report.IsDeleted = true;
        report.DeletedOn = DateTime.UtcNow;
        report.DeletedBy = _currentUser.UserId?.ToString() ?? "System";
        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Periodic report {ReportId} deleted (soft) by {UserId}", report.Id, _currentUser.UserId);
    }

    #endregion

    #region Orphan Lookup (UC-ORR-02)

    /// <summary>
    /// Look an orphan up by sponsorship code (UC-ORR-02). Returns null for unknown codes
    /// and for orphans outside the caller's scope — both read as "not found".
    /// </summary>
    public async Task<OrphanLookupDto?> GetOrphanByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var normalized = code.Trim();

        var orphan = await _orphanRepository.AsQueryable()
            .Include(o => o.Family!)
                .ThenInclude(f => f.Providers)
            .FirstOrDefaultAsync(o => o.Code == normalized && !o.IsDeleted);

        if (orphan == null || !await IsOrphanInCallerScopeAsync(orphan))
            return null;

        var scopedReports = ApplyCallerScope(_reportRepository.AsQueryable())
            .Where(r => r.OrphanId == orphan.Id);
        var totalReports = await scopedReports.CountAsync();
        var pendingReports = await scopedReports.CountAsync(r => !r.Reviewed);

        string? charityName = null;
        if (orphan.FK_CharityId.HasValue)
        {
            var charity = await _charityRepository.GetByIdAsync(orphan.FK_CharityId.Value);
            charityName = charity?.Name;
        }

        return new OrphanLookupDto
        {
            OrphanId = orphan.Id,
            Code = orphan.Code,
            FullName = orphan.FullName,
            CharityId = orphan.FK_CharityId,
            CharityName = charityName,
            Gender = orphan.Gender,
            Age = CalculateAge(orphan.DateOfBirth),
            BirthDate = orphan.DateOfBirth,
            FamilyId = orphan.FamilyId,
            FamilyCode = orphan.Family?.Code,
            GuardianName = orphan.Family?.Providers
                ?.Where(p => !p.IsDeleted)
                .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                .Select(p => p.FullName)
                .FirstOrDefault(),
            TotalReports = totalReports,
            PendingReports = pendingReports
        };
    }

    #endregion

    #region Review Operations (UC-ORR-07, UC-ORR-08)

    /// <summary>
    /// Accept (UC-ORR-07) or refuse (UC-ORR-08) a report. The reviewer is always the
    /// current user; refusing requires a reason from the lookup or free text.
    /// </summary>
    public async Task<PeriodicOrphanReportDto> ReviewReportAsync(ReviewPeriodicReportDto dto)
    {
        await _reviewValidator.ValidateAndThrowAsync(dto);

        var report = await GetScopedReportAsync(dto.ReportId);

        var reviewerId = _currentUser.UserId
            ?? throw new BusinessException("Only authenticated head-office reviewers can review reports.");

        // Review D4 2026-08-26: review is an HQ act — the endpoint authorizes the review roles,
        // the service re-asserts it for any other caller — and no one reviews their own
        // submission: the submitter self-approving was an unguarded path.
        if (_currentUser.IsInRole("Charity"))
            throw new UnauthorizedAccessException("Reviewing periodic reports is a head-office action.");
        if (string.Equals(report.CreatedBy, reviewerId.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("A report cannot be reviewed by its own submitter.");

        // §25.5 — a decided report stays decided; re-deciding goes through the
        // charity's edit-and-resubmit path (9-5), never a second review call.
        if (report.Reviewed)
            throw new BusinessException("This report has already been reviewed.");

        if (dto.IsApproved)
        {
            report.Reviewed = true;
            report.IsAccepted = true;
            report.IsRefused = false;
            report.ReviewStatus = "Approved";
            report.ReviewedDate = DateTime.UtcNow;
            report.ReviewerId = reviewerId;
            report.RefuseReason = null;
            report.RefuseReasonId = null;
            report.ReviewComments = string.IsNullOrWhiteSpace(dto.ReviewComments) ? null : dto.ReviewComments.Trim();
        }
        else
        {
            // Resolve the refusal reason: explicit free text wins, otherwise the lookup entry.
            string reasonText = (dto.RefuseReason ?? string.Empty).Trim();
            if (dto.RefuseReasonId.HasValue)
            {
                var lookupReason = await _refuseReasonRepository.GetByIdAsync(dto.RefuseReasonId.Value)
                    ?? throw new BusinessException("The selected refuse reason does not exist.");
                if (!lookupReason.IsActive)
                    throw new BusinessException("The selected refuse reason is not active.");
                if (reasonText.Length == 0)
                    reasonText = lookupReason.NameAr ?? lookupReason.NameEn ?? lookupReason.Name ?? string.Empty;
            }

            report.Reviewed = true;
            report.IsAccepted = false;
            report.IsRefused = true;
            report.ReviewStatus = "Rejected";
            report.ReviewedDate = DateTime.UtcNow;
            report.ReviewerId = reviewerId;
            report.RefuseReason = reasonText;
            report.RefuseReasonId = dto.RefuseReasonId;
            report.ReviewComments = string.IsNullOrWhiteSpace(dto.ReviewComments) ? null : dto.ReviewComments.Trim();
        }

        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Periodic report {ReportId} reviewed ({Decision}) by {UserId}",
            report.Id, dto.IsApproved ? "accepted" : "refused", reviewerId);

        return (await GetByIdAsync(report.Id))!;
    }

    #endregion

    #region List and Filter Operations

    /// <summary>List/filter periodic reports (UC-ORR-01, UC-ORR-09).</summary>
    public async Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetReportsAsync(
        PeriodicOrphanReportFilterDto filter)
    {
        return await GetPagedAsync(filter);
    }

    /// <summary>List accepted reports (UC-ORR-12).</summary>
    public async Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetApprovedReportsAsync(
        PeriodicOrphanReportFilterDto filter)
    {
        filter.Reviewed = true;
        filter.IsAccepted = true;
        filter.IsRefused = false;
        return await GetPagedAsync(filter);
    }

    /// <summary>List refused reports (UC-ORR-13).</summary>
    public async Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetRejectedReportsAsync(
        PeriodicOrphanReportFilterDto filter)
    {
        filter.Reviewed = true;
        filter.IsAccepted = false;
        filter.IsRefused = true;
        return await GetPagedAsync(filter);
    }

    /// <summary>
    /// An orphan's complete periodic report history, newest first (UC-ORR-01). Review
    /// 2026-08-24: the discriminator is pinned to Child — a guardian report rides the
    /// carrier child's OrphanId and must not appear in the child's own history (6-6).
    /// </summary>
    public async Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetReportsByOrphanAsync(
        Guid orphanId, int pageNumber = 1, int pageSize = 20)
    {
        var orphan = await _orphanRepository.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == orphanId && !o.IsDeleted)
            ?? throw new NotFoundException(nameof(Orphan), orphanId);
        if (!await IsOrphanInCallerScopeAsync(orphan))
            throw new NotFoundException(nameof(Orphan), orphanId);

        var filter = new PeriodicOrphanReportFilterDto
        {
            OrphanId = orphanId,
            ChildOrParent = Domain.Enums.ReportBeneficiaryType.Child,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = "CreatedOn",
            SortDirection = "DESC"
        };
        return await GetPagedAsync(filter);
    }

    /// <summary>
    /// UC-HOU-06 (§11.S.3 التقارير الدورية للأسر الساكنة): one housing beneficiary's report
    /// history, selected by the ChildOrParent discriminator. Child ⇒ <paramref name="beneficiaryId"/>
    /// is an orphan id (legacy rows included — every pre-housing report is a child report by
    /// definition); Parent ⇒ <paramref name="beneficiaryId"/> is the housing family's guardian
    /// (Provider) id — resolved to the family, then read through FK_HousingFamilyId +
    /// ChildOrParent == Parent. Unknown / non-housing / out-of-scope beneficiaries all answer
    /// <see cref="NotFoundException"/> (404-not-leak — a foreign id must not prove existence);
    /// the report query itself carries the caller-scope pin via <see cref="ApplyCallerScope"/>.
    /// </summary>
    public async Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetHousingBeneficiaryReportsAsync(
        Guid beneficiaryId, Domain.Enums.ReportBeneficiaryType beneficiaryType, int pageNumber = 1, int pageSize = 20)
    {
        var filter = new PeriodicOrphanReportFilterDto
        {
            ChildOrParent = beneficiaryType,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = "CreatedOn",
            SortDirection = "DESC"
        };

        if (beneficiaryType == Domain.Enums.ReportBeneficiaryType.Parent)
        {
            // Guardian branch: the beneficiary is the Provider of a HOUSING family — resolve,
            // scope-gate, then list the family's Parent reports (linked by family, not orphan).
            var provider = await _providerRepository.AsQueryable()
                .FirstOrDefaultAsync(p => p.Id == beneficiaryId && !p.IsDeleted)
                ?? throw new NotFoundException(nameof(Provider), beneficiaryId);

            var family = provider.FamilyId.HasValue
                ? await _familyRepository.GetByIdAsync(provider.FamilyId.Value)
                : null;

            if (family == null || family.IsDeleted
                || family.FamilyType != Domain.Enums.FamilyType.Housing
                || !await IsFamilyInCallerScopeAsync(family))
            {
                throw new NotFoundException(nameof(Provider), beneficiaryId);
            }

            filter.HousingFamilyId = family.Id;
        }
        else
        {
            // Child branch: the beneficiary is an orphan — identical gate to the epic-9 read.
            var orphan = await _orphanRepository.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == beneficiaryId && !o.IsDeleted)
                ?? throw new NotFoundException(nameof(Orphan), beneficiaryId);
            if (!await IsOrphanInCallerScopeAsync(orphan))
                throw new NotFoundException(nameof(Orphan), beneficiaryId);

            filter.OrphanId = beneficiaryId;
        }

        return await GetPagedAsync(filter);
    }

    /// <summary>Report counters for one orphan (UC-ORR-01).</summary>
    public async Task<PeriodicOrphanReportSummaryDto> GetOrphanReportSummaryAsync(Guid orphanId)
    {
        var orphan = await _orphanRepository.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == orphanId && !o.IsDeleted)
            ?? throw new NotFoundException(nameof(Orphan), orphanId);
        if (!await IsOrphanInCallerScopeAsync(orphan))
            throw new NotFoundException(nameof(Orphan), orphanId);

        var scoped = ApplyCallerScope(_reportRepository.AsQueryable())
            .Where(r => r.OrphanId == orphanId);

        return new PeriodicOrphanReportSummaryDto
        {
            OrphanId = orphanId,
            OrphanName = orphan.FullName,
            TotalReports = await scoped.CountAsync(),
            PendingReports = await scoped.CountAsync(r => !r.Reviewed),
            ApprovedReports = await scoped.CountAsync(r => r.Reviewed && r.IsAccepted),
            RejectedReports = await scoped.CountAsync(r => r.Reviewed && r.IsRefused),
            LockedReports = await scoped.CountAsync(r => r.Locked)
        };
    }

    #endregion

    #region Export Operations (UC-ORR-11, UC-ORR-12, UC-ORR-13 — delivered with their stories)

    /// <summary>Export periodic reports to Excel. Implemented with UC-ORR-11 (epic 9 story 9-11).</summary>
    public Task<byte[]> ExportToExcelAsync(PeriodicOrphanReportFilterDto filter, bool includeAllFields = false)
    {
        // TODO (9-11): ExcelJS client-side export reads the paged API; server workbook on request.
        return Task.FromResult(Array.Empty<byte>());
    }

    /// <summary>Export an orphan's report history to Excel. Implemented with UC-ORR-11.</summary>
    public Task<byte[]> ExportOrphanHistoryToExcelAsync(Guid orphanId)
    {
        // TODO (9-11): ExcelJS client-side export reads the by-orphan API.
        return Task.FromResult(Array.Empty<byte>());
    }

    #endregion

    #region Status Management

    public async Task LockReportAsync(Guid id)
    {
        var report = await GetScopedReportAsync(id);
        report.Locked = true;
        report.LockedDate = DateTime.UtcNow;
        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Periodic report {ReportId} locked by {UserId}", report.Id, _currentUser.UserId);
    }

    public async Task UnlockReportAsync(Guid id)
    {
        var report = await GetScopedReportAsync(id);
        report.Locked = false;
        report.LockedDate = null;
        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Periodic report {ReportId} unlocked by {UserId}", report.Id, _currentUser.UserId);
    }

    public async Task ActivateReportAsync(Guid id)
    {
        var report = await GetScopedReportAsync(id);
        report.Active = true;
        report.ActiveDate = DateTime.UtcNow;
        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeactivateReportAsync(Guid id)
    {
        var report = await GetScopedReportAsync(id);
        report.Active = false;
        await _reportRepository.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Helper Methods

    /// <summary>A report is editable while it is neither locked nor accepted (refused reports resubmit).</summary>
    public async Task<bool> CanEditReportAsync(Guid id)
    {
        var report = await _reportRepository.AsQueryable()
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (report == null || !await IsWithinCallerScopeAsync(report))
            return false;
        return !report.Locked && !report.IsAccepted;
    }

    /// <summary>
    /// True when the named user may review reports. Roles come from the caller's token —
    /// this answers for the current user and denies everyone else by default.
    /// </summary>
    public Task<bool> CanUserReviewReportsAsync(Guid userId)
    {
        if (_currentUser.UserId != userId)
            return Task.FromResult(false);

        var canReview = _currentUser.IsInRole("SuperAdmin")
                     || _currentUser.IsInRole("Admin")
                     || _currentUser.IsInRole("Accountant")
                     || _currentUser.IsInRole("Employee");
        return Task.FromResult(canReview);
    }

    /// <summary>
    /// Whether periodic reports are enabled for a charity (UC-6.12). Charity-level feature
    /// flags arrive with epic 19 (system configuration); until then the feature is on.
    /// </summary>
    public Task<bool> IsPeriodicReportsEnabledForCharityAsync(Guid charityId)
    {
        // TODO (epic 19): read the charity's PeriodicReportsEnabled configuration flag.
        return Task.FromResult(true);
    }

    #endregion

    #region Private helpers

    /// <summary>
    /// Core paged read shared by every list operation. Applies caller scope first,
    /// then request filters — so a charity pin can never be widened by request data.
    /// </summary>
    private async Task<PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>> GetPagedAsync(
        PeriodicOrphanReportFilterDto filter)
    {
        // §23.S.1 closed-vocabulary tokens (UC-RPT-02) — fail loudly, never silently match nothing
        await _filterValidator.ValidateAndThrowAsync(filter);

        var page = Math.Max(filter.PageNumber, 1);
        var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);

        var query = ApplyCallerScope(_reportRepository.AsQueryable());
        query = ApplyFilters(query, filter);
        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        var totalCount = await query.CountAsync();

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new PeriodicOrphanReportListDto
            {
                Id = r.Id,
                // Review 2026-08-24 (6-6): expose the discriminator so the housing grid can
                // tell a child report from the guardian report riding a carrier child.
                ChildOrParent = r.ChildOrParent == Domain.Enums.ReportBeneficiaryType.Parent
                    ? "Parent"
                    : "Child",
                ReportNo = r.ReportNo,
                ReportDate = r.ReportDate,
                ReportPeriodFrom = r.ReportPeriodFrom,
                ReportPeriodTo = r.ReportPeriodTo,
                OrphanId = r.OrphanId,
                OrphanCode = r.Orphan.Code,
                OrphanName = r.Orphan.FullName,
                CharityId = r.CharityId,
                CharityName = r.Charity != null ? r.Charity.Name : null,
                PrayerStatus = r.PrayerStatus,
                EducationalLevelId = r.EducationalLevelId,
                MedicalStatus = r.MedicalStatus,
                Reviewed = r.Reviewed,
                IsAccepted = r.IsAccepted,
                IsRefused = r.IsRefused,
                ReviewerId = r.ReviewerId,
                ReviewedDate = r.ReviewedDate,
                RefuseReasonId = r.RefuseReasonId,
                RefuseReason = r.RefuseReason,
                ReviewComments = r.ReviewComments,
                ReviewStatus = r.ReviewStatus ?? "Pending",
                Married = r.Married,
                Dead = r.Dead,

                // §14.S.4 orphan-status wide grid (UC-ORR-09) — one query; the nav
                // traversals translate to LEFT JOINs, no N+1
                OrphanDateOfBirth = r.Orphan.DateOfBirth,
                OrphanGender = r.Orphan.Gender,
                OrphanNationalId = r.Orphan.NationalId,
                OrphanPhone = r.Orphan.Phone,
                FamilyCode = r.Orphan.Family != null ? r.Orphan.Family.Code : null,
                // §11.S.2 multi-guardian: the wide grid shows the PRIMARY guardian —
                // first live row by CreatedOn/Id (the legacy single-seat pick).
                GuardianName = r.Orphan.Family != null
                    ? r.Orphan.Family.Providers
                        .Where(p => !p.IsDeleted)
                        .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                        .Select(p => p.FullName)
                        .FirstOrDefault()
                    : null,
                GuardianRelation = r.Orphan.Family != null
                    ? r.Orphan.Family.Providers
                        .Where(p => !p.IsDeleted)
                        .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                        .Select(p => p.RelationshipToFamily)
                        .FirstOrDefault()
                    : null,
                GuardianNationalId = r.Orphan.Family != null
                    ? r.Orphan.Family.Providers
                        .Where(p => !p.IsDeleted)
                        .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                        .Select(p => p.NationalId)
                        .FirstOrDefault()
                    : null,
                GuardianJob = r.Orphan.Family != null
                    ? r.Orphan.Family.Providers
                        .Where(p => !p.IsDeleted)
                        .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                        .Select(p => p.Job)
                        .FirstOrDefault()
                    : null,
                GuardianEducationLevelName = r.Orphan.Family != null
                    ? r.Orphan.Family.Providers
                        .Where(p => !p.IsDeleted)
                        .OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                        .Select(p => p.EducationLevel != null ? p.EducationLevel.NameAr : null)
                        .FirstOrDefault()
                    : null,
                RegionName = r.Orphan.Family != null && r.Orphan.Family.Region != null
                    ? r.Orphan.Family.Region.NameAr : null,
                CenterName = r.Orphan.Family != null && r.Orphan.Family.Center != null
                    ? r.Orphan.Family.Center.NameAr : null,
                CityVillage = r.Orphan.Family != null ? r.Orphan.Family.CityVillage : null,
                Address = r.Orphan.Family != null ? r.Orphan.Family.Address : null,
                HomePhone = r.Orphan.Family != null ? r.Orphan.Family.PhoneNumber : null,
                SchoolType = r.SchoolType,
                Faculty = r.Faculty,
                School = r.School,
                MarriageDate = r.MarriageDate,
                DeathDate = r.DeathDate,
                Disease = r.Disease,
                Disability = r.Disability,
                Grade = r.Grade,
                Specialization = r.Specialization,
                EducationDegree = r.EducationDegree,
                UpdatedOn = r.UpdatedOn,
                CreatedOn = r.CreatedOn
            })
            .ToListAsync();

        await EnrichListAsync(rows);

        return new PeriodicOrphanReportPagedResult<PeriodicOrphanReportListDto>
        {
            Items = rows,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>Charity claim pins to that charity; country claim pins through the charity; head office is unscoped.</summary>
    private IQueryable<PeriodicOrphanReport> ApplyCallerScope(IQueryable<PeriodicOrphanReport> query)
    {
        // Review 2026-08-24: no global soft-delete filter exists on this platform — every
        // scoped read drops soft-deleted rows here. Duplicate-slot checks and the report-
        // number sequence bypass this helper deliberately (they must see deleted rows).
        query = query.Where(r => !r.IsDeleted);

        if (_currentUser.CharityId.HasValue)
            return query.Where(r => r.CharityId == _currentUser.CharityId.Value);

        if (_currentUser.CountryId.HasValue)
            return query.Where(r => r.Charity != null && r.Charity.CountryId == _currentUser.CountryId.Value);

        return query;
    }

    private static IQueryable<PeriodicOrphanReport> ApplyFilters(
        IQueryable<PeriodicOrphanReport> query, PeriodicOrphanReportFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(r =>
                r.Orphan.Code.Contains(term) ||
                r.Orphan.FullName.Contains(term) ||
                (r.ReportNo != null && r.ReportNo.Contains(term)));
        }

        // §14.S.1 dedicated orphan filters (كود اليتيم / اسم اليتيم)
        if (!string.IsNullOrWhiteSpace(filter.OrphanCode))
            query = query.Where(r => r.Orphan.Code.Contains(filter.OrphanCode.Trim()));

        if (!string.IsNullOrWhiteSpace(filter.OrphanName))
            query = query.Where(r => r.Orphan.FullName.Contains(filter.OrphanName.Trim()));

        if (filter.OrphanId.HasValue)
            query = query.Where(r => r.OrphanId == filter.OrphanId.Value);

        // §11.S.3 housing-beneficiary filters (6-6): discriminator + family link
        if (filter.ChildOrParent.HasValue)
            query = query.Where(r => r.ChildOrParent == filter.ChildOrParent.Value);
        if (filter.HousingFamilyId.HasValue)
            query = query.Where(r => r.FK_HousingFamilyId == filter.HousingFamilyId.Value);

        // A charity-scoped caller is already pinned by ApplyCallerScope; for head office
        // this is a plain filter over every charity.
        if (filter.CharityId.HasValue)
            query = query.Where(r => r.CharityId == filter.CharityId.Value);

        // رقم التقرير — dedicated partial match (UC-ORR-12/13 extract criterion)
        if (!string.IsNullOrWhiteSpace(filter.ReportNo))
        {
            var reportNo = filter.ReportNo.Trim();
            query = query.Where(r => r.ReportNo != null && r.ReportNo.Contains(reportNo));
        }

        // Review P5 2026-08-24: an unknown status token must never read as "no filter" —
        // that silently returns the unfiltered register. Fail loudly instead (mapped 400).
        switch (filter.ReviewStatus?.Trim().ToLowerInvariant())
        {
            case null:
            case "":
                break;
            case "pending":
                query = query.Where(r => !r.Reviewed);
                break;
            case "approved":
                query = query.Where(r => r.Reviewed && r.IsAccepted);
                break;
            case "rejected":
            case "refused":
                query = query.Where(r => r.Reviewed && r.IsRefused);
                break;
            default:
                throw new BusinessException(
                    $"Unknown review status filter '{filter.ReviewStatus}'. Valid values: pending, approved, rejected.");
        }

        if (filter.Reviewed.HasValue)
            query = query.Where(r => r.Reviewed == filter.Reviewed.Value);
        if (filter.IsAccepted.HasValue)
            query = query.Where(r => r.Reviewed && r.IsAccepted == filter.IsAccepted.Value);
        if (filter.IsRefused.HasValue)
            query = query.Where(r => r.Reviewed && r.IsRefused == filter.IsRefused.Value);

        if (filter.ReviewerId.HasValue)
            query = query.Where(r => r.ReviewerId == filter.ReviewerId.Value);

        if (filter.ReportDateFrom.HasValue)
            query = query.Where(r => r.ReportDate >= filter.ReportDateFrom.Value.Date);
        if (filter.ReportDateTo.HasValue)
            // Inclusive end date: the whole of "to", not midnight sharp.
            query = query.Where(r => r.ReportDate < filter.ReportDateTo.Value.Date.AddDays(1));

        // §14.S.4 orphan-status family (UC-ORR-09) — every status predicate is
        // collected and then combined with && or || per AndOr (default and).
        // Combine, never replace: the accumulated scope/date/orphan predicates above
        // are untouched (the 15-1 defect-4 class).
        var statusPredicates = new List<Expression<Func<PeriodicOrphanReport, bool>>>();

        if (filter.EducationalStageId.HasValue)
            statusPredicates.Add(r => r.EducationalStageId == filter.EducationalStageId.Value);
        if (filter.EducationalLevelId.HasValue)
            statusPredicates.Add(r => r.EducationalLevelId == filter.EducationalLevelId.Value);

        if (!string.IsNullOrWhiteSpace(filter.MedicalStatus))
        {
            var medical = filter.MedicalStatus.Trim();
            statusPredicates.Add(r => r.MedicalStatus != null && r.MedicalStatus.Contains(medical));
        }

        if (!string.IsNullOrWhiteSpace(filter.SchoolType))
        {
            var schoolType = filter.SchoolType.Trim();
            statusPredicates.Add(r => r.SchoolType != null && r.SchoolType.Contains(schoolType));
        }

        if (!string.IsNullOrWhiteSpace(filter.EducationDegree))
        {
            var degree = filter.EducationDegree.Trim();
            statusPredicates.Add(r => r.EducationDegree != null && r.EducationDegree.Contains(degree));
        }

        switch (filter.MaritalStatus?.Trim().ToLowerInvariant())
        {
            case "married":
                statusPredicates.Add(r => r.Married == true);
                break;
            case "single":
                statusPredicates.Add(r => r.Married != true && r.Dead != true);
                break;
            case "deceased":
                statusPredicates.Add(r => r.Dead == true);
                break;
        }

        switch (filter.EducationalStatus?.Trim().ToLowerInvariant())
        {
            case "studying":
                statusPredicates.Add(r => r.IsOrphanStudent == true);
                break;
            case "graduated":
                statusPredicates.Add(r => !string.IsNullOrEmpty(r.HighestEducationalLevel));
                break;
            case "dropout":
                statusPredicates.Add(r => r.DropOut == true);
                break;
        }

        if (statusPredicates.Count > 0)
        {
            var useOr = string.Equals(filter.AndOr?.Trim(), "or", StringComparison.OrdinalIgnoreCase);
            query = query.Where(CombinePredicates(statusPredicates, useOr));
        }

        if (filter.Active.HasValue)
            query = query.Where(r => r.Active == filter.Active.Value);
        if (filter.Locked.HasValue)
            query = query.Where(r => r.Locked == filter.Locked.Value);

        return query;
    }

    /// <summary>
    /// Combines the §14.S.4 status predicates into one expression — AndAlso or
    /// OrElse per the And/Or radio. Each body is parameter-rebound to the first
    /// predicate's parameter so EF translates the result as a single WHERE.
    /// </summary>
    private static Expression<Func<T, bool>> CombinePredicates<T>(
        IReadOnlyList<Expression<Func<T, bool>>> predicates, bool useOr)
    {
        var combined = predicates[0].Body;
        var parameter = predicates[0].Parameters[0];

        for (var i = 1; i < predicates.Count; i++)
        {
            var rebound = new ParameterRebinder(predicates[i].Parameters[0], parameter)
                .Visit(predicates[i].Body);
            combined = useOr
                ? Expression.OrElse(combined, rebound)
                : Expression.AndAlso(combined, rebound);
        }

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    /// <summary>Rebinds a lambda body's parameter onto another lambda's parameter.</summary>
    private sealed class ParameterRebinder(ParameterExpression source, ParameterExpression target)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == source ? target : node;
    }

    /// <summary>Whitelisted sort keys only — keeps the EF translation a plain ORDER BY.</summary>
    private static IQueryable<PeriodicOrphanReport> ApplySorting(
        IQueryable<PeriodicOrphanReport> query, string? sortBy, string? sortDirection)
    {
        var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        return (sortBy?.Trim().ToLowerInvariant()) switch
        {
            "reportno" => descending ? query.OrderByDescending(r => r.ReportNo) : query.OrderBy(r => r.ReportNo),
            "reportdate" => descending ? query.OrderByDescending(r => r.ReportDate) : query.OrderBy(r => r.ReportDate),
            "orphanname" => descending ? query.OrderByDescending(r => r.Orphan.FullName) : query.OrderBy(r => r.Orphan.FullName),
            "revieweddate" => descending ? query.OrderByDescending(r => r.ReviewedDate) : query.OrderBy(r => r.ReviewedDate),
            _ => descending ? query.OrderByDescending(r => r.CreatedOn) : query.OrderBy(r => r.CreatedOn)
        };
    }

    /// <summary>
    /// Resolve the names a projection cannot join inline: reviewer full names (identity table)
    /// and education level names (lookup table reached only by id).
    /// </summary>
    private async Task EnrichListAsync(List<PeriodicOrphanReportListDto> rows)
    {
        if (rows.Count == 0)
            return;

        var reviewerIds = rows.Where(r => r.ReviewerId.HasValue)
            .Select(r => r.ReviewerId!.Value)
            .Distinct()
            .ToList();
        if (reviewerIds.Count > 0)
        {
            var names = await _userRepository.AsQueryable()
                .Where(u => reviewerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);
            foreach (var row in rows.Where(r => r.ReviewerId.HasValue))
                if (names.TryGetValue(row.ReviewerId!.Value, out var fullName))
                    row.ReviewerName = fullName;
        }

        var levelIds = rows.Where(r => r.EducationalLevelId.HasValue)
            .Select(r => r.EducationalLevelId!.Value)
            .Distinct()
            .ToList();
        if (levelIds.Count > 0)
        {
            var levels = await _educationLevelRepository.AsQueryable()
                .Where(l => levelIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.NameAr ?? l.NameEn ?? l.Name);
            foreach (var row in rows.Where(r => r.EducationalLevelId.HasValue))
                if (levels.TryGetValue(row.EducationalLevelId!.Value, out var levelName))
                    row.EducationalLevelName = levelName;
        }
    }

    /// <summary>Fetch a report for a write path, enforcing scope (out-of-scope reads as
    /// NotFound). Review 2026-08-24: soft-deleted rows answer NotFound too — update,
    /// review and delete must never operate on a deleted report.</summary>
    private async Task<PeriodicOrphanReport> GetScopedReportAsync(Guid id)
    {
        var report = await _reportRepository.AsQueryable()
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted)
            ?? throw new NotFoundException(nameof(PeriodicOrphanReport), id);

        if (!await IsWithinCallerScopeAsync(report))
            throw new NotFoundException(nameof(PeriodicOrphanReport), id);

        return report;
    }

    /// <summary>Scope check for a single report, loading the charity lazily when needed.</summary>
    private async Task<bool> IsWithinCallerScopeAsync(PeriodicOrphanReport report)
    {
        if (_currentUser.CharityId.HasValue)
            return report.CharityId == _currentUser.CharityId.Value;

        if (_currentUser.CountryId.HasValue)
        {
            if (report.Charity == null && report.CharityId.HasValue)
                report.Charity = await _charityRepository.GetByIdAsync(report.CharityId.Value);
            return report.Charity?.CountryId == _currentUser.CountryId.Value;
        }

        return true;
    }

    /// <summary>
    /// UC-HOU-06: scope check for a family — same pins as <see cref="IsOrphanInCallerScopeAsync"/>
    /// (charity claim compares FK_CharityId; country claim resolves the charity; HQ unscoped).
    /// </summary>
    private async Task<bool> IsFamilyInCallerScopeAsync(Family family)
    {
        if (_currentUser.CharityId.HasValue)
            return family.FK_CharityId == _currentUser.CharityId.Value;

        if (_currentUser.CountryId.HasValue)
        {
            if (family.FK_CharityId is null)
                return false;
            var charity = await _charityRepository.GetByIdAsync(family.FK_CharityId.Value);
            return charity?.CountryId == _currentUser.CountryId.Value;
        }

        return true;
    }

    /// <summary>Scope check for an orphan: charity pin compares FK_CharityId directly; country pin resolves the charity.
    /// Review 2026-08-24: a child with no FK_CharityId of its own falls back to its family's
    /// charity (the guardian path always scopes through the family) — without the fallback
    /// such a child is invisible to its own charity.</summary>
    private async Task<bool> IsOrphanInCallerScopeAsync(Orphan orphan)
    {
        var charityId = orphan.FK_CharityId;
        if (charityId is null && orphan.FamilyId.HasValue)
        {
            var family = await _familyRepository.GetByIdAsync(orphan.FamilyId.Value);
            charityId = family?.FK_CharityId;
        }

        if (_currentUser.CharityId.HasValue)
            return charityId == _currentUser.CharityId.Value;

        if (_currentUser.CountryId.HasValue)
        {
            if (charityId is null)
                return false;
            var charity = await _charityRepository.GetByIdAsync(charityId.Value);
            return charity?.CountryId == _currentUser.CountryId.Value;
        }

        return true;
    }

    /// <summary>Parse the §11.U.6 discriminator (blank or Child ⇒ Child). The validator gates the value.</summary>
    private static Domain.Enums.ReportBeneficiaryType ParseBeneficiaryType(string? childOrParent)
    {
        return string.Equals(childOrParent?.Trim(), "Parent", StringComparison.OrdinalIgnoreCase)
            ? Domain.Enums.ReportBeneficiaryType.Parent
            : Domain.Enums.ReportBeneficiaryType.Child;
    }

    /// <summary>
    /// A housing family read for a write path: must exist, be live, be FamilyType.Housing and
    /// sit inside the caller's scope — anything else reads as NotFound (404-not-leak).
    /// </summary>
    private async Task<Family> GetScopedHousingFamilyAsync(Guid familyId)
    {
        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null || family.IsDeleted
            || family.FamilyType != Domain.Enums.FamilyType.Housing
            || !await IsFamilyInCallerScopeAsync(family))
        {
            throw new NotFoundException(nameof(Family), familyId);
        }
        return family;
    }

    /// <summary>
    /// UC-HOU-08 guardian branch: resolve the 6-7 picker's guardian (Provider) to the housing
    /// family, guard the family-level one-guardian-report-per-month rule, then pick the carrier
    /// child. Returns (carrierOrphanId, charityId, familyId) — OrphanId is NOT NULL on the
    /// report, so a guardian report rides a family child with no report that month; the choice
    /// respects the (OrphanId, ReportMonth, ReportYear) unique index, which is filtered to
    /// child rows ([ChildOrParent] = 1) — a soft-deleted CHILD report still holds the carrier
    /// slot, a soft-deleted guardian report does not.
    /// </summary>
    private async Task<(Guid OrphanId, Guid? CharityId, Guid FamilyId)> ResolveGuardianReportTargetAsync(
        CreatePeriodicOrphanReportDto dto, int reportYear, int reportMonth)
    {
        var provider = await _providerRepository.AsQueryable()
            .FirstOrDefaultAsync(p => p.Id == dto.HousingBeneficiaryId!.Value && !p.IsDeleted)
            ?? throw new NotFoundException(nameof(Provider), dto.HousingBeneficiaryId.Value);

        var family = provider.FamilyId.HasValue
            ? await _familyRepository.GetByIdAsync(provider.FamilyId.Value)
            : null;

        // Unknown / non-housing / out-of-scope guardians all read as NotFound — and a route
        // family id that disagrees with the guardian's actual family likewise proves nothing.
        if (family == null || family.IsDeleted
            || family.FamilyType != Domain.Enums.FamilyType.Housing
            || !await IsFamilyInCallerScopeAsync(family)
            || (dto.HousingFamilyId.HasValue && dto.HousingFamilyId.Value != family.Id))
        {
            throw new NotFoundException(nameof(Provider), dto.HousingBeneficiaryId.Value);
        }

        // One guardian report per family per month (live rows — a deleted one may be re-entered;
        // matches the filtered unique index backing this rule).
        var guardianDuplicate = await _reportRepository.AsQueryable()
            .AnyAsync(r => r.FK_HousingFamilyId == family.Id
                        && !r.IsDeleted
                        && r.ChildOrParent == Domain.Enums.ReportBeneficiaryType.Parent
                        && r.ReportYear == reportYear
                        && r.ReportMonth == reportMonth);
        if (guardianDuplicate)
            throw new BusinessException(
                $"A periodic report already exists for this family's guardian in {reportYear}/{reportMonth:D2}.");

        // Carrier child, ordered by name for a deterministic pick; the occupied check matches
        // the unique index discipline (see the ChildOrParent=1 filter note in
        // PeriodicOrphanReportConfiguration): a soft-deleted CHILD row still holds the
        // (OrphanId, month) slot, a soft-deleted guardian row does not.
        var familyChildIds = await _orphanRepository.AsQueryable()
            .Where(o => o.FamilyId == family.Id && !o.IsDeleted)
            .OrderBy(o => o.FullName)
            .Select(o => o.Id)
            .ToListAsync();

        // Review 2026-08-24: distinct refusal — a family with no children at all cannot
        // carry a guardian report (OrphanId is NOT NULL); "every child has a report"
        // would be false and confusing.
        if (familyChildIds.Count == 0)
            throw new BusinessException(
                $"This housing family has no children on record, so the guardian report for {reportYear}/{reportMonth:D2} cannot be carried.");

        var occupiedOrphanIds = await _reportRepository.AsQueryable()
            .Where(r => r.ReportYear == reportYear
                     && r.ReportMonth == reportMonth
                     && familyChildIds.Contains(r.OrphanId)
                     // Review P7 2026-08-24: both slot indexes are filtered to
                     // [IsDeleted] = 0, so only LIVE rows occupy a carrier — a deleted
                     // child report frees its month slot (delete-then-re-enter, 9-6) and
                     // a deleted guardian row never blocked its carrier.
                     && !r.IsDeleted)
            .Select(r => r.OrphanId)
            .ToListAsync();

        var carrierId = familyChildIds.FirstOrDefault(id => !occupiedOrphanIds.Contains(id));
        if (carrierId == Guid.Empty)
            throw new BusinessException(
                $"Every child of this housing family already has a report in {reportYear}/{reportMonth:D2}; " +
                "the guardian report cannot be recorded for that month.");

        return (carrierId, family.FK_CharityId, family.Id);
    }

    /// <summary>
    /// §11.S.4 create-time review flags: only when the form ticked a decision. Mirrors
    /// ReviewReportAsync so the 6-6 grid columns (تم الاعتماد / تاريخ الاعتماد) read true.
    /// </summary>
    private async Task ApplyCreateReviewFlagsAsync(PeriodicOrphanReport report, CreatePeriodicOrphanReportDto dto)
    {
        // Review 2026-08-24 (decision D1): a Charity-role caller cannot self-approve —
        // the review workflow is HQ-only (9-7/9-8). isAccepted on a Charity token is
        // ignored (the report stays Pending); HQ-role flags are honored as bound.
        // Review P10 2026-08-24: symmetric guard — self-refusing fakes an HQ review
        // stamp (ReviewerId = the charity user), so a Charity token's isRefused is
        // ignored too; only HQ roles can set either decision flag at create.
        var isHqCaller = !_currentUser.IsInRole("Charity");
        var accepts = dto.IsAccepted == true && isHqCaller;
        var refuses = dto.IsRefused == true && isHqCaller;

        if (!accepts && !refuses)
            return; // plain create — the Pending defaults set by the initializer stand

        report.Reviewed = true;
        report.ReviewedDate = DateTime.UtcNow;
        report.ReviewerId = _currentUser.UserId;

        if (accepts)
        {
            report.IsAccepted = true;
            report.IsRefused = false;
            report.ReviewStatus = "Approved";
            return;
        }

        report.IsAccepted = false;
        report.IsRefused = true;
        report.ReviewStatus = "Rejected";
        report.RefuseReasonId = dto.RefuseReasonId;
        if (dto.RefuseReasonId.HasValue)
        {
            var lookupReason = await _refuseReasonRepository.GetByIdAsync(dto.RefuseReasonId.Value)
                ?? throw new BusinessException("The selected refuse reason does not exist.");
            if (!lookupReason.IsActive)
                throw new BusinessException("The selected refuse reason is not active.");
            report.RefuseReason = lookupReason.NameAr ?? lookupReason.NameEn ?? lookupReason.Name;
        }
    }

    /// <summary>
    /// Guard the (OrphanId, ReportMonth, ReportYear) unique index. Review P7 2026-08-24:
    /// the index is filtered to live child rows ([ChildOrParent] = 1 AND [IsDeleted] = 0),
    /// so a soft-deleted report no longer squats its month slot — delete-then-re-enter is
    /// the 9-6 correction flow. The pre-check matches the index plus guardian occupation:
    /// any LIVE row on the orphan+month blocks (a live guardian report occupies its
    /// carrier child too). A duplicate must surface as a friendly 400, never a raw 500.
    /// </summary>
    private async Task EnsureNoDuplicateAsync(Guid orphanId, int reportYear, int reportMonth, Guid? excludeReportId)
    {
        var duplicate = await _reportRepository.AsQueryable()
            .AnyAsync(r => r.OrphanId == orphanId
                        && r.ReportYear == reportYear
                        && r.ReportMonth == reportMonth
                        && !r.IsDeleted
                        && (excludeReportId == null || r.Id != excludeReportId.Value));

        if (duplicate)
            throw new BusinessException(
                $"A periodic report already exists for this orphan in {reportYear}/{reportMonth:D2}.");
    }

    /// <summary>
    /// Review P11 2026-08-24: guard the guardian (FK_HousingFamilyId, ReportYear,
    /// ReportMonth) unique index on the update path — one live Parent report per family
    /// per month, excluding the report being re-dated.
    /// </summary>
    private async Task EnsureNoFamilyMonthDuplicateAsync(Guid housingFamilyId, int reportYear, int reportMonth,
        Guid excludeReportId)
    {
        var duplicate = await _reportRepository.AsQueryable()
            .AnyAsync(r => r.FK_HousingFamilyId == housingFamilyId
                        && r.ReportYear == reportYear
                        && r.ReportMonth == reportMonth
                        && r.ChildOrParent == Domain.Enums.ReportBeneficiaryType.Parent
                        && !r.IsDeleted
                        && r.Id != excludeReportId);

        if (duplicate)
            throw new BusinessException(
                $"This housing family already has a guardian report for {reportYear}/{reportMonth:D2}.");
    }

    /// <summary>
    /// Review P9 2026-08-24: map a unique-index refusal from the PeriodicOrphanReport
    /// indexes onto the friendly duplicate message. Returns false for any other update
    /// failure so it keeps bubbling as a 500.
    /// </summary>
    private static bool IsDuplicateReportSlotViolation(DbUpdateException ex, out string message)
    {
        message = string.Empty;
        for (var e = (Exception?)ex; e != null; e = e.InnerException)
        {
            var msg = e.Message;
            if (msg.Contains("IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear"))
            {
                message = "A periodic report already exists for this orphan in that month.";
                return true;
            }
            if (msg.Contains("IX_PeriodicOrphanReport_FK_HousingFamilyId_ReportYear_ReportMonth"))
            {
                message = "This housing family already has a guardian report for that month.";
                return true;
            }
            if (msg.Contains("IX_PeriodicOrphanReport_ReportNo"))
            {
                message = "The report number was just issued to another report — please retry.";
                return true;
            }
        }
        return false;
    }

    /// <summary>POR-&lt;year&gt;-&lt;sequence&gt; report numbers, e.g. POR-2026-0007.</summary>
    private async Task<string> GenerateReportNumberAsync(int reportYear)
    {
        // Review follow-up (2026-08-24): MAX-based, not COUNT-based. A count breaks the
        // moment rows leave the year's set (hard deletes in dev; also any future purge) —
        // it then proposes a number that already exists and the unique ReportNo index
        // refuses the insert with a 500. Max-suffix + 1 always clears every row that
        // still exists. Concurrent creates can still race to the same number; the filtered
        // unique index is the hard guard that turns that into a refusal instead of a
        // silent duplicate.
        var maxSuffix = await _reportRepository.AsQueryable()
            .IgnoreQueryFilters()
            .Where(r => r.ReportYear == reportYear && r.ReportNo != null && r.ReportNo.StartsWith($"POR-{reportYear}-"))
            .Select(r => r.ReportNo!)
            .ToListAsync();

        var max = 0;
        foreach (var no in maxSuffix)
        {
            if (int.TryParse(no.AsSpan($"POR-{reportYear}-".Length), out var seq) && seq > max)
                max = seq;
        }
        return $"POR-{reportYear}-{max + 1:D4}";
    }

    private static int? CalculateAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
            return null;

        var today = DateTime.UtcNow.Date;
        var born = dateOfBirth.Value.Date;
        var age = today.Year - born.Year;
        if (born > today.AddYears(-age))
            age--;
        return age < 0 ? null : age;
    }

    /// <summary>Map an entity (with Orphan/Charity loaded) onto the detail DTO, resolving display names.</summary>
    private async Task<PeriodicOrphanReportDto> MapToDetailDtoAsync(PeriodicOrphanReport report)
    {
        var dto = new PeriodicOrphanReportDto
        {
            Id = report.Id,
            OrphanId = report.OrphanId,
            OrphanCode = report.Orphan?.Code,
            OrphanName = report.Orphan?.FullName,
            OrphanPaymentId = report.OrphanPaymentId,
            ReportDate = report.ReportDate,
            ReportPeriodFrom = report.ReportPeriodFrom,
            ReportPeriodTo = report.ReportPeriodTo,
            ReportNo = report.ReportNo,
            CharityId = report.CharityId,
            CharityName = report.Charity?.Name,
            ChildOrParent = report.ChildOrParent.ToString(),
            HousingFamilyId = report.FK_HousingFamilyId,
            PrayerStatus = report.PrayerStatus,
            MannersStatus = report.MannersStatus,
            HadeethStatus = report.HadeethStatus,
            QuranParts = report.QuranParts,
            QuranVerses = report.QuranVerses,
            MedicalStatus = report.MedicalStatus,
            Disease = report.Disease,
            Disability = report.Disability,
            DisabilityDescription = report.DisabilityDescription,
            DiseaseDescription = report.DiseaseDescription,
            MedicalReportImageId = report.MedicalReportImageId,
            Hobby = report.Hobby,
            Course = report.Course,
            CourseName = report.CourseName,
            SportName = report.SportName,
            ProfessionName = report.ProfessionName,
            Achievement = report.Achievement,
            AchievementArr = report.AchievementArr,
            Wish = report.Wish,
            WishArr = report.WishArr,
            OrphanMessage = report.OrphanMessage,
            EducationalStageId = report.EducationalStageId,
            EducationalStageName = report.EducationStage,
            EducationalLevelId = report.EducationalLevelId,
            Grade = report.Grade,
            School = report.School,
            SchoolType = report.SchoolType,
            EducationDegree = report.EducationDegree,
            HighestEducationalLevel = report.HighestEducationalLevel,
            HighestEducationalLevelYear = report.HighestEducationalLevelYear,
            IsOrphanStudent = report.IsOrphanStudent,
            EducationalYear = report.EducationalYear,
            AnnualFeeForStudy = report.AnnualFeeForStudy,
            StudyingYears = report.StudyingYears,
            RestStudyingYears = report.RestStudyingYears,
            GraduationYear = report.GraduationYear,
            DropOut = report.DropOut,
            DropOutYear = report.DropOutYear,
            DropOutStageId = report.DropOutStageId,
            Faculty = report.Faculty,
            Department = report.Department,
            Specialization = report.Specialization,
            Married = report.Married,
            MarriageDate = report.MarriageDate,
            OrphanMarriageImageId = report.OrphanMarriageImageId,
            Dead = report.Dead,
            DeathDate = report.DeathDate,
            OrphanDeadImageId = report.OrphanDeadImageId,
            OrphanCertificateImageId = report.OrphanCertificateImageId,
            OrphanImageId = report.OrphanImageId,
            MissingDocuments = report.MissingDocuments,
            MissingDocumentsName = report.MissingDocumentsName,
            Reviewed = report.Reviewed,
            ReviewedDate = report.ReviewedDate,
            ReviewerId = report.ReviewerId,
            Locked = report.Locked,
            LockedDate = report.LockedDate,
            Active = report.Active,
            ActiveDate = report.ActiveDate,
            Deleted = report.Deleted,
            DeletedDate = report.DeletedDate,
            IsAccepted = report.IsAccepted,
            IsRefused = report.IsRefused,
            RefuseReason = report.RefuseReason,
            RefuseReasonId = report.RefuseReasonId,
            MessageId = report.MessageId,
            ReviewComments = report.ReviewComments,
            CreatedOn = report.CreatedOn,
            CreatedBy = report.CreatedBy,
            UpdatedOn = report.UpdatedOn ?? report.CreatedOn,
            UpdatedBy = report.UpdatedBy
        };

        // EducationalLevelName: prefer the lookup, fall back to the legacy free-text column.
        if (report.EducationalLevelId.HasValue)
        {
            var level = await _educationLevelRepository.GetByIdAsync(report.EducationalLevelId.Value);
            dto.EducationalLevelName = level?.NameAr ?? level?.NameEn ?? level?.Name ?? report.EducationLevel;
        }
        else
        {
            dto.EducationalLevelName = report.EducationLevel;
        }

        // RefuseReasonName: the canonical lookup label behind the stored reason.
        if (report.RefuseReasonId.HasValue)
        {
            var reason = await _refuseReasonRepository.GetByIdAsync(report.RefuseReasonId.Value);
            dto.RefuseReasonName = reason?.NameAr ?? reason?.NameEn ?? reason?.Name;
        }

        // ReviewerName: identity table, resolved by id.
        if (report.ReviewerId.HasValue)
        {
            var reviewer = await _userRepository.GetByIdAsync(report.ReviewerId.Value);
            dto.ReviewerName = reviewer?.FullName;
        }

        // UC-ORR-16: the ids are the single truth; the URL fields carry the relative
        // attachment route for API consumers (the SPA still fetches by id as a blob —
        // an <img> src cannot carry the Bearer header; see the 9-16 record).
        dto.MedicalReportImageUrl = report.MedicalReportImageId.HasValue
            ? $"/api/Attachments/{report.MedicalReportImageId.Value}/image" : null;
        dto.OrphanMarriageImageUrl = report.OrphanMarriageImageId.HasValue
            ? $"/api/Attachments/{report.OrphanMarriageImageId.Value}/image" : null;
        dto.OrphanDeadImageUrl = report.OrphanDeadImageId.HasValue
            ? $"/api/Attachments/{report.OrphanDeadImageId.Value}/image" : null;
        dto.OrphanCertificateImageUrl = report.OrphanCertificateImageId.HasValue
            ? $"/api/Attachments/{report.OrphanCertificateImageId.Value}/image" : null;
        dto.OrphanImageUrl = report.OrphanImageId.HasValue
            ? $"/api/Attachments/{report.OrphanImageId.Value}/image" : null;

        return dto;
    }

    #endregion
}
