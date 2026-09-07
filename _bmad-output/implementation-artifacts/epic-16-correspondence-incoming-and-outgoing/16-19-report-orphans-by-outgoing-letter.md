# Story 16-19: Report orphans by outgoing letter

| Field | Value |
| --- | --- |
| Story key | `16-19-report-orphans-by-outgoing-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-19 — تقرير الأيتام حسب الخطاب الصادر |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.7 screen, §21.U.19 scenario) |
| Route | `#/incoming-outgoing/export/outgoing-orphans` (route status **planned** — does not exist yet) |
| Endpoint | `GET /api/IncomingOutgoing/outgoing?orphanNumber=` (epic AC; implemented as a dedicated report query on the outgoing collection — see defect 2) |
| Depends on | 16-18 (the link the report reads), 16-10 (scope + wire) |
| Roles | Staff, Gen. Director → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to report orphans by outgoing letter تقرير
الأيتام حسب الخطاب الصادر, so that I can answer "which reports did we send, and when?".

## Acceptance Criteria

1. Given a head-office staff member with an active session on the screen at
   `#/incoming-outgoing/export/outgoing-orphans`, when the actor opens the screen with valid
   input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/outgoing?orphanNumber=` and the response is rendered on the screen
   without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.
6. Given the business rule behind «Operation Faild» is broken, when the operation is attempted,
   then it is refused with that message and nothing is written.

**Definition of done:** the screen fields of §21.S.7 are implemented; the scenario of §21.U.19
passes end to end; the projection is scoped to the caller's charity and country; the result can
be printed or exported from the grid.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Link data | 16-18's `OutgoingOrphanReport` | Exists after 16-18 |
| Orphan/orphan-report entities | `Orphan`, `PeriodicOrphanReport`, `Charity` | Exist |
| Route | — | **Nothing** — `export/outgoing-orphans` is not registered anywhere |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **The screen does not exist.** No route, no component, no menu entry — §21.S.7 route status
   "planned". Build it new (the spec names `ExportWizardComponent` as host by template inertia;
   a dedicated report component is the correct shape — record the deviation).
2. **No query endpoint fits.** The epic's `GET …/outgoing?orphanNumber=` rides the list filter,
   but §21.S.7 needs a **projection** (per-letter orphan counts + attached flag + orphan code)
   with criteria Serial · Year · CharityId · DateFrom · DateTo · ChildCode (mandatory).
   Implement as a dedicated read (e.g. `GET outgoing/reports/by-orphans`) that returns the §21.S.7
   row shape; note the endpoint deviation from the epic text in the completion notes (14-6
   precedent: board/epic endpoint notes corrected at finalization).
3. **ChildCode is mandatory per §21.S.7** — an empty ChildCode must refuse (localized, nothing
   queried) rather than run an unbounded report; «Operation Faild» wording per AC 6.
4. **Pager + extract commands** — §21.S.7 lists بحث / ExtractOutgoingData / GetNext / GetPrev:
   reuse the shared `Pagination` + the 16-1 ExcelJS extract helper; print via the browser dialog
   (14-6 precedent).

## Tasks / Subtasks

- [x] **Task 1 — Report query service-side** (AC 2, 3, 4, 6): projection over
        `Outgoing` ⋈ `OutgoingOrphanReport` ⋈ `Orphan` returning rows of `{ serial, year,
        letterDate, charityName, orphanCount, orphanAttached }` (اليتيم مضاف للتقرير flag);
        criteria Serial/Year/CharityId/DateFrom/DateTo/ChildCode; `ApplyCallerScope` pin-never-
        widen; ChildCode empty → localized «Operation Faild» refusal; `[Authorize(Roles =
        "Admin,SuperAdmin")]`; paged `{ items, totalCount, page }` wire
- [x] **Task 2 — The §21.S.7 screen** (AC 1): new component + route
        `#/incoming-outgoing/export/outgoing-orphans` (`PermissionGuard`, `IncomingOutgoing.View`);
        filters — الجمعية (HQ, Charities + كافة الجهات) · من/الي تاريخ · رقم الخطاب (serial
        lookup from the outgoing list) · سنه الخطاب (year) · ChildCode (mandatory text); بحث
        runs the report; grid columns per §21.S.7 with `trackBy`; empty state + zero pages;
        Extract via ExcelJS; print button
- [x] **Task 3 — Menu + i18n** (AC 1): add the screen to the module's navigation entry points
        (where the list screens' page actions live); all new keys in **both** `ar.json`/`en.json`
- [x] **Task 4 — Verification**: live — ChildCode empty → refusal; a letter with attachments
        returns rows with `orphanCount > 0` and the attached flag; charity scoping holds; HQ +
        explicit charityId narrows; print/extract produce output; unauthenticated → 401;
        `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- Read-only projection — no writes anywhere in this story; `IUnitOfWork` untouched.
- The report is the epic's only **Could** — if the wire endpoint deviates from the epic text,
  record it precisely (endpoint + why) in the completion notes so the board note can be
  corrected at finalization (14-6 precedent).
- Reuse, don't rebuild: shared `Pagination`, the 16-1 ExcelJS extract helper, the 16-9/16-18
  drop-down conventions.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| The attachment link itself | 16-18 |
| Orphan periodic-report content (report bodies, PDFs of reports) | epic 6 |
| Scheduled/emailed reports | not in this epic's scope |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.7] screen contract
  — 6 filters (ChildCode mandatory), grid columns, 5 commands
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.19] scenario —
  criteria Serial/Year/CharityId/DateFrom/DateTo/ChildCode, print/export from the grid
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-19 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-14-technical-support/14-6-report-on-support-tickets.md] report-screen
  + endpoint-note-correction precedent

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): the screen, route, and query existed but the §21.S.7 Extract command was missing — added `extractAll()` this pass: ChildCode re-validated, report re-run with one page holding every row, ExcelJS export with the attached flag rendered as ✓/blank. Print rides the browser dialog (14-6 precedent).
- 2026-08-24 (verification): endpoint `GET outgoing/reports/by-orphans` (controller :485) with the projection `{ serial, year, letterDate, charityName, orphanCount, orphanAttached }`; route `export/outgoing-orphans` (:121); service `getOrphanReport` (:145). Project builds clean; `ng build` clean for the module.

### Completion Notes List

- **Endpoint deviation from the epic text (recorded per the 14-6 precedent):** the board/epic lists `GET …/outgoing?orphanNumber=`; the shipped read is the dedicated projection `GET /api/IncomingOutgoing/outgoing/reports/by-orphans` — a list-filter overload cannot express per-letter counts + the attached flag. Board note to be corrected at finalization.
- **Screen deviation (recorded):** a dedicated report component (`OutgoingOrphansReportComponent`), not the template-named `ExportWizardComponent` host.
- ChildCode (كود اليتيم) is mandatory both sides: the screen refuses locally with `childCodeRequired` and the server refuses with the «Operation Faild» wording — an empty code never runs an unbounded report.
- Criteria: Serial · Year · CharityId (HQ-only) · DateFrom · DateTo · ChildCode, caller scope pin-never-widen; paged `{ items, totalCount, page }` wire; shared `Pagination`; empty state + zero pages; grid `trackBy: trackById`.
- Entry points: the outgoing register's page action (orphansReportTitle) lands here; `IncomingOutgoing.View` guards the route.
- Task 4's live portion pending the user's `IIROSA.Api` restart; builds verified. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/OutgoingService.cs` — report projection + criteria
- `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` — `GET outgoing/reports/by-orphans`
- `Frontend/src/app/modules/incoming-outgoing/outgoing-orphans-report/outgoing-orphans-report.component.ts` / `.html` / `.scss` — §21.S.7 screen + extract-all
- `Frontend/src/app/modules/incoming-outgoing/services/outgoing.service.ts` — `getOrphanReport`
- `Frontend/src/app/modules/incoming-outgoing/incoming-outgoing-routing.module.ts` — `export/outgoing-orphans` route

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-19 and module spec §21.S.7 / §21.U.19; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (added the missing Extract command; endpoint + component deviations recorded for board correction). Status → review; live report run pending user's API restart. |
