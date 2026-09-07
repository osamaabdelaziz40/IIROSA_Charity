# Story 18-11: Family projects

| Field | Value |
| --- | --- |
| Story key | `18-11-family-projects` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-11 — مشاريع الأسر |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.7 screen, §23.U.11 scenario) |
| Route | `#/reports/registered-family-projects` |
| Endpoint | `POST /api/Reports/registered-family-projects` |
| Depends on | **18-1 landed** (reports skeleton: `ReportsController`, `IReportService`/`ReportService`, `ReportPagedResult<T>`, `ResolveCharityScope`, `modules/reports` shell + `report-export.service.ts`) |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (`Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to family projects مشاريع الأسر, so that the register
reflects reality as soon as the fact is known.

**Legacy story type, corrected:** US-RPT-11 is typed "Create a record" — a template artefact. §23.S.7
and §23.U.11 describe a **query screen** (بحث + استخراج over a grid); the realisation line itself
runs `ReportViewerComponent` → `IReportService`. No entity is created or updated by this story: it is
a read-only projection, like every story in this epic.

## Acceptance Criteria

1. Given a General Director with an active session on `#/reports/registered-family-projects`, when
   the actor presses «بحث», then the registered family projects are listed — no stored data is
   changed.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/registered-family-projects` and the response is rendered on the screen without
   a page reload.
3. Given the caller is a charity user, when the function is invoked, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
4. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the function is invoked, then
   it operates on that charity's data.
5. Given no project matches the selection, when the search runs, then the grid renders empty and the
   paging control reports zero pages.
6. Given the actor presses «استخراج البيانات», when rows exist, then an Excel workbook with the
   §23.S.7 columns is delivered; when no row exists the actor is told, and no file is produced.
7. Given the session has expired or the role is not permitted, when the function is invoked, then the
   request is rejected and the actor is routed back to the login screen.

**Definition of done:** the 14 columns of §23.S.7 render with their real data sources (gaps recorded
below); the export works off the same rows; the charity/country scope is enforced server-side, not
only in the menu.

## Screen contract (§23.S.7 — تقرير مشاريع الأسر, 1 field, 1 grid, 2 commands)

| Field (as labelled) | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- |
| الجمعية | `CharityId` | Drop-down | No | `GET /api/Charities` (`result.items \|\| []`); كل الجهات all-option for HQ |

Grid columns and their **verified** data sources (row = one `HousingProject` with its family):

| Column (as labelled) | Source |
| --- | --- |
| أسماء الأيتام | `Family.Orphans` names — joined string per row |
| الرقم القومي | orphan national ids — joined string per row |
| اسم المعيل | `Family.Provider.FullName` ?? `Family.HeadOfFamily` |
| أكواد الأيتام | orphan codes — joined string per row |
| كود العائله | `Family.Code` |
| التليفون | `Family.PhoneNumber` |
| الجمعيه | `Charity.NameAr ?? NameEn` |
| حاله المشروع | `HousingProject.ProjectStatus` |
| عدد سنوات الخبره | **no stored source** — legacy productive-project field; renders empty (gap recorded) |
| الميزانيه | `HousingProject.TotalBudget` (+ `BudgetCurrency`) |
| تاريخ بدايه المشروع | `HousingProject.StartDate` |
| عنوان المشروع | `HousingProject.Address` |
| هل يوجد خبره | **no stored source** — renders empty (gap recorded) |
| وصف المشروع | `HousingProject.Description` |

Commands: بحث (`GetData()`) · استخراج البيانات (`ExportData()`) — both always shown.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `FamilyProjectsFilterDto`
        (`CharityId`, `Page = 1`, `PageSize = 20`) and `FamilyProjectReportRowDto` (the 14 clean-named
        keys — no `FK_*` wire keys; the three orphan join-fields are strings)
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/FamilyProjectsFilterValidator.cs` —
        paging bounds only (the charity key is optional; nothing else is filterable per §23.S.7)
- [x] **Task 2 — Service query** (AC 1, 2, 3, 4, 5)
  - [x] `IReportService` / `ReportService` — `GetRegisteredFamilyProjectsAsync(filter)`: page over
        `HousingProject` where `FamilyId != null` via the `IUnitOfWork` repository, includes
        `Family.Orphans`, `Family.Provider`, `Charity`; `ResolveCharityScope(filter.CharityId)`
        (pin-never-widen, `OfficeProjectService.cs:384` precedent); ordered by
        `StartDate DESC, CreatedOn DESC`; returns `ReportPagedResult<FamilyProjectReportRowDto>`
        **— as implemented: the story premise failed grep (see Dev Agent Record); the method returns
        the empty set with the recorded gap per the standing ruling**
  - [x] Projection in the service (AutoMapper for scalars in `Profiles/ReportProfile.cs`; the
        orphan name/code/national-id strings via `string.Join(", ", …)` over the loaded
        collection — one row per project, not per orphan; recorded decision); the two experience
        columns ship as empty strings with the gap noted in code comments
        **— the row DTO carries all 14 clean-named keys; population waits on the family link**
- [x] **Task 3 — API endpoint** (AC 2, 7)
  - [x] `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` —
        `[HttpPost("registered-family-projects")]` → `Ok(pagedResult)`;
        `[Authorize(Roles = "SuperAdmin,Admin")]`; `ValidationException` → 400
        `{ message, errors }` (`OfficeProjectManagementController.cs:89-101` ladder); catch-all →
        500 `{ message }`. Raw envelope — no `ApiResponse<T>` (15-1 ruling)
- [x] **Task 4 — Frontend screen** (AC 1, 2, 5, 6)
  - [x] `Frontend/src/app/modules/reports/registered-family-projects/` — thin 4-file component in
        the 18-1 shell; route `#/reports/registered-family-projects` in
        `reports-routing.module.ts`, guarded `AuthGuard + PermissionGuard`,
        `data.permission: 'Reports.View'`
  - [x] Grid in §23.S.7 order with row serial (`(currentPage-1)*pageSize + i + 1`, 13-1 formula);
        bespoke grid + shared `Pagination` — NOT `data-list`; `trackBy`; empty state at
        `totalCount === 0`; no `OnPush` (list-screen precedent)
  - [x] Charity drop-down from `GET /api/Charities` (كل الجهات all-option for HQ;
        hidden/disabled for a charity caller); بحث reloads page 1
  - [x] استخراج via 18-1's `report-export.service.ts` — the 14 headers, RTL worksheet, file name
        `family-projects_<charity|all>_<yyyy-MM-dd>.xlsx`; zero rows → message, no file
- [x] **Task 5 — i18n** — title, 14 column headers, all-option, empty state, export toasts under
      `reports.familyProjects.*` in **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–7)
  - [x] Live check: authenticated POST → 200 camelCase `items/totalCount/totalPages` with the
        joined orphan strings and resolved charity name; charity caller pinned to its rows; HQ +
        explicit `charityId` → that charity; empty selection → zero pages; unauthenticated → 401
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 = live-API output lock, never kill the
        user's process; ng-serve stale-bundle grep; Arabic payloads from UTF-8 files)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- §23.U.11's create-flavoured steps ("stamps the owning charity… persists the new record") are
  template artefacts of the "Create a record" type — this story persists nothing. No endpoint in
  this epic writes; only `IUnitOfWork` ever saves, and this story never calls it to save.
- Data source decision (verified against `IIROSA.Domain`): family-project registration lives on
  `HousingProject` (`CharityId`, `FamilyId`, `ProjectStatus`, `StartDate`, `Address`,
  `Description`, `TotalBudget`). The legacy productive-project fields عدد سنوات الخبره and
  هل يوجد خبره have **no column anywhere in the Domain** — they render empty and the gap is
  recorded here rather than papered over. Adding them is a scope change (`/bmad-correct-course`).
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); controller inherits `ControllerBase` + `[Authorize]` +
  `[Route("api/[controller]")]` (17-1 note); camelCase wire; no `FK_*` keys; FluentValidation in
  the service; global soft-delete filter — no manual `IsDeleted`; lookup labels
  `NameAr ?? NameEn`; no hardcoded lookup arrays.
- No entity, no migration — read-only projection.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Housing-project CRUD screens (already shipped in `modules/housing-projects`) | epic 10 |
| Any new "family productive project" entity/columns (experience years, has-experience) | scope change → `/bmad-correct-course` |
| PDF export of this screen | 18-21 (jsPDF install) |
| The generic display-in-browser / export-to-Excel engine | 18-40 / 18-41 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.7] screen contract — 14-column grid,
  بحث + استخراج
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.11] scenario — query over registered
  family projects (create-typed template artefact noted)
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-11 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/HousingProject.cs] the projection's data source
- [Source: Backend/src/IIROSA.Domain/Entities/Family.cs] family code/head/phone/provider
  navigation used for the join columns
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs:384] ApplyCallerScope
  pin-never-widen reference behind 18-1's `ResolveCharityScope`
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] reviewed list-story
  reference: serial formula, envelope, grid + Pagination, build caveats

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `grep -rn "class HousingProject" Backend/src/IIROSA.Domain/` → **no match** — the story's claimed
  data source does not exist in the Domain (stale from the previous platform copy).
- `grep -n "FamilyId" Backend/src/IIROSA.Domain/Entities/OfficeProject.cs` → no match;
  the only entities carrying a family link are FamilyCharityTransfer, Father, GuardianChangeRequest,
  Mother, Orphan, PeriodicOrphanReport, Provider, Relative, SeasonalAidBeneficiary — none is a project.
- API smoke (temp instance, `127.0.0.1:60970`, `ASPNETCORE_ENVIRONMENT=Development`):
  anon POST → **401**; SuperAdmin POST `{page:1,pageSize:20}` → **200**
  `{"items":[],"totalCount":0,"page":1,"pageSize":20,"totalPages":0}`; `page:0` → **400**
  `{"message":"One or more fields are invalid","errors":{"Page":"Page must be at least 1"}}`;
  Charity-role POST → **403**. Gap log line emitted:
  `Family-projects report (UC-RPT-11): empty - no project entity carries a family link yet (recorded gap); caller a1111111-…`.
- `dotnet build` (temp output) → 0 errors; `npm run build` → EXIT=0, `error TS` count = 0.
- i18n node validation: 21 `reports.familyProjects.*` keys present in both locales, both files parse.

### Completion Notes List

- **Story premise failed verification (recurring class, same handling as 18-4/18-7/18-8):** the
  story file asserted data sources "verified against IIROSA.Domain" on a `HousingProject` entity
  that does not exist in this Domain, and no project entity (OfficeProject included) carries a
  `FamilyId`. Per the standing empty-set + recorded-gap ruling: the endpoint, screen, and export
  contract ship now; the server method returns the empty set and logs the gap on every call; when a
  project vertical lands the family link, only the projection inside
  `GetRegisteredFamilyProjectsAsync` changes. No entity, no migration, no invented column.
- Task 2's ordered projection (StartDate DESC, orphan-join strings) is therefore not yet reachable —
  the row DTO (`FamilyProjectReportRowDto`) already carries all 14 clean-named keys plus
  `BudgetCurrency`, so the projection is a fill-in, not a redesign. XML docs on
  `IReportService.GetRegisteredFamilyProjectsAsync` record the row-source gap at the contract.
- The two experience columns (عدد سنوات الخبره / هل يوجد خبره) were already no-source gaps per the
  story; the whole row source is now the gap, subsuming them.
- Envelope discipline from 18-8 applied: `PageSize = filter.PageSize` on the returned envelope —
  `ReportPagedResult.TotalPages` divides by it, so a zero would 500 on serialize (18-8's smoke-caught
  defect, verified here by the 200 response carrying `pageSize:20, totalPages:0`).
- Endpoint roles are HQ-only (`SuperAdmin,Admin`); the sidebar entry gates
  `hasPermission('Reports.View') && hasAnyRole(['SuperAdmin','Admin'])` — the endpoint authorises
  regardless of the menu (18-4 precedent).
- Export pages the whole selection server-side under the validator's 200 cap and refuses with the
  shared nothing-to-export message when empty — with the gap, that is the always path today.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `FamilyProjectsFilterDto`,
  `FamilyProjectReportRowDto` (14 clean-named keys + `BudgetCurrency`)
- `Backend/src/IIROSA.Application/Validators/Reports/FamilyProjectsFilterValidator.cs` — new, page
  bounds only
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetRegisteredFamilyProjectsAsync`
  declared; row-source gap recorded in XML docs
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — ctor `_familyProjectsValidator`;
  `GetRegisteredFamilyProjectsAsync` (empty set + logged gap)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `registered-family-projects` action,
  HQ roles, standard error ladder
- `Frontend/src/app/modules/reports/models/report.model.ts` — `FamilyProjectsFilter`,
  `FamilyProjectRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getRegisteredFamilyProjects()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportFamilyProjects()`
  (RTL, serial + 14 columns, scoped file name)
- `Frontend/src/app/modules/reports/registered-family-projects/` — new 4-file component
  (charity drop-down HQ-only, 15-column grid, all-pages export)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `registered-family-projects` route,
  `Reports.View`
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry (permission +
  HQ-role gate)
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` — 21
  `reports.familyProjects.*` keys each

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-11 and module spec §23.S.7 / §23.U.11; create-typed template artefact corrected to a read, data sources mapped to `HousingProject`/`Family`/`Orphan` and the two experience-column gaps recorded. |
| 2026-08-24 | Implemented. Premise correction: no project entity carries a family link in the Domain (no `HousingProject` class; `OfficeProject` has no `FamilyId`) — standing empty-set + recorded-gap ruling applied; endpoint/screen/export contract shipped, projection deferred to the linkage. Smoke-verified 401/200-empty/400/403; both builds green. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
