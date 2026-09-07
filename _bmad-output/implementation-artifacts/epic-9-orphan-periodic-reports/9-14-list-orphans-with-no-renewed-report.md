# Story 9-14: List orphans with no renewed report

| Field | Value |
| --- | --- |
| Story key | `9-14-list-orphans-with-no-renewed-report` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-14 — الأيتام بدون تقرير مجدد |
| Priority / size | Should · 3 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.U.14 scenario; the non-renewed commands of §14.S.1) |
| Route | command on `#/periodic-orphan-reports` opening the non-renewed view (list + count-only variant) |
| Endpoint | `POST /api/Reports/non-renewed-reports` (new — creates `ReportsController`, the api/Reports root EP-18 will build on) |
| Depends on | **9-1** (scope, list screen hosting the commands) |
| Roles | HQ roles → `SuperAdmin`, `Admin`, `Accountant`, `Employee` |

## Status

review

## Story

As a HQ role, I want to be able to list orphans with no renewed report الأيتام بدون تقرير مجدد, so
that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a HQ role in the module, when the actor runs the non-renewed extract for a charity (and
   date window), then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `POST
   /api/Reports/non-renewed-reports` and the response renders without a page reload.
3. Given the charity's coded orphans, when the extract runs, then the result lists exactly those
   with **no accepted report covering the window** — the chase list before a payment run — with
   orphan identity (code, name, family code) and last report date if any.
4. Given the count-only variant is requested (`countOnly: true`), when the extract runs, then the
   response carries just the count (the legacy `_Number` screens).
5. Given a charity user, when the function is invoked, then only that charity's orphans are
   considered; an HQ role may pass an explicit `charityId`.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.14 passes end to end; the list reconciles (charity's coded orphans −
orphans with an accepted in-window report = the extract); scoping server-side.

**Variant collapse (recorded):** the legacy realisation shipped four shapes (`non-renewed-reports`,
`…V2`, `…_Number`, `GetBeginingScreen`). Ship **one** endpoint with a `countOnly` flag (AC 4);
V2/begin-screen layouts are presentation variants of the same query — do not fork endpoints.
**Batch deferral:** the legacy signature's `batchId` has no Batch entity yet — accept-and-ignore
with `// TODO EP-10: batch scoping` (17-2 deferred-rule precedent); the date window carries the
semantic meanwhile.

## What exists already

**Nothing on this path** — there is no `ReportsController` (api/Reports) anywhere in
`IIROSA.Api/Controllers/` today, no non-renewed service method, and the frontend has no call
(the §14.S.1 command icons `ExtractOrphanNonRenewedReportData*` are inert). This story is
greenfield on all three layers, over existing entities (`Orphan`, `PeriodicOrphanReport`,
`Charity`).

## Tasks / Subtasks

- [x] **Task 1 — Controller root** (AC 2): new `Backend/src/IIROSA.Api/Controllers/ReportsController.cs`
      — `ControllerBase`, `[Route("api/Reports")]`, `[Authorize]`; this is the root EP-18 (Reports
      & Printing, 41 stories) will populate — keep it thin and free of module-specific logic
      — premise corrected at implementation time: the families epic (UC-FAM-14/UC-RPT-01) had
      already founded `ReportsController` + `IReportService`/`ReportService` in DI. The story's
      addition is `POST non-renewed-reports` on that existing root — thin, delegating, roles
      `SuperAdmin,Admin,Accountant,Employee,Charity`, `{ message, errors }` 400 shape matching
      the controller's house style.
- [x] **Task 2 — Query + DTO** (AC 1, 3, 4, 5): `NonRenewedReportsRequestDto` (`CharityId?`,
      `DateFrom`, `DateTo`, `CountOnly`) + result DTO (orphans with code/name/family/last-report
      fields, or `Count`); query: coded, active orphans of the (scoped) charity **left-joined** to
      accepted reports whose period intersects the window — `!accepted.Any(...)`; one EF query, no
      per-orphan round trips; caller scope per 9-1 (charity pin; HQ explicit `charityId`)
      — `ReportService.GetNonRenewedReportAsync` (placement per the recorded preference — EP-18
      grows here): `TableNoTracking` orphans, `!IsDeleted && Code non-empty` (uncoded never join a
      payment run), scope via the service's existing `ApplyCharityScopeAsync` (pin-never-widen);
      the accepted-covering set is one translatable `!acceptedIds.Contains(o.Id)` over
      `Reviewed && IsAccepted` reports intersecting [DateFrom, DateTo] (inclusive end; a report
      with an incomplete period falls back to its ReportDate in-window) — BR-11: pending/refused
      never clear an orphan. `LastReportDate` is a correlated subquery inside the projection, not
      a per-row round trip. `CountOnly` returns `{ count, items: [] }`; otherwise one clamped page
      (≤100). Validator: `NonRenewedReportsRequestValidator` (window required + ordered, page
      range) invoked with `ValidateAndThrowAsync` in the service layer. Batch marker:
      `// TODO EP-10: batch scoping`.
  - [x] Service placement resolved: `ReportService` (the spec's named realisation; already
        registered — no 9-1-style DI mistake).
- [x] **Task 3 — Frontend** (AC 1, 3, 4): wire the §14.S.1 non-renewed command icons (list +
      count variant) to the endpoint; view with criteria (charity HQ-gated, window), grid, count
      tile, empty state, `OnPush`/`trackBy`; استخراج exports via ExcelJS (9-11 builder reuse)
      — new `NonRenewedReportsComponent` at `orphan-reports/non-renewed` (route +
      `PermissionGuard PeriodicReports.View`), reached by the §14.S.1 register command.
      `countOnly` toggle = the `_Number` variant (count tile + hint, export disabled); grid:
      code/name/family/charity/last-report with client-side paging; ExcelJS export off the
      loaded rows reusing 9-11's column keys; service call added to the existing
      `modules/reports` `ReportService` (`getNonRenewedReports`) + typed models.
- [x] **Task 4 — i18n** — labels under `periodicReports.nonRenewed.*` in **both** `ar.json` and
      `en.json`
      — 15 keys per locale incl. 3 `columns.*`.
- [ ] **Task 5 — Verification** (AC 1–6): live check — seed one charity with orphans A (accepted
      in-window report), B (pending report), C (no report): extract lists B and C only; `countOnly`
      → 2; charity pin holds; empty charity → zero; unauthenticated → 401; `npm run build` green;
      tests excluded per the standing user decision
      — `dotnet build` Application + Api both 0 errors; `npx tsc --noEmit` zero findings for the
      touched modules; `npm run build` pending (batched with the later stories' edits).

## Dev Notes

### Platform rules that bind this story

- Read-only; raw envelope; camelCase wire; pagination clamps if paged (a chase list can be large —
  page it like the 9-1 list).
- Accepted-covering semantics: BR-11 — only an **accepted** report satisfies the prerequisite;
  pending/refused do not clear an orphan from the chase list.
- "Coded" = has a sponsorship `Code` (the 9-2 gate) — exclude uncoded orphans explicitly.
- No new entities/migrations — pure query over existing tables.
- `ReportsController` naming starts EP-18's root: keep it minimal and discoverable.

### Out of scope (later epics/stories — do not build)

| Item | Story |
| --- | --- |
| Batch-scoped variant | EP-10 (TODO marker) |
| The 41 print/report endpoints of EP-18 | EP-18 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.14] scenario (legacy variant
  list)
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.S.1] the two non-renewed command
  icons
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-14 acceptance criteria
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] EP-18 — the api/Reports root this story
  founds

## Dev Agent Record

### Agent Model Used

Claude (GLM-5) via Claude Code — `/bmad-dev-story` pass over EP-09.

### Debug Log References

- `dotnet build Backend/src/IIROSA.Application/IIROSA.Application.csproj` — Build succeeded.
- `dotnet build Backend/src/IIROSA.Api/IIROSA.Api.csproj` — Build succeeded.
- `npx tsc --noEmit` — zero findings filtered for the touched modules.
- Full `npm run build` pending — batched with the later stories' frontend edits.

### Completion Notes List

- **Story premise corrected**: "no ReportsController anywhere" was stale — the families epic
  (UC-FAM-14 print sheets + UC-RPT-01) had founded `api/Reports`, `IReportService` and its DI
  registration. This story extended that root rather than founding it (and the story's
  Task 1 guidance — thin, no module logic — still applied verbatim).
- **Variant collapse honoured**: one `POST /api/Reports/non-renewed-reports` with `countOnly`;
  V2/begin-screen layouts are presentation over the same query; no forked endpoints.
- **Covering semantics**: an accepted report covers the window when its period intersects
  [DateFrom, DateTo] (inclusive end date); a report whose period is incomplete falls back to its
  ReportDate inside the window. Only `Reviewed && IsAccepted` clears an orphan (BR-11).
- **"Active" reading**: the entity carries no Active flag — coded + `!IsDeleted` is the chase
  population; recorded rather than inventing a predicate.
- **LastReportDate** is any-state (an accepted one would have cleared the orphan) — a correlated
  subquery in the projection, not a per-row round trip.
- **countOnly UX**: count tile + hint line, export disabled while counting (nothing to produce
  otherwise).
- **Row cap**: one page of 100 per run in list mode (server-clamped); the count is always the
  full matched count.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `NonRenewedReportsRequestDto`,
  `NonRenewedOrphanRowDto`, `NonRenewedReportsResultDto`
- `Backend/src/IIROSA.Application/Validators/Reports/NonRenewedReportsRequestValidator.cs` — new
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetNonRenewedReportAsync`
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — periodic-report repo +
  non-renewed validator deps; `GetNonRenewedReportAsync` (scope, accepted-covering predicate,
  countOnly, projection, charity-name resolution)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST non-renewed-reports`
- `Frontend/src/app/modules/reports/models/report.model.ts` — `NonRenewedReportsRequest`,
  `NonRenewedOrphanRow`, `NonRenewedReportsResult`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getNonRenewedReports`
- `Frontend/src/app/modules/periodic-orphan-reports/non-renewed-reports/non-renewed-reports.component.ts/.html/.scss`
  — new chase-list screen (criteria, count tile, grid, countOnly variant, ExcelJS export)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-orphan-reports-routing.module.ts` —
  `orphan-reports/non-renewed` route + PermissionGuard
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-reports-list/periodic-reports-list.component.html`
  — §14.S.1 non-renewed command button
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` —
  `periodicReports.nonRenewed.*` (15 keys per locale)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-14 and module spec §14.U.14; greenfield ReportsController scoped; legacy variant collapse + batch deferral recorded. |
| 2026-08-24 | Implemented on the existing (families-epic) ReportsController root: non-renewed query with accepted-covering predicate + countOnly collapse, validator, service method; chase-list screen with count tile + ExcelJS export; i18n ar+en. Backend 0 errors (Application + Api), tsc clean; full build pending. |
| 2026-08-24 | Verification pass: final gates run — `npm run build` GREEN (epic-9 module compiled; NG8107 optional-chain warnings only) and backend 0 errors for epic-9 code (the only 2 solution errors are the parallel epic-18 session’s in-flight untracked `ReportService.cs` — CS0019 ×2, left untouched per convention). Live API wedged (accepts TCP, empty replies) — restart pending; live walkthrough stays batched. Status ready-for-dev → review. |


### Review Findings (epic review 2026-08-24)

> Review Outcome 2026-08-24 — all items patched & verified: D2 resolution recorded under Review Outcome.

- [x] [Review][Decision] D2 Co-owned-file defects (main-layout menu, auth.service login, ReportService family incl. ApplyCharityScopeAsync fallthrough) — patch under epic-9 or leave to owning sessions? — **resolved 2026-08-24: patch only epic-9's own additions** (ReportService.ApplyCharityScopeAsync now fails closed on missing charity claim); remaining co-owned defects left to their owning sessions
- [x] [Review][Patch] P18 ReportsController catch-alls never log — _logger.LogError [ReportsController.cs]
- [x] [Review][Patch] P19 Page clamp — 11 validators injected, 1 shipped; Page=0 negative skip 500
- [x] [Review][Patch] P17 MezaCards ReportNo=0 → 400 instead of grid — treat 0 as unnumbered [ReportsController]
- [x] [Review][Patch] P40 Non-renewed fetches only page 1 (100 rows) — count>100 silently truncated; export ships partial as complete [non-renewed-reports.component.ts:106-118]
- [x] [Review][Patch] P42d Pagination clamp [non-renewed-reports.component.ts:133]
- [x] [Review][Patch] P30e HQ charity dropdown markForCheck [non-renewed-reports.component.ts:73]
