# Story 19-4: Load a generic lookup list

| Field | Value |
| --- | --- |
| Story key | `19-4-load-a-generic-lookup-list` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-04 — القوائم المرجعية |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.4 scenario) |
| Route | `#/lookup-management` (+ countries/regions/centers/departments children) |
| Endpoint | the catalogue family on `GET /api/LookupManagement/*` (dedicated-endpoint ruling below) |
| Depends on | EP-03 lookups module (live); this story is the recorded **lookup sweep** owner (page-cap + export gating deferrals land here) |
| Roles | All roles read; catalogue writes stay per-action SuperAdmin/Admin |

## Status

done

## Story

As a signed-in user, I want to be able to load a generic lookup list القوائم المرجعية, so that
I can find the record I need without leaving the system.

## Acceptance Criteria

1. Given a signed-in user with an active session, when a host screen loads, then the reference
   lists that screen needs are served by their dedicated catalogue endpoints — no stored data
   is changed.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/LookupManagement/<catalogue>` and the response is rendered on the screen without a
   page reload.
3. Given a catalogue exceeds the page the client asked for, when the read is served, then the
   result's `totalCount` is the true row count — no silent truncation without a detectable
   signal.
4. Given a non-privileged role, when the bulk `tables/{tableName}/export` or `tables/summary`
   endpoints are invoked, then the request is refused — bulk table export is HQ-admin only.
5. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

> **Endpoint ruling (recorded deviation):** the board names a single "generic"
> `GET /api/LookupManagement` by list id. The shipped platform pattern is **one typed endpoint
> per catalogue** (~50 live today: countries, regions, centers, departments, banks,
> education-levels, health-statuses, refuse-reasons, the refugee-form set…), each returning
> `LookupPagedResult`/`List<Dto>` with `NameAr/NameEn/IsActive`. That pattern is the
> platform's generic mechanism — building a by-id reflection endpoint alongside it would be a
> duplicate capability (PRD §7 non-goal). Do **not** build one.

**Definition of done:** the §24.U.4 scenario passes for the existing catalogue family; the
page-cap behaviour is normalised with a detectable signal; the bulk export/summary pair is
role-gated; the rulings are recorded here for every later story that consumes lookups.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Controller | `Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs` | Rich and live: tables/summary + tables/{name}/export, countries CRUD (7), regions (5, incl. `by-country/{id}`), centers (5, incl. `by-region/{id}`), departments (4), banks (7), cheque support (3), housing (2), education-levels / health-statuses / refuse-reasons, refugee-form set (7) |
| Service layer | `IIROSA.Application/Services/LookupManagementService.cs` — `LookupServiceBase<TEntity,TDto,TCreate,TUpdate>` generic + `ILookupService`/`ILookupManagementService` | The **19-x pattern** every new lookup must copy (7-file chain: repo interface + typed repo, service interface + base-derived service, DTO trio, profile, DI, controller action) |
| DTO shapes | `DTOs/LookupManagement/LookupDtos.cs` — `LookupDto` (`Id, Name, NameAr, NameEn, IsActive, SortOrder…`), `LookupPagedResult<T>` (`items, totalCount, pageNumber, pageSize, totalPages`) | Live contract — `NameAr ?? NameEn` labelling on the client |
| Frontend | `modules/lookup-management/` — overview (`tables/summary` cards) + countries/regions/centers/departments list screens; `shared/services/lookup.service.ts` caches by URL for `app-drop-down`; `lookup-management.service.ts` carries the full method set | Live |
| Scoping | `CurrentUserService` (`CharityId` claim; `IsHeadOffice` = no charity claim + SuperAdmin/Admin) | Live — but catalogue reads are **global reference data by design** (16-8 ruling); tenancy applies to the *data* that references them, not the catalogue rows |

## Verified defects this story must fix (recorded platform deferrals + audit, 2026-08-25)

1. **Catalogue reads hard-cap `PageSize = 1000` and silently truncate** (epic-15/6/5/7 all
   deferred it here). Normalise in `LookupServiceBase.GetLookupItemsAsync` + the list
   consumers: honour the requested page size within a sane ceiling, and guarantee `totalCount`
   is the **true** count so clients can detect truncation (`totalPages > 1` at the asked size).
   Dropdown consumers that need everything past the cap move to searchable/typeahead select —
   that UI migration stays with the owning screens (epic-16 deferral); this story lands the
   honest signal.
2. **Bulk read-widening on `tables/{tableName}/export` + `tables/summary`** (epic-15
   deferral): class-level auth lets any authenticated role bulk-export any lookup table. Gate
   the export/summary pair behind `SuperAdminOnly` (the per-action policy idiom already used
   for writes).
3. **Mid-flight parallel work on this controller.** The working tree carries an uncommitted
   rewrite (class-level `SuperAdminOnly` → plain JWT + per-action policies, ~+400 lines).
   Before building: read the current file, confirm **every** write action — bulk
   `tables/{tableName}/import` above all — carries a per-action policy, and record the state
   here. Do not commit or revert another session's work; build on top of it.
4. **Seed gaps surface here:** `HealthStatus` (and other form catalogues) render empty on
   fresh databases until rows are added via lookup management (epic-7 note). Not this story's
   job to seed every catalogue — but the overview screen must make the emptiness visible
   (item counts per table — verify `tables/summary` cards render `0` correctly).

## Tasks / Subtasks

- [x] **Task 1 — Page-cap normalisation** (AC 1, 3)
  - [x] Audit first: the three paged reads (`LookupServiceBase.GetLookupItemsAsync`,
        `RegionService`, `CenterService`) already compute `totalCount` post-filter — the true
        count. The defect was **no ceiling at all** (any `pageSize` honoured verbatim) plus 13
        dropdown endpoints hard-coding `PageSize = 1000` and returning bare lists. Fix:
        `LookupFilterDto.MaxPageSize = 5000` with setter clamps (`PageSize` → `Math.Clamp(1,
        MaxPageSize)`, `Page` → `≥ 1` — binds every consumer, model-bound or hand-constructed);
        controller `DropdownPageSize = LookupFilterDto.MaxPageSize` replaces all 13 magic values
        (grep-verified 13/13, zero `= 1000` left). No code path drops rows past the ceiling
        without `totalCount`/`totalPages` betraying it.
  - [x] Truncate-detection note landed in `deferred-work.md` (new "19-4 lookup sweep" section;
        the epic-15 ×2, epic-6 and epic-7 sibling entries marked RESOLVED with pointers) —
        dropdown → typeahead migrations stay with the owning screens
- [x] **Task 2 — Gate the bulk pair** (AC 4)
  - [x] `tables/{tableName}/export` now carries `[Authorize(Policy = "SuperAdminOnly")]`
        (minimal-break ruling taken: **export gated, summary readable** — the overview cards
        render for every authenticated role; recorded in the action's doc comment)
- [x] **Task 3 — Parallel-work verification** (AC 5)
  - [x] Audit of the uncommitted rewrite (current tree): class-level = plain JWT; **all 17
        write actions** (countries 5, regions 2, centers 2, departments 2, banks 5) carry
        per-action `SuperAdminOnly` — zero ungated writes. **No `tables/{tableName}/import`
        action exists at all** — `ImportLookupTableAsync` is a `NotImplementedException` stub
        behind no route (recorded in `deferred-work.md`: any future import action must carry
        the same policy). Cheque/housing reads carry role sets; catalogue reads plain
        authenticated — the 16-8 global-reference ruling.
- [x] **Task 4 — Verification**
  - [x] Build clean (warnings pre-existing); live smoke on 61970: `countries?pageSize=1` →
        `{items:1, totalCount:25}` (true-count signal); `?pageSize=99999` → clamped 5000, 25
        rows; `?pageSize=-5` → clamped 1, safe; export → **403 Charity / 200 SuperAdmin /
        401 unauth**; `tables/summary` → 200 as Charity; `refuse-reasons`/`education-levels`
        dropdowns intact after the constant swap. Defect-4 check: `GetTableSummaryAsync`
        computes counts from the full list → empty tables surface as `ItemCount: 0` cards
        (summary's 8-table scope recorded in deferred-work). Tests excluded per the standing
        decision

## Dev Notes

### Platform rules that bind this story

- **No `ApiResponse<T>` on this controller** — raw shapes are the 2026-08-19 standing decision;
  the platform envelope migration is its own story.
- Lookups are `LookupEntity` (int) in `MappingDefaults.LOOKUP_SCHEMA`; no manual `DbSet`;
  labels resolve `NameAr ?? NameEn`; Arabic primary.
- This file is shared by many epics — smallest safe diffs; never reformat the file.
- Never kill the user's running API; smoke on the private port.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Guardian-specific lists (marital status) | 19-5 |
| Refusal reasons audit | 19-6 |
| Geography cascade consumers | 19-7 |
| Banks / jobs / payment categories / NID rules / NID check / error handling | 19-8 … 19-13 |
| New catalogue screens in the admin UI | with each catalogue's own story |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.4] scenario + §24.2 UC-SYS-04 row (the catalogue list)
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-04 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/LookupManagementService.cs] `LookupServiceBase` — the 19-x pattern
- [Source: _bmad-output/implementation-artifacts/deferred-work.md] epic-15 read-widening + PageSize-1000 deferrals (this story is their owner)
- [Source: _bmad-output/implementation-artifacts/epic-16-correspondence-incoming-and-outgoing/16-8-select-the-routing-department.md] departments — HQ-catalogue, read-module-wide ruling

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Private instance 61970 (restarted on the 19-4 build); tokens cached /tmp/ep19-su.txt ·
  /tmp/ep19-ch.txt. Battery output: pageSize 1/99999/-5 → 1+25 / 5000+25 / 1+25; export
  403/200/401; summary-as-Charity 200; dropdown endpoints 200 post-swap.

### Completion Notes List

- The "hard cap" as deferred was half-right: `totalCount` was already true on the paged reads;
  the real gaps were the missing ceiling (any pageSize verbatim) and 13 magic-1000 dropdown
  endpoints. Both closed with the named `MaxPageSize = 5000` + `DropdownPageSize` alias —
  smallest-diff fix, zero service-layer changes, zero frontend contract changes.
- Export gated `SuperAdminOnly`; summary deliberately readable (overview cards) — minimal-break
  ruling recorded on the action and in deferred-work.
- Write-action audit clean: 17/17 gated; no import route exists (stub service method only).
- `LookupFilterDto` clamp is setter-level so it also covers hand-constructed filters inside the
  controller — verified live via the 99999 → 5000 echo.

### File List

- Backend/src/IIROSA.Application/DTOs/LookupManagement/LookupDtos.cs (LookupFilterDto: MaxPageSize const + Page/PageSize clamps)
- Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs (DropdownPageSize const, 13× magic-1000 replaced, export SuperAdminOnly gate)
- _bmad-output/implementation-artifacts/deferred-work.md (19-4 section; epic-15/6/7 sibling entries RESOLVED)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-04 and module spec §24.U.4; dedicated-endpoint ruling recorded; assigned the platform lookup-sweep deferrals (page-cap signal, bulk-export gating, parallel-work audit). |
| 2026-08-25 | Implemented + verified (review-and-complete pass): MaxPageSize 5000 ceiling + true-totalCount signal normalised; 13 dropdown endpoints de-magicked; export gated SuperAdminOnly (summary readable per ruling); 17/17 write-action audit clean, no import route exists; live battery green on 61970. |
