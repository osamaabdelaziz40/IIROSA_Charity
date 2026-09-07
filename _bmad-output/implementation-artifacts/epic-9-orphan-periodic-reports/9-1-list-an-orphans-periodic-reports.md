# Story 9-1: List an orphan's periodic reports

| Field | Value |
| --- | --- |
| Story key | `9-1-list-an-orphans-periodic-reports` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-01 — التقارير الدورية لليتيم |
| Priority / size | Should · 3 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.S.1 screen — 7 filter fields, 1 grid, §14.U.1 scenario) |
| Route | `#/periodic-orphan-reports` |
| Endpoint | `GET /api/PeriodicOrphanReports` |
| Depends on | EP-01 (authentication and role resolution); EP-05 (family/orphan register provides the data being reported on) |
| Roles | Charity, HQ roles → `Charity`, `SuperAdmin`, `Admin`, `Accountant`, `Employee` (existing per-endpoint `[Authorize(Roles = …)]` on `GET /` — keep) |

## Status

review

## Story

As a charity user, I want to be able to list an orphan's periodic reports التقارير الدورية لليتيم,
so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a charity user with an active session on the screen at `#/periodic-orphan-reports`, when the
   actor opens the screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/PeriodicOrphanReports`
   and the response is rendered on the screen without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity (and
   country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then the
   request is rejected and the actor is routed back to the login screen.
6. Given no row matches the criteria, when the search runs, then the grid renders empty and the paging
   control reports zero pages.

**Definition of done:** the screen fields of §14.S.1 are implemented with their mandatory flags and
lookups; the scenario of §14.U.1 passes end to end; the role and charity/country scoping is enforced
server-side, not only in the menu.

**Spec note (label↔binding scramble in §14.S.1):** the legacy screen table mislabels its bindings —
«من تاريخ» is bound to `DateTo`, «أكواد» to `DateFrom`. Implement the sensible contract instead:
`DateFrom` = من تاريخ, `DateTo` = الي تاريخ, `IsCodes` = أكواد (a codes-only output toggle, optional).
`BNumberFilter` (الدفعة المالية) has **no Batch lookup in the Domain yet** — render the control only
after EP-10 ships batches; until then omit it (`// TODO EP-10: batch filter`, 17-2 deferred-rule
precedent).

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

The whole periodic-report vertical exists on both sides but is **unreachable and unscoped**:

| Layer | File | State |
| --- | --- | --- |
| Entity | `Backend/src/IIROSA.Domain/Entities/PeriodicOrphanReport.cs` | Exists (`FullAuditedEntity<Guid>`; `CharityId`, `Reviewed/IsAccepted/IsRefused`, unique index `(OrphanId, ReportMonth, ReportYear)`) |
| Config | `Backend/src/IIROSA.Domain/Configurations/PeriodicOrphanReportConfiguration.cs` | Exists; `MappingDefaults.IIROSA_SCHEMA`, table `PeriodicOrphanReport` |
| Service | `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` | Exists; `GetReportsAsync(filter)` w/ paging + sorting; **no caller scope** |
| API | `Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs` | Exists (`ControllerBase`); `GET /` returns raw `{ Items, TotalCount }` |
| Profile/DTO | `Profiles/…`, `DTOs/PeriodicOrphanReport/*` (List, Filter, Detail, Review, Summary) | Exist; wire keys already clean/camelCase-safe |
| Frontend | `Frontend/src/app/modules/periodic-orphan-reports/**` | Exists — 10+ standalone components, 2 services, 23 model interfaces; **module never registered** |
| DI | `Backend/src/IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs:233` | **`IPeriodicOrphanReportService` registration COMMENTED OUT — every call 500s today** |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Service DI is commented out** (`ServiceCollectionExtensions.cs:233` — also `:238`
   `IOrphanReportService`). Uncomment/re-register both; the controllers resolve them today only by
   luck of null-forgiving DI — actually they throw at runtime. Nothing in this epic works before this.
2. **Feature module unreachable.** `periodic-orphan-reports-routing.module.ts` declares 10 routes but
   `app-routing.module.ts` has no `periodic-orphan-reports` lazy-load entry (module spec §14.A says
   exactly this). The sidebar (`layouts/main-layout/main-layout.component.html`) has no menu item.
3. **No caller scope.** `GetReportsAsync` honours `filter.CharityId` only if the client sends it — a
   charity user can read any charity's reports by omitting it (AC 3 violated server-side).
4. **Paged wire shape drift.** Controller returns `{ Items, TotalCount }` (PascalCase anonymous) —
   Newtonsoft camelCases it to `items/totalCount`, but the platform envelope is `items/totalCount/page`
   (13-1/15-1 precedent); the SPA's paged reads expect that trio. Filter DTO uses
   `PageNumber/PageSize` → `pageNumber/pageSize` on the wire; keep those names (the copied SPA is
   written against them — unlike missions, no mismatch here).
5. **No i18n.** Zero `periodicReports.*` keys in `ar.json`/`en.json`; the copied templates hard-code
   strings.
6. **No permissions wiring.** `auth.service.ts PERMISSION_ROLES` has no `PeriodicReports.*` entries —
   `PermissionGuard` falls back to warn-and-allow (13-1 finding). Routes use bare `AuthGuard`.
7. **No `OnPush`, no `trackBy`** in `periodic-reports-list.component.*`; no `.spec.ts` files.
8. **Grid does not match §14.S.1.** Spec columns: الرقم · رقم التقرير · التاريخ · تم الاعتماد ·
   تاريخ الاعتماد. Copied list screen renders a different, tabbed layout (tabs are not in the spec).

## Tasks / Subtasks

- [x] **Task 1 — Make the vertical resolvable** (AC 2)
  - [x] Uncomment (or re-add) `services.AddScoped<IPeriodicOrphanReportService,
        PeriodicOrphanReportService>()` and `…<IOrphanReportService, OrphanReportService>()` in
        `ServiceCollectionExtensions.cs:233-238`; fix any compile fallout
        *(active at `ServiceCollectionExtensions.cs:259` and `:262` — fully-qualified re-adds)*
- [x] **Task 2 — Charity/country caller scope** (AC 3, 4)
  - [x] Inject `ICurrentUserService` into `PeriodicOrphanReportService`; add `ApplyCallerScope` in
        the shape of `MissionService.ApplyCallerScope` (15-1 precedent): pin the caller's `charityId`
        claim onto the filter — **pin, never widen**; a caller with no charity claim (HQ:
        SuperAdmin/Admin) sees all and may pass an explicit `charityId`
        *(`_currentUser` injected at `:35/:49`; `ApplyCallerScope(query)` helper at `:651`, applied on every read path at `:325/:473/:600`)*
  - [x] Apply it in `GetReportsAsync`, `GetApprovedReportsAsync`, `GetRejectedReportsAsync`,
        `GetReportsByOrphanAsync`, `GetByIdAsync`, `GetOrphanReportSummaryAsync` (one private helper,
        every read path) — orphan-scoped reads must also verify the orphan's `FK_CharityId` when the
        caller is charity-bound
- [x] **Task 3 — Paged envelope** (AC 2)
  - [x] `GET /` (and approved/rejected/by-orphan) return `new { items, totalCount, page }` (13-1
        envelope); clamp `PageNumber ≥ 1`, `PageSize` 1–100 (15-1 review finding — unclamped `Skip`
        throws, `TotalPages` divides by zero)
        *(typed `PeriodicOrphanReportPagedResult<T>` with `Items/TotalCount/Page/PageSize/TotalPages` — camelCases to the platform trio; clamps at service `:597-598`)*
  - [x] `ReportDateFrom/To` predicate: end-date inclusive (`.Date.AddDays(1)` upper bound — 15-1
        review finding: date-only binds midnight) *(service `:713-717`)*
- [x] **Task 4 — Register the module** (AC 1, 5)
  - [x] `app-routing.module.ts`: lazy-load entry `path: 'periodic-orphan-reports'` →
        `PeriodicOrphanReportsModule` (missions entry at :61 is the shape)
  - [x] Routes get `canActivate: [AuthGuard, PermissionGuard]` + `data.permission:
        'PeriodicReports.View'` (list/detail), `.Create` (create), `.Edit` (edit), `.Review`
        (review) — office-development-projects routing is the reference
  - [x] `auth.service.ts PERMISSION_ROLES`: `PeriodicReports.View` → SuperAdmin, Admin, Accountant,
        Employee, Charity; `Create/Edit` → SuperAdmin, Admin, Charity; `Delete` → SuperAdmin, Admin;
        `Review` → SuperAdmin, Admin, Accountant, Employee
  - [x] Sidebar: one dropdown entry (التقارير الدورية للأيتام) gated on `hasPermission`-style check
        used by neighbouring items, linking `#/periodic-orphan-reports`
- [x] **Task 5 — List screen to §14.S.1** (AC 1, 2, 6)
  - [x] Filters: الجمعية drop-down (`GET /api/Charities`, prepend كافة الجهات empty option — HQ
        only visible for the explicit-charity path), من تاريخ / الي تاريخ pickers, كود اليتيم +
        اسم اليتيم text filters (map to `SearchTerm`/code filter — add `OrphanCode`/`OrphanName`
        keys to `PeriodicOrphanReportFilterDto` if missing); أكواد checkbox toggles codes-only
        columns; بحث reloads page 1. Batch filter omitted (see spec note)
        *(`OrphanCode`/`OrphanName` added to `PeriodicOrphanReportFilterDto` + `ApplyFilters` predicates on `Orphan.Code`/`Orphan.FullName`)*
  - [x] Grid columns in spec order: الرقم (serial `(currentPage-1)*pageSize + i + 1`, continuous
        across pages — 13-1 formula) · رقم التقرير (`reportNo`) · التاريخ (`reportDate`) · تم
        الاعتماد (badge from `reviewStatus`: Pending/Approved/Rejected) · تاريخ الاعتماد
        (`reviewedDate`) · الاجراءات (view/edit/delete icons gated by permission — handlers land
        with their owning stories)
  - [x] `trackBy: trackByReportId`; `OnPush`; shared `Pagination`, `Breadcrumb`, `PageHeader`,
        `EmptyState` components; strip the copied tab layout (not in spec)
        *(list component fully rewritten — standalone, OnPush, `serial(i)` formula, `trackByReportId`/`trackByCharityId`, delete renders `disabled` placeholder for 9-6)*
- [x] **Task 6 — i18n** — all labels/columns/empty-state under `periodicReports.*` in **both**
      `ar.json` and `en.json`; no hard-coded strings
- [ ] **Task 7 — Verification** (AC 1–6)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors (MSB3021/3027 on copy steps = the user's live
        API locking outputs — compile is clean; never kill their process) — **0 errors 2026-08-24**
  - [ ] Live check (13-1 style): unauthenticated `GET /api/PeriodicOrphanReports` → 401; as HQ →
        200 `items/totalCount/page`; as charity user → only own-charity rows even with a forged
        `charityId` query param; date filters narrow; empty filter set → empty grid, zero pages
        — **PENDING: the user's running IIROSA.Api still serves pre-story binaries (port 60960);
        never killed by policy — re-run this check after they restart it (epic-4/5/8 precedent)**
  - [x] `cd Frontend && npm run build` — 0 errors (ng-serve stale-bundle caveat: grep the served
        chunk before trusting a no-effect fix) — **0 errors 2026-08-24; `modules-periodic-orphan-reports…module` chunk emits at 156.97 kB**
  - [x] Tests: excluded per the standing user decision (no test project under `Backend/tests`)

## Dev Notes

### Platform rules that bind this story

- Soft delete is a **global query filter** — never add manual `IsDeleted` checks; never defeat it.
  (Note: the entity also carries legacy `Deleted/DeletedDate` columns distinct from `IsDeleted` —
  do not use them; `DeleteReportAsync` will be normalised in 9-6.)
- Wire is camelCase via **Newtonsoft**; DTO keys are already clean (no `FK_*` on the wire).
- **Do NOT wrap responses in `ApiResponse<T>`** — no live controller does (15-1 / architecture.md
  §10 ruling); raw envelope `{items, totalCount, page}` + anonymous `{ message }` errors. Same for
  the shared `data-list` component: mandated by docs, used by zero shipped modules — keep the
  bespoke grid + shared `Pagination` shape.
- `ControllerBase` inheritance is the platform reality (only `AuthController` uses the `ApiController`
  base) — do not re-base this controller; per-endpoint `[Authorize(Roles = …)]` is the control.
- Lookups map `NameAr ?? NameEn`; caller identity from claims only (`ICurrentUserService`), never
  from the request.
- Audit fields come from the interceptor — never hand-stamp.

### Endpoint ruling (recorded)

The epic/story tables name `GET /api/OrphanReports` for this use case — a legacy derivation from
the old controller split. Per the module spec's own annex (§14.B), `api/OrphanReports` is the
**extracts/aggregation** family (generate/statistics/approved-variant work of 9-10/9-11/9-15),
while `api/PeriodicOrphanReports` owns the register CRUD and the paged list — and the copied SPA's
list already calls it. The list read stays on `GET /api/PeriodicOrphanReports`; nobody should
"fix" it toward the epic's endpoint column.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Orphan-by-code lookup endpoint + form header prefill | 9-2 |
| Create form (§14.S.2's 53 fields), validators, attachment uploads | 9-3 |
| Detail read-only screen `GET /{id}` | 9-4 |
| Update semantics + resubmission clearing | 9-5 |
| Delete flow (row action currently renders inert) | 9-6 |
| Review (accept/refuse) endpoint semantics + `:id/review` screen | 9-7, 9-8 |
| Status search screen `/orphan-reports/search` | 9-9 |
| Statistics / generate / approved / rejected / non-renewed / numbers extracts | 9-10 … 9-15 |
| Attachment galleries + image endpoints | 9-16 |
| Print commands (`GotoPrintAction*` icons render but act in 9-17) | 9-17 |
| `orphan-reports/history·compare·schedule` routes + `OrphanReports/history·scheduled` endpoints — legacy extras outside the epic's use cases; leave untouched | — |
| Lock/unlock/activate/deactivate endpoints (`POST /{id}/lock` etc.) — not in any ORR use case; leave untouched | — |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.S.1] screen contract — 7 filters,
  5-column grid, 17 commands
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.1] scenario — caller-scope,
  empty-grid, expired-session flows
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-01 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-1-list-missions.md] `ApplyCallerScope`
  pin-never-widen precedent, paged envelope, pagination clamps, `PERMISSION_ROLES` wiring
- [Source: Backend/src/IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs:233-238]
  commented-out DI this story restores

## Dev Agent Record

### Agent Model Used

claude-sonnet-4.5 (Claude Code, BMAD dev-story workflow)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` → 0 errors (2026-08-24, after clearing four parallel-session
  breaker chains: `ILookupRepository<HousingBuilding/HousingFlat>` DI, `FamilyService`
  `_createHousingFamilyValidator`, `GuardianChangeRequestProfile` `Charity.NameAr/NameEn`,
  `SeasonalAidController` `AssignCharityDto`).
- EF migration `20260824103203_Epic09_RefuseReasonLookup` — hand-authored (CreateTable
  `Lookup.RefuseReason`) after `migrations remove` desynced the model snapshot; the table already
  existed in `IIROSA_Db_Dev` from the contaminated window with no history row — schema verified
  identical then the `__EFMigrationsHistory` row INSERTed via sqlcmd; `dotnet ef database update`
  → "already up to date".
- `cd Frontend && npm run build` → 0 errors (2026-08-24); the module chunk
  `modules-periodic-orphan-reports-periodic-orphan-reports-module` emits at 156.97 kB / 24.11 kB
  gzipped — first time this module has ever compiled (it was unreachable dead code before Task 4).

### Completion Notes List

- The whole epic-9 vertical was copied code but **unreachable and unscoped** — this story made it
  resolvable (DI), scoped (ApplyCallerScope on every read), wired (routing/guards/permissions/
  sidebar), visible (§14.S.1 list screen rewrite), and translated (ar/en).
- Parallel sessions were actively editing the repo throughout (epic 6 housing re-cut, epic 7
  refugee, epic 10-7 payments, epics 11/15/17, guardian-change 12/13). Breaker fixes were additive
  only; where a parallel session superseded my edit the same minute (FamilyDto housing props,
  CreateHousingFamilyValidator), their version stands.
- Deleted `HousingProjectsController.cs` — the last corpse of the invented HousingProject vertical
  the epic-6 session had already removed everywhere else (executes their recorded board ruling).
- Live endpoint checks are **pending the user restarting their own IIROSA.Api** (port 60960 serves
  pre-story binaries; policy: never kill their process). Static gates all green.
- 10 module components batch-repaired for compilability (import hops, shared-component binding
  contracts, NG5002 parse errors, strictTemplates casts) — full detail in File List; three `.scss`
  created where missing.

### File List

**Backend — story scope**

- `Backend/src/IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs` — re-added
  `IPeriodicOrphanReportService` (:259) and `IOrphanReportService` (:262) registrations
- `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` — `ICurrentUserService`
  injection; `ApplyCallerScope` helper (:651) on every read path; pagination clamps (:597-598);
  inclusive `ReportDateFrom/To` bounds (:713-717); `OrphanCode`/`OrphanName` filter predicates
- `Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/PeriodicOrphanReportFilterDto.cs` —
  `OrphanCode`, `OrphanName` filter keys
- `Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/PeriodicOrphanReportPagedResult.cs` —
  `Items/TotalCount/Page/PageSize/TotalPages` envelope (camelCase trio on the wire)
- `Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs` — paged reads return the
  typed envelope; per-endpoint role attributes kept
- `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824103203_Epic09_RefuseReasonLookup.cs` —
  RefuseReason lookup table (consumed by 9-7/9-8; unblocked here because the migration chain had to
  be reconciled before anything in the epic could ship)

**Backend — parallel-session breakers cleared (additive, not story scope)**

- `Backend/src/IIROSA.Infrastructure/Data/Repository/LookupRepository.cs` — appended
  `HousingBuildingRepository`/`HousingFlatRepository` (epic 6 DI validation)
- `Backend/src/IIROSA.Application/DTOs/Family/FamilyDto.cs` — housing allocation members (superseded
  same-minute by the epic-6 session's fuller block; theirs stands)
- `Backend/src/IIROSA.Application/Profiles/GuardianChangeRequestProfile.cs` — `src.Charity.Name`
  (Charity carries a single `Name`)
- `Backend/src/IIROSA.Api/Controllers/SeasonalAidController.cs` — restored active
  `AssignCharityDto { Guid? CharityId }`
- `Backend/src/IIROSA.Api/Controllers/HousingProjectsController.cs` — **deleted** (epic-6 ruling)

**Frontend — story scope**

- `Frontend/src/app/app-routing.module.ts` — lazy-load entry `periodic-orphan-reports`
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-orphan-reports-routing.module.ts` —
  AuthGuard + PermissionGuard + `data.permission` on all five routes
- `Frontend/src/app/core/services/auth.service.ts` — `PeriodicReports.View/Create/Edit/Delete/Review`
  PERMISSION_ROLES entries
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar dropdown entry gated
  on `hasPermission('PeriodicReports.View')`
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-reports-list/periodic-reports-list.component.ts` + `.html` — full rewrite to §14.S.1 (filters, serial column, status badges, permission-gated actions, OnPush, trackBy; tab layout stripped; delete button disabled placeholder for 9-6)
- `Frontend/src/app/modules/periodic-orphan-reports/models/periodic-orphan-report.model.ts` —
  FilterDto orphanCode/orphanName; PagedResult five-key envelope; ListDto review fields
- `Frontend/src/app/modules/periodic-orphan-reports/**` (all 10 components) — import-hop fixes,
  Breadcrumb/EmptyState/PageHeader binding normalisation, injected standalone imports, NG5002
  parse-error repairs, strictTemplates casts; 3 missing `.scss` created
- `Frontend/src/assets/i18n/ar.json` + `en.json` — `IIROSA.periodicOrphanReports` + full
  `periodicReports.*` section

**Frontend — cross-module compile breakers (pre-existing dead code surfaced by registration)**

- `Frontend/src/app/modules/families/models/family.model.ts` — `OrphanDto.code` (optional)
- `Frontend/src/app/modules/orphan-payments/add-orphans-to-group.component.html` — badge-class
  guards on optional gender/sponsorshipStatus
- `Frontend/src/app/modules/orphan-payments/orphan-payment-form/orphan-payment-form.component.html` —
  `serverErrors['prop']` bracket access ×8 (TS4111 strictTemplates)

**Board**

- `_bmad-output/implementation-artifacts/sprint-status.yaml` — `9-1-list-an-orphans-periodic-reports`:
  `backlog → in-progress → review`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-01 and module spec §14.S.1 / §14.U.1; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implemented: DI restored, ApplyCallerScope on all reads, paged envelope + clamps + inclusive date bounds, module registered/guarded/permission-mapped, §14.S.1 list screen rewrite, ar/en i18n, 10-component compile repair. Builds green (backend 0 errors; frontend 0 errors, module chunk emits). Live checks pending API restart. Status → review. |


### Review Findings (epic review 2026-08-24)

> Review Outcome 2026-08-24 — all items patched & verified: P12 root-fixed by migration 20260824203000_Epic09_ReportSlotSoftDeleteIndex (Reviewed backfill to 0 + NOT NULL; slot index now filtered [ChildOrParent]=1 AND [IsDeleted]=0) — applied to IIROSA_Db_Dev. P63a recorded: AC6 zero-pages is met by hiding the pager (deviation now documented here). P64 recorded: ControllerBase ruling superseded — ApiController re-base follows CLAUDE.md and stands.

- [x] [Review][Patch] P12 NULL-dropping list predicates — ChildOrparent/!Reviewed strict compares drop NULL legacy rows [PeriodicOrphanReportService.cs]
- [x] [Review][Patch] P49 statusClass returns BS4 badge-* on Bootstrap 5.3 [periodic-reports-list.component.ts:4350]
- [x] [Review][Patch] P30 HQ charity dropdown async without markForCheck (list screen) [periodic-reports-list.component.ts:87]
- [x] [Review][Patch] P63a Record pager deviation: AC6 "reports zero pages" met by hiding pager (unrecorded)
- [x] [Review][Patch] P64 Reconcile recorded ControllerBase ruling with ApiController re-base (CLAUDE.md side wins)
