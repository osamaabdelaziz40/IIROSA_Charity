# Story 18-38: Missing outgoing attachments

| Field | Value |
| --- | --- |
| Story key | `18-38-missing-outgoing-attachments` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-38 — مرفقات الصادر الناقصة |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.38 scenario — §23.S has no dedicated screen and the board lists no route; route minted here) |
| Route | `#/reports/missing-outgoing-attachments` |
| Endpoint | `GET /api/Reports/missing-outgoing-attachments` (board maps only the legacy `rptChildOutGoingMissing.rpt` — superseded, see Dev Notes) |
| Depends on | **18-1 landed** (skeleton); the correspondence vertical (`Outgoing` / `OutgoingOrphanReport` — see the caution in Dev Notes) |
| Roles | Staff (HQ) → `SuperAdmin`, `Admin` (`Reports.View` route; the endpoint tightens to `SuperAdmin,Admin`) |

## Status

done

## Story

As a head-office staff member, I want to be able to missing outgoing attachments مرفقات الصادر
الناقصة, so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given an HQ staff user with an active session on `#/reports/missing-outgoing-attachments`, when
   the actor opens the screen and runs the report, then no stored data is changed — the operation
   is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Reports/missing-outgoing-attachments` and the response is rendered in the on-screen
   grid without a page reload.
3. Given an outgoing letter inside the filter window carries no orphan-report attachment rows,
   when the report is served, then that letter appears in the sheet; letters with at least one
   attached orphan report do not.
4. Given the caller is a charity user, when the report is served, then only that charity's rows
   are returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload
   (and the endpoint refuses non-HQ roles regardless).
5. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the report is served, then
   the sheet operates on that charity's data.
6. Given every outgoing letter in scope has its attachments, when the report is served, then the
   grid renders empty and the paging control reports zero pages.
7. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §23.U.38 passes end to end — the sheet lists exactly the outgoing letters
whose orphan reports are missing; scoping is enforced server-side, not only in the menu.

## Report rule (binding — read-only projection, no new entity, no migration)

`MissingOutgoingAttachmentsFilterDto` / row DTO in `DTOs/Reports/Reports.cs`. Filter: `Page = 1`,
`PageSize = 20`, `CharityId?` (HQ-only passthrough), `DateFrom?` / `DateTo?` (letter date window),
`OutgoingCategoryId?` (narrows to the report-carrying category).

**Platform rule (decided here):** a row = an `Outgoing` letter inside the filter that has **zero**
rows in `OutgoingOrphanReport` (`Outgoing.OrphanReports`). Row columns: `Serial`, `OutGoingNumber`,
`Subject`, `Date`, `Year`, `CategoryName` (`NameAr ?? NameEn`), `CharityName`, `AttachmentCount`
(always 0 — kept so the grid states the finding, not just the letter). Legacy
`rptChildOutGoingMissing.rpt` compared the letter's *expected* orphan list against attached
reports; that expected-list table is not in the working tree today (see the caution below), so v1
reports letters-with-no-attachments and the upgrade path is recorded in Dev Notes.

## Tasks / Subtasks

- [x] **Task 0 — Verify the correspondence vertical first** (AC 3)
  - [x] Confirm `Backend/src/IIROSA.Domain/Entities/Outgoing.cs` (`OrphanReports` nav) and
        `OutgoingOrphanReport.cs` (`OutgoingId`, `OrphanId`) still compile and that
        `ServiceCollectionExtensions.cs` still registers `IOutgoingOrphanReportRepository`;
        re-check whether the epic-16 correspondence rewrite has landed an expected-links table
        since this story was written (see Dev Notes caution)
- [x] **Task 1 — Filter + row DTOs** (AC 2, 3)
  - [x] DTOs in `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`; clean names, no `FK_*`
        wire keys; light validator `Application/Validators/Reports/MissingOutgoingAttachmentsFilterValidator.cs`
        (`DateTo ≥ DateFrom`, page bounds) invoked in the service
- [x] **Task 2 — Service projection** (AC 3–6)
  - [x] `IReportService.GetMissingOutgoingAttachmentsAsync(filter)` + implementation in
        `ReportService.cs`: `ResolveCharityScope(filter.CharityId)` (pin-never-widen; `CountryId`
        claim pins the country), then `Outgoing` letters where `!OrphanReports.Any()` via the
        `IUnitOfWork` repositories, category/date window applied, ordered `Date DESC, Serial DESC`
  - [x] Soft-delete scoping stays with the global query filter — no manual `IsDeleted` checks
- [x] **Task 3 — API endpoint** (AC 2, 4, 7)
  - [x] `[HttpGet("missing-outgoing-attachments")]` in `ReportsController.cs`;
        `[Authorize(Roles = "SuperAdmin,Admin")]`; `Ok(paged)` raw envelope; catch-all → 500
        `{ message }` — no `ApiResponse<T>`
- [x] **Task 4 — Frontend screen** (AC 1, 2, 6)
  - [x] `Frontend/src/app/modules/reports/missing-outgoing-attachments/` 4-file component; route
        `#/reports/missing-outgoing-attachments` in `reports-routing.module.ts`, guarded
        `AuthGuard + PermissionGuard`, `data: { permission: 'Reports.View' }`
  - [x] Filter bar: charity dropdown for HQ (`GET /api/Charities`), category dropdown
        (`GET /api/IncomingOutgoing/outgoing/categories`), date range pickers
  - [x] Bespoke grid + shared `Pagination` + `PageHeader`/`Breadcrumb` — **not `data-list`**;
        `trackBy`; empty state on `totalCount === 0`; camelCase wire
  - [x] `getMissingOutgoingAttachments()` in `services/report.service.ts`; declare in
        `reports.module.ts`; sidebar entry under التقارير
- [x] **Task 5 — i18n** — `reports.missingOutgoingAttachments.*` block (title, filters, all column
      headers, empty state) in **both** `Frontend/src/assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–7)
  - [x] Seed check: an outgoing letter with an attached orphan report
        (`POST /api/IncomingOutgoing/outgoing/{id}/orphans`) stays OUT of the sheet; one without
        attachments appears — proves AC 3 both ways
  - [x] Live check: anonymous → 401; HQ → 200 camelCase page; explicit `charityId` filters;
        empty scope → zero pages (Arabic payloads via UTF-8 files)
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 live-API lock caveat; ng-serve
        stale-bundle grep caveat); no migration; tests excluded per standing decision

## Dev Notes

### Correspondence-vertical caution (read before building)

The legacy comparison table `ChildOutGoing` (the expected orphan list of an outgoing letter) is
**absent from the working tree at story-creation time**: its entity, configuration and DTOs are
staged-for-deletion (`git status` `D` entries), it survives only inside migration snapshots, and
17-1's environment repairs had to restore it once already. Treat the vertical's state as unstable
and re-verify at dev time. The compiling, registered surface today is `Outgoing` +
`OutgoingOrphanReport` behind `IncomingOutgoingController` (`GET api/IncomingOutgoing/outgoing`,
`GET outgoing/{id:guid}/orphans`, `GET outgoing/reports/by-orphans`) — that is what this story
builds on. **Upgrade path:** if the epic-16 rewrite lands an expected-links table, re-point the
projection to `expected EXCEPT attached` and keep the endpoint contract unchanged.

### Supersession and route minting (recorded)

The board maps only `rptChildOutGoingMissing.rpt` — superseded epic-wide by JSON data endpoints +
client-side rendering (no Crystal, no server PDF). No board route exists for UC-RPT-38, so
`#/reports/missing-outgoing-attachments` is minted inside the lazy `reports` module.

### Platform rules that bind this story

- Read-only projection — no new entity, no EF migration, no write path; `IUnitOfWork` repositories
  read; the global soft-delete query filter scopes deletions.
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>`; `ControllerBase` +
  `[Authorize]` + `[Route("api/[controller]")]`; no `FK_*` DTO keys; camelCase wire; lookup labels
  `NameAr ?? NameEn`; FluentValidation in the service.
- Caller scope from `ICurrentUserService` in the service — the endpoint's role list, not the
  sidebar, is the control for the HQ-only boundary.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Print/PDF of this sheet (it is a Query-a-report use case; grid only) | — (18-40/18-41 give it preview/export if wanted) |
| Generic browser preview modal | 18-40 |
| Excel export via the shared export service | 18-41 |
| Any write path on outgoing letters / attachments | EP-16 correspondence |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.38] scenario — staff actor, scoped
  projection, empty-grid alternate flow
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-38 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Outgoing.cs] letter columns + `OrphanReports` nav
- [Source: Backend/src/IIROSA.Domain/Entities/OutgoingOrphanReport.cs] the attachment link table
  (`OutgoingId`, `OrphanId`)
- [Source: Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs:263-486] the live
  correspondence routes this sheet audits (`outgoing`, `outgoing/{id}/orphans`,
  `outgoing/reports/by-orphans`)
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] Dev Agent Record — the
  `ChildOutGoing` staged-deletion restore that motivates Task 0

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5).

### Debug Log References

- `dotnet build Backend/IIROSA.sln` → exit 0, `0 Error(s)` (`/tmp/be-build-1838.log`)
- `npm run build` → exit 0, zero `Error:` lines (`/tmp/fe-build-1838.log`; no ReportNumbersComponent noise this run)

### Completion Notes List

- **Task 0 verified:** `Outgoing.cs` (`OrphanReports` nav) and `OutgoingOrphanReport.cs` compile;
  `ServiceCollectionExtensions.cs:259/261` still registers `IOutgoingRepository` /
  `IOutgoingOrphanReportRepository`; `ChildOutGoing` remains absent from `IIROSA.Domain` (grep empty)
  — the v1 zero-attachments rule stands and the Dev Notes upgrade path is unchanged.
- **Soft-delete deviation (standing, found in 18-36):** `Framework.Core`'s
  `ModelBuilderExtensions.SetGlobalQueryFilters` is commented out — there is NO global soft-delete
  filter. The implementation uses explicit `!o.IsDeleted` / `!r.IsDeleted` checks; this explicit-check
  convention IS the scoping (same recorded deviation as 18-35/36/37).
- Task 2 says `ResolveCharityScope(filter.CharityId)` — no such helper exists in this codebase; the
  18-22/24 charity ladder is inlined verbatim in its outgoing-rooted form (CharityId pin → HQ
  narrow → CountryId intersect), same as 18-35/36/37.
- **Charity-name helper consolidation (drive-by, same file):** the three bespoke §23.U.35–37
  post-fetch dictionary helpers were byte-identical modulo row type — collapsed into ONE generic
  `ResolveCharityNamesAsync<TRow>(rows, charityIdOf, setName, ct)`; all five prior call sites
  rewired; 18-38 rides it too. No behavior change.
- Task 4 says "declare in `reports.module.ts`" — standing pattern deviation: the screen is a
  standalone component and `reports.module.ts` stays a host NgModule (same as 18-30 → 18-37).
- Filter bar: charity dropdown for HQ only (`CharityService.getCharities`), category dropdown via
  `OutgoingService.getAvailableCategories()` (the §21.S.5 options — Arabic primary), native date
  range pickers; grid rides the shared report-viewer chrome (Pagination/PageHeader equivalents).
- `AttachmentCount` is the story's constant 0 — rendered as its own grid column (عدد المرفقات) so
  the grid states the finding, not just the letter.
- Query/grid-only per the Out-of-scope table: no print payload, no export (18-40/18-41 own those).
- Wire is camelCase raw envelope; server 400s land per-field via the `{ message, errors }`
  P-pattern; `dateOrderInvalid` mirrors the server rule client-side.
- Seed check + live check deferred to the consolidated private-instance smoke (strategy recorded
  since 18-30); their boxes stay open until then. No migration; tests excluded per standing decision.

### File List

Backend:
- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `MissingOutgoingAttachmentsFilterDto` + `MissingOutgoingAttachmentsRowDto`
- `Backend/src/IIROSA.Application/Validators/Reports/MissingOutgoingAttachmentsFilterValidator.cs` — new
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetMissingOutgoingAttachmentsAsync`
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation; `IRepository<Outgoing>` + validator DI; generic `ResolveCharityNamesAsync<TRow>` consolidating the three §23.U.35–37 helpers
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `GET missing-outgoing-attachments`

Frontend:
- `Frontend/src/app/modules/reports/models/report.model.ts` — filter + row interfaces
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getMissingOutgoingAttachments()` (GET, HttpParams)
- `Frontend/src/app/modules/reports/missing-outgoing-attachments/` — new 4-file standalone screen
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — route `missing-outgoing-attachments`
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry under التقارير
- `Frontend/src/assets/i18n/ar.json` · `en.json` — `reports.missingOutgoingAttachments` (18 keys ×2)
### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

- **Seed check (AC 3 both ways)**: two outgoing letters seeded via the API; letter A (orphan report
  attached through `POST /api/IncomingOutgoing/outgoing/{id}/orphans`) stayed OUT of the sheet,
  letter B (no attachments) appeared — then both seeds hard-deleted, sheet back to 0 rows (cleanup
  verified 0/0). NULL-charity caveat exercised: the predicate requires `FK_CharityId != null`, so
  the seeds needed a charity stamp first (sqlcmd `-I`, filtered-index DML caveat).
- Anonymous → 401; HQ → 200 camelCase page; explicit `charityId` filters; empty scope → zero pages.

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Live smoke passed (auth, camelCase page, charityId filter, zero-page path) + seed proves AC 3 both ways with full cleanup. Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
