using IIROSA.Application.DTOs.Reports;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// UC-RPT-01 — read-only report projections (EP-18 epic skeleton). One method per report key;
/// every caller scope is resolved server-side from <see cref="ICurrentUserService"/>
/// (pin-never-widen), never from the payload. Nothing here writes.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// §23.S.3 بيانات الأيتام — the full orphan master listing with guardian, residence,
    /// status, education and org columns, ordered by orphan code. Age is computed server-side
    /// from DateOfBirth; lookup labels resolve NameAr ?? NameEn.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">
    /// Age below 1, AgeTo &lt; AgeFrom, or PageSize above 200.
    /// </exception>
    Task<ReportPagedResult<OrphanDataListDto>> GetOrphanDataAsync(
        OrphanDataFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §14.U.14 الأيتام بدون تقرير مجدد (UC-ORR-14) — the chase list before a payment run:
    /// the (scoped) charity's coded orphans with NO accepted report whose period intersects
    /// the window. BR-11: only an accepted report clears an orphan — pending/refused do not.
    /// <paramref name="request"/>.CountOnly returns just the count (the legacy _Number screens).
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">
    /// Unset or inverted window, or an out-of-range page.
    /// </exception>
    Task<NonRenewedReportsResultDto> GetNonRenewedReportAsync(
        NonRenewedReportsRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.12 متابعة الجمعيات (UC-RPT-20) — the cross-charity tracking grid over one
    /// payment batch: one row per charity present in the batch, with its member count,
    /// entered-report count, and the batch upload state. HQ-only endpoint; the pins
    /// (charity claim, country claim) still hold defensively. <see cref="CharityPaymentTrackingFilterDto.DateOfStartingUpdate"/>
    /// rides the filter for 18-21's print and is not consumed by this query.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">
    /// Out-of-range page bounds, an over-long batch number, or an implausible date.
    /// </exception>
    Task<ReportPagedResult<CharityPaymentTrackingRowDto>> GetCharityPaymentTrackingAsync(
        CharityPaymentTrackingFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.12's family-update sheet (UC-RPT-21 متابعة تحديث بيانات الأسر) — the families of
    /// the anchor payment's charities whose files were refreshed in
    /// [<see cref="FamilyUpdateTrackingFilterDto.Date"/>, the payment's period end]. The anchor
    /// resolves <see cref="FamilyUpdateTrackingFilterDto.PaymentId"/> → the batch's latest
    /// payment (<see cref="FamilyUpdateTrackingFilterDto.BatchNo"/>) → the latest payment.
    /// Read-only; the PDF is produced client-side (browser print — the recorded epic-wide
    /// path), so there is no /export/pdf endpoint. HQ roles only; the pins (charity claim,
    /// country claim) still hold defensively.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">
    /// Missing or implausible update-start date, an over-long batch number, or out-of-range
    /// page bounds.
    /// </exception>
    Task<ReportPagedResult<FamilyUpdateTrackingRowDto>> GetFamilyUpdateTrackingAsync(
        FamilyUpdateTrackingFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.11 أيتام مستحقون دفعات سابقة (UC-RPT-22) — the arrears grid: one row per scoped
    /// orphan with at least one entitled-but-unreceived batch (item present, <c>IsGotIt</c>
    /// false, not stopped — a stopped item is a deliberate stop, not an arrears), carrying the
    /// per-batch receipt map that drives the grid's dynamic batch columns. سبب طلب الاستعداد
    /// has no persisted source — the column renders null (real-data-only ruling). Caller scope
    /// from <see cref="ICurrentUserService"/> only (pin-never-widen).
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<MissedPaymentReportRowDto>> GetMissedPaymentsAsync(
        MissedPaymentsReportFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.18 صور الأيتام (UC-RPT-24) — the paged photograph manifest: photos attached to
    /// accepted periodic reports whose report date falls in [من تاريخ, الى تاريخ]. من تاريخ
    /// is mandatory; الجمعية is an HQ narrow (charity callers pinned from the token). Each row
    /// carries the attachment metadata + the live download URL — the SPA builds the ExcelJS
    /// workbook client-side (recorded export ruling; no server EPPlus).
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">
    /// Missing من تاريخ, الى تاريخ &lt; من تاريخ, or out-of-range page bounds.
    /// </exception>
    Task<ReportPagedResult<OrphanFileManifestRowDto>> ExportOrphanFilesAsync(
        OrphanFilesExportFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.18 صور الشهادات (UC-RPT-25) — the certificates manifest: 18-24's read with the
    /// kind selector flipped to the report's certificate image column. Same filter shape,
    /// same validator, same envelope — one screen, two grids.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">
    /// Missing من تاريخ, الى تاريخ &lt; من تاريخ, or out-of-range page bounds.
    /// </exception>
    Task<ReportPagedResult<OrphanFileManifestRowDto>> ExportCertificateFilesAsync(
        OrphanFilesExportFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.U.28 أيتام لم يصرف لهم (UC-RPT-28) — the zero-disbursement gaps of one payment
    /// batch: item rows with no disbursement instrument and nothing received (no cheque, no
    /// transfer, !IsGotIt). HQ-only endpoint role gate; the charity pin is defence-in-depth.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">
    /// Missing الدفعة (PaymentId) or out-of-range page bounds.
    /// </exception>
    Task<ReportPagedResult<OrphansWithoutPaymentListDto>> GetOrphansWithoutPaymentAsync(
        OrphansWithoutPaymentFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.U.29 المستلمون / غير المستلمين / الموقوفون (UC-RPT-29) — the three outcome
    /// lists of one cheque batch, printed from the orphan-payment-detail commands. ONE
    /// endpoint serves all three variants through <see cref="PaymentsOutcomeFilterDto.Variant"/>
    /// (received / notReceived / stopped) over one dataset definition — the numbers cannot
    /// diverge between the lists. Charity callers are pinned to their own charity; HQ may
    /// narrow by <see cref="PaymentsOutcomeFilterDto.CharityId"/>. AC-4: an empty variant
    /// returns rows:[] + message, never an empty document.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">
    /// Missing batch number or a variant outside the three discriminator values.
    /// </exception>
    Task<PaymentsOutcomeReportDto> GetPaymentsOutcomeAsync(
        PaymentsOutcomeFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.U.30 أرقام الشيكات (UC-RPT-30) — the cheque-numbers sheet of one batch: the
    /// payment rows with a cheque number recorded (10-12's settlement column; the checks
    /// register is NOT joined — no Check↔batch link exists). Charity callers pinned to
    /// their own charity; HQ may narrow. AC-4: no cheque recorded ⇒ rows:[] + message.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Missing batch number.</exception>
    Task<ChequeNumbersReportDto> GetChequeNumbersAsync(
        ChequeNumbersFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.U.31 كروت الاستلام (UC-RPT-31) — the batch's collection cards: one card per
    /// (guardian, orphan) payment item, the paper each guardian signs at collection. Read-only:
    /// the IsPrinted/PrintedOn stamps belong to the payments vertical's own print flow, this
    /// projection only reads them. Charity callers pinned to their own charity; HQ may narrow.
    /// AC-3: no rows in scope ⇒ rows:[] + message, never an empty document.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Missing batch number.</exception>
    Task<ReceiptCardsReportDto> GetReceiptCardsAsync(
        ReceiptCardsFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.U.35 الأيتام والأرامل الجدد (UC-RPT-35) — the sponsorship-offer lists: orphans /
    /// widows registered in the window, the four legacy .rpt variants collapsed to one
    /// variant-keyed read. Read-only. Charity callers pinned to their own charity; HQ may
    /// narrow; a country claim intersects. The registration date is CreatedOn (the recorded
    /// proxy — no dedicated registration column exists).
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Missing dateFrom / bad variant / page bounds.</exception>
    Task<NewBeneficiariesReportDto> GetNewBeneficiariesAsync(
        NewBeneficiariesFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.U.36 كشوف المتابعة والتسليم (UC-RPT-36) — the legacy Crystal trio collapsed to one
    /// variant-keyed read (FollowUp = one row per orphan with its latest accepted report,
    /// FollowUpFamily = one row per family, Tasleem = the handover sheet with the guardian).
    /// Read-only. Charity callers pinned to their own charity; HQ may narrow; a country claim
    /// intersects. The optional window scopes the row ANCHOR's registration (orphan variants:
    /// Orphan.CreatedOn; family variant: Family.CreatedOn — the 18-35 ruling; there is no
    /// dedicated registration column). Manual !IsDeleted checks everywhere — the global
    /// soft-delete query filter in Framework.Core is commented out (verified), so this file's
    /// explicit-check convention IS the deletion scoping.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Bad variant / window / page bounds.</exception>
    Task<ReportPagedResult<FollowUpSheetRowDto>> GetFollowUpSheetsAsync(
        FollowUpSheetFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.U.37 كشوف تعريف العائل والأرامل (UC-RPT-37) — the identification sheets: every
    /// guardian, the widows, or one family's guardians + widows. Read-only print payload — the
    /// WHOLE selection (a print document is never page 1). The producing user is stamped from
    /// <see cref="ICurrentUserService.UserId"/>, never from the payload (the legacy userId
    /// parameter is ruled out). Charity callers pinned to their own charity; HQ may narrow; a
    /// country claim intersects. The optional start date filters the ROW ANCHOR's CreatedOn
    /// (Provider/Mother registration) in the list variants; the single-family variant takes the
    /// whole family (the explicit family choice is the frame — recorded).
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">
    /// Variant outside the enum, or SingleFamily without FamilyId (§23.U.37's missing-parameter
    /// exception).
    /// </exception>
    Task<GuardianIdentificationSheetDto> GetGuardianIdentificationSheetsAsync(
        GuardianIdentificationSheetFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.U.38 مرفقات الصادر الناقصة (UC-RPT-38) — the outgoing letters inside the window
    /// carrying ZERO orphan-report attachment rows (v1 rule — the legacy expected-list table
    /// <c>ChildOutGoing</c> is absent from the working tree; upgrade path re-points to
    /// expected EXCEPT attached, contract unchanged). Read-only; HQ-only endpoint roles; the
    /// charity pin still holds defensively.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Inverted window or page bounds.</exception>
    Task<ReportPagedResult<MissingOutgoingAttachmentsRowDto>> GetMissingOutgoingAttachmentsAsync(
        MissingOutgoingAttachmentsFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.U.39 أيتام الأسر بتاريخ (UC-RPT-39) — one charity's families as at a date, each with
    /// its orphans grouped under it. As-at anchor: the family's dedicated <c>RegistrationDate</c>
    /// column (Family has one — no CreatedOn proxy needed, unlike 18-35). A page = families;
    /// orphan ages are computed server-side at the chosen date. Soft-delete scoping is manual
    /// (the global query filter is commented out — standing 18-36 finding).
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Missing as-at date; out-of-range page bounds.</exception>
    Task<ReportPagedResult<FamilyWithOrphansDto>> GetFamilyOrphansByDateAsync(
        FamilyOrphansByDateFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.4 الايتام المستبعدين (UC-RPT-03) — the excluded set, one narrow (charity, HQ only)
    /// and the 6-column grid contract. The exclusion write path belongs to EP-08; this read
    /// reports whatever that vertical recorded. No exclusion columns exist today (epic-wide
    /// no-migration ruling) — the set is empty and the gap is logged, never patched.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<ExcludedOrphanListDto>> GetExcludedOrphansAsync(
        ExcludedOrphansFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.5 أيتام انتهت كفالتهم (UC-RPT-04) — orphans whose sponsorship term has expired
    /// and need re-ponsoring/closure. HQ-only endpoint roles. The domain carries NO ended state
    /// today (verified: SponsorshipStatus is a free string — Sponsored/Unsponsored/Pending —
    /// with no end date and no sponsor history; dev DB holds no such rows either). Per the
    /// recorded product ruling the report ships with an EMPTY set + logged gap — the predicate
    /// lands in one place when the sponsorship verticals add the state. No column invented.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<OrphanStatusReportListDto>> GetFinishedSponsorshipOrphansAsync(
        OrphanStatusReportFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.6 أيتام غير مكفولين (UC-RPT-05) — coded orphans whose sponsorship status is
    /// Unsponsored. Per the EP-08 ruling the first code assignment flips null/Pending →
    /// Unsponsored; an orphan with an empty <c>Code</c> is UNCODED, not unsponsored, and never
    /// appears here. Shares the §23.S.5 filter/grid shape; only the predicate differs.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<OrphanStatusReportListDto>> GetUnsponsoredOrphansAsync(
        OrphanStatusReportFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.9 ارامل مطلوب لهم كفاله (UC-RPT-06) — the widow grid (22 columns) over the
    /// family's MOTHER joined to the family's residence/income columns. The platform has no
    /// standalone Widow entity and no dedicated widow-sponsorship flag, so the predicate is the
    /// data's floor (recorded): mother alive + not deleted, family registered, husband death
    /// date present on the father, and at least one non-deleted orphan in the family. Columns
    /// with no backing field (Mobile2, DevelopmentProject) render null — recorded gap.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<WidowSponsorshipListDto>> GetWidowsAllowingSponsorshipAsync(
        WidowSponsorshipFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.8 تقرير الكروت المسجله (UC-RPT-07) — families carrying registered guardian Meza
    /// cards, with orphan names/codes aggregated per row. The domain carries NO Meza/card column
    /// anywhere (grep-proven) — no family can be a card carrier today, so per the recorded
    /// product ruling (same as UC-RPT-03/04) the report ships with an EMPTY set + logged gap;
    /// the family+guardian+children projection and the card predicate land in this one method
    /// when the guardian registration gains the card fields. No column invented.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<MezaCardsListDto>> GetMezaCardsAsync(
        MezaCardsFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.U.8 استخراج كروت العائل (UC-RPT-08) — the bank-file extract over the SAME row
    /// source as UC-RPT-07 with the extract keys applied (ReportNo, BatchId, date range,
    /// MezaCardExist). Returns the FULL selection (the bank file is never page 1) in the paged
    /// envelope with Page=1. The card-column gap recorded on 18-7 carries over unchanged: no
    /// card storage exists yet, so the extract is empty with the gap logged — the projection and
    /// extract predicates land in this one method when the registration vertical adds storage.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">
    /// ReportNo ≤ 0, inverted date range, or out-of-range page bounds.
    /// </exception>
    Task<ReportPagedResult<MezaCardsListDto>> GetMezaCardsForExportAsync(
        MezaCardsFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.13 بيانات أسر المساعدات (UC-RPT-09) — the distinct families benefiting from
    /// seasonal-aid assistance (SeasonalAidBeneficiary joined to Family/Campaign), one row per
    /// family with the campaign names aggregated. The legacy screen had no grid — the platform
    /// adaptation renders a grid so the استخراج command has rows to extract. The §23.U.9
    /// <c>userId</c> payload key is ruled out: caller identity comes from the JWT.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<BeneficiaryFamilyListDto>> GetBeneficiaryFamiliesAsync(
        BeneficiaryFamilyFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.7 مشاريع الأسر (UC-RPT-11) — registered family projects: one row per project with
    /// the family's orphans/guardian joined. US-RPT-11 is typed "Create a record" — a template
    /// artefact; this is a read, like every story in this epic.
    ///
    /// Row-source gap (grep-proven, standing ruling): NO project entity carries a family link —
    /// no <c>HousingProject</c> class exists in the Domain and <c>OfficeProject</c> (donors,
    /// beneficiaries-count) has no FamilyId. §23.S.7's family+project row is therefore
    /// unrepresentable today: the report ships with an EMPTY set + logged gap; the projection
    /// lands in this one method when a family-project vertical adds the entity/link. The two
    /// experience columns have no backing field anywhere — they render null either way.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<FamilyProjectReportRowDto>> GetRegisteredFamilyProjectsAsync(
        FamilyProjectsFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.19 تقارير تعديل المعيل (UC-RPT-12) — one row per provider assignment on a family
    /// with the family/charity/orphan context joined. US-RPT-12 is typed "Update a record" —
    /// a template artefact; this is a read.
    ///
    /// Data-source decision (story finding): the platform keeps NO guardian-change audit trail —
    /// <c>Provider</c> holds the current assignment only, so the row projects the current
    /// guardian and تاريخ التعديل from the audit stamps (UpdatedOn when later than CreatedOn).
    /// The three prior-guardian columns (اسم المعيل السابق, صله القرابه السابقة, سبب التغيير)
    /// ship null — no storage is invented; a ProviderChangeHistory entity is a scope change.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<ProviderChangeReportRowDto>> GetProviderSponsorChangesAsync(
        ProviderChangeFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.14 أيتام مكودون مطلوب لهم تقرير (UC-RPT-15) — the summary grid: one row per
    /// charity with its count of coded orphans having no accepted report covering the trailing
    /// 12 months (BR-11 period semantics — aligned with the UC-ORR-14 chase list so the two
    /// screens agree on who needs a report). Read-only; charity scope pins server-side.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<OrphansMissingReportsSummaryDto>> GetOrphansMissingReportsAsync(
        OrphansMissingReportsFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.14 drill-down (ExtractDetails) — one charity's chase list. A charity caller
    /// naming another charity is clamped to its own rows (pin-never-widen).
    /// Review P17 2026-08-26: takes the request DTO and runs the FluentValidation validator
    /// (was scalar params + a controller-side Math.Max clamp).
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds or an empty charityId.</exception>
    Task<ReportPagedResult<OrphansMissingReportsDetailDto>> GetOrphansMissingReportsDetailAsync(
        OrphansMissingReportsDetailRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.15 أيتام مطلوب لهم ملفات (UC-RPT-16) — a charity's missing-files worklist: coded
    /// orphans whose LATEST periodic report is flagged MissingDocuments. Read-only; charity
    /// scope pins server-side. The epic's «import a file» wording is a template artefact —
    /// §23.S.15 is a query screen (recorded ruling).
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<OrphansMissingFilesListDto>> GetOrphansMissingFilesAsync(
        OrphansMissingFilesFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.16 تقارير في انتظار الموافقة (UC-RPT-17) — the HQ review queue: submitted
    /// periodic reports that are neither accepted nor refused, oldest submission first.
    /// Read-only; charity scope pins server-side, never from the payload.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<ReportsAwaitingApprovalListDto>> GetReportsAwaitingApprovalAsync(
        ReportsAwaitingApprovalFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// §23.S.17 تقارير تم رفضها (UC-RPT-18) — the refused worklist: refused periodic reports
    /// with the reason resolved (catalogue label preferred, free-text fallback), newest refusal
    /// first. Read-only; the decision write path stays in the periodic-reports module
    /// (`POST /api/PeriodicOrphanReports/{id}/review`) — this report only jumps to it.
    /// </summary>
    /// <exception cref="FluentValidation.ValidationException">Out-of-range page bounds.</exception>
    Task<ReportPagedResult<RefusedReportsListDto>> GetRefusedReportsAsync(
        RefusedReportsFilterDto filter,
        CancellationToken cancellationToken = default);
}
