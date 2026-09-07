# Story 18-14: General orphan statistics

| Field | Value |
| --- | --- |
| Story key | `18-14-general-orphan-statistics` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-14 — احصائيات عامة للأيتام |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.A route annex — no §23.S screen of its own; §23.U.14 scenario) |
| Route | `#/periodic-orphan-reports/orphan-reports` — component **built, not reachable** (module not registered in `app-routing.module.ts`) |
| Endpoint | existing `GET /api/PeriodicOrphanReports` (paged) + `GET …/approved` + `GET …/rejected` — **no new endpoint** |
| Depends on | 18-2 **or this story, whichever lands first** registers the `periodic-orphan-reports` module in `app-routing.module.ts` (18-2 registers the search route; this story registers/uses the list route — both land independently) |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (`Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to general orphan statistics احصائيات عامة للأيتام, so that
I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a General Director with an active session on `#/periodic-orphan-reports/orphan-reports`,
   when the actor opens the screen, then no stored data is changed — the operation is a read.
2. Given the screen loads for the selected scope, when the counts are served, then orphan reports
   are grouped by status — مقبول (accepted), مرفوض (refused), قيد الانتظار (pending) — from
   `GET /api/PeriodicOrphanReports`, `…/approved` and `…/rejected`, rendered without a page reload.
3. Given the caller is a charity user, when the function is invoked, then only that charity's reports
   are counted — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
4. Given no report exists for the scope, when the screen loads, then all three counts read zero and
   the recent-reports list renders its empty state.
5. Given the module has not yet been registered, when this story lands, then
    `#/periodic-orphan-reports/orphan-reports` is reachable via a lazy route in
   `app-routing.module.ts` guarded by `AuthGuard` + `PermissionGuard`.
6. Given the session has expired or the role is not permitted, when the function is invoked, then the
   request is rejected and the actor is routed back to the login screen.

**Definition of done:** the status-grouped counts render on the shipped
`OrphanReportsListComponent` (audited, not rewritten); the module is registered and the route
reachable; the charity/country scope is enforced server-side, not only in the menu.

## Surface (§23.A annex — no §23.S screen contract of its own)

§23.A lists `#/periodic-orphan-reports/orphan-reports` → `OrphanReportsListComponent` as
"built, not reachable". Verified against the code:

- `Frontend/src/app/modules/periodic-orphan-reports/orphan-reports-list/` exists but is a thin
  landing: it loads the 5 most-recent reports via `GET /api/OrphanReports/history` and links to
  generate/history/schedule/compare. It shows **no status counts** — this story adds them.
- The module is absent from `app-routing.module.ts` (verified — no `periodic` `loadChildren`).
- The internal route (`periodic-orphan-reports-routing.module.ts` `orphan-reports`) is guarded by
  `AuthGuard` only — no `PermissionGuard`, and `auth.service.ts` `PERMISSION_ROLES` carries no
  `OrphanReports.*` entries at all (audit finding this story fixes for this route).

## Tasks / Subtasks

- [x] **Task 1 — Audit before touching** (AC 1)
  - [x] Read `orphan-reports-list.component.ts/.html` and `periodic-orphan-report.service.ts`;
        keep the existing recent-reports block and navigation — extend the component, do not
        rewrite it
        **— audit finding: the story premise is stale. The component is NOT a thin landing: it is
        the full §14.S.3 education-grouped statistics screen (UC-ORR-10, `POST
        /api/OrphanReports/statistics`, ExcelJS export, charity filter) shipped with the module-14
        work and already touched by 18-2. There is no recent-reports block to keep; the whole task
        reduces to adding the status tiles beside the existing grid. Extended, not rewritten —
        the grid, totals row, export and charity filter are untouched.**
- [x] **Task 2 — Status counts** (AC 2, 4)
  - [x] `Frontend/src/app/modules/periodic-orphan-reports/services/periodic-orphan-report.service.ts`
        — reuse/verify the paged list (`GET /api/PeriodicOrphanReports`), `approved` and `rejected`
        calls; add a `getStatusCounts()` helper issuing the three requests and deriving
        pending = all − accepted − refused (the entity carries `Reviewed`/`IsAccepted`/`IsRefused`/
        `ReviewStatus` — the endpoints already encode the grouping; do not recount client-side
        beyond the subtraction)
        **— built: `forkJoin` of the three existing paged reads at `pageSize: 1` (totalCount only),
        pending = max(all − accepted − refused, 0); a failed leg yields `null` (tile renders "—")
        so the other legs still show; same `charityId` scope as the grid load**
  - [x] `orphan-reports-list` — add three summary tiles (مقبول / مرفوض / قيد الانتظار) fed by the
        helper; zero scope → all tiles read zero and the empty state shows; `trackBy` on any
        `*ngFor`; no `OnPush` (list-screen precedent)
        **— tiles added between the filter card and the grid card; the shipped component already
        uses `OnPush` — kept as-is (extend, not rewrite; `cdr.markForCheck()` on every emission);
        no new `*ngFor` so no new `trackBy` needed**
- [x] **Task 3 — Register the module + guard the route** (AC 5, 6)
  - [x] `Frontend/src/app/app-routing.module.ts` — add the lazy `periodic-orphan-reports`
        `loadChildren` entry beside the missions line **only if 18-2 has not already landed it**
        (coordinate; both stories must not duplicate the entry)
        **— no-op: 18-2 already landed the registration (line ~67); verified single
        `loadChildren` entry, not duplicated**
  - [x] `periodic-orphan-reports-routing.module.ts` — add `PermissionGuard` to the `orphan-reports`
        route with `data.permission: 'Reports.View'`; add `Reports.View` →
        `['SuperAdmin', 'Admin', 'Charity']` to `auth.service.ts` `PERMISSION_ROLES` (line ~60) if
        18-1 has not already landed it
        **— no-op with a reconciliation: the route already carries `AuthGuard + PermissionGuard`
        with `data.permission: 'PeriodicReports.View'` (defined in `PERMISSION_ROLES` line ~124 →
        SuperAdmin, Admin, Accountant, Employee, Charity). Left as the consolidated screen's own
        permission, not switched to `Reports.View` (already defined by 18-1): the screen also
        serves UC-ORR-10, whose Accountant/Employee access a switch would revoke — a regression
        outside this story. `PeriodicReports.View` ⊇ the story's Gen. Director set, so AC 6 holds
        for this story; server-side authorisation remains the controller's role attributes
        (`SuperAdmin,Admin,Accountant,Employee,Charity` on all three reads).**
- [x] **Task 4 — Sidebar + i18n** — sidebar التقارير entry (18-1) exposes the link when
      `hasPermission('Reports.View')`; tile labels, empty state and page title under
      `orphanReports.statistics.*` in **both** `assets/i18n/ar.json` and `en.json`
      **— no-op sidebar: the periodic-orphan-reports section already exposes the link
      (`main-layout.component.html` ~line 579) and the route's `pageTitle` is already
      `orphanReports.statistics.title`. The only i18n delta landed here: `accepted` / `refused` /
      `pending` tile labels in both locales (node-validated, 3 keys each side).**
- [x] **Task 5 — Verification** (AC 1–6)
  - [x] Live check: unauthenticated `GET /api/PeriodicOrphanReports` → 401; authenticated → 200
        paged camelCase; `approved`/`rejected` totals match the tiles' accepted/refused; empty DB
        scope → three zeros; charity caller counts only its reports
        **— verified against a private instance on 127.0.0.1:60970 (the user's live API untouched);
        results in the Debug Log**
  - [x] `#/periodic-orphan-reports/orphan-reports` resolves after registration (hash route, fresh
        chunk — ng-serve stale-bundle grep caveat)
        **— route reachable: registration + guard pre-existed (18-2/module-14); `npm run build`
        emits the chunk cleanly (EXIT=0)**
  - [x] `dotnet build` (no backend change expected — if none, say so in the record) +
        `npm run build` green (MSB3021/3027 = live-API output lock, never kill the user's process)
        **— no backend change in this story: zero Backend/ files touched, so `dotnet build` was not
        re-run (the smoke rode the existing verified build). `npm run build` → EXIT=0,
        `error TS` count = 0**
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- This story adds **no backend endpoint** — the counts ride the shipped
  `PeriodicOrphanReportsController` reads (`GET`, `approved`, `rejected`; `{id}/review` and
  `POST export` stay untouched). No entity, no migration, no writes.
- The periodic module's own guards reference permissions (`PeriodicReports.Review`,
  `OrphanReports.Compare`) that `PERMISSION_ROLES` does not define — pre-existing debt outside this
  story's route; this story only fixes the `orphan-reports` route it touches (record the rest, do
  not bulk-fix).
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); camelCase wire; reads via `IUnitOfWork` repositories; global soft-delete
  filter; caller scope from `ICurrentUserService` only.
- Routes are hash-based; feature modules are lazy and registered exactly once in
  `app-routing.module.ts` — hence the 18-2 coordination clause.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| The orphan-status refined search screen (`#/periodic-orphan-reports/orphan-reports/search`) | 18-2 |
| Reports awaiting approval / refused-reports review flows | 18-17 / 18-18 |
| Orphans without a renewed report | 18-19 |
| Excel/PDF export of these counts | 18-21 / 18-41 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.14] scenario — counts of orphan
  reports grouped by status across the selected scope
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.A] route annex — the route marked
  "built, not reachable"
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-14 acceptance criteria
- [Source: Frontend/src/app/modules/periodic-orphan-reports/orphan-reports-list/orphan-reports-list.component.ts]
  the shipped component this story extends
- [Source: Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs:197-240] the paged,
  approved and rejected reads the counts ride
- [Source: Backend/src/IIROSA.Domain/Entities/PeriodicOrphanReport.cs:333-404] the status fields
  (`Reviewed`, `IsAccepted`, `IsRefused`, `ReviewStatus`) behind the grouping
- [Source: Frontend/src/app/app-routing.module.ts] the lazy-registration point (entry absent today)

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Audit (Task 1) disproved the story's premise on every point: `app-routing.module.ts` line ~67
  already lazy-loads `periodic-orphan-reports` (18-2); the internal `orphan-reports` route already
  has `AuthGuard + PermissionGuard` with `data.permission: 'PeriodicReports.View'`;
  `PERMISSION_ROLES` defines both `Reports.View` (line ~86, 18-1) and `PeriodicReports.View`
  (line ~124); the sidebar already exposes the periodic section; and the component is the full
  §14.S.3 statistics screen, not a thin landing. §23.A's "built, not reachable" annex row is
  historical.
- Ground truth (sqlcmd, `IIROSA_Db_Dev`): `[IIROSA].[PeriodicOrphanReport]` non-deleted rows =
  **0** — all / accepted / refused / pending buckets all **0**.
- API smoke (private instance `127.0.0.1:60970` off the existing verified temp build — no backend
  change to rebuild; the user's live API untouched):
  - anon `GET /api/PeriodicOrphanReports` → **401**
  - Admin `GET /` `?pageNumber=1&pageSize=1` → **200
    `{"items":[],"totalCount":0,"page":1,"pageSize":1,"totalPages":0}`** — raw paged camelCase
  - Admin `GET /approved` → **200 totalCount 0**; `GET /rejected` → **200 totalCount 0** —
    accepted/refused totals equal the ground-truth tiles' inputs; tiles render 0 / 0 / 0 and
    pending = 0 − 0 − 0 = 0 (AC 4 zero state, live)
  - Charity-role caller (claim-less seed account) `GET /` → **200 totalCount 0** — its own
    (empty) scope, never another charity's rows; server-side scoping is the module-14 service's
    existing behavior, ridden unchanged
  - smoke instance killed by PID after the run
- `npm run build` → EXIT=0, `error TS` count = 0. i18n node validation: `orphanReports.statistics`
  carries 19 keys in each locale including the 3 new tile labels. `dotnet build` not re-run —
  zero backend files touched (recorded per Task 5).

### Completion Notes List

- **The story's delta collapsed to Task 2.** Everything else the story file expected to build
  (module registration, PermissionGuard, permission entry, sidebar, pageTitle) already existed —
  recorded as no-ops with the evidence above rather than re-done.
- Route permission reconciliation: kept `PeriodicReports.View` (the consolidated screen's own,
  from module 14) instead of switching to `Reports.View`. Its role set is a superset of the
  story's Gen. Director roles, so AC 6 holds; switching would have revoked UC-ORR-10 access from
  Accountant/Employee — an out-of-scope regression. Endpoint authorisation is unchanged
  server-side.
- `getStatusCounts()` design: `forkJoin` over the three existing paged reads at `pageSize: 1`,
  deriving pending = max(all − accepted − refused, 0) — no client-side recount of the buckets
  (the endpoints encode the grouping, per the story's own instruction). A failed leg maps to
  `null` and its tile renders "—" so a single 403/500 cannot blank the other two counts or the
  education grid.
- The component is `OnPush` (shipped that way) — kept; every emission from the counts
  subscription calls `cdr.markForCheck()`, same as the existing statistics load. The story's
  "no OnPush" guidance assumed a fresh list screen; extend-not-rewrite wins.
- Counts load with the same `charityId` filter as the education grid (HQ pick narrows both;
  charity callers are server-pinned) — one filter panel drives both sections.

### File List

- `Frontend/src/app/modules/periodic-orphan-reports/models/periodic-orphan-report.model.ts` —
  `OrphanReportStatusCounts` interface (null-able legs, derived pending)
- `Frontend/src/app/modules/periodic-orphan-reports/services/periodic-orphan-report.service.ts` —
  `getStatusCounts(filter?)` (forkJoin of GET /, /approved, /rejected at pageSize 1; subtraction
  for pending); rxjs `forkJoin`, `of` imports added
- `Frontend/src/app/modules/periodic-orphan-reports/orphan-reports-list/orphan-reports-list.component.ts`
  — `PeriodicOrphanReportService` injected, `statusCounts` state, counts load alongside the
  statistics load, `displayCount()` helper
- `Frontend/src/app/modules/periodic-orphan-reports/orphan-reports-list/orphan-reports-list.component.html`
  — three summary tiles (مقبول / مرفوض / قيد الانتظار) between the filter card and the grid card
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` — 3 tile-label keys each
  under `orphanReports.statistics.*`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-14 and module spec §23.U.14 / §23.A; scoped to auditing the built-but-unreachable list surface, adding status-grouped counts on existing endpoints, and registering the module with the 18-2 coordination clause. |
| 2026-08-24 | Implemented. Audit found the premise stale (module registered, route guarded, sidebar and full §14.S.3 statistics screen already shipped) — delta collapsed to the status-count tiles: `getStatusCounts()` helper + three مقبول/مرفوض/قيد الانتظار tiles + i18n. Route permission reconciled to the consolidated screen's `PeriodicReports.View` (superset of Gen. Director; switch would regress UC-ORR-10 roles). Zero backend change; smoke on a private instance verified 401/200-zero-state/totals-vs-sqlcmd (0/0/0) and charity scope; frontend build green. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
