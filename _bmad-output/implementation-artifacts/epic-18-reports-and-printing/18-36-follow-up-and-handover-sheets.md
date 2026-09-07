# Story 18-36: Follow-up and handover sheets

| Field | Value |
| --- | --- |
| Story key | `18-36-follow-up-and-handover-sheets` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-36 — كشوف المتابعة والتسليم |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.36 scenario — §23.S has no dedicated screen and the board lists no route; route minted here, see Dev Notes) |
| Route | `#/reports/follow-up-sheets` |
| Endpoint | `GET /api/Reports/follow-up-sheets` (board maps only the legacy Crystal trio `rptFollowUp.rpt` / `rptFollowUpFamily.rpt` / `rptFollowUpTasleem.rpt` — superseded, see Dev Notes) |
| Depends on | **18-1 landed** (`ReportsController` + `IReportService`/`ReportService` + `ReportPagedResult<T>` + `ResolveCharityScope` + `modules/reports` skeleton); **18-21 landed** (client PDF service used by the print command) |
| Roles | HQ roles → `SuperAdmin`, `Admin` (`Reports.View` on the route; the endpoint tightens to `SuperAdmin,Admin`) |

## Status

done

## Story

As a HQ roles, I want to be able to follow-up and handover sheets كشوف المتابعة والتسليم, so that
I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given an HQ user with an active session on the screen at `#/reports/follow-up-sheets`, when the
   actor opens the screen and runs the report, then no stored data is changed — the operation is a
   read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Reports/follow-up-sheets` and the response is rendered in the on-screen grid without a
   page reload.
3. Given the actor switches the sheet selector between the three variants — general follow-up
   (متابعة), family follow-up (متابعة الأسر), handover (تسليم) — when the report is re-run, then
   each variant returns its own projection from the same endpoint.
4. Given the caller is a charity user, when the report is served, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
5. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the report is served, then
   the sheet operates on that charity's data.
6. Given no row matches the criteria, when the report is served, then the grid renders empty and the
   paging control reports zero pages.
7. Given the actor presses طباعة on a non-empty grid, when the client PDF service renders the
   sheet, then a printable document is produced; on an empty grid the actor is told there is
   nothing to produce rather than receiving an empty file.
8. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** all three sheet variants of §23.U.36 are served from the one endpoint with
their own projections; the scenario passes end to end (criteria → run → grid → print); the charity
and country scoping is enforced server-side, not only in the menu.

## Projection design (binding — read-only, no new entity, no migration)

DTOs in `DTOs/Reports/Reports.cs` (18-1's container). Filter: `Variant` (enum
`FollowUp | FollowUpFamily | Tasleem`, default `FollowUp`), `Page = 1`, `PageSize = 20`,
`CharityId?` (HQ-only passthrough), `DateFrom?` / `DateTo?`.

| Variant | Grain | Row columns (sources) |
| --- | --- | --- |
| `FollowUp` (rptFollowUp) | one row per orphan | `OrphanCode`, `OrphanName` (`Orphan.Code`/`FullName`), `FamilyCode` (`Family.Code`), `CharityName` (`NameAr ?? NameEn`), `SponsorshipStatus`, `LastReportDate` (latest `PeriodicOrphanReport` per orphan — verify the status/date field names against the entity), `MonthlyAmount` |
| `FollowUpFamily` (rptFollowUpFamily) | one row per family | `FamilyCode`, `HeadOfFamily`, `CharityName`, `OrphansCount`, `FamilyStatus` (`Family.*`), `RegistrationDate`, `LastUpdate` (`UpdatedOn`) |
| `Tasleem` (rptFollowUpTasleem — handover/delivery) | one row per orphan | `OrphanCode`, `OrphanName`, `GuardianName` (family `Provider.FullName` / `HeadOfFamily`), `FamilyCode`, `CharityName`, `MonthlyAmount`, `SponsorshipStatus` |

## Tasks / Subtasks

- [x] **Task 1 — Filter + row DTOs** (AC 2, 3)
  - [x] `FollowUpSheetFilterDto` + `FollowUpSheetRowDto` (superset column set; unused keys null per
        variant) in `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`; `ReportPagedResult<T>`
        from 18-1 carries the result — **no new envelope**, no `FK_*` wire keys
  - [x] `Application/Validators/Reports/FollowUpSheetFilterValidator.cs`: `Variant` `IsInEnum()`,
        `DateTo ≥ DateFrom` when both set, page bounds — invoked in the service (platform rule)
- [x] **Task 2 — Service projections** (AC 3, 4, 5, 6)
  - [x] `IReportService.GetFollowUpSheetsAsync(FollowUpSheetFilterDto)` + implementation in
        `Backend/src/IIROSA.Application/Services/ReportService.cs`: `ResolveCharityScope(filter.CharityId)`
        first (charity user pinned to `ICurrentUserService.CharityId`; `IsHeadOffice` may pass an
        explicit id; `CountryId` claim pins the country — pin-never-widen), then one query per
        variant over `IUnitOfWork` repositories (`Orphan`+`Family`, `PeriodicOrphanReport` for
        `LastReportDate`), ordered `Code` ascending
  - [x] Reads go through repositories only; the global soft-delete query filter does the deletion
        scoping — no manual `IsDeleted` checks
- [x] **Task 3 — API endpoint** (AC 2, 8)
  - [x] `[HttpGet("follow-up-sheets")] GetFollowUpSheets([FromQuery] FollowUpSheetFilterDto)` in
        `Backend/src/IIROSA.Api/Controllers/ReportsController.cs`; `[Authorize(Roles = "SuperAdmin,Admin")]`
        (server-side, not menu-only); `Ok(paged)` on the raw envelope; catch-all → 500
        `{ message }` — no `ApiResponse<T>` (15-1 ruling)
- [x] **Task 4 — Frontend screen** (AC 1–3, 6, 7)
  - [x] `Frontend/src/app/modules/reports/follow-up-sheets/` 4-file component (`.ts`/`.html`/`.scss`/
        `.spec.ts`); route `#/reports/follow-up-sheets` in `reports-routing.module.ts`, guarded
        `AuthGuard + PermissionGuard`, `data: { permission: 'Reports.View' }`
  - [x] Filter bar: variant selector (three i18n labels), charity dropdown for HQ callers
        (`GET /api/Charities`, label `nameAr ?? nameEn`), date range pickers
  - [x] Bespoke grid + shared `Pagination` + `PageHeader`/`Breadcrumb` — **do NOT use `data-list`**;
        `trackBy` on the `*ngFor`; empty state on `totalCount === 0`; camelCase wire
  - [x] Print command طباعة → `report-pdf.service.ts` (18-21) renders the current variant's rows;
        empty grid → "nothing to produce" message, no file
  - [x] `getFollowUpSheets()` in `services/report.service.ts`; declare the screen in
        `reports.module.ts`; sidebar entry under the التقارير group
- [x] **Task 5 — i18n** — the `reports.followUpSheets.*` block (title, variant labels, all column
      headers per variant, filters, print/empty messages) in **both**
      `Frontend/src/assets/i18n/ar.json` and `en.json`; no hard-coded UI strings
- [x] **Task 6 — Verification** (AC 1–8)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors, **no migration** (read-only story). MSB3021/
        3027 on copy steps = the user's live `IIROSA.Api` locking outputs; compile is clean; never
        kill their process
  - [x] Live check: anonymous → 401; HQ → 200 camelCase page; each variant returns its projection;
        explicit `charityId` filters; empty scope → zero pages
  - [x] `cd Frontend && npm run build` — 0 errors; ng-serve stale-bundle caveat (grep the served
        chunk for a new key before trusting a no-effect fix); tests excluded per standing decision

## Dev Notes

### Route minted and supersession (recorded)

The board lists no route for UC-RPT-36 (legacy was three Crystal printouts with no screen); this
story mints `#/reports/follow-up-sheets` inside the lazy `reports` module — grid-first, printing
rides on the grid. Epic-wide decision: Crystal `.rpt` and server `/export/pdf` are **superseded**
by JSON data endpoints + client-side jsPDF (18-21); do not build a server-PDF path.

### Platform rules that bind this story

- Read-only projection — **no new entity, no EF migration, no write path**; only `IUnitOfWork`
  repositories read; the global soft-delete query filter scopes deletions.
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); controllers inherit `ControllerBase` + `[Authorize]` +
  `[Route("api/[controller]")]`.
- camelCase wire; no `FK_*` DTO keys; lookup labels `NameAr ?? NameEn`; FluentValidation in the
  service; bespoke grid, not `data-list`.
- Caller scope from `ICurrentUserService` in the service — never parse claims in the controller,
  never trust a payload charity id for a charity-bound caller.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Generic browser preview modal (inline-frame review before save) shared by all report screens | 18-40 |
| Excel export of the grid via the shared export service | 18-41 |
| Identification sheets (تعريف) — a different use case, different params | 18-37 |
| Any write-back of print state (none recorded for these sheets in §23.U.36) | — |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.36] scenario — criteria → run →
  scoped projection → grid, printable from the grid
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-36 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Interfaces/ICurrentUserService.cs] `CharityId` /
  `CountryId` / `IsHeadOffice` — the scope source of truth
- [Source: Backend/src/IIROSA.Domain/Entities/PeriodicOrphanReport.cs] latest-report column source
  (verify field names at dev time)
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] reviewed greenfield list
  story — grid, envelope and verification patterns this story reuses on 18-1's skeleton

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code harness)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` → 0 Error(s) (pre-existing warnings only).
- `npm run build` → exit 0, 0 `Error:` lines (first pass).

### Completion Notes List

- **Soft-delete scoping deviation (recorded):** Task 2's premise — "the global soft-delete query
  filter does the deletion scoping, no manual IsDeleted checks" — is FALSE on this platform:
  Framework.Core's `ModelBuilderExtensions.SetGlobalQueryFilters` is **commented out** (verified,
  `ModelBuilderExtensions.cs` lines 61–88 sit inside `/* */`). The explicit `!IsDeleted`
  convention used across every ReportService query IS the deletion scoping; this story follows
  it (also `!r.IsDeleted` on the periodic-report subquery).
- **Date-window semantics (recorded):** the optional `DateFrom`/`DateTo` scopes the row ANCHOR's
  registration date — `Orphan.CreatedOn` for متابعة/تسليم, `Family.CreatedOn` for متابعة الأسر
  (the 18-35 CreatedOn-proxy ruling extended; no dedicated registration column exists).
- **LastReportDate (متابعة only):** the latest ACCEPTED periodic report per orphan (BR-11 — an
  accepted report is the only clearing state; pending/refused never count). Stitched post-page:
  `OrphanId` rides the page projection (never the wire DTO), one grouped `Max(ReportDate)`
  query per page.
- **GuardianName (تسليم):** `Family.Provider.FullName` with `Family.HeadOfFamily` as fallback
  (the story offered "Provider.FullName / HeadOfFamily").
- **Variant wire:** the enum names (`FollowUp` / `FollowUpFamily` / `Tasleem`) ride the query
  string; default `FollowUp`; validator `IsInEnum()` rejects anything else.
- **متابعة and تسليم share one row source** (one orphan query, one column-set fork — the 18-35
  variant-collapse convention); متابعة الأسر runs the family projection.
- **Charity scope:** the 18-22/24 ladder verbatim — orphan-rooted inline over `o.FK_CharityId`,
  family-rooted inline over `f.FK_CharityId` (the story's `ResolveCharityScope` name collapses
  into the ladder each branch already carries).
- **reports.module.ts declaration N/A:** the module is a host NgModule — every EP-18 screen is
  standalone and referenced directly by the routing module (the module's own header comment);
  nothing is ever declared there. Sidebar entry added under التقارير (after 18-28's entry).
- **Task 6 live check deferred** to the epic's consolidated private-instance smoke
  (127.0.0.1:60970 pattern) — the live-check box stays open until then; tests excluded per the
  standing user decision.

### File List

Backend:
- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `FollowUpSheetVariant` enum,
  `FollowUpSheetFilterDto`, `FollowUpSheetRowDto` (column superset).
- `Backend/src/IIROSA.Application/Validators/Reports/FollowUpSheetFilterValidator.cs` — NEW.
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetFollowUpSheetsAsync`.
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation (variant fork,
  ladder, LastReportDate stitch, `ResolveFollowUpCharityNamesAsync` helper) + validator DI.
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `GET follow-up-sheets`
  (HQ roles; raw envelope; 400 errors-map / 500 `{ message }` ladder).

Frontend:
- `Frontend/src/app/modules/reports/models/report.model.ts` — `FollowUpSheetVariant`,
  `FollowUpSheetFilter`, `FollowUpSheetRow` (envelope = shared `ReportPagedResult<T>`).
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getFollowUpSheets` (GET,
  query-string params).
- `Frontend/src/app/modules/reports/follow-up-sheets/` — NEW 4-file component (filter bar,
  3-button variant group, per-variant grids, printSheet builder).
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `follow-up-sheets` route.
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry under
  التقارير.
- `Frontend/src/assets/i18n/ar.json`, `en.json` — `reports.followUpSheets` (31 keys ×2).

### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

- Anonymous → 401; HQ → 200 camelCase page.
- Each variant (FollowUp / FollowUpFamily / Tasleem) returns its own projection; explicit
  `charityId` filters; empty scope → zero pages.
- Browser pass (18-40/41 on this screen): 10-row FollowUp page rendered, preview framed the sheet
  (RTL, variant title), and `follow-up-orphans-20260825.xlsx` exported through the engine walker
  with the full sheet contract (RTL/frozen/typed/autofilter/footer).
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-36 and module spec §23.U.36; route minted, three-variant projection designed, Crystal trio recorded as superseded. |
| 2026-08-25 | Tasks 1–5 + Task 6's build subtasks implemented and checked; soft-delete-filter and window-semantics rulings recorded; Dev Agent Record written; Status → in-progress (live check pending the epic's consolidated smoke). |
| 2026-08-25 | Live smoke passed (auth, variants, scoping, zero-page path) + browser evidence via the 18-40/41 pass. Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
