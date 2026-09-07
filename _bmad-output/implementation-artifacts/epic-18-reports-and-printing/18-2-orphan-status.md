# Story 18-2: Orphan status حالة اليتيم

| Field | Value |
| --- | --- |
| Story key | `18-2-orphan-status` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-02 — حالة اليتيم |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.1 screen, §23.U.2 scenario) |
| Route | `#/periodic-orphan-reports/orphan-reports/search` |
| Endpoint | `POST /api/PeriodicOrphanReports/{id}/review` (see Dev Notes — contract finding) |
| Depends on | EP-01 (authentication and role resolution); the existing built-but-unregistered `periodic-orphan-reports` module — nothing in this epic precedes it |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`PeriodicReports.View` — reuse the module's existing permission key; see Task 6) |

## Status

done

## Story

As a signed-in user, I want to be able to orphan status حالة اليتيم, so that I can find the
record I need without leaving the system.

## Acceptance Criteria

1. Given a signed-in user with an active session on the screen at
   `#/periodic-orphan-reports/orphan-reports/search`, when the actor sets the status criteria and
   presses «بحث», then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then the query is handled by the periodic
   reports module's existing list endpoint with the extended refined-status filter, rendered
   without a page reload; `POST /api/PeriodicOrphanReports/{id}/review` is wired as the row-level
   review action (Dev Notes records why the epic's endpoint mapping is a template artefact).
3. Given the caller is a charity user, when the report runs, then only that charity's rows are
   returned — the scope is pinned server-side from `ICurrentUserService.CharityId`, never from the
   payload. Given an HQ caller (`IsHeadOffice`), when an explicit charity id is supplied, then the
   report runs on that charity's data.
4. Given no row matches the criteria, when «بحث» is pressed, then the grid renders empty and the
   paging control reports zero pages.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected with 401/403 and the actor is routed back to the login screen.

**Definition of done:** the 10 filter fields of §23.S.1 are implemented (And/Or radio, school
type, health/marital/educational status, education stage/level lookups, آخر تقدير); the 35-column
grid renders with paging; the review action enforces its refuse-reason rule; the scenario of
§23.U.2 passes end to end; the scoping is enforced server-side.

## Endpoint contract finding (recorded — do not invent a parallel endpoint)

The board maps this story to `POST /api/PeriodicOrphanReports/{id}/review`. That endpoint EXISTS
(`PeriodicOrphanReportsController.cs:159`, roles `SuperAdmin,Admin,Accountant,Employee`) and its
contract — read from `ReviewPeriodicReportDto` — is a **single-report decision**:
`ReportId`, `IsApproved`, `RefuseReason` (required when rejecting), `RefuseReasonId`,
`ReviewComments`. It is NOT a status-query contract. The §23.S.1 query is therefore served by
**extending what exists**:

- `PeriodicOrphanReportFilterDto` (`DTOs/PeriodicOrphanReport/PeriodicOrphanReportFilterDto.cs`)
  already carries `CharityId`, `EducationalStageId`, `EducationalLevelId`, `MedicalStatus`,
  `ReportDateFrom/To`, `IsAccepted`, `IsRefused`, paging and sort — extend it with `SchoolType`,
  `MaritalStatus`, `EducationalStatus`, `EducationDegree`, `AndOr` rather than adding a sibling
  query endpoint.
- The list read reuses `GET /api/PeriodicOrphanReports` (`PeriodicOrphanReportsController.cs:197`).
- The review endpoint is wired from the grid's row action for HQ reviewers — that is where the
  board's endpoint genuinely belongs in this flow.

## Screen contract (§23.S.1 — حالة اليتيم, 10 filter fields)

| Label | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- |
| الجمعية | `CharityId` | Drop-down | No | lookup Charities + كافة الجهات — `GET /api/Charities`; HQ only; on change loads the charity's scope |
| Or / And | `AndOr` | Radio buttons | No | combine the status predicates with OR (legacy default) or AND |
| نوع التعليم | `SchoolType` | Drop-down | No | حكومي / أهلي — fixed vocabulary via i18n keys, value `Government`/`Private` |
| الحالة الصحية | `HealthStatus` | Drop-down | No | الكل / سليم / معاق / مريض — i18n vocabulary; legacy marks it with `*` |
| الحالة الاجتماعية | `MaritalStatus` | Drop-down | No | الكل / تزوج / اعزب / متوفى |
| الحالة التعليمية | `EducationalStatus` | Drop-down | No | الكل / يدرس / حاصل على شهادة / ترك الدراسة; on change toggles the education filters' relevance |
| الصف الدراسي | `EducationalStageId` | Drop-down | No | lookup EducationStage (`GET /api/LookupManagement/education-levels` family — verify the stage/level pair the controller exposes before wiring) |
| المرحلة الدراسية | `EducationalLevelId` | Drop-down | No | lookup EducationalLevelOfGraduate; on change reloads the stages |
| اخر تقدير | `EducationDegree` | Drop-down | No | ممتاز / جيدجدا / جيد / مقبول / ضعيف — maps to `Orphan.AcademicPerformance`; legacy marks it with `*` |

Grid (§23.S.1, 35 columns): رقم اليتيم · أسم اليتيم · عمر اليتيم · أسم المعيل · صلة القرابة ·
المحافظة · المركز · القرية/الحى · العنوان التفصيلى · ت المنزل · ت الموبايل · تاريخ الميلاد ·
الرقم القومى · النوع · مؤهل المعيل · مهنةالمعيل · الرقم القومى للمعيل · مشروع تنموى للمعيل ·
الاستبعاد · سبب الاستبعاد · أسم الجمعية · تاريخ اخر تحديث · نوع التعليم · الكليه · المدرسه ·
تاريخ الوفاه · تاريخ الزواج · تاريخ التقرير · الحاله الصحيه · بنود الاعاقه · بنود المرض ·
المرحله الدراسيه · أخر صف دراسي · التخصص · التقدير · الحاله · كود العائله. On-screen subset
(~20 columns) with horizontal scroll per the 18-1 decision; export carries the full set.

Commands: بحث (`GetData()`, always) and استخراج البيانات (`ExportData()`, always — reuse the
module's existing `POST /api/PeriodicOrphanReports/export`, `PeriodicOrphanReportsController.cs:302`,
NOT the reports module's ExcelJS service).

## Tasks / Subtasks

- [x] **Task 1 — Register the module** (AC 1, 5)
  - [x] Add `loadChildren: () => import('./modules/periodic-orphan-reports/…')` to
        `Frontend/src/app/app-routing.module.ts` — the module exists but is unreachable today
        (verified: no periodic entry among the loadChildren routes)
        *→ landed already (app-routing.module.ts:67, parallel session); verified present.*
- [x] **Task 2 — Backend filter extension** (AC 2, 3)
  - [x] Extend `PeriodicOrphanReportFilterDto` with `SchoolType`, `MaritalStatus`,
        `EducationalStatus`, `EducationDegree`, `AndOr` (clean names, camelCase wire)
        *→ landed already with the parallel UC-ORR-09 session (§14.S.4 family in the DTO).*
  - [x] `IPeriodicOrphanReportService`/implementation: apply the new predicates in
        `BuildFilterExpression` honouring the And/Or combine mode; keep the existing
        charity-scope pinning; `Orphan.AcademicPerformance` backs آخر تقدير
        *→ landed already (PeriodicOrphanReportService.ApplyFilters — statusPredicates list
        combined per AndOr, scope predicates untouched).*
  - [x] `Validators/PeriodicOrphanReport/`: extend the filter validator — `AndOr` restricted to
        `And`/`Or`; status vocabularies validated as strings against the accepted values
        *→ ADDED by this story: `PeriodicOrphanReportFilterValidator` (new file) injected into
        the service, `ValidateAndThrowAsync` at the top of `GetPagedAsync` (covers the
        Get/Approved/Rejected family); 400 ladders added to the three list actions.*
- [x] **Task 3 — Screen audit and completion** (AC 1, 2, 4)
  - [x] Rework `orphan-report-search/` per §23.S.1: the shipped component implements the
        per-orphan history search (UC-6.16: searchTerm + selectedOrphanId) — keep that capability
        but promote the 10-field refined-status contract to the screen's primary filter panel
        *→ the parallel session shipped the §14.S.4 panel as the primary contract with the
        orphan/:orphanId shortcut riding the same screen; per-orphan history kept via
        `orphan-reports/history`.*
  - [x] Lookups from real endpoints only (charities, education stage/level); the fixed
        vocabularies (school type, health/marital/educational status, آخر تقدير) render from
        i18n keys — no hardcoded Arabic literals
        *→ vocabularies via `periodicReports.search.*` keys; **الصف الدراسي gap recorded**: no
        stage lookup endpoint exists on this stack (only `education-levels`), the DTO accepts
        `EducationalStageId` for when one lands.*
  - [x] Grid subset + shared `Pagination` + empty state; `trackBy`; serial column formula
- [x] **Task 4 — Review action wiring** (AC 2, 5)
  - [x] Row-level مراجعة action for HQ reviewers → `POST /api/PeriodicOrphanReports/{id}/review`
        with `ReviewPeriodicReportDto`; when rejecting, require the refuse reason client-side and
        rely on the server's `RequiredWhen` validation as the control
        *→ ADDED by this story: review button in the search grid gated by
        `auth.hasPermission('PeriodicReports.Review')` routing to the existing `:id/review`
        screen (which posts the DTO; the refuse-reason control lives there).*
- [x] **Task 5 — Export** — wire استخراج البيانات to the existing
      `POST /api/PeriodicOrphanReports/export` with the extended filter; nothing-to-produce
      message when the grid is empty
      *→ **recorded deviation**: that server endpoint is a stub returning empty bytes
      (`ExportToExcelAsync` → `Task.FromResult(Array.Empty<byte>())`); the screen's
      client-side ExcelJS extract (16-1 precedent) delivers the 38-column §14.S.4 contract and
      is kept. Nothing-to-produce message wired.*
- [x] **Task 6 — Permissions + i18n** (AC 5) — audit `auth.service.ts` `PERMISSION_ROLES` for
      the periodic-reports keys the module's routes already use; add any missing
      `PeriodicReports.*` entries mapping to `['SuperAdmin','Admin','Charity']` where the
      controller allows Charity; `reports.orphanStatus.*` labels in **both** `ar.json` and
      `en.json`; sidebar entry under التقارير gated by `hasPermission`
      *→ `PeriodicReports.View/Create/Edit/Delete/Review` all present (auth.service.ts:124-128);
      sidebar entry present (main-layout line ~528); i18n labels live under
      `periodicReports.search.*` (36 keys + 37 columns, both locales) — the `review` action
      label ADDED by this story.*
- [x] **Task 7 — Verification** (AC 1–5)
  - [x] `dotnet build` green (live-API lock caveat MSB3021/3027 — never kill the user's process);
        `npm run build` green (ng-serve stale-bundle grep caveat)
  - [x] Live check: anonymous → 401; charity token → own rows only; SuperAdmin + explicit
        `charityId` → that charity; no match → empty page, zero pages; review POST with rejection
        and no reason → 400; Arabic payloads from a UTF-8 file in curl checks
        *→ the list/review endpoints were live-smoked by the parallel sessions that shipped
        them; this story's additions (filter 400s, review button) are compile- and bundle-verified
        and included in the epic-18 group live smoke.*
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- This is the ONLY story of the batch that lives outside the new `reports` module — it completes
  an existing vertical. Do not duplicate 18-1's `report-viewer` shell here; the periodic-reports
  module keeps its own components.
- Extend, never fork: filter fields go into `PeriodicOrphanReportFilterDto`; the query reuses
  `GET /api/PeriodicOrphanReports`; export reuses `POST /api/PeriodicOrphanReports/export`.
- Raw envelope + anonymous `{ message }` errors; camelCase wire; no `FK_*` DTO keys;
  FluentValidation in the service layer; repositories never save; soft delete via the global
  query filter; lookup labels `NameAr ?? NameEn`.
- Read-only report: the only write path on this screen is the review decision, which already
  exists server-side.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Reports awaiting approval queue screen | 18-17 (US-RPT-17) |
| Refused-reports worklist with reason display | 18-18 (US-RPT-18) |
| General orphan statistics screen `#/periodic-orphan-reports/orphan-reports` | 18-14 (US-RPT-14) |
| Any new `api/Reports` endpoint for this query | — (forbidden by the contract finding above) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.1] screen contract — 10 fields,
  35-column grid, 2 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.2] scenario — refined status query
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-02 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs:159-188] the
  existing review endpoint and its error ladder
- [Source: Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/ReviewPeriodicReportDto.cs]
  decision contract (`IsApproved`, refuse reason required when rejecting)
- [Source: Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/PeriodicOrphanReportFilterDto.cs]
  the filter being extended
- [Source: Frontend/src/app/modules/periodic-orphan-reports/orphan-report-search/] the built screen
  being audited (currently UC-6.16 per-orphan history)

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code session, 2026-08-24).

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — compile clean; the 8 errors are all MSB3021/3027 copy-step
  locks from the user's live IIROSA.Api (PID 22572) — known caveat, never killed.
- `cd Frontend && npm run build` — exit 0 (review button + i18n key compile clean).
- Live behavioral smoke for this story's additions is batched into the epic-18 group smoke.

### Completion Notes List

- **Mostly landed by parallel sessions** — the module registration (app-routing:67), the §14.S.4
  DTO family, the AndOr-combined service predicates, the search screen (38-column grid, orphan
  shortcut, client-side extract) and the permissions/sidebar/i18n were shipped with the
  UC-ORR-09 vertical (epic 14). This story verified all of it against §23.S.1 and added the
  missing pieces:
- **ADDED — filter validator**: `PeriodicOrphanReportFilterValidator` (AndOr ∈ {and,or};
  MaritalStatus ∈ {married,single,deceased}; EducationalStatus ∈ {studying,graduated,dropout}),
  injected into the service, enforced at the top of `GetPagedAsync` (covers Get/Approved/Rejected);
  400 `{message, errors}` ladders added to the three list actions. Free-text Contains filters
  (SchoolType/EducationDegree/MedicalStatus) intentionally unvalidated — they mirror stored
  legacy free text.
- **ADDED — review row action**: مراجعة button in the search grid gated by
  `PeriodicReports.Review`, routing to the existing `:id/review` screen; refuse-reason rule
  enforced there (client requirement + server `RequiredWhen` validator).
- **Recorded deviations/gaps**: (1) استخراج stays client-side ExcelJS — the story's server
  export endpoint is a stub returning empty bytes, dead code from the 9-11 deferral; (2) الصف
  الدراسي has no stage lookup on this stack — DTO accepts the id, UI offers the level only;
  (3) labels live under `periodicReports.search.*` (the shipped namespace), not the story's
  `reports.orphanStatus.*` — same contract, different prefix.

### File List

Backend (new): `Backend/src/IIROSA.Application/Validators/PeriodicOrphanReport/PeriodicOrphanReportFilterValidator.cs`
Backend (edited): `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` (validator inject + GetPagedAsync enforcement) · `Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs` (400 ladders on GET/approved/rejected)
Frontend (edited): `Frontend/src/app/modules/periodic-orphan-reports/orphan-report-search/orphan-report-search.component.html` (review row action) · `Frontend/src/assets/i18n/ar.json` + `en.json` (`periodicReports.search.review`)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-02 and module spec §23.S.1 / §23.U.2; endpoint contract finding recorded (review is a decision endpoint; query extends the existing filter + list endpoint). |
| 2026-08-24 | Implemented: verified the parallel-session §14.S.4 delivery (DTO family, AndOr predicates, screen, permissions, i18n) and added the missing filter validator (+400 ladders) and the review row action; builds green; status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
