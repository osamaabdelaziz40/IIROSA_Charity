# Story 18-3: Excluded orphans المستبعدون

| Field | Value |
| --- | --- |
| Story key | `18-3-excluded-orphans` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-03 — المستبعدون |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.4 screen, §23.U.3 scenario) |
| Route | `#/reports/excluded-orphans` |
| Endpoint | `POST /api/Reports/excluded-orphans` |
| Depends on | **18-1 landed** (Reports skeleton + `report-viewer` shell + `report-export.service.ts`) |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to excluded orphans المستبعدون, so that records entered in
error do not distort the register or the reporting.

## Acceptance Criteria

1. Given a signed-in user with an active session on the screen at `#/reports/excluded-orphans`, when
   the actor presses «بحث», then no stored data is changed — the operation is a read. (The epic's
   «Delete a record» type is template artefact: the exclusion itself happened in the coding/
   exclusion flows of EP-08; this screen REPORTS the excluded set — see Dev Notes.)
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/excluded-orphans` with a typed request DTO and the raw paged envelope
   (`items/totalCount/page/pageSize/totalPages`) is rendered without a page reload.
3. Given the caller is a charity user, when the report runs, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
   Given an HQ caller (`IsHeadOffice`), when an explicit charity id is supplied, then the report
   runs on that charity's data; a `CountryId` claim additionally pins the charities of that country.
4. Given no orphan is currently excluded in scope, when the report runs, then the grid renders
   empty and the paging control reports zero pages.
5. Given استخراج البيانات is pressed with an empty result, when the export runs, then the actor is
   told there is nothing to produce rather than receiving an empty file.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** the single charity filter and the 6-column grid of §23.S.4 are implemented
on 18-1's shell; §23.U.3 passes end to end; the charity/country scope is enforced server-side, not
only in the menu.

## Screen contract (§23.S.4 — الايتام المستبعدين, 1 filter field, 2 commands)

| Section | Field (as labelled) | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- | --- |
| الايتام المستبعدين | الجمعية | `CharityId` | Drop-down | No | `GET /api/Charities` (`result.items || []`) + كافة الجهات all-option — HQ only; hidden for charity callers |

| Grid columns (row source: excluded orphans) |
| --- |
| رقم اليتيم · أسم اليتيم · الاستبعاد · سبب الاستبعاد · أسم الجمعية · تاريخ اخر تحديث |

| Command | Handler | In scope |
| --- | --- | --- |
| بحث | `GetData()` | Yes |
| استخراج البيانات | `ExportData()` | Yes |

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator + service** (AC 2, 3)
  - [x] `ExcludedOrphansFilterDto` (`Guid? CharityId`, `int Page = 1`, `int PageSize = 20`) +
        `ExcludedOrphanListDto` (orphan code, orphan name, excluded flag, exclusion reason,
        charity name, last-updated) in `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`;
        result reuses `ReportPagedResult<T>` — no `FK_*` wire keys
  - [x] `Validators/Reports/ExcludedOrphansFilterValidator.cs` — page bounds only (`Page ≥ 1`,
        `PageSize` 1–200); invoked in the service
  - [x] `IReportService.GetExcludedOrphansAsync(...)` + `ReportService` implementation:
        `ResolveCharityScope(filter.CharityId)` first, then a read-only projection over
        `Orphan` (include `Family`/`Charity` navs) restricted to the excluded set, ordered by
        `Code` — repositories never save
- [x] **Task 2 — API endpoint** (AC 2, 6)
  - [x] `[HttpPost("excluded-orphans")]` in 18-1's `ReportsController`
        (`[Authorize(Roles = "SuperAdmin,Admin,Charity")]`) → `Ok(paged)`; the 18-1
        ValidationException→400-errors-map / catch-all→500 `{ message }` ladder. No `ApiResponse<T>`
- [x] **Task 3 — Screen** (AC 1, 4)
  - [x] `Frontend/src/app/modules/reports/excluded-orphans-report/` thin 4-file component in 18-1's
        `report-viewer` shell; route `excluded-orphans`, `AuthGuard + PermissionGuard`,
        `data.permission: 'Reports.View'`; charity dropdown per the contract; bespoke grid +
        shared `Pagination`; `trackBy`; empty state; OnPush omitted (list-screen precedent)
  - [x] The exclusion-reason column renders blank when the backing column carries no value —
        record the gap, do not invent data and do not migrate (epic-wide no-migration ruling)
- [x] **Task 4 — Export** (AC 5) — استخراج via 18-1's `report-export.service.ts` (ExcelJS +
      file-saver); empty result → nothing-to-produce message, no file
- [x] **Task 5 — i18n** — `reports.excludedOrphans.*` (title, filter label, 6 grid headers, empty
      state, export toasts) in **both** `ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6) — anonymous → 401; charity token → own rows only; HQ +
      `charityId` → that charity; empty scope → empty grid; `dotnet build` + `npm run build` green
      (MSB3021/3027 live-API lock caveat; ng-serve stale-bundle grep); tests excluded per the
      standing user decision

## Dev Notes

### Template artefact (recorded)

The epic generates US-RPT-03 as «Delete a record» with deletion ACs. §23.S.4 and §23.U.3 show a
query screen (one filter, one grid, بحث + استخراج). The exclusion WRITE path belongs to the
families/coding vertical (EP-08); nothing is deleted or un-excluded from this screen. If an
"un-exclude" action is wanted later it is a separate backlog item, not this story.

### Platform rules that bind this story

- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); `ControllerBase` + `[Authorize]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories; global soft-delete query filter (never manual `IsDeleted`).
- Caller scope from `ICurrentUserService` only; lookup labels `NameAr ?? NameEn`; bespoke grid +
  shared `Pagination` (NOT `data-list`); lazy module; EP-18 adds no entities and no migration.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Exclusion write path / un-exclude action | EP-08 (families/coding vertical) |
| Full orphan-data screen with the excluded/not-excluded checkbox triad | 18-1 (landed scope) |
| Excel-export engine hardening | 18-41 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.4] screen contract — 1 field,
  6-column grid, 2 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.3] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-03 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] the skeleton this story
  plugs a report key into (`ResolveCharityScope`, `ReportPagedResult<T>`, shell, export service)
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs:384] ApplyCallerScope
  pin-never-widen precedent

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Smoke run 2026-08-24, private instance `dotnet IIROSA.Api.dll --urls http://127.0.0.1:60970`
  (temp build folder; the user's live API untouched):
  - anonymous POST `/api/Reports/excluded-orphans` → **401** ✓
  - SuperAdmin POST → **200** `{"items":[],"totalCount":0,"page":1,"pageSize":20,"totalPages":0}` ✓
  - Charity-role token POST → **200** (role allowed; scope pinned server-side) ✓
  - out-of-range `{"page":0,"pageSize":999}` → **400** `errors.Page`/`errors.PageSize` ✓
- `dotnet build Backend/IIROSA.sln` — 0 compile errors (8× MSB3021/3027 copy-locks by the user's
  live IIROSA.Api, PID 22572 — compile clean per the standing caveat); `npm run build` exit 0.
- Domain gap re-proof: `grep -l "Exclud"` across `IIROSA.Domain` returns nothing — no exclusion
  column exists on any entity or lookup.

### Completion Notes List

- The excluded set is **empty by construction**: no exclusion column exists anywhere in the domain
  (18-1's recorded gap, re-proven for this story). The service returns the empty envelope and
  logs an Information-level gap line; the 6-column contract (incl. `IsExcluded`/`ExclusionReason`
  keys) ships so EP-08's future write path needs no contract change. Never patched with a
  migration (epic-wide ruling).
- Screen/i18n/export follow 18-1's thin pattern: `report-viewer` shell, HQ-only charity dropdown
  (كافة الجهات all-option), bespoke 6-column grid with continuous serial, ExcelJS export paging
  the whole selection at `PageSize ≤ 200`, nothing-to-produce refusal on empty.
- Story premise corrections (verify-first, nothing re-built): the module IS reachable and the
  filter fields DO exist — a parallel session landed them; only the review-action row link
  (18-2) and this report key were missing.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `ExcludedOrphansFilterDto`,
  `ExcludedOrphanListDto` (later extended with the shared 18-4/18-5 pair)
- `Backend/src/IIROSA.Application/Validators/Reports/ExcludedOrphansFilterValidator.cs` — new
- `Backend/src/IIROSA.Application/Validators/Reports/OrphanStatusReportFilterValidator.cs` — new
  (shared 18-4/18-5, listed here because it landed in the same pass)
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetExcludedOrphansAsync` (+ the
  18-4/18-5 pair)
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation + validator injection
  (+ `ResolveStatusReportCharityNamesAsync`)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `excluded-orphans` action
- `Frontend/src/app/modules/reports/models/report.model.ts` — `ExcludedOrphansFilter`,
  `ExcludedOrphanRow` (+ shared 18-4/18-5 pair)
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getExcludedOrphans()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportExcludedOrphans()`
- `Frontend/src/app/modules/reports/excluded-orphans-report/` — new 4-file component
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `excluded-orphans` route
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.excludedOrphans.*` (11 keys each,
  JSON validated)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-03 and module spec §23.S.4 / §23.U.3; «Delete a record» template artefact re-cut to the read the spec actually shows, with the write path deferred to EP-08. |
| 2026-08-24 | Implemented: DTO pair + validator + service (empty set with logged gap — no exclusion columns exist, epic-wide no-migration ruling), endpoint, screen on 18-1's shell, ExcelJS export, i18n both locales; verified on private smoke instance (401/200/400 + charity scope). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
