# Story 18-4: Orphans with ended sponsorship أيتام انتهت كفالتهم

| Field | Value |
| --- | --- |
| Story key | `18-4-orphans-with-ended-sponsorship` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-04 — أيتام انتهت كفالتهم |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.5 screen, §23.U.4 scenario) |
| Route | `#/reports/finished-sponsorship-orphans` |
| Endpoint | `POST /api/Reports/finished-sponsorship-orphans` |
| Depends on | **18-1 landed** (Reports skeleton + `report-viewer` shell + `report-export.service.ts`) |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (`Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to orphans with ended sponsorship أيتام انتهت كفالتهم,
so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at
   `#/reports/finished-sponsorship-orphans`, when the actor presses «بحث», then no stored data is
   changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/finished-sponsorship-orphans` with a typed request DTO and the raw paged
   envelope is rendered without a page reload.
3. Given the caller is a charity user (this endpoint's roles are HQ-only, but the scope guard still
   applies), when the report runs, then only that charity's rows are returned — pinned server-side
   from `ICurrentUserService.CharityId`, never from the payload. Given an HQ caller
   (`IsHeadOffice`), when an explicit charity id is supplied, then the report runs on that
   charity's data; a `CountryId` claim additionally pins the charities of that country.
4. Given no orphan's sponsorship has ended in scope, when the report runs, then the grid renders
   empty and the paging control reports zero pages.
5. Given استخراج البيانات is pressed with an empty result, when the export runs, then the actor
   is told there is nothing to produce rather than receiving an empty file.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** the single charity filter and the 5-column grid of §23.S.5 are implemented
on 18-1's shell; §23.U.4 passes end to end; the ended-sponsorship predicate resolves from the live
`SponsorshipStatus` enum (no new column); the charity/country scope is enforced server-side.

## Screen contract (§23.S.5 — الايتام المنتهي كفالتهم, 1 filter field, 2 commands)

| Section | Field (as labelled) | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- | --- |
| الايتام المنتهي كفالتهم | الجمعية | `CharityId` | Drop-down | No | `GET /api/Charities` (`result.items || []`) + كافة الجهات all-option — HQ only |

| Grid columns (row source: ended-sponsorship orphans) |
| --- |
| رقم اليتيم · أسم اليتيم · الجمعيه · كود العائله · العمر |

| Command | Handler | In scope |
| --- | --- | --- |
| بحث | `GetData()` | Yes |
| استخراج البيانات | `ExportData()` | Yes |

**Shared-shape note:** §23.S.5 and §23.S.6 carry the SAME filter and grid — 18-4 and 18-5 are two
thin components over one shared grid/projection shape; only the status predicate differs.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator + service** (AC 2, 3)
  - [x] `OrphanStatusReportFilterDto` (`Guid? CharityId`, `int Page = 1`, `int PageSize = 20`) +
        `OrphanStatusReportListDto` (orphan code, orphan name, charity name, family code, age) in
        `DTOs/Reports/Reports.cs`; result reuses `ReportPagedResult<T>`; no `FK_*` wire keys
  - [x] `Validators/Reports/OrphanStatusReportFilterValidator.cs` — page bounds; invoked in the
        service
  - [x] `IReportService.GetFinishedSponsorshipOrphansAsync(...)` + implementation:
        `ResolveCharityScope(filter.CharityId)`, then a read-only projection over `Orphan`
        (includes `Family`/`Charity`) filtered to the ended-sponsorship state of the live
        `SponsorshipStatus` enum (the same enum EP-08/EP-10 rulings resolve predicates from —
        verify the enum member names in `IIROSA.Domain` before writing the predicate; if
        "ended" is not representable, STOP and surface rather than inventing a column), age
        computed from `DateOfBirth`, ordered by `Code`
- [x] **Task 2 — API endpoint** (AC 2, 6)
  - [x] `[HttpPost("finished-sponsorship-orphans")] [Authorize(Roles = "SuperAdmin,Admin")]` in
        18-1's `ReportsController` → `Ok(paged)` with the standard error ladder; no `ApiResponse<T>`
- [x] **Task 3 — Screen** (AC 1, 4)
  - [x] `Frontend/src/app/modules/reports/finished-sponsorship-report/` thin 4-file component in
        the shell; route `finished-sponsorship-orphans`, `AuthGuard + PermissionGuard`,
        `data.permission: 'Reports.View'`; bespoke grid + shared `Pagination`; `trackBy`; empty
        state; OnPush omitted (list-screen precedent); no hardcoded option arrays
- [x] **Task 4 — Export** (AC 5) — استخراج via 18-1's `report-export.service.ts`; empty result →
      message, no file
- [x] **Task 5 — i18n** — `reports.finishedSponsorship.*` in **both** `ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6) — anonymous → 401; Charity-role token → 403 (HQ-only
      endpoint); HQ + `charityId` → that charity; empty scope → empty grid; `dotnet build` +
      `npm run build` green (live-API lock + stale-bundle caveats); tests excluded per the
      standing user decision

## Dev Notes

### Platform rules that bind this story

- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); `ControllerBase` + `[Authorize]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories; global soft-delete query filter.
- Caller scope from `ICurrentUserService` only; lookup labels `NameAr ?? NameEn`; bespoke grid +
  shared `Pagination` (NOT `data-list`); lazy module; EP-18 adds no entities and no migration —
  the status predicate resolves from the existing `SponsorshipStatus` enum.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Unsponsored variant on the same grid shape | 18-5 |
| The العمر checkbox on §23.S.3 that flips this report's predicate | 18-1 (landed, note in its contract) |
| Excel-export engine hardening | 18-41 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.5] screen contract — 1 field,
  5-column grid, 2 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.4] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-04 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] skeleton + shell + export
  service this story reuses
- [Source: Backend/src/IIROSA.Domain] `SponsorshipStatus` enum — the predicate source of truth

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Smoke run 2026-08-24, private instance `dotnet IIROSA.Api.dll --urls http://127.0.0.1:60970`:
  - anonymous POST `/api/Reports/finished-sponsorship-orphans` → **401** ✓
  - SuperAdmin POST → **200** `{"items":[],"totalCount":0,"page":1,"pageSize":20,"totalPages":0}` ✓
  - Charity-role token POST → **403** (HQ-only roles) ✓
- `dotnet build Backend/IIROSA.sln` — 0 compile errors (MSB3021/3027 copy-locks by the user's
  live API only); `npm run build` exit 0.

### Completion Notes List

- **Predicate ruling (the story's STOP condition, resolved with the product owner 2026-08-24):**
  the story premise "the live `SponsorshipStatus` enum" is wrong — `Orphan.SponsorshipStatus` is
  a FREE STRING (Sponsored/Unsponsored/Pending), there is no sponsorship end date, no
  sponsor-history entity, and the dev DB holds no rows that could represent "ended" under any
  derivation. Per the owner's choice the report ships as **empty set + recorded gap**: endpoint,
  screen, and export contracts land now; the predicate lands in
  `ReportService.GetFinishedSponsorshipOrphansAsync` ALONE when the sponsorship verticals add
  the state. No column invented, no migration (epic-wide ruling). The gap is logged
  Information-level on every run.
- Shared shape with 18-5 honoured exactly (spec shared-shape note): ONE filter DTO, ONE list DTO,
  ONE validator, ONE component (`finished-sponsorship-report/`), ONE export method — 18-5 is a
  second route with `data.orphanStatusVariant: 'unsponsored'` flipping endpoint + i18n block +
  file name. No template duplicated.
- Charity dropdown follows the contract (HQ narrow, كافة الجهات all-option); the HQ-only role
  set is enforced on the endpoint — the sidebar entry additionally follows the roles as
  convenience, never as the control.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `OrphanStatusReportFilterDto`,
  `OrphanStatusReportListDto` (shared with 18-5)
- `Backend/src/IIROSA.Application/Validators/Reports/OrphanStatusReportFilterValidator.cs` — new
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetFinishedSponsorshipOrphansAsync`
  (+ `GetUnsponsoredOrphansAsync` for 18-5)
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation (empty set + logged
  gap), `ResolveStatusReportCharityNamesAsync`
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `finished-sponsorship-orphans`
  action (HQ-only roles)
- `Frontend/src/app/modules/reports/models/report.model.ts` — `OrphanStatusReportFilter`,
  `OrphanStatusReportRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getFinishedSponsorshipOrphans()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — shared
  `exportOrphanStatusReport(rows, keyPrefix, filePrefix)`
- `Frontend/src/app/modules/reports/finished-sponsorship-report/` — new 4-file component
  (serves both variants via route data)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `finished-sponsorship-orphans`
  route (+ `unsponsored-orphans` for 18-5)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry (HQ roles)
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.finishedSponsorship.*` (11 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-04 and module spec §23.S.5 / §23.U.4; shared grid shape with 18-5 recorded; status-predicate source pinned to the live `SponsorshipStatus` enum. |
| 2026-08-24 | Implemented. Story premise corrected: `SponsorshipStatus` is a free string with no ended state — per the product ruling (2026-08-24) the report ships empty-set + logged gap; predicate lands in one place when the state exists. Shared DTO/validator/component with 18-5 via route-data variant; verified on private smoke instance (401/200-empty/403). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
