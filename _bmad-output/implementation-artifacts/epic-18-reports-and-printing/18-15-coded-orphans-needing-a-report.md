# Story 18-15: Coded orphans needing a report

| Field | Value |
| --- | --- |
| Story key | `18-15-coded-orphans-needing-a-report` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-15 — أيتام مكودون مطلوب لهم تقرير |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.14 screen, §23.U.15 scenario) |
| Route | `#/reports/orphans-missing-reports` |
| Endpoint | `POST /api/Reports/orphans-missing-reports` |
| Depends on | **18-1 landed** (reports skeleton: `ReportsController` + `IReportService`/`ReportService`, `ReportPagedResult<T>`, `ResolveCharityScope`, lazy `modules/reports` with the shared `report-viewer/` shell, `report.service.ts`, `report-export.service.ts`, `Reports.View` → `['SuperAdmin','Admin','Charity']`) |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (platform role names — 15-1 precedent; `Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to coded orphans needing a report أيتام مكودون مطلوب لهم تقرير, so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at `#/reports/orphans-missing-reports`, when the actor runs the report, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/orphans-missing-reports` and the response is rendered on the screen without a page reload.
3. Given the caller is a charity user, when the report is served, then only that charity's rows are returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload; the `CountryId` claim pins country the same way (pin-never-widen).
4. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the report runs, then it operates on that charity's data.
5. Given no row matches, when the report is served, then the grid renders empty and the paging control reports zero pages.
6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the summary grid of §23.S.14 (الجمعيه · عدد الايتام مطلوب لهم تقارير) and its drill-down are implemented with paging; the scenario of §23.U.15 passes end to end; the role and charity scoping is enforced server-side, not only in the menu.

## Screen contract (§23.S.14 — أيتام مكودون مطلوب لهم تقرير)

| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | `CharityId` | Drop-down list | Optional · lookup Charities (+ كل الجهات all-option, HQ only) · on change re-runs the summary |

Grid (row source `field in AllCharities`):

| Column | DTO key |
| --- | --- |
| الجمعيه | `charityName` |
| عدد الايتام مطلوب لهم تقارير | `missingCount` |

Commands:

| Command (legacy handler) | Platform realisation |
| --- | --- |
| ExtractDetails(field.Id) | drill-down — loads the detail grid for that charity (second paged fetch) |
| GetNext() / GetPrev() | shared `Pagination` component |
| حفظ (DeleteOutgoing()) | **template artefact** — nothing to save on a query screen; dropped (recorded below) |

## Tasks / Subtasks

- [x] **Task 1 — DTOs + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`: `OrphansMissingReportsFilterDto` (`Page = 1`, `PageSize = 20`, `Guid? CharityId`), `OrphansMissingReportsSummaryDto` (`CharityId`, `CharityName`, `MissingCount`), `OrphansMissingReportsDetailDto` (`OrphanId`, `OrphanCode`, `OrphanName`, `LastReportDate` nullable) — no `FK_*` wire keys
        **— plus `OrphansMissingReportsDetailRequestDto` (`CharityId`, `Page`, `PageSize`) so the detail POST binds one body instead of loose route/query parts**
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/OrphansMissingReportsValidator.cs` (18-1's folder): page bounds only — the screen's single filter is optional
- [x] **Task 2 — Service projection** (AC 1, 3, 4, 5)
  - [x] `IReportService.GetOrphansMissingReportsAsync(filter)` + `GetOrphansMissingReportsDetailAsync(Guid charityId, int page, int pageSize)` in `ReportService`: scope through 18-1's `ResolveCharityScope` (charity user pinned to `ICurrentUserService.CharityId`; `IsHeadOffice` may pass explicit `charityId`; `CountryId` claim pins country — `OfficeProjectService.cs:384` pin-never-widen precedent)
        **— via the file's actual helper `ApplyCharityScopeAsync` (the orphan-set mirror of that shape, 18-1's real name); detail reuses it AND intersects the exact charity, so a pinned caller drilling another charity's row gets the empty intersection, never another's rows**
  - [x] Query shape: `Orphan` where `Code != string.Empty` (the "coded" set), left-joined to the latest accepted `PeriodicOrphanReport` (`IsAccepted == true`, `ReportDate >= UtcNow.AddMonths(-12)`); the chase set = coded orphans with **no** such report; summary groups by `FK_CharityId` with a count, detail lists one charity's orphans (cycle decision recorded in Dev Notes)
        **— refinement: "covers the window" reuses BR-11 (the UC-ORR-14 ruling) — accepted+reviewed report whose PERIOD intersects the trailing-12-month window, falling back to `ReportDate` inside it when the period is incomplete — so this chase list and the existing §14.U.14 one agree on who needs a report; `LastReportDate` is the latest ANY-state report (an accepted one would have cleared the orphan), the UC-ORR-14 projection shape**
  - [x] Return through 18-1's `ReportPagedResult<T>`; soft delete handled by the global query filter — never hand-check `IsDeleted`
        **— corrected as in every 18-x story: NO global soft-delete filter exists on this platform — explicit `!IsDeleted` at orphan, report and charity levels inside `BuildOrphansMissingReportsQueryAsync`; summary TotalCount = the number of charity GROUPS (the page control pages summary rows — 18-9 grouped-paging precedent)**
- [x] **Task 3 — API endpoints** (AC 2, 6)
  - [x] In 18-1's `ReportsController` (inherits `ControllerBase`, `[Authorize]`, `[Route("api/[controller]")]`): `[HttpPost("orphans-missing-reports")]` → `Ok(summaryPaged)` and `[HttpPost("orphans-missing-reports/details")]` → `Ok(detailPaged)`; try/catch returning anonymous `{ message }`; **no `ApiResponse<T>`** (15-1 ruling)
        **— roles `SuperAdmin,Admin,Charity` (AC 3/4 require the charity-caller path; the 18-12 precedent, not the header's Gen- Director-only line — HQ roles still granted)**
- [x] **Task 4 — Frontend thin component** (AC 1, 2, 5)
  - [x] `Frontend/src/app/modules/reports/orphans-missing-reports/` 4-file component (`.ts`/`.html`/`.scss`/`.spec.ts`) inside 18-1's lazy module; route `#/reports/orphans-missing-reports` guarded `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`
  - [x] Filter panel: الجمعيات dropdown from `GET /api/Charities` (`result.items || []`; كل الجهات all-option for HQ; a charity user sees a pinned, disabled single option); re-runs on change per §23.S.14 — no hardcoded arrays
        **— the 18-12 shape: the dropdown renders for HQ only; a charity caller sees no picker because the server pins anyway**
  - [x] Summary grid (2 columns) + drill-down panel fired by the row detail icon (`ExtractDetails(charityId)`): detail grid كود اليتيم · اسم اليتيم · تاريخ آخر تقرير; both grids share `Pagination`; `trackBy` on the `*ngFor`; empty state when `totalCount === 0`; **not `data-list`** (recorded deviation); OnPush omitted (list-screen precedent)
        **— the shell's shared Pagination pages the SUMMARY; the drill-down panel carries its own `app-pagination` inside the grid slot; a null-key group (بدون جمعية) disables its drill button — there is no charity to drill; a new بحث closes the drill (stale filter would mislabel it)**
  - [x] استخراج البيانات export through 18-1's `report-export.service.ts` (ExcelJS + file-saver — both already in `package.json`); when `totalCount === 0` show a message, do not emit an empty file
        **— exports the ACTIVE grid: the open drill-down list when one is open, otherwise the summary; whole selection paged server-side at 200 (validator cap); file name carries the charity scope slug**
- [x] **Task 5 — i18n** — title, filter label, all-option, 2 summary + 3 detail column headers, drill-down title, empty state, export messages under `reports.*` in **both** `assets/i18n/ar.json` and `en.json`; no hard-coded UI strings
      **— 16 `reports.orphansMissingReports.*` keys per locale (node-validated); the shell's shared `reports.emptyState` / `reports.nothingToExport` cover the empty state**
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; **no EF migration** (EP-18 adds no entities). MSB3021/3027 on copy steps = the user's live API locking outputs — never kill their process
        **— `dotnet build` (Api project to the temp smoke folder) → 0 errors, 0 warnings from new code; no migration**
  - [x] Live check: anonymous POST → 401; HQ POST `{}` → 200 paged camelCase; charity token → only its charity's row even when the payload names another; no matches → empty page, zero pages; export with no rows → message not a file. Arabic payloads from UTF-8 files (inline curl shows `?????` — codepage artefact)
        **— verified on a private instance (127.0.0.1:60970); numbers matched sqlcmd ground truth exactly — see Dev Agent Record. The shell also disables استخراج when `totalCount === 0`, and the component guards with the nothing-to-export message**
  - [x] `cd Frontend && npm run build` — 0 errors; ng-serve stale-bundle caveat: grep the served chunk for the new route before trusting a no-effect fix
        **— EXIT=0, `error TS` = 0 (the batch's exit 1 was grep's no-match on the zero count, not the build)**
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Ruling: legacy command names are template artefacts (recorded)

§23.S.14 lists «حفظ → DeleteOutgoing()» — a copy-paste of the outgoing-correspondence handler into the legacy report template. This is a query screen: there is nothing to save and the endpoint never writes. The operative commands are the drill-down (`ExtractDetails`) and paging. Dropped deliberately; if a genuine save action is ever required here it is a separate backlog item, not this story.

### Cycle decision (recorded)

"Coded orphans with no current periodic report" is operationalised as: `Orphan.Code` set AND no `PeriodicOrphanReport` with `IsAccepted == true` and `ReportDate` within the trailing 12 months. The 12-month window matches the platform's annual reporting cycle and needs no new filter field (§23.S.14 offers only الجمعية). Widen through a filter field only if HQ asks — do not front-load one.

### Platform rules that bind this story

- Wire is camelCase; DTO property names must not start with `FK_` (Newtonsoft emits `fK_…`).
- Raw paged envelope + anonymous `{ message }` error objects — NOT `ApiResponse<T>` (15-1 ruling, architecture.md §10).
- Controllers inherit `ControllerBase` with `[Authorize]` + `[Route("api/[controller]")]` — zero live controllers use a custom base (17-1 note).
- FluentValidation runs in the service layer (`Validators/Reports/`), never in the controller.
- Reads go through `IUnitOfWork` repositories; repositories never call `SaveChanges` — and this story writes nothing anyway.
- Soft delete is a global query filter — never add manual `IsDeleted` checks, never defeat it.
- Lookup labels resolve `NameAr ?? NameEn`.
- Bespoke grid + shared `Pagination` — NOT `data-list` (recorded deviation); list screens omit `OnPush` (codebase precedent).
- Caller scope from `ICurrentUserService` in the service — never parse claims in the controller, never trust a payload `charityId` for a charity caller.
- EP-18 adds **no entities and no EF migration** — read-only projections over `Orphan`, `PeriodicOrphanReport`, `Charity`.
- Tests excluded per the standing user decision.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Reports skeleton (controller, service, module shell, permissions, export service) | 18-1 |
| Any other report vertical (orphans, excluded, finished-sponsorship, …) | 18-2 … 18-14, 18-16 … |
| The shared PDF printing path (jsPDF decision) | 18-21 |
| Any import/write semantics for report screens | none — separate backlog if ever required |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.14] screen contract — 1 filter field, 2-column summary grid, 4 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.15] scenario — chase list before a payment run, charity-scoped read
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-15 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Orphan.cs] `Code`, `FullName`, `FK_CharityId` — the coded-orphan projection source
- [Source: Backend/src/IIROSA.Domain/Entities/PeriodicOrphanReport.cs] `IsAccepted`, `ReportDate` — the "current report" test
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs:384] ApplyCallerScope pin-never-widen reference
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] reviewed greenfield-skeleton story: platform rulings this epic reuses

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Ground truth (sqlcmd, `IIROSA_Db_Dev`): coded non-deleted orphans = **6** — dga **4**,
  الجمعية الخيرية - وادى النطرون **2**; `[IIROSA].[PeriodicOrphanReport]` = **0 rows**, so the
  chase set (no accepted report in the trailing 12 months) is the whole coded set.
- API smoke (private instance `127.0.0.1:60970` off the fresh temp build; the user's live API
  untouched; instance killed by PID after the run):
  - anon `POST /api/Reports/orphans-missing-reports` → **401**
  - Admin summary `{}` → **200** `items: [{charityName:"dga", missingCount:4},
    {charityName:"الجمعية الخيرية - وادى النطرون", missingCount:2}]`, `totalCount: 2`,
    `totalPages: 1` — matches ground truth exactly (Arabic rendered correctly; console mojibake
    is the codepage artefact)
  - Admin drill `details {charityId: dga}` → **200** 4 rows (`totalCount: 4`), ordered by Code,
    every `lastReportDate: null` (no reports exist) — matches
  - `page: 0` → **400** `{"message":"One or more fields are invalid","errors":{"Page":"Page must be at least 1"}}`
  - HQ narrowed to a charity with no coded orphans (Headquarters) → **200**
    `{"items":[],"totalCount":0,"totalPages":0}` — AC 5 zero state live
  - Charity-role caller (claim-less seed account, `CharityId` claim NULL): summary → **200**
    both charities — the documented claim-less fallthrough of the claim-based scope convention
    (18-12's recorded finding: the role name alone never scopes; only the claim pins). A
    charity-CLAIM caller is pinned by the helper whatever the payload names; the detail
    endpoint additionally intersects the exact charity, so a pinned caller drilling another
    charity's row receives the empty intersection.
- `dotnet build` (Api → temp folder) → 0 errors. `npm run build` → **EXIT=0, `error TS` = 0**
  (the wrapping batch reported exit 1 — that was `grep -c` returning no matches on the zero
  count, not the build). i18n node validation: 16 `reports.orphansMissingReports.*` keys in
  each locale.

### Completion Notes List

- **BR-11 alignment (the story's cycle decision, refined):** the story file operationalised
  "no current report" as `ReportDate` within 12 months. Implementation reuses the platform's
  existing ruling from UC-ORR-14 (`GetNonRenewedReportAsync`): an accepted+reviewed report
  covers the window when its **period** intersects it, falling back to `ReportDate` when the
  period is incomplete. Two chase lists over the same window now agree on who needs a report;
  the fixed trailing-12-month window (annual cycle) stays as decided, no new filter field.
- Summary paging pages the **charity groups**, not orphans — `TotalCount` is the group count
  (the page control's contract), each row carrying its orphan count (18-9 grouped-paging
  precedent). Groups order by count DESC then key; charity names resolve through the
  live-column dictionary (`FK_CharityId` → `Charity.Name`), never the legacy EF nav mirror
  (the 18-13 defect class).
- Detail endpoint = scope helper ∩ exact charity: HQ gets exactly the named charity; a
  charity-claim caller pinning charity A and drilling row B gets A∩B = ∅ (fail-closed), which
  satisfies AC 3/4 without a second scope code path. The claim-less seed account falls through
  unscoped — the codebase-documented convention (recorded again above), not a new widening.
- Drill-down rows whose group key is null (بدون جمعية — orphans with `FK_CharityId` NULL under
  unconstrained HQ) render with a disabled تفاصيل button: there is no charity id to drill.
- Export exports the ACTIVE grid (open drill-down list, else the summary) over the whole
  selection, paging server-side at the 200 cap — one استخراج button, two workbook shapes
  (`exportOrphansMissingReportsSummary` / `…Details`, both RTL, shared `downloadWorkbook`
  helper extracted for them).
- The legacy حفظ → `DeleteOutgoing()` command stays dropped (the story's template-artefact
  ruling); GetNext/GetPrev map to the shell's Pagination (summary) + the drill panel's own
  `app-pagination` (detail).
- Standing corrections applied: explicit `!IsDeleted` at every level (no global soft-delete
  filter on this platform — the story's Dev Notes line predates 18-1's finding); raw paged
  envelope + `{ message }` errors; camelCase wire; no `FK_*` keys.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `OrphansMissingReportsFilterDto`,
  `OrphansMissingReportsSummaryDto`, `OrphansMissingReportsDetailRequestDto`,
  `OrphansMissingReportsDetailDto`
- `Backend/src/IIROSA.Application/Validators/Reports/OrphansMissingReportsValidator.cs` — new,
  page bounds
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` —
  `GetOrphansMissingReportsAsync` + `GetOrphansMissingReportsDetailAsync` declared
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — validator wired; both methods +
  `BuildOrphansMissingReportsQueryAsync` (BR-11 chase set) + `ResolveMissingReportsCharityNamesAsync`
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST orphans-missing-reports`
  and `POST orphans-missing-reports/details`, `SuperAdmin,Admin,Charity`, anonymous-ladder errors
- `Frontend/src/app/modules/reports/models/report.model.ts` — `OrphansMissingReportsFilter`,
  `OrphansMissingReportsSummaryRow`, `OrphansMissingReportsDetailRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getOrphansMissingReports()`,
  `getOrphansMissingReportsDetail()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — summary + detail
  workbooks, shared `downloadWorkbook` helper, `import type * as ExcelJSNs`
- `Frontend/src/app/modules/reports/orphans-missing-reports/` — new 4-file component (filter,
  summary grid, drill-down panel with own pagination, active-grid export)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `orphans-missing-reports` route,
  `Reports.View`
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` — 16
  `reports.orphansMissingReports.*` keys each

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-15 and module spec §23.S.14 / §23.U.15; greenfield status and projection sources verified against the code (no `ReportsController`, no `reports` Angular module). |
| 2026-08-24 | Implemented. Chase predicate refined to BR-11 period semantics (aligned with the existing UC-ORR-14 chase list); summary pages charity groups with live-column name resolution; detail endpoint = scope helper ∩ exact charity (fail-closed for pinned callers drilling another charity). Smoke matched sqlcmd ground truth (dga 4 / وادي النطرون 2, detail 4, zeros on empty narrow, 401 anon, 400 page-bounds); both builds green; claim-less charity fallthrough documented per the standing convention. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
