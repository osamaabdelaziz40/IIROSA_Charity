namespace IIROSA.Application.DTOs.Reports;

/// <summary>
/// UC-RPT-01 (§23.S.3 بيانات الأيتام) — filter for the orphan-data report.
/// Clean property names only (never <c>FK_*</c> — Newtonsoft emits <c>fK_…</c>, the 13-3/15-6
/// defect class). The charity scope is resolved server-side from the caller; <see cref="CharityId"/>
/// is honoured only for an HQ caller (pin-never-widen).
/// </summary>
public class OrphanDataFilterDto
{
    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — capped at 200 by the validator.</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>الدفعة المالية المنصرقة للايتام — filters to orphans paid in this batch.</summary>
    public string? BatchNumber { get; set; }

    /// <summary>الجمعية — explicit charity narrow; HQ only (ignored for charity callers).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>المحافظة — Family.RegionId.</summary>
    public int? GovernorateId { get; set; }

    /// <summary>المركز — Family.CenterId.</summary>
    public int? CenterId { get; set; }

    /// <summary>من — minimum age in years (≥ 1).</summary>
    public int? AgeFrom { get; set; }

    /// <summary>الى — maximum age in years (≥ 1, ≥ AgeFrom).</summary>
    public int? AgeTo { get; set; }

    /// <summary>المستبعدين — excluded set only. No exclusion columns exist yet (epic ruling: no
    /// migration), so this set is empty and the report returns nothing.</summary>
    public bool Excluded { get; set; }

    // Review P4 2026-08-26: IsFinishedSponsorship / AllOrphans / NotExcluded removed — all
    // three were accepted on the wire and never read (18-4's finished set is its OWN endpoint;
    // "all orphans" is the default; "not excluded" excluded nobody). A dead flag is a client
    // that cannot distinguish "no data" from "nonsense input"; unknown JSON properties are
    // ignored by binding, so older clients that still send them keep working.
}

/// <summary>
/// One grid row of the orphan-data report — the §23.S.3 column contract (identity, guardian,
/// residence &amp; income, status, education, father, exclusion, audit/org). Columns with no
/// backing field today (e.g. <see cref="ExclusionReason"/>, <see cref="MobileNumber2"/>) stay
/// null and render blank — data is never invented (epic-wide no-migration ruling).
/// </summary>
public class OrphanDataListDto
{
    // ── identity ── رقم اليتيم · أسم اليتيم · الرقم القومى · تاريخ الميلاد · العمر · النوع
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    /// <summary>Computed server-side from DateOfBirth.</summary>
    public int? Age { get; set; }
    public string? Gender { get; set; }

    // ── guardian ── أسم المعيل · صلة القرابة · الرقم القومى للمعيل · مؤهل المعيل · مهنة المعيل
    //                الحالة الصحية للمعيل · الحالة الإجتماعية للمعيل · مشروع تنموى للمعيل
    /// <summary>Provider first, then Father, then Mother — first non-empty wins.</summary>
    public string? GuardianName { get; set; }
    /// <summary>Provider.RelationshipToFamily, falling back to Family.ProviderType.</summary>
    public string? GuardianRelationship { get; set; }
    public string? GuardianNationalId { get; set; }
    public string? GuardianEducationLevel { get; set; }
    public string? GuardianJob { get; set; }
    public string? GuardianHealthStatus { get; set; }
    /// <summary>Only Provider carries a social status today.</summary>
    public string? GuardianSocialStatus { get; set; }
    /// <summary>مشروع تنموى للمعيل — no backing column; renders blank.</summary>
    public string? GuardianDevelopmentProject { get; set; }

    // ── residence & income ── المحافظة · المركز · القرية/الحى · العنوان التفصيلى · ت الموبايل ·
    //                           ت الموبايل2 · ملكية السكن · قيمة الايجار · نوع السكن · حالة مستويات السكن · قيمة الدخل
    public string? GovernorateName { get; set; }
    public string? CenterName { get; set; }
    public string? CityVillage { get; set; }
    public string? DetailedAddress { get; set; }
    public string? MobileNumber { get; set; }
    /// <summary>ت الموبايل2 — no backing column; renders blank.</summary>
    public string? MobileNumber2 { get; set; }
    public string? HouseOwnershipName { get; set; }
    public decimal? RentAmount { get; set; }
    public string? HousingTypeName { get; set; }
    public string? HouseStatusName { get; set; }
    public decimal? MonthlyIncome { get; set; }

    // ── status ── الحالة الاجتماعية · الحالة الصحية · نوعية العمل
    public string? SocialStatusName { get; set; }
    public string? HealthStatusName { get; set; }
    public string? Profession { get; set; }

    // ── education ── المرحلة الدراسية · الصف الدراسى · اسم الموسسة التعليمية · الكلية · القسم · حاصل على موهل دراسى
    public string? EducationLevelName { get; set; }
    public string? GradeClass { get; set; }
    public string? SchoolName { get; set; }
    public string? FacultyName { get; set; }
    public string? DepartmentName { get; set; }
    /// <summary>حاصل على موهل دراسى — no backing column; renders blank (null, not false).</summary>
    public bool? HasAcademicDegree { get; set; }

    // ── father ── تاريخ وفاة الاب · سبب وفاة الاب
    public DateTime? FatherDeathDate { get; set; }
    /// <summary>سبب وفاة الاب — no backing column; renders blank.</summary>
    public string? FatherDeathCause { get; set; }

    // ── exclusion ── الاستبعاد · سبب الاستبعاد (no backing columns — recorded gap)
    public bool IsExcluded { get; set; }
    public string? ExclusionReason { get; set; }

    // ── audit / org ── الملاحظات · أسم الجمعية · تاريخ اخر تحديث
    public string? Notes { get; set; }
    /// <summary>The orphan's charity id — resolved to <see cref="CharityName"/> server-side.</summary>
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}

/// <summary>
/// Paged envelope shared by every report key (OfficeProject shape,
/// <c>OfficeProjectPagedResult&lt;T&gt;</c> at OfficeProjects.cs:187).
/// </summary>
public class ReportPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    // Review P6 2026-08-26: guard the division — PageSize 0 once crashed serialisation in a
    // smoke (the NonRenewedReportsResultDto sibling at :798 already guarded; this shared
    // envelope did not).
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((decimal)TotalCount / PageSize) : 0;
}

// ---- §23.S.4 excluded orphans (UC-RPT-03) ---------------------------------------

/// <summary>
/// UC-RPT-03 (§23.S.4 الايتام المستبعدين) — filter for the excluded-orphans report.
/// One filter: الجمعية (HQ narrow only; charity callers are pinned server-side).
/// </summary>
public class ExcludedOrphansFilterDto
{
    /// <summary>الجمعية — explicit charity narrow; HQ only (ignored for charity callers).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — capped at 200 by the validator.</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// One §23.S.4 grid row — رقم اليتيم · أسم اليتيم · الاستبعاد · سبب الاستبعاد ·
/// أسم الجمعية · تاريخ اخر تحديث. The exclusion flag/reason have no backing columns today
/// (epic-wide no-migration ruling) — the keys stay in the contract, values render blank.
/// </summary>
public class ExcludedOrphanListDto
{
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    /// <summary>الاستبعاد — no backing column; false today (recorded gap).</summary>
    public bool IsExcluded { get; set; }
    /// <summary>سبب الاستبعاد — no backing column; renders blank (recorded gap).</summary>
    public string? ExclusionReason { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}

// ---- §23.S.5 / §23.S.6 orphan status reports (UC-RPT-04, UC-RPT-05) --------------

/// <summary>
/// Shared filter for the §23.S.5 / §23.S.6 pair — the two screens carry the SAME contract
/// (one charity narrow, HQ only); only the status predicate differs.
/// </summary>
public class OrphanStatusReportFilterDto
{
    /// <summary>الجمعية — explicit charity narrow; HQ only (ignored for charity callers).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — capped at 200 by the validator.</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// One §23.S.5 / §23.S.6 grid row — رقم اليتيم · أسم اليتيم · الجمعيه · كود العائله · العمر.
/// Age is computed server-side from DateOfBirth.
/// </summary>
public class OrphanStatusReportListDto
{
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public string? FamilyCode { get; set; }
    public int? Age { get; set; }
}

// ---- §23.S.9 widows requiring sponsorship (UC-RPT-06) ----------------------------

/// <summary>UC-RPT-06 — filter for POST /api/Reports/widows-allowing-sponsorship.</summary>
public class WidowSponsorshipFilterDto
{
    /// <summary>الجمعية — HQ-only narrow; a charity caller is pinned server-side.</summary>
    public Guid? CharityId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

/// <summary>
/// UC-RPT-06 — one §23.S.9 grid row: the widow (family mother) with her family's
/// residence/income columns and her own education/profession/health columns. Columns with no
/// backing field (Mobile2, DevelopmentProject) stay null and render blank — recorded gap,
/// never a migration.
/// </summary>
public class WidowSponsorshipListDto
{
    public string WidowName { get; set; } = string.Empty;
    public string? GovernorateName { get; set; }
    public string? CenterName { get; set; }
    public string? CityVillage { get; set; }
    public string? DetailedAddress { get; set; }
    public string? MobileNumber { get; set; }
    /// <summary>No backing column today (same gap as 18-1's MobileNumber2).</summary>
    public string? MobileNumber2 { get; set; }
    public string? HouseOwnershipName { get; set; }
    public decimal? RentAmount { get; set; }
    public string? HousingTypeName { get; set; }
    public string? HouseStatusName { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public DateTime? HusbandDeathDate { get; set; }
    /// <summary>No backing column today — مشروع تنموي gap.</summary>
    public string? DevelopmentProject { get; set; }
    public string? EducationLevelName { get; set; }
    public string? Profession { get; set; }
    public string? HealthStatusName { get; set; }
    public string? NationalId { get; set; }
    public string? Notes { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}

// ---- §23.S.8 registered Meza cards (UC-RPT-07) -----------------------------------

/// <summary>
/// UC-RPT-07/08 — filter for POST /api/Reports/meza-cards. One DTO serves both variants:
/// the paged read (18-7) and the extract (18-8, presence of <see cref="ReportNo"/> selects it).
/// </summary>
public class MezaCardsFilterDto
{
    /// <summary>الجمعية — HQ-only narrow; the endpoint roles are HQ-only anyway.</summary>
    public Guid? CharityId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    // ---- §23.U.8 extract keys (UC-RPT-08) — ride the same request DTO ----

    /// <summary>Extract report number — mandatory for the extract; its presence selects the extract path.</summary>
    public int? ReportNo { get; set; }

    /// <summary>Include orphan codes in the bank file.</summary>
    public bool IsCodes { get; set; } = false;

    /// <summary>الدفعة — narrow to one payment batch.</summary>
    public string? BatchId { get; set; }

    /// <summary>Date range start (nullable).</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>Date range end (nullable); must be ≥ DateFrom when both set.</summary>
    public DateTime? DateTo { get; set; }

    /// <summary>Only rows that already carry a card.</summary>
    public bool MezaCardExist { get; set; } = false;
}

/// <summary>
/// UC-RPT-07 — one §23.S.8 grid row: a family with its guardian (provider) and the aggregated
/// orphan names/codes. The Meza-card number/expiry have NO backing column today (grep-proven —
/// no Meza/Card member exists domain-wide); the keys ship in the contract and render blank until
/// the guardian registration gains them (recorded gap, never a migration).
/// </summary>
public class MezaCardsListDto
{
    public string FamilyCode { get; set; } = string.Empty;
    public string GuardianName { get; set; } = string.Empty;
    public string? GuardianNationalId { get; set; }
    public string? Phone { get; set; }
    /// <summary>Aggregated orphan names, joined per the §23.S.8 single-row contract.</summary>
    public string? OrphanNames { get; set; }
    /// <summary>Aggregated orphan codes, joined per the §23.S.8 single-row contract.</summary>
    public string? OrphanCodes { get; set; }
    /// <summary>No backing column — recorded gap.</summary>
    public string? MezaCardNumber { get; set; }
    /// <summary>No backing column — recorded gap.</summary>
    public DateTime? MezaCardExpiry { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
}

// ---- §23.S.13 assistance family data (UC-RPT-09) ---------------------------------

/// <summary>
/// UC-RPT-09 — filter for POST /api/Reports/beneficiary-family-details. One optional narrow
/// (الجمعية, HQ only); the charity scope resolves server-side from the caller. The legacy
/// §23.U.9 payload's <c>userId</c> is ruled out — caller identity comes from the JWT.
/// </summary>
public class BeneficiaryFamilyFilterDto
{
    /// <summary>الجمعية — explicit charity narrow; HQ only (ignored for charity callers).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — capped at 200 by the validator.</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// UC-RPT-09 — one §23.S.13 grid row: a DISTINCT family benefiting from seasonal-aid
/// assistance. الحملة aggregates every campaign the family was registered in (a family in two
/// campaigns is still one row — the story fixes "one row per distinct benefiting family").
/// </summary>
public class BeneficiaryFamilyListDto
{
    public string FamilyCode { get; set; } = string.Empty;
    /// <summary>رب الأسرة — Provider first, then Father, then Mother (first non-empty wins).</summary>
    public string? HeadOfFamilyName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    /// <summary>الحملة — the family's campaign names, joined.</summary>
    public string? CampaignNames { get; set; }
}

// ---- §23.S.7 family projects (UC-RPT-11) -----------------------------------------

/// <summary>
/// UC-RPT-11 — filter for POST /api/Reports/registered-family-projects. One optional narrow
/// (الجمعية, HQ only); nothing else is filterable per §23.S.7.
/// </summary>
public class FamilyProjectsFilterDto
{
    /// <summary>الجمعية — explicit charity narrow; HQ only (ignored for charity callers).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — capped at 200 by the validator.</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// UC-RPT-11 — one §23.S.7 grid row: a family's registered project with the orphan
/// name/code/national-id strings joined per row (one row per project, not per orphan).
/// The two legacy productive-project columns have no backing field anywhere in the Domain
/// (grep-proven) — they ship in the contract and render null (recorded gap, never a migration).
/// </summary>
public class FamilyProjectReportRowDto
{
    public string? OrphanNames { get; set; }
    public string? OrphanNationalIds { get; set; }
    public string? GuardianName { get; set; }
    public string? OrphanCodes { get; set; }
    public string FamilyCode { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public string? ProjectStatus { get; set; }
    /// <summary>عدد سنوات الخبره — no backing column; renders blank (recorded gap).</summary>
    public int? YearsOfExperience { get; set; }
    public decimal? TotalBudget { get; set; }
    public string? BudgetCurrency { get; set; }
    public DateTime? ProjectStartDate { get; set; }
    public string? ProjectAddress { get; set; }
    /// <summary>هل يوجد خبره — no backing column; renders blank (recorded gap).</summary>
    public bool? HasExperience { get; set; }
    public string? ProjectDescription { get; set; }
}

// ---- §23.S.19 guardian change history (UC-RPT-12) --------------------------------

/// <summary>
/// UC-RPT-12 (تقارير تعديل المعيل) — one row per provider assignment on a family. The platform
/// keeps no guardian-change audit trail (no prior-value snapshot, no change reason — recorded
/// gap), so the report projects the CURRENT guardian with its audit stamps; the three
/// prior-guardian columns ship null and render empty.
/// </summary>
public class ProviderChangeFilterDto
{
    /// <summary>الجمعية — explicit charity narrow; HQ only (ignored for charity callers).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size (validator bounds 1–200).</summary>
    public int PageSize { get; set; } = 20;
}

public class ProviderChangeReportRowDto
{
    public Guid ProviderId { get; set; }

    /// <summary>كود الاسره — Family.Code.</summary>
    public string FamilyCode { get; set; } = string.Empty;

    /// <summary>FamilyId carried for the orphan-code join (not rendered).</summary>
    public Guid? FamilyId { get; set; }

    /// <summary>اسم المعيل السابق — no stored source (gap); renders empty.</summary>
    public string? PreviousGuardianName { get; set; }

    /// <summary>صله القرابه السابقة — no stored source (gap); renders empty.</summary>
    public string? PreviousRelationship { get; set; }

    /// <summary>سبب التغيير — no stored source (gap); renders empty.</summary>
    public string? ChangeReason { get; set; }

    /// <summary>اسم المعيل الجديد — Provider.FullName.</summary>
    public string? NewGuardianName { get; set; }

    /// <summary>صله القرابه الجديدة — Provider.RelationshipToFamily.</summary>
    public string? NewRelationship { get; set; }

    /// <summary>تاريخ التعديل — UpdatedOn when later than CreatedOn, else CreatedOn.</summary>
    public DateTime? ChangeDate { get; set; }

    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }

    /// <summary>كود اليتيم — the family's orphan codes, joined per row.</summary>
    public string? OrphanCodes { get; set; }
}

// ---- §23.S.10 family & orphan entry tracking (UC-RPT-13) -------------------------

/// <summary>
/// UC-RPT-13 (متابعة إدخلات الأسر والأيتام) — the one-endpoint query behind the §23.S.10
/// screen's two data commands: إجماليات (<see cref="Mode"/> = totals) and تفاصيل
/// (<see cref="Mode"/> = details). The charity key arrives from the route
/// (<c>GET /api/Families/{id}/follow-up</c>); Guid.Empty means كل الجهات (HQ only).
/// </summary>
public class FamilyEntryTrackingFilterDto
{
    /// <summary>الجمعية — from the route id; Guid.Empty = all charities (HQ only; a charity
    /// caller is pinned server-side whatever the route says).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>تاريخ بدء التقرير — entries on/after this date; omit → all entries.</summary>
    public DateTime? Date { get; set; }

    /// <summary>totals | details (validator whitelist; default details).</summary>
    public string Mode { get; set; } = "details";

    /// <summary>1-based page number (details mode only).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size (details mode only; validator bounds 1–200).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>إجماليات إدخالات الأسر والأيتام — families and orphans entered since the date.</summary>
public class FamilyFollowUpTotalsDto
{
    public int NewFamilies { get; set; }
    public int NewOrphans { get; set; }
    public DateTime? SinceDate { get; set; }
}

/// <summary>
/// تفاصيل إدخالات الأسر الجديدة — one row per family entered on/after the date
/// (كود الأسرة · رب الأسرة · الجمعية · تاريخ التسجيل · عدد الأيتام).
/// </summary>
public class FamilyFollowUpDetailDto
{
    public Guid FamilyId { get; set; }
    public string FamilyCode { get; set; } = string.Empty;
    public string HeadOfFamily { get; set; } = string.Empty;

    /// <summary>The live tenancy column — carried for name resolution (not rendered).</summary>
    public Guid? CharityId { get; set; }

    public string? CharityName { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public int OrphansCount { get; set; }
}

// ---- §23.S.14 coded orphans needing a report (UC-RPT-15) ------------------------

/// <summary>
/// UC-RPT-15 (أيتام مكودون مطلوب لهم تقرير) — the summary request. The screen's single
/// filter is the optional charity narrow; the chase window is fixed to the trailing 12
/// months (annual reporting cycle — §23.S.14 offers only الجمعية, so no window field).
/// </summary>
public class OrphansMissingReportsFilterDto
{
    /// <summary>الجمعية — explicit charity narrow; HQ only (ignored for charity callers).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number (pages the per-charity summary rows).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size (validator bounds 1–200).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>One summary grid row — الجمعيه · عدد الايتام مطلوب لهم تقارير.</summary>
public class OrphansMissingReportsSummaryDto
{
    /// <summary>Orphan.FK_CharityId — the group key; a null key renders بدون جمعية.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>Charity.Name resolved per page (live-column dictionary — never the EF nav mirror).</summary>
    public string? CharityName { get; set; }

    /// <summary>عدد الايتام مطلوب لهم تقارير — the charity's coded orphans in the chase set.</summary>
    public int MissingCount { get; set; }
}

/// <summary>
/// UC-RPT-15 drill-down request (ExtractDetails) — one charity's chase list. A charity
/// caller naming another charity is clamped to its own rows (pin-never-widen, AC 3/4).
/// </summary>
public class OrphansMissingReportsDetailRequestDto
{
    /// <summary>الجمعيه — the summary row clicked.</summary>
    public Guid CharityId { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — 1..100, validator-enforced (Review P17 2026-08-26).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>One drill-down row — كود اليتيم · اسم اليتيم · تاريخ آخر تقرير.</summary>
public class OrphansMissingReportsDetailDto
{
    public Guid OrphanId { get; set; }

    /// <summary>رقم اليتيم — coded orphans only (uncoded never join a payment run).</summary>
    public string OrphanCode { get; set; } = string.Empty;

    /// <summary>اسم اليتيم — Orphan.FullName.</summary>
    public string OrphanName { get; set; } = string.Empty;

    /// <summary>Latest report of ANY state — an accepted one would have cleared the orphan.</summary>
    public DateTime? LastReportDate { get; set; }
}

// ---- §23.S.15 orphans missing files (UC-RPT-16) ---------------------------------

/// <summary>
/// UC-RPT-16 (أيتام مطلوب لهم ملفات) — the worklist filter. §23.S.15 offers only الجمعية;
/// the missing-files rule itself is fixed (see the service).
/// </summary>
public class OrphansMissingFilesFilterDto
{
    /// <summary>الجمعية — explicit charity narrow; HQ only (ignored for charity callers).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size (validator bounds 1–200).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// One §23.S.15 worklist row. An orphan is IN the list when its LATEST periodic report is
/// flagged <c>MissingDocuments == true</c>; <see cref="MissingDocumentsName"/> carries what is
/// missing (export detail column — not part of the on-screen 5-column grid).
/// </summary>
public class OrphansMissingFilesListDto
{
    public Guid OrphanId { get; set; }

    /// <summary>FamilyId carried for row identity (not rendered).</summary>
    public Guid? FamilyId { get; set; }

    /// <summary>رقم اليتيم — Orphan.Code.</summary>
    public string OrphanCode { get; set; } = string.Empty;

    /// <summary>اسم اليتيم — Orphan.FullName.</summary>
    public string OrphanName { get; set; } = string.Empty;

    /// <summary>العنوان — Family.Address; null-safe (orphans without a live family render empty).</summary>
    public string? Address { get; set; }

    /// <summary>القريه — Family.CityVillage; null-safe.</summary>
    public string? Village { get; set; }

    /// <summary>Orphan.FK_CharityId — the live tenancy column.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>Charity.Name resolved per page (dictionary — never the EF nav mirror).</summary>
    public string? CharityName { get; set; }

    /// <summary>ما ينقص — the latest flagged report's MissingDocumentsName.</summary>
    public string? MissingDocumentsName { get; set; }
}

// ---- §23.S.16 review queue — awaiting approval (UC-RPT-17) ---------------------

/// <summary>
/// UC-RPT-17 (تقارير في انتظار الموافقة) — the HQ review-queue filter: the charity
/// narrow is the single optional input, everything else is paging.
/// </summary>
public class ReportsAwaitingApprovalFilterDto
{
    /// <summary>الجمعية — explicit charity narrow; HQ only (pin-never-widen for claim callers).</summary>
    public Guid? CharityId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

/// <summary>
/// §23.S.16 grid row — a submitted periodic report that is neither accepted nor refused.
/// سبب الرفض is carried (always null on this screen) because the legacy grid is shared
/// with §23.S.17; 18-18 forks this shape and makes the column meaningful.
/// </summary>
public class ReportsAwaitingApprovalListDto
{
    /// <summary>Row identity — the EditOrpReport jump target.</summary>
    public Guid ReportId { get; set; }

    public Guid OrphanId { get; set; }

    /// <summary>الجمعيه — Charity.Name.</summary>
    public string? CharityName { get; set; }

    /// <summary>اسم اليتيم — Orphan.FullName.</summary>
    public string OrphanName { get; set; } = string.Empty;

    /// <summary>تاريخ التقرير — Report.ReportDate.</summary>
    public DateTime ReportDate { get; set; }

    /// <summary>كود اليتيم — Orphan.Code.</summary>
    public string OrphanCode { get; set; } = string.Empty;

    /// <summary>سبب الرفض — always null on this screen (grid shared with §23.S.17 / 18-18).</summary>
    public string? RefuseReason { get; set; }
}

// ---- §23.S.17 refused worklist (UC-RPT-18) --------------------------------------

/// <summary>
/// UC-RPT-18 (تقارير تم رفضها) — the refused-worklist filter: the charity narrow is the
/// single optional input, everything else is paging.
/// </summary>
public class RefusedReportsFilterDto
{
    /// <summary>الجمعية — explicit charity narrow; HQ only (pin-never-widen for claim callers).</summary>
    public Guid? CharityId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

/// <summary>
/// §23.S.17 grid row — a refused periodic report with its reason resolved (catalogue label
/// preferred, free-text fallback). The same grid shape as §23.S.16 (18-17); سبب الرفض is
/// the meaningful column here. The decision write path is NOT this report — it stays
/// `POST /api/PeriodicOrphanReports/{id}/review` (story ruling).
/// </summary>
public class RefusedReportsListDto
{
    /// <summary>Row identity — the EditOrpReport jump target.</summary>
    public Guid ReportId { get; set; }

    public Guid OrphanId { get; set; }

    /// <summary>Report.CharityId — the scoping column (copied from the orphan at create).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>الجمعيه — Charity.Name.</summary>
    public string? CharityName { get; set; }

    /// <summary>اسم اليتيم — Orphan.FullName.</summary>
    public string OrphanName { get; set; } = string.Empty;

    /// <summary>تاريخ التقرير.</summary>
    public DateTime ReportDate { get; set; }

    /// <summary>كود اليتيم — Orphan.Code.</summary>
    public string OrphanCode { get; set; } = string.Empty;

    /// <summary>
    /// سبب الرفض — resolved: the RefuseReasonId catalogue label when present, else the
    /// reviewer's free text; empty only when the refusal carries neither (legacy rows).
    /// </summary>
    public string? RefuseReason { get; set; }

    /// <summary>Carried for the label resolution (18-12 attach pattern) — not rendered.</summary>
    public int? RefuseReasonId { get; set; }

    /// <summary>تاريخ المراجعة — when the refusal was recorded.</summary>
    public DateTime? ReviewedDate { get; set; }

    /// <summary>The deciding reviewer.</summary>
    public Guid? ReviewerId { get; set; }
}

// ---- §14.U.14 non-renewed chase list (UC-ORR-14) --------------------------------

/// <summary>
/// UC-ORR-14 (الأيتام بدون تقرير مجدد) — the chase-list request. The legacy realisation
/// shipped four endpoint shapes (non-renewed-reports, …V2, …_Number, GetBeginingScreen);
/// this is the collapsed one: <see cref="CountOnly"/> replaces the _Number variant, the
/// V2/begin-screen layouts are presentation over the same query.
/// <see cref="BatchId"/> (18-19) selects batch mode — the batch's orphans without a
/// report renewed for the current cycle; a window sent without a batch keeps the
/// §14.U.14 date-window chase list.
/// </summary>
public class NonRenewedReportsRequestDto
{
    /// <summary>الجمعية — explicit charity narrow; HQ only (ignored for charity callers).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// 18-19 (§23.U.19) — رقم الدفعة: matched against <c>OrphanPayment.BatchNo</c>. Absent
    /// with no window means the current batch — the latest payment by <c>GroupDate</c>.
    /// </summary>
    public string? BatchId { get; set; }

    /// <summary>من — window start (inclusive); window mode only (ignored when batched).</summary>
    public DateTime DateFrom { get; set; }

    /// <summary>الي — window end (inclusive: the whole of "to"); window mode only.</summary>
    public DateTime DateTo { get; set; }

    /// <summary>
    /// The legacy _Number screens — the response carries just <c>Count</c> with no rows.
    /// </summary>
    public bool CountOnly { get; set; }

    /// <summary>1-based page number (ignored when <see cref="CountOnly"/>).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — clamped to 100 in the service (ignored when CountOnly).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>One chase-list row — the orphan and, if any, its latest report date.</summary>
public class NonRenewedOrphanRowDto
{
    public Guid OrphanId { get; set; }
    /// <summary>رقم اليتيم — coded orphans only (uncoded never join a payment run).</summary>
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? FamilyCode { get; set; }
    /// <summary>Latest report date of ANY state (an accepted one would have cleared the orphan).</summary>
    public DateTime? LastReportDate { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    /// <summary>الدفعة — the resolved batch number (batch mode only; null in window mode).</summary>
    public string? BatchNo { get; set; }
}

/// <summary>
/// The chase list. <see cref="Count"/> is always the full matched count; <see cref="Items"/>
/// is empty when the request was <c>CountOnly</c>, otherwise one clamped page.
/// <see cref="TotalCount"/>/<see cref="TotalPages"/> are the ReportPagedResult wire aliases
/// (18-19) — the §14.U.14 count screen keeps reading <c>count</c>.
/// </summary>
public class NonRenewedReportsResultDto
{
    public int Count { get; set; }
    public List<NonRenewedOrphanRowDto> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    /// <summary>0 when countOnly — no page was served.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((decimal)TotalCount / PageSize) : 0;
}

// ---- §23.S.12 charity payment tracking (UC-RPT-20) -------------------------------

/// <summary>
/// UC-RPT-20 (§23.S.12 متابعة الجمعيات) — the tracking filter. HQ-only screen; the
/// explicit charity is an HQ narrow, never a scope grant. <see cref="DateOfStartingUpdate"/>
/// rides the filter so 18-21's print reuses this model (families updated from this date).
/// </summary>
public class CharityPaymentTrackingFilterDto
{
    /// <summary>الجمعية — explicit charity narrow (كل الجهات when null).</summary>
    public Guid? CharityId { get; set; }

    /// <summary>الدفعة — matched on <c>OrphanPayment.BatchNo</c>; absent = the current batch.</summary>
    public string? BatchId { get; set; }

    /// <summary>من فضلك ادخل تاريخ بدا التحديث — consumed by 18-21's print, not this query.</summary>
    public DateTime? DateOfStartingUpdate { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — clamped to 100 in the service.</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// One tracking row — a charity's presence in the batch. Only states the platform
/// persists today (omitted-not-faked ruling): per-charity printed/confirmed flags do
/// not exist as data and are not columns.
/// </summary>
public class CharityPaymentTrackingRowDto
{
    public Guid CharityId { get; set; }
    public string? CharityName { get; set; }
    /// <summary>عدد الايتام بالدفعة — distinct batch members of this charity.</summary>
    public int OrphansInBatch { get; set; }
    /// <summary>التقارير المدخلة — entered (any-state, non-deleted) reports of those members.</summary>
    public int ReportsEntered { get; set; }
    /// <summary>تم رفع الدفعة — any of the charity's batch payments uploaded.</summary>
    public bool BatchUploaded { get; set; }
    /// <summary>تاريخ الرفع — latest upload date among the charity's batch payments.</summary>
    public DateTime? UploadDate { get; set; }
}

// ---- §23.U.21 family update tracking (UC-RPT-21) ---------------------------------

/// <summary>
/// UC-RPT-21 (§23.U.21 متابعة تحديث بيانات الأسر) — the monitoring-sheet data request.
/// The payment anchors the window: families of the payment's charities refreshed in
/// [<see cref="Date"/>, payment's period end]. <see cref="PaymentId"/> is the precise
/// anchor; <see cref="BatchNo"/> (the §23.S.12 panel's field) resolves the latest payment
/// of that batch; neither → the latest payment. A print sheet is one page-set, not a
/// browse — the page size is a page-set bound (500).
/// </summary>
public class FamilyUpdateTrackingFilterDto
{
    /// <summary>The payment group anchor (precise).</summary>
    public Guid? PaymentId { get; set; }

    /// <summary>رقم الدفعة — the panel's batch field; resolves the batch's latest payment.</summary>
    public string? BatchNo { get; set; }

    /// <summary>من فضلك ادخل تاريخ بدا التحديث — required (validator).</summary>
    public DateTime Date { get; set; }

    /// <summary>الجمعية — explicit HQ narrow.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — clamped to 500 in the service (one print page-set).</summary>
    public int PageSize { get; set; } = 500;
}

/// <summary>One monitoring-sheet row — a family file refreshed in the window.</summary>
public class FamilyUpdateTrackingRowDto
{
    public Guid FamilyId { get; set; }
    public string FamilyCode { get; set; } = string.Empty;
    public string HeadOfFamily { get; set; } = string.Empty;
    public string? CharityName { get; set; }
    public DateTime? UpdatedOn { get; set; }
}

// ---- §23.U.22 missed payments (UC-RPT-22) -----------------------------------------

/// <summary>
/// UC-RPT-22 (§23.U.22 أيتام مستحقون دفعات سابقة) — the arrears read. الجمعية is an HQ-only
/// narrow; charity callers are pinned server-side from the token, never the payload.
/// </summary>
public class MissedPaymentsReportFilterDto
{
    /// <summary>الجمعية — explicit HQ narrow.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// UC-RPT-23's widened scope (جميع الدفعات الفائتة) — the §23.S.11 all-orphans toggle.
    /// HQ-only: a non-HQ caller setting it is refused server-side (the client toggle is never
    /// trusted); an explicit <see cref="CharityId"/> wins over the flag.
    /// </summary>
    public bool AllOrphans { get; set; } = false;

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — the export pages under the same cap (validator clamps to 100).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// One orphan's per-batch receipt state. A row exists because at least one batch the orphan
/// was entitled to was never received (IsGotIt false, not stopped). The grid's batch columns
/// are dynamic: only batch numbers present in the result set get a column.
/// </summary>
public class MissedPaymentReportRowDto
{
    public Guid OrphanId { get; set; }
    public string OrphanCode { get; set; } = string.Empty;
    public string OrphanName { get; set; } = string.Empty;
    public string? CharityName { get; set; }

    /// <summary>
    /// سبب طلب الاستعداد — the platform persists NO arrears-reason field anywhere
    /// (grep-proven: OrphanPaymentItem carries state dates/flags only). Per the real-data-only
    /// ruling the column renders null — omitted, not fabricated.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>Batch number → received. Only batches the orphan has a non-stopped item in.</summary>
    public Dictionary<string, bool> BatchStates { get; set; } = new();

    /// <summary>How many entitled-but-unreceived batches the row carries.</summary>
    public int MissedBatches { get; set; }
}

// ---- §23.U.24 orphan photograph manifest (UC-RPT-24) -------------------------------

/// <summary>
/// UC-RPT-24 (§23.U.24 صور الأيتام) — the photographs manifest request: photos attached to
/// accepted periodic reports whose <see cref="PeriodicOrphanReport.ReportDate"/> falls in
/// [<see cref="DateFrom"/>, <see cref="DateTo"/>]. من تاريخ is mandatory (validator);
/// الى تاريخ is optional and ≥ DateFrom (the legacy binding swap is corrected — story
/// Screen contract note). The response is a JSON manifest the SPA turns into an ExcelJS
/// workbook (client-side export ruling) — no server EPPlus.
/// </summary>
public class OrphanFilesExportFilterDto
{
    /// <summary>الجمعية — explicit HQ narrow; charity callers are pinned from the token.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>من تاريخ — mandatory (validator).</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>الى تاريخ — optional; ≥ DateFrom when set; open-ended upper when null.</summary>
    public DateTime? DateTo { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — image manifests are heavy; default 20, clamped to 100 (validator).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// One manifest row — one photograph of one accepted report. Carries everything the grid
/// thumbnail, the per-row download command and the ExcelJS workbook need; the download URL
/// targets the live attachment endpoint (19-2) — never rebuilt here.
/// </summary>
public class OrphanFileManifestRowDto
{
    public Guid OrphanId { get; set; }
    public string OrphanCode { get; set; } = string.Empty;
    public string OrphanName { get; set; } = string.Empty;
    public string? CharityName { get; set; }
    public Guid AttachmentId { get; set; }
    /// <summary>Stored file name — resolved from the Framework attachment store (page rows only).</summary>
    public string? FileName { get; set; }
    /// <summary>Stored content type — drives the workbook's image extension.</summary>
    public string? ContentType { get; set; }
    /// <summary>The live download endpoint for this attachment.</summary>
    public string DownloadUrl { get; set; } = string.Empty;
}

// ==================== §23.U.28 — أيتام لم يصرف لهم (orphans receiving nothing) ====================

/// <summary>
/// UC-RPT-28 — the zero-disbursement gaps of one payment batch. PaymentId is mandatory
/// (the batch IS the report's frame); CharityId is an HQ narrow only — charity callers
/// are pinned from the token, never the payload.
/// </summary>
public class OrphansWithoutPaymentFilterDto
{
    /// <summary>الدفعة — the payment group whose gaps are listed (mandatory, validator).</summary>
    public Guid PaymentId { get; set; }

    /// <summary>الجمعية — explicit HQ narrow; charity callers are pinned from the token.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size — default 20, clamped to 100 (validator).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// One gap row — an orphan of the batch for whom nothing was disbursed (no cheque, no
/// transfer, nothing received). Amount is the item's expected entitlement (المبلغ المستحق),
/// not a disbursed figure.
/// </summary>
public class OrphansWithoutPaymentListDto
{
    public Guid OrphanId { get; set; }
    public string OrphanCode { get; set; } = string.Empty;
    public string OrphanName { get; set; } = string.Empty;
    public string? CharityName { get; set; }
    /// <summary>المبلغ المستحق — the item's recorded amount, 0 when none was recorded.</summary>
    public decimal Amount { get; set; }
    /// <summary>وقف الصرف — a stopped orphan is a legitimate zero-disbursement gap.</summary>
    public bool IsStopped { get; set; }
}

/// <summary>
/// UC-RPT-29 filter — one outcome list of one batch: received (المستلمون), not-received
/// (غير المستلمين) or stopped (الموقوفون). OrpCheckBatchNo is the batch discriminator
/// (mandatory); CharityId is an HQ narrow — charity callers are pinned from the token.
/// Variant rides the single endpoint because the EP-10 10-19/10-20 sibling keys have not
/// landed — one dataset, three discriminator values (recorded in the story).
/// </summary>
public class PaymentsOutcomeFilterDto
{
    /// <summary>received | notReceived | stopped — normalised case-insensitively by the validator.</summary>
    public string Variant { get; set; } = "received";

    /// <summary>الدفعة — the batch number shared by the payment groups in scope (mandatory).</summary>
    public string OrpCheckBatchNo { get; set; } = string.Empty;

    /// <summary>الجمعية — explicit HQ narrow; charity callers are pinned from the token.</summary>
    public Guid? CharityId { get; set; }
}

/// <summary>One outcome row — the orphan's entitlement and its disbursement evidence.</summary>
public class PaymentsOutcomeRowDto
{
    public Guid OrphanId { get; set; }
    public string OrphanCode { get; set; } = string.Empty;
    public string OrphanName { get; set; } = string.Empty;
    /// <summary>المعيل — the orphan's family provider (guardian), when recorded.</summary>
    public string? GuardianName { get; set; }
    /// <summary>المبلغ — the item's recorded amount, 0 when none was recorded.</summary>
    public decimal Amount { get; set; }
    /// <summary>رقم الشيك — receipt evidence, when a cheque was issued.</summary>
    public string? ChiqueNo { get; set; }
    /// <summary>تاريخ الطباعة — the item's print stamp.</summary>
    public DateTime? PrintDate { get; set; }
    /// <summary>المستلم — the recorded beneficiary/collector (من استلم).</summary>
    public string? CollectorName { get; set; }
}

/// <summary>
/// The printable outcome document — header + rows + totals, unpaged (the whole list is
/// the document; print-run flagging stays with 10-10/10-24, nothing is written).
/// </summary>
public class PaymentsOutcomeReportDto
{
    /// <summary>received | notReceived | stopped — echoes the requested variant.</summary>
    public string Variant { get; set; } = string.Empty;
    public string BatchNo { get; set; } = string.Empty;
    /// <summary>الجمعية — the effective scope's label (null = كل الجهات for an HQ full run).</summary>
    public string? CharityName { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
    /// <summary>العملة — from the batch's groups; batches mixing currencies report the first.</summary>
    public string? Currency { get; set; }
    public List<PaymentsOutcomeRowDto> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    /// <summary>إجمالي المبالغ — the sum of the rows' amounts.</summary>
    public decimal TotalAmount { get; set; }
    /// <summary>Empty-variant message (AC 4) — set when the batch has no row in this variant.</summary>
    public string? Message { get; set; }
}

// ---- §23.U.30 cheque numbers list (UC-RPT-30) ------------------------------------

/// <summary>
/// UC-RPT-30 filter — أرقام الشيكات of one cheque batch. No bank/date/type dimension
/// here (that is 18-33's cheque statement); the batch number is the only frame.
/// </summary>
public class ChequeNumbersFilterDto
{
    /// <summary>رقم الدفعة — the batch whose recorded cheque numbers print.</summary>
    public string OrpCheckBatchNo { get; set; } = string.Empty;

    /// <summary>الجمعية — HQ narrow only; a charity caller is pinned from the token.</summary>
    public Guid? CharityId { get; set; }
}

/// <summary>One sheet row — a payment item with a cheque number recorded (10-12's settlement).</summary>
public class ChequeNumbersRowDto
{
    public Guid OrphanId { get; set; }
    public string? OrphanCode { get; set; }
    public string? OrphanName { get; set; }
    /// <summary>المعيل — orphan → family → provider (nulls render blank).</summary>
    public string? GuardianName { get; set; }
    public decimal Amount { get; set; }
    /// <summary>رقم الشيك — the item's recorded cheque number (wire name per the story contract).</summary>
    public string? ChiqueNo { get; set; }
    public DateTime? PrintDate { get; set; }
    /// <summary>المستلم — من استلم, recorded at settlement.</summary>
    public string? CollectorName { get; set; }
}

/// <summary>The report envelope — header + rows + totals. No cheques ⇒ rows:[] + message (AC 4).</summary>
public class ChequeNumbersReportDto
{
    public string BatchNo { get; set; } = string.Empty;
    /// <summary>الجمعية — the effective scope's label (null = كل الجهات for an HQ full run).</summary>
    public string? CharityName { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
    /// <summary>العملة — from the batch's groups; batches mixing currencies report the first.</summary>
    public string? Currency { get; set; }
    public List<ChequeNumbersRowDto> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    /// <summary>إجمالي المبالغ — the sum of the rows' amounts.</summary>
    public decimal TotalAmount { get; set; }
    /// <summary>Empty-sheet message (AC 4) — set when no cheque is recorded for the batch.</summary>
    public string? Message { get; set; }
}

// ---- §23.U.31 receipt cards (UC-RPT-31) ----------------------------------------

/// <summary>
/// UC-RPT-31 filter — كروت الاستلام of one cheque batch: the cards each guardian signs at
/// collection. Same frame as 18-30's sheet; no bank/date dimension exists for cards.
/// </summary>
public class ReceiptCardsFilterDto
{
    /// <summary>رقم الدفعة — the batch whose collection cards print.</summary>
    public string OrpCheckBatchNo { get; set; } = string.Empty;

    /// <summary>الجمعية — HQ narrow only; a charity caller is pinned from the token.</summary>
    public Guid? CharityId { get; set; }
}

/// <summary>
/// One card — one orphan's payment handed to that orphan's guardian (the card is signed at
/// collection). A guardian with several orphans in the batch signs one card per orphan; the
/// card contract lists a single orphan's identity per §23.U.31.
/// </summary>
public class ReceiptCardDto
{
    public Guid OrphanId { get; set; }
    public string? OrphanCode { get; set; }
    public string? OrphanName { get; set; }
    /// <summary>المعيل — orphan → family → provider (nulls render blank).</summary>
    public string? GuardianName { get; set; }
    /// <summary>المبلغ — the item's recorded amount, 0 when none was recorded.</summary>
    public decimal Amount { get; set; }
    /// <summary>رقم الشيك — receipt evidence when a cheque was issued (renders blank otherwise).</summary>
    public string? ChiqueNo { get; set; }
}

/// <summary>
/// The cards envelope — header + one row per (guardian, orphan) payment item of the batch.
/// Non-paged by design (the recorded choice over a capped ReportPagedResult): cards print in
/// one pass. No rows in scope ⇒ rows:[] + message (AC 3), never an empty document.
/// </summary>
public class ReceiptCardsReportDto
{
    public string BatchNo { get; set; } = string.Empty;
    /// <summary>الجمعية — the effective scope's label (null = كل الجهات for an HQ full run).</summary>
    public string? CharityName { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
    /// <summary>العملة — from the batch's groups; batches mixing currencies report the first.</summary>
    public string? Currency { get; set; }
    public List<ReceiptCardDto> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    /// <summary>إجمالي المبالغ — the sum of the rows' amounts (the cut-sheet's footer).</summary>
    public decimal TotalAmount { get; set; }
    /// <summary>Empty message (AC 3) — set when the batch has no card rows in the requested scope.</summary>
    public string? Message { get; set; }
}

// ---- §23.U.32 payment summary page (UC-RPT-32) --------------------------------

/// <summary>
/// UC-RPT-32 — the summary-page figures of one payment batch for a charity: the batch
/// view's totals band. Every figure is null-safe — an absent set renders 0, never a 500
/// (AC 4); the empty case is told apart by <see cref="Message"/>.
/// </summary>
public class PaymentSummaryDto
{
    /// <summary>The requested payment group id (echoed for the panel's identity).</summary>
    public Guid PaymentId { get; set; }
    /// <summary>رقم الدفعة — the group's batch number; groups sharing it aggregate together.</summary>
    public string? BatchNo { get; set; }
    /// <summary>تاريخ الدفعة — the group's date (first by GroupDate of the batch's groups).</summary>
    public DateTime? BatchDate { get; set; }
    /// <summary>الجمعية — the effective scope's label (null = كل الجهات for an HQ full run).</summary>
    public string? CharityName { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
    /// <summary>العملة — from the batch's groups; batches mixing currencies report the first.</summary>
    public string? Currency { get; set; }
    /// <summary>عدد الأيتام — distinct orphans with a payment row in the batch.</summary>
    public int OrphanCount { get; set; }
    /// <summary>إجمالي المصروف — the sum of every payment-row amount in scope.</summary>
    public decimal TotalAmount { get; set; }
    /// <summary>المستلم — rows flagged IsGotIt.</summary>
    public int ReceivedCount { get; set; }
    public decimal ReceivedAmount { get; set; }
    /// <summary>غير المستلم — rows not flagged IsGotIt (the stopped slice rides this figure
    /// too — §15.U.18–20's one-dataset definition, no divergent predicates).</summary>
    public int NotReceivedCount { get; set; }
    public decimal NotReceivedAmount { get; set; }
    /// <summary>الموقوف — rows flagged IsStopped (10-9's stop lock).</summary>
    public int StoppedCount { get; set; }
    public decimal StoppedAmount { get; set; }
    /// <summary>الشيكات — rows with a cheque number recorded (10-12's settlement).</summary>
    public int ChequeCount { get; set; }
    public decimal ChequeTotal { get; set; }
    /// <summary>Empty-batch companion (AC 4) — set when no payment rows fall in scope; figures stay 0.</summary>
    public string? Message { get; set; }
}

// ---- §23.U.35 new orphans / new widows (UC-RPT-35) ------------------------------

/// <summary>
/// UC-RPT-35 filter — the four legacy .rpt variants collapsed to one endpoint + variant key
/// (the 9-17 convention). Params: charityId (all-option for HQ), dateFrom/dateTo (the
/// registration window — dateFrom mandatory), variant.
/// </summary>
public class NewBeneficiariesFilterDto
{
    public Guid? CharityId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    /// <summary>orphans · orphansV2 · widows · widowsByFamily (canonicalised lowercase).</summary>
    public string Variant { get; set; } = "orphans";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// The superset row of the four variants — each variant fills its own columns and leaves the
/// rest null (blank-where-absent; the screens pick the variant's column set).
/// </summary>
public class NewBeneficiaryListDto
{
    /// <summary>رقم اليتيم — orphans / orphansV2.</summary>
    public string? OrphanCode { get; set; }
    /// <summary>أسم اليتيم — orphans / orphansV2.</summary>
    public string? OrphanName { get; set; }
    /// <summary>تاريخ الميلاد — orphans / orphansV2.</summary>
    public DateTime? BirthDate { get; set; }
    /// <summary>كود العائلة — orphansV2 / widowsByFamily.</summary>
    public string? FamilyCode { get; set; }
    /// <summary>اسم المعيل — orphansV2.</summary>
    public string? GuardianName { get; set; }
    /// <summary>اسم الأرملة — widows / widowsByFamily.</summary>
    public string? WidowName { get; set; }
    /// <summary>الرقم القومي — widows.</summary>
    public string? NationalId { get; set; }
    /// <summary>تاريخ وفاة الزوج — widows.</summary>
    public DateTime? HusbandDeathDate { get; set; }
    /// <summary>عدد الأبناء — widowsByFamily.</summary>
    public int ChildrenCount { get; set; }
    /// <summary>الجمعية — every variant (resolved post-fetch; the row set can span charities on an HQ run).</summary>
    public string? CharityName { get; set; }
    /// <summary>تاريخ التسجيل — CreatedOn of the orphan/family (the recorded proxy; no registration column exists).</summary>
    public DateTime? RegistrationDate { get; set; }
    /// <summary>Scope key for the charity-name resolution — never a wire filter.</summary>
    public Guid? CharityId { get; set; }
}

/// <summary>
/// UC-RPT-35 envelope — the paged rows plus the canonical variant key echo (AC 2's
/// "composed payload + variant key", never PDF bytes).
/// </summary>
public class NewBeneficiariesReportDto : ReportPagedResult<NewBeneficiaryListDto>
{
    /// <summary>The canonicalised variant the rows answer (the screen keys its columns off it).</summary>
    public string Variant { get; set; } = string.Empty;
}

/// <summary>
/// §23.U.36 sheet selector — the legacy Crystal trio collapsed to one variant-keyed read
/// (rptFollowUp / rptFollowUpFamily / rptFollowUpTasleem).
/// </summary>
public enum FollowUpSheetVariant
{
    /// <summary>متابعة — one row per orphan with its latest accepted report.</summary>
    FollowUp = 1,

    /// <summary>متابعة الأسر — one row per family.</summary>
    FollowUpFamily = 2,

    /// <summary>تسليم — the handover/delivery sheet, one row per orphan with its guardian.</summary>
    Tasleem = 3
}

/// <summary>
/// §23.U.36 filter — variant + page bounds + the charity narrow (HQ-only passthrough) and an
/// optional registration window on the row anchor (orphan for orphan variants, family for the
/// family variant — the 18-35 convention).
/// </summary>
public class FollowUpSheetFilterDto
{
    /// <summary>The sheet variant — picks the projection (default متابعة).</summary>
    public FollowUpSheetVariant Variant { get; set; } = FollowUpSheetVariant.FollowUp;

    /// <summary>الجمعية — an HQ-only narrow; a charity caller is pinned from the token.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>من تاريخ — optional registration-window start on the row anchor.</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>الى تاريخ — optional registration-window end (inclusive, day granularity).</summary>
    public DateTime? DateTo { get; set; }

    /// <summary>1-based page index.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size (validator caps at 100).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// §23.U.36 row — the superset of the three sheets' columns; each variant fills its own and
/// leaves the rest null (the screen picks the variant's column set).
/// </summary>
public class FollowUpSheetRowDto
{
    /// <summary>رقم اليتيم (FollowUp / Tasleem).</summary>
    public string? OrphanCode { get; set; }

    /// <summary>أسم اليتيم (FollowUp / Tasleem).</summary>
    public string? OrphanName { get; set; }

    /// <summary>اسم المعيل — the family provider's full name, HeadOfFamily fallback (Tasleem).</summary>
    public string? GuardianName { get; set; }

    /// <summary>كود العائلة (all variants; the family variant's lead column).</summary>
    public string? FamilyCode { get; set; }

    /// <summary>Resolution key for CharityName — never a wire FK.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>الجمعية — resolved NameAr ?? NameEn post-fetch.</summary>
    public string? CharityName { get; set; }

    /// <summary>حالة الكفالة — Orphan.SponsorshipStatus free string (FollowUp / Tasleem).</summary>
    public string? SponsorshipStatus { get; set; }

    /// <summary>تاريخ آخر تقرير — latest ACCEPTED periodic report (FollowUp; BR-11).</summary>
    public DateTime? LastReportDate { get; set; }

    /// <summary>المبلغ الشهري — Orphan.MonthlyAmount (FollowUp / Tasleem).</summary>
    public decimal? MonthlyAmount { get; set; }

    /// <summary>رب الأسرة (FollowUpFamily).</summary>
    public string? HeadOfFamily { get; set; }

    /// <summary>عدد الأيتام في الأسرة (FollowUpFamily).</summary>
    public int OrphansCount { get; set; }

    /// <summary>حالة الأسرة — Family.FamilyStatus free string (FollowUpFamily).</summary>
    public string? FamilyStatus { get; set; }

    /// <summary>تاريخ التسجيل — CreatedOn proxy, the 18-35 ruling (FollowUpFamily).</summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>آخر تحديث — Family.UpdatedOn (FollowUpFamily).</summary>
    public DateTime? LastUpdate { get; set; }
}

/// <summary>
/// §23.U.37 identification-sheet selector — the legacy ForGuardians / ForWidows /
/// ForWidows_Family printouts collapsed to one variant-keyed read.
/// </summary>
public enum GuardianIdentificationSheetVariant
{
    /// <summary>تعريف العائل — every provider row of the (scoped) charity's families.</summary>
    AllGuardians = 1,

    /// <summary>الأرامل — every widow (alive mother, husband death date present) of the scoped families.</summary>
    WidowsOnly = 2,

    /// <summary>أسرة محددة — the guardians + widows of ONE family (FamilyId required).</summary>
    SingleFamily = 3
}

/// <summary>
/// §23.U.37 filter — variant + the charity narrow (HQ-only passthrough) + the start date
/// (§23.S.10's تاريخ بدء التقرير: rows' CreatedOn on/after it) + FamilyId for the
/// single-family variant. The legacy <c>userId</c> parameter is resolved server-side from
/// <c>ICurrentUserService.UserId</c> — it is never accepted from the payload.
/// </summary>
public class GuardianIdentificationSheetFilterDto
{
    /// <summary>The sheet variant — picks the projection (default تعريف العائل).</summary>
    public GuardianIdentificationSheetVariant Variant { get; set; }
        = GuardianIdentificationSheetVariant.AllGuardians;

    /// <summary>الجمعية — an HQ-only narrow; a charity caller is pinned from the token.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>تاريخ بدء التقرير — optional; limits rows to register entries created on/after it.</summary>
    public DateTime? Date { get; set; }

    /// <summary>أسرة محددة — REQUIRED when Variant = SingleFamily (validator enforces).</summary>
    public Guid? FamilyId { get; set; }
}

/// <summary>
/// §23.U.37 row — the superset of the guardians' and widows' column sets; each variant fills
/// its own and leaves the rest null (the client sheet builder picks the variant's columns).
/// </summary>
public class GuardianIdentificationRowDto
{
    /// <summary>اسم المعيل — Provider.FullName (AllGuardians / SingleFamily).</summary>
    public string? GuardianName { get; set; }

    /// <summary>اسم الأرملة — Mother.FullName (WidowsOnly / SingleFamily).</summary>
    public string? WidowName { get; set; }

    /// <summary>الرقم القومي — Provider.NationalId / Mother.NationalId.</summary>
    public string? NationalId { get; set; }

    /// <summary>الهاتف — Provider.Phone / Mother.Phone.</summary>
    public string? Phone { get; set; }

    /// <summary>صلة القرابة — Provider.RelationshipToFamily (guardians only).</summary>
    public string? RelationshipToFamily { get; set; }

    /// <summary>الوظيفة — Provider.Job (guardians only).</summary>
    public string? Job { get; set; }

    /// <summary>تاريخ الميلاد — Mother.DateOfBirth (widows only).</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>كود العائلة — the row's family.</summary>
    public string? FamilyCode { get; set; }

    /// <summary>Resolution key for CharityName — never a wire FK.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>الجمعية — resolved NameAr ?? NameEn post-fetch.</summary>
    public string? CharityName { get; set; }
}

/// <summary>
/// §23.U.37 payload — the whole selection (a print document is never page 1) + the producing
/// user stamped server-side from the token (never from the payload).
/// </summary>
public class GuardianIdentificationSheetDto
{
    /// <summary>The canonicalised variant the rows answer.</summary>
    public string Variant { get; set; } = string.Empty;

    /// <summary>The producing user — ICurrentUserService.UserId, stamped server-side.</summary>
    public string? ProducedBy { get; set; }

    /// <summary>When the sheet was produced (server clock, UTC).</summary>
    public DateTime ProducedOn { get; set; }

    /// <summary>The row count (drives the nothing-to-produce guard client-side).</summary>
    public int TotalCount { get; set; }

    /// <summary>The whole selection's rows.</summary>
    public List<GuardianIdentificationRowDto> Rows { get; set; } = new();

    /// <summary>Populated when the selection is empty — the nothing-to-produce message.</summary>
    public string? Message { get; set; }

    /// <summary>Review P14 2026-08-26: the 5000-row ceiling was hit — the sheet is
    /// deliberately incomplete; the client warns instead of printing silently truncated.</summary>
    public bool Truncated { get; set; }
}

/// <summary>
/// §23.U.38 مرفقات الصادر الناقصة — filter: the letter-date window + the category narrow +
/// the charity narrow (HQ-only passthrough) + page bounds.
/// </summary>
public class MissingOutgoingAttachmentsFilterDto
{
    /// <summary>الجمعية — an HQ-only narrow; the endpoint refuses non-HQ roles regardless.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>من تاريخ — letter date window start.</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>الى تاريخ — letter date window end (inclusive, day granularity).</summary>
    public DateTime? DateTo { get; set; }

    /// <summary>التصنيف — narrows to one outgoing category.</summary>
    public int? OutgoingCategoryId { get; set; }

    /// <summary>1-based page index.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size (validator caps at 100).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// §23.U.38 row — an outgoing letter inside the window that carries ZERO orphan-report
/// attachment rows (the v1 rule; the legacy expected-list table is absent — upgrade path in
/// the story Dev Notes).
/// </summary>
public class MissingOutgoingAttachmentsRowDto
{
    /// <summary>مسلسل الصادر.</summary>
    public int? Serial { get; set; }

    /// <summary>رقم الصادر.</summary>
    public string? OutGoingNumber { get; set; }

    /// <summary>الموضوع.</summary>
    public string? Subject { get; set; }

    /// <summary>تاريخ الخطاب.</summary>
    public DateTime? Date { get; set; }

    /// <summary>السنة.</summary>
    public int? Year { get; set; }

    /// <summary>التصنيف — resolved NameAr ?? NameEn.</summary>
    public string? CategoryName { get; set; }

    /// <summary>Resolution key for CharityName — never a wire FK.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>الجمعية — resolved NameAr ?? NameEn post-fetch.</summary>
    public string? CharityName { get; set; }

    /// <summary>Always 0 — kept so the grid states the finding, not just the letter.</summary>
    public int AttachmentCount { get; set; }
}

/// <summary>§23.U.39 filter — أيتام الأسر بتاريخ: one charity + one as-at date.</summary>
public class FamilyOrphansByDateFilterDto
{
    /// <summary>الجمعية — an HQ-only narrow; the endpoint refuses non-HQ roles regardless.</summary>
    public Guid? CharityId { get; set; }

    /// <summary>بتاريخ — the as-at date; a family is in when registered on or before it.</summary>
    public DateTime Date { get; set; }

    /// <summary>1-based page index (a page = families).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size (validator caps at 100).</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>§23.U.39 orphan row — one orphan inside its family group.</summary>
public class FamilyOrphanRowDto
{
    /// <summary>رقم اليتيم.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>أسم اليتيم.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>تاريخ الميلاد — the age basis.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>العمر at the chosen date — computed post-fetch; null when the birth date is unknown.</summary>
    public int? Age { get; set; }
}

/// <summary>§23.U.39 family group — the header band (كود العائلة · اسم المعيل · المحافظة/المركز) plus its orphans.</summary>
public class FamilyWithOrphansDto
{
    /// <summary>كود العائلة.</summary>
    public string FamilyCode { get; set; } = string.Empty;

    /// <summary>اسم المعيل.</summary>
    public string HeadOfFamily { get; set; } = string.Empty;

    /// <summary>المحافظة — Region resolved NameAr ?? NameEn.</summary>
    public string? RegionName { get; set; }

    /// <summary>المركز — Center resolved NameAr ?? NameEn.</summary>
    public string? CenterName { get; set; }

    /// <summary>تاريخ تسجيل الأسرة — the as-at anchor (a dedicated column, unlike 18-35's CreatedOn proxy).</summary>
    public DateTime RegistrationDate { get; set; }

    /// <summary>عدد أيتام الأسرة.</summary>
    public int OrphansCount { get; set; }

    /// <summary>The family's non-deleted orphans as at the run.</summary>
    public List<FamilyOrphanRowDto> Orphans { get; set; } = new();
}
