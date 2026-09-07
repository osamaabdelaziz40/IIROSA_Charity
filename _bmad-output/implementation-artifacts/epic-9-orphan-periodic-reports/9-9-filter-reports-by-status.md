# Story 9-9: Filter reports by status

| Field | Value |
| --- | --- |
| Story key | `9-9-filter-reports-by-status` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-09 — حالة اليتيم |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.S.4 screen — 10 filter fields, wide grid; §14.U.9 scenario) |
| Route | `#/periodic-orphan-reports/orphan-reports/search` |
| Endpoint | `GET /api/PeriodicOrphanReports` (the 9-1 filter read, extended — see Dev Notes on the spec's `POST /{id}/review` line) |
| Depends on | **9-1** (registered module, scope, filter pipeline) |
| Roles | HQ roles, charity → `Charity`, `SuperAdmin`, `Admin`, `Accountant`, `Employee` |

## Status

review

## Story

As a HQ role, I want to be able to filter reports by status حالة اليتيم, so that I can locate a
record from partial information.

## Acceptance Criteria

1. Given a HQ role on `#/periodic-orphan-reports/orphan-reports/search`, when the actor presses
   «بحث» with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by the periodic-reports
   filter endpoint and the matching rows render in the result grid without a page reload, scoped to
   the caller's charity (and country) per the 9-1 rules.
3. Given the And/Or radio is set, when multiple status filters are active, then the predicates
   combine accordingly (And = all must hold; Or = any).
4. Given no row matches the criteria, when the search runs, then the grid renders empty and the
   paging control reports zero pages.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the 10 filter fields of §14.S.4 are implemented with their option sets;
§14.U.9 passes end to end; the scoping is enforced server-side.

**Endpoint decision (recorded):** §14.U.9's realisation line (`POST /api/PeriodicOrphanReports/{id}/review`
carrying `OrphanStatusRefinedContract`) is a legacy derivation artifact — it names the *review*
action, which a search screen cannot use. The realisation is the 9-1 filter read
(`GET /api/PeriodicOrphanReports`) extended with the orphan-status predicates this story adds to
`PeriodicOrphanReportFilterDto`. The legacy contract name survives only as the And/Or combine flag
(`AndOr`).

## Screen contract (§14.S.4 — 10 filter fields)

| Label | Filter key (new/extended) | Control · options |
| --- | --- | --- |
| الجمعية | `CharityId` (exists) | Drop-down · Charities lookup + كافة الجهات · HQ only |
| Or / And | `AndOr` ("and"/"or", default "and") | Radio pair |
| نوع التعليم | `SchoolType` | Drop-down · الكل / حكومي / أهلي |
| * الحالة الصحية | `MedicalStatus` (exists) | Drop-down · الكل / سليم / معاق / مريض |
| الحالة الاجتماعية | `MaritalStatus` | Drop-down · الكل / تزوج / اعزب / متوفى → maps to `Married`/`Dead` report flags |
| الحالة التعليمية | `EducationalStatus` | Drop-down · الكل / يدرس / حاصل على شهادة / ترك الدراسة → maps to `IsOrphanStudent`/`HighestEducationalLevel`/`DropOut` |
| الصف الدراسي | `EducationalStageId` (exists) | Drop-down · lookup |
| المرحلة الدراسية | `EducationalLevelId` (exists) | Drop-down · lookup |
| * اخر تقدير | `EducationDegree` (add to filter) | Drop-down · ممتاز / جيدجدا / جيد / مقبول / ضعيف |

Grid (spec, 38 columns — orphan-centric): رقم اليتيم · أسم اليتيم · عمر اليتيم · أسم المعيل ·
صلة القرابة · المحافظة · المركز · القرية/الحى · العنوان التفصيلى · ت المنزل · ت الموبايل ·
تاريخ الميلاد · الرقم القومى · النوع · مؤهل المعيل · مهنة المعيل · الرقم القومى للمعيل ·
مشروع تنموى للمعيل · الاستبعاد · سبب الاستبعاد · أسم الجمعية · تاريخ اخر تحديث · نوع التعليم ·
الكليه · المدرسه · تاريخ الوفاه · تاريخ الزواج · تاريخ التقرير · الحاله الصحيه · بنود الاعاقه ·
بنود المرض · المرحله الدراسه · أخر صف دراسي · التخصص · التقدير · الحاله (report state) · كود
العائله.

**Grid-scope honesty (recorded):** the orphan/guardian/address half of the columns lives on
`Orphan`/`Family`/guardian entities, not on the report. Serve them by extending the list projection
to Include `Orphan → Family (→ guardian)` and flattening into an extended list DTO. If a column's
source field genuinely does not exist on the copied entities (e.g. مشروع تنموى للمعيل, الاستبعاد
pair), render it as — and record the gap in the completion notes rather than inventing columns.
The report-dimension columns come from the report row itself.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Backend filter | `PeriodicOrphanReportFilterDto` + `GetReportsAsync` | Charity/orphan/dates/status/search + paging/sorting; missing the §14.S.4 status keys and And/Or |
| Frontend | `orphan-report-search.component.*` | Exists (standalone), unreachable before 9-1; skeleton filters, no And/Or, hardcoded strings |

## Tasks / Subtasks

- [x] **Task 1 — Filter extensions** (AC 2, 3): add `AndOr`, `SchoolType`, `MaritalStatus`,
      `EducationalStatus`, `EducationDegree` to `PeriodicOrphanReportFilterDto`; build the
      predicate tree in `GetReportsAsync`: translate the tri-state selections to their underlying
      flags/fields, combine with `&&` or `||` per `AndOr` (default and) — combine, never replace
      (the 15-1 defect-4 class); keep the 9-1 caller scope and pagination clamps
      — shipped in `ApplyFilters`: a status-predicate family (SchoolType · MedicalStatus ·
      MaritalStatus · EducationalStatus · EducationDegree · EducationalStageId ·
      EducationalLevelId) collected then reduced by `CombinePredicates` (Expression
      AndAlso/OrElse with a parameter-rebinding visitor — single translatable WHERE); scope/
      date/orphan predicates stay hard-AND'ed before it. MaritalStatus tokens married/single/
      deceased → Married/Dead flags (single = neither); EducationalStatus tokens studying/
      graduated/dropout → IsOrphanStudent / HighestEducationalLevel-set / DropOut.
- [x] **Task 2 — Wide projection** (AC 2): extend the list DTO (+ AutoMapper profile) with the
      orphan/family/guardian flatten; Include the navigations (one query, no N+1); verify
      `trackBy` ids remain the report id
      — the shipped pipeline projects manually inside `GetPagedAsync` (no AutoMapper in the
      list path), so the flatten went there: Orphan (dob/gender/nationalId/phone), Family
      (code/region/center/cityVillage/address/homePhone), Provider (name/relation/nationalId/
      job/education level) + the report-dimension columns — nav traversals translate to LEFT
      JOINs in the one query. `trackBy: trackByReportId` unchanged.
- [x] **Task 3 — Search screen** (AC 1, 3, 4): rebuild `orphan-report-search.component` to the
      10-field contract above (lookup-driven where a lookup exists, translated closed sets where
      not); بحث reloads page 1; wide grid with horizontal scroll, spec column order, `trackBy`,
      `OnPush`, empty state; استخراج البيانات command exports the current result to Excel
      client-side (ExcelJS — the 16-1 extract precedent)
      — rebuilt (the old UC-6.16 per-orphan timeline mock is gone); column set defined once in
      TS and shared by grid + export; عمر اليتيم computed from orphanDateOfBirth; export loops
      paged reads of 100 (server clamp) capped at 2000 rows; `orphan/:orphanId` shortcut wired
      (prefills the OrphanId filter, trivial per the story's own note).
- [x] **Task 4 — Reachability**: the `/orphan-reports/search` route is registered by 9-1; add the
      sidebar sub-entry (حالة اليتيم) and link from the list screen where the spec hosts it
      — sidebar sub-entry under التقارير الدورية + حالة اليتيم link button on the register
      header; both routes re-pointed from dead `orphanReports.*` i18n keys to
      `periodicReports.search.title` and gained PermissionGuard `PeriodicReports.View`.
- [x] **Task 5 — i18n** — all filter labels, option sets, and column headers under
      `periodicReports.search.*` in **both** `ar.json` and `en.json`
      — 36 label/option keys + 37 column keys per locale (the spec prose says 38 columns but
      its enumerated list has 37 — the enumerated list is implemented).
- [ ] **Task 6 — Verification** (AC 1–5): live check — each single filter narrows; And vs Or
      differ on a two-filter case (verify with SQL-comparable counts); charity scope holds; empty
      result → empty grid/zero pages; export downloads a non-empty xlsx when rows exist;
      unauthenticated → 401; `npm run build` green; tests excluded per the standing user decision
      — `dotnet build` IIROSA.Application 0 errors; `npm run build` pending (final build after
      the 9-5..9-8 gating build exits); live endpoint checks pending the user restarting their
      own IIROSA.Api (never killed by policy).

## Dev Notes

### Platform rules that bind this story

- Read-only path; 9-1's clamps, envelope `{items, totalCount, page}`, and scope unchanged.
- Combine predicates — never replace the accumulated expression (15-1 defect class).
- camelCase wire; raw envelope; lookups `nameAr ?? nameEn`.
- Closed option sets (health/marital/educational tri-states, grades, school type) ship as
  translated constants — they are WAR-fixed enums, not user-maintained lookups (9-3 ruling).

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Grouped statistics screen (`/orphan-reports`) | 9-10 |
| Detailed extract (`/orphan-reports/generate`) | 9-11 |
| `orphan/:orphanId` shortcut route (same component, prefilled orphan — wire only if trivial; not separately specced) | — |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.S.4] the 10-field contract + grid
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.9] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-09 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-16-correspondence-incoming-and-outgoing/16-1-list-incoming-letters.md] ExcelJS
  client-side extract precedent

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5), 2026-08-24.

### Debug Log References

- The copied `orphan-report-search` component was a UC-6.16 per-orphan timeline mock (dead
  `orphans` TODO, commented-out search, hardcoded English `alert()` strings) — replaced wholesale
  per Task 3's rebuild mandate, not patched.
- Both routes pointed at dead i18n keys (`orphanReports.search` does not exist in either locale)
  — pageTitle/breadcrumb were broken before this story.

### Completion Notes List

- **Grid gaps (spec columns with no source field on this stack, rendered "—"):** مشروع تنموى
  للمعيل (guardian development project), الاستبعاد + سبب الاستبعاد (exclusion pair). The
  columns are present with headers; the value accessors return '' so the grid shows "—". No
  columns were invented.
- **الصف الدراسي filter gap:** the filter key `EducationalStageId` exists on the DTO and the
  report entity (FK_EducationalStage), but there is **no EducationalStage lookup entity or
  endpoint on this stack** (only EducationLevel) — the drop-down has no options source, so the
  control is not rendered; المرحلة الدراسية ships via `getEducationLevels()`. Wireable without
  service changes once a stage lookup lands.
- **Column count honesty:** the spec prose says "38 columns" but its enumerated list has 37 —
  the enumerated list (spec order) is implemented.
- **And/Or scope:** the radio governs only the §14.S.4 status family; charity scope, dates and
  orphan pins always AND (a charity pin must never be OR'ed away — 15-1 defect-4 class).
- **Storage-token filters:** SchoolType/MedicalStatus/EducationDegree match the stored free-text
  Arabic values via Contains (legacy data is Arabic free text); MaritalStatus/EducationalStatus
  are stable English tokens translated server-side to flags — locale-independent wire values.
- **Export:** current *filtered* result via paged reads of 100 (the service's PageSize clamp),
  capped at 2000 rows — the 16-1 single-shot precedent cannot apply against a clamped endpoint.
- Live checks (And vs Or SQL-comparable counts, 401, charity pin) pending the user restarting
  their own IIROSA.Api; never killed by policy.

### File List

- `Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/PeriodicOrphanReportFilterDto.cs` (AndOr + 4 status keys)
- `Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/PeriodicOrphanReportListDto.cs` (wide-grid fields)
- `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` (status-family ApplyFilters, CombinePredicates + ParameterRebinder, wide projection)
- `Frontend/src/app/modules/periodic-orphan-reports/models/periodic-orphan-report.model.ts` (filter + list interfaces)
- `Frontend/src/app/modules/periodic-orphan-reports/orphan-report-search/orphan-report-search.component.ts/.html/.scss` (rebuilt to §14.S.4)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-orphan-reports-routing.module.ts` (live i18n keys + PermissionGuard)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-reports-list/periodic-reports-list.component.html` (حالة اليتيم entry link)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` (sidebar sub-entry)
- `Frontend/src/assets/i18n/ar.json` + `en.json` (`periodicReports.search.*`)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-09 and module spec §14.S.4 / §14.U.9; legacy review-endpoint realisation overridden; grid column honesty rule recorded. |
| 2026-08-24 | Implemented: status-predicate family with And/Or expression combine, wide 37-column projection (orphan/family/guardian flatten), §14.S.4 screen rebuilt with ExcelJS extract, sidebar + register reachability, i18n ar/en. Backend build 0 errors; frontend build + live checks pending. Status → review on green build. |
| 2026-08-24 | Verification pass: final gates run — `npm run build` GREEN (epic-9 module compiled; NG8107 optional-chain warnings only) and backend 0 errors for epic-9 code (the only 2 solution errors are the parallel epic-18 session’s in-flight untracked `ReportService.cs` — CS0019 ×2, left untouched per convention). Live API wedged (accepts TCP, empty replies) — restart pending; live walkthrough stays batched. Status ready-for-dev → review. |


### Review Findings (epic review 2026-08-24)

> Review Outcome 2026-08-24 — all items patched & verified: P63b recorded: AC4 zero-pages is met by hiding the pager (deviation documented here). P63c recorded: manual !IsDeleted predicates are a necessary, now-documented deviation from the platform rule (no global query filter exists).

- [x] [Review][Patch] P5 Unknown ReviewStatus token silently ignored — unfiltered register; validate, 400 [filter binding]
- [x] [Review][Patch] P47a toISOString off-by-one day for UTC+3 in fmtDate + Excel [orphan-report-search.component.ts:292]
- [x] [Review][Patch] P34 Excel export recursion no cancel — takeUntil [orphan-report-search.component.ts:1835]
- [x] [Review][Patch] P23 history + schedule companion routes lack PermissionGuard [routing:176,200]
- [x] [Review][Patch] P24 OrphanReports.Compare absent from PERMISSION_ROLES — hasPermission fails open [auth.service.ts:447]
- [x] [Review][Patch] P26 i18n: orphanReports.history/compare/schedule + breadcrumb.* routing keys + 59 template keys + common.exportExcel missing from BOTH locales [ar.json:3206/en.json:3207]
- [x] [Review][Patch] P27 Hard-coded English "Report ${n}" fallback [orphan-report-comparison.component.ts:989]
- [x] [Review][Patch] P28 toLocaleDateString without locale [orphan-report-comparison.component.ts:985]
- [x] [Review][Patch] P57b Dead code: viewReport + unused Router [orphan-report-search.component.ts:1795]
- [x] [Review][Patch] P63b Record pager deviation: AC4 zero-pages met by hiding pager (unrecorded)
- [x] [Review][Patch] P63c Record manual !IsDeleted deviation vs platform rule (necessary; no global filter)
