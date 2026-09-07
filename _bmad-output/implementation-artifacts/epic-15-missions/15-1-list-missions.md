# Story 15-1: List missions

| Field | Value |
| --- | --- |
| Story key | `15-1-list-missions` |
| Epic | EP-15 — Missions (المأموريات) |
| Use case | UC-MSN-01 — قائمة المأموريات |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/20-UC-MSN-Missions.md` (§20.S.1 screen, §20.U.1 scenario) |
| Route | `#/missions` |
| Endpoint | `GET /api/MissionManagement/my-missions` |
| Depends on | EP-01 (authentication and role resolution) |
| Roles | Gen. Director, Staff → `SuperAdmin`, `Admin` (controller already `[Authorize(Roles = "Admin,SuperAdmin")]` — consistent, keep) |

## Status

done

## Story

As a General Director, I want to be able to list missions قائمة المأموريات, so that
I can find the record I need without leaving the system.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at `#/missions`, when the actor
   opens the screen, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/MissionManagement/my-missions` and the response is rendered on the screen without a
   page reload, scoped to the caller's country (server-side, pin-never-widen).
3. Given no row matches the criteria, when the search runs, then the grid renders empty and the
   paging control reports zero pages.
4. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §20.S.1 are implemented with their mandatory flags and
lookups; the scenario of §20.U.1 passes end to end; the role and charity/country scoping is
enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

The whole mission vertical exists and the module is registered (`app-routing.module.ts:61`). Both
sides need correction, not creation:

| Layer | File | State |
| --- | --- | --- |
| Entity | `Backend/src/IIROSA.Domain/Entities/Mission.cs` | Exists; has no Charity link (gap, Task 4) |
| Repo | `Backend/src/IIROSA.Infrastructure/Data/Repository/MissionRepository.cs` | Exists; `IncludeNavigationProperties()` omits `AssignedUser` |
| Service | `Backend/src/IIROSA.Application/Services/MissionService.cs` | Exists; filter bugs + wrong `my-missions` semantics + no caller scope |
| API | `Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs` | Exists; `my-missions` endpoint present, hand-parses the NameIdentifier claim |
| Profile/DTO | `MissionProfile.cs`, `DTOs/MissionManagement/Missions.cs` | Exist; `MissionListDto` missing 5 of 13 grid columns |
| Frontend | `Frontend/src/app/modules/missions/**` | Exists; `mission-list`, `mission-form`, `mission-detail`, `my-missions` components + `mission.service.ts` |

## Verified defects this story must fix (found by reading the code, 2026-08-23)

1. **Wire contract dead end-to-end.** `mission.service.ts buildHttpParams()` sends
   `search, missionTypeId, missionTimeTypeId, isCompleted, countryId, regionId, centerId, assignedTo,
   dateFrom, dateTo, page, pageSize` — but `MissionFilterDto` binds `SearchText, FK_MissionTypeId,
   FK_MissionTimeTypeId, FK_CountryId, FK_RegionId, FK_CenterId, FK_UserId, IsMissionCompleted,
   StartDate, EndDate, Page, PageSize`. **Not one filter key matches**, so every filter the UI sends
   is silently ignored. Also the SPA reads `response.pageNumber`; the API sends `page`.
2. **Grid reads alias fields the API never sends** (same defect class story 13-1 fixed in the
   projects grid): `mission.missionTypeName` (wire: `missionType`), `mission.regionName` (wire:
   `region`), `mission.centerName` (wire: `center`), `mission.assignedToName` (wire: `assignedTo`),
   `mission.missionDetails` (not on `MissionListDto` at all) — these columns render `-` since the
   copy.
3. **`my-missions` has the wrong semantics.** `MissionService.GetMyMissionsAsync` hard-pins
   `filter.FK_UserId = userId` (assigned-to-me only). §20.U.1 requires the register list "scoped to
   the caller's charity and country" — the endpoint name is a legacy artefact, not an
   assigned-to-me filter.
4. **Filter expression replacement bug.** `GetMissionsFilteredAsync` builds a combined `&&`
   expression, then when *both* dates are set **replaces it** with a dates-only lambda
   (`MissionService.cs:63-68`), discarding every other filter.
5. **`AssignedUser` never included.** `IncludeNavigationProperties()` includes MissionType,
   MissionTimeType, Country, Region, Center — not `AssignedUser`, so the profile's
   `AssignedTo ← AssignedUser.FullName` always maps null. The اسم القائم column cannot work.
6. **`MissionListDto` cannot serve §20.S.1.** Missing: `EntityName` (الجهه), `Details` (التفاصيل),
   `Village` (الحي), `MissionLocation` (الموقع), `MissionCompletedTxt` (النتيجه) — 5 of the 13
   specified columns.
7. **Dead calls on the list screen.** `loadStatusCounts()` calls `GET /api/missionmanagement/status-counts`
   — no such endpoint (controller has `status-summary`); 404 on every load, and the status dashboard
   is not part of §20.S.1 at all. `getCurrentUserId()` is a TODO returning `''`, so the
   "My missions" toggle filters nothing.
8. **No `Missions.*` entries in `PERMISSION_ROLES`** (`auth.service.ts`) — `PermissionGuard`
   falls back to warn-and-allow (story 13-1 finding). The missions routes use `AuthGuard` with
   `data.roles` instead of the done-module pattern `canActivate: [AuthGuard, PermissionGuard]` +
   `data.permission`.
9. **No charity filter, and no charity dimension on the entity.** §20.S.1 specifies a الجمعية
   drop-down (lookup Charities + كافة الجهات); `Mission` has no `FK_CharityId`.

## Tasks / Subtasks

- [x] **Task 1 — Make `my-missions` the caller-scoped register read** (AC 2)
  - [x] In `MissionService.GetMyMissionsAsync`: drop the `filter.FK_UserId = userId` hard-pin;
        assigned-user stays an *optional* filter (`AssignedToUserId`). Change the signature on both
        `IMissionService` and the implementation from `(Guid userId, MissionFilterDto filter)` to
        `(MissionFilterDto filter)` — do not leave an unused parameter behind
  - [x] Inject `ICurrentUserService` into `MissionService` and add an `ApplyCallerScope` in the
        shape of `OfficeProjectService.ApplyCallerScope` (story 13-1): pin the caller's `countryId`
        claim onto the filter — pin, never widen; a caller without the claim sees all (HQ module)
  - [x] Keep `[Authorize(Roles = "Admin,SuperAdmin")]` as is; remove the controller's manual
        NameIdentifier claim parsing (the service derives the caller via `ICurrentUserService`)
- [x] **Task 2 — Fix the filter pipeline** (AC 2, story 15-2's foundation)
  - [x] Rename `MissionFilterDto` keys to the clean wire names the SPA already sends (13-3
        precedent for `FK_*` → clean): `Search, MissionTypeId, MissionTimeTypeId, CountryId,
        RegionId, CenterId, AssignedToUserId, IsCompleted, DateFrom, DateTo, Page, PageSize` (+
        `CharityId` from Task 4) — update the service expression accordingly
  - [x] Fix the both-dates branch to **combine** with the other predicates, not replace them
        (defect 4)
  - [x] Update `mission.service.ts buildHttpParams` to the final key names; fix the paged mapping
        to read `page` (not `pageNumber`)
- [x] **Task 3 — Serve all 13 columns of §20.S.1** (AC 2)
  - [x] Add `.Include(m => m.AssignedUser)` to `MissionRepository.IncludeNavigationProperties()`
  - [x] Extend `MissionListDto` with nullable `EntityName, Details, Village, MissionLocation,
        MissionCompletedTxt` (straight pass-throughs; profile needs no new mapping for these —
        AutoMapper maps by name; verify `AssignedTo`/`MissionType`/`Region` mappings stay intact)
- [x] **Task 4 — Add the charity dimension (the one schema change)** (AC 2, §20.S.1 filter field)
  - [x] `Mission`: add `Guid? FK_CharityId` + `virtual Charity? Charity` navigation
        (`Charity` is `FullAuditedEntity` → Guid key)
  - [x] `MissionConfiguration`: map the FK + relationship via `MappingDefaults` schema conventions
        (never a literal schema string); add a migration with the CLAUDE.md `dotnet ef migrations
        add` command and apply it
  - [x] Add `Guid? CharityId` to `MissionFilterDto` and the service expression
- [x] **Task 5 — Rebuild the list screen to §20.S.1** (AC 1–3)
  - [x] Filters exactly as specified: الجمعية drop-down (options from the existing
        `GET /api/Charities` — epic 3 is done; prepend the كافة الجهات "all" empty option),
        من تاريخ / الي تاريخ date pickers, بحث command reloading page 1
  - [x] Grid columns in spec order: رقم المأموريه (serial, `(currentPage-1)*pageSize + i + 1`,
        continuous across pages — 13-1 formula) · نوع المأموريه · الجهه · اسم القائم · الهدف ·
        التفاصيل · التاريخ · المحافظه · الحي · الموقع · حاله المأموريه · النتيجه · الاجراءات —
        bound to the REAL wire fields from Task 3
  - [x] Load via `mission.service.getMyMissions(...)` (`GET /api/MissionManagement/my-missions`),
        not `getMissions`
  - [x] Remove the status dashboard + `loadStatusCounts()` (dead endpoint, not in §20.S.1) and the
        `getCurrentUserId()` TODO path
  - [x] Keep the shipped listing shape: bespoke grid + shared `Pagination`, `Breadcrumb`,
        `PageHeader` components (see Dev Notes — data-list is NOT used by any shipped module)
- [x] **Task 6 — Permissions** (AC 4)
  - [x] Routes: `canActivate: [AuthGuard, PermissionGuard]` + `data.permission` =
        `Missions.View` (list/detail), `Missions.Create` (create), `Missions.Edit` (edit) —
        office-development-projects routing is the reference
  - [x] `auth.service.ts PERMISSION_ROLES`: add `Missions.View/Create/Edit` →
        `Admin, SuperAdmin`; `Missions.Delete` → `SuperAdmin` only (UC-MSN-08 is Gen. Director's
        alone — the list's delete action gates on it)
- [x] **Task 7 — i18n**
  - [x] Add the new column/filter keys under the existing `missions` block in **both**
        `ar.json` (line ~805) and `en.json` (line ~773); no hard-coded UI strings
- [x] **Task 8 — Verification**
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; apply the migration
  - [x] Live check (13-1 style): `GET /api/MissionManagement/my-missions` returns camelCase
        `items/totalCount/page` with populated `missionType`, `assignedTo`, `entityName`; date +
        charity filters actually narrow the result; an unauthenticated call is rejected
  - [x] `cd Frontend && npm run build` — 0 errors
  - [x] Tests: excluded per the standing user decision (no test project under `Backend/tests`)

## Dev Notes

### Platform rules that bind this story

- Soft delete is a **global query filter** (`Framework.Core/Data/ModelBuilderExtensions.cs:83`
  applies `HasQueryFilter(!IsDeleted)`) — never add manual `IsDeleted` checks; never defeat it.
- Wire is camelCase — verified empirically in story 13-1; the mission frontend's camelCase reads
  are correct once the alias fields are fixed.
- Bilingual lookups map `NameAr ?? NameEn` — follow `MissionProfile`'s existing convention for any
  new lookup mapping.
- **Do NOT wrap responses in `ApiResponse<T>` for this story.** CLAUDE.md/architecture.md state it,
  but **no live controller does it** (only a `.bak.bak2` stray) — every reviewed-done module
  (epics 11, 12, 13, 16) returns the raw paged envelope with try/catch + anonymous error objects,
  and `architecture.md` §10's rule is "code wins where the client is built on it". Wrapping only
  missions would make the module inconsistent with the platform. Same for the shared `data-list`
  component: mandated by the docs, used by **zero** shipped modules — keep the bespoke grid +
  shared Pagination shape that 13-1 shipped with. Both are recorded platform-level deviations, not
  this story's to resolve.
- **Do NOT touch the write path.** `MissionService.CreateMissionAsync` calls
  `_missionRepository.SaveChangesAsync()` (violates "only IUnitOfWork saves") and has no
  FluentValidation validator. That debt belongs to stories 15-6/15-7 — leave it; scope creep here
  risks regressions in stories that will rewrite those methods anyway.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| `#/missions/create` & `:id/edit` form fixes, validators, UoW conversion | 15-6, 15-7 |
| `GET /api/MissionManagement/{id}` detail read + `mission.service.getMissionById`'s broken `success/data` wrapper expectations | 15-7 |
| Delete flow (`DELETE`, confirmation, `Missions.Delete` gating in the UI) | 15-8 |
| `POST /api/MissionManagement/{id}/event` register-result semantics (currently a PUT "RecordConferenceEntity") + `#/missions/:id/register` screen (unbuilt, spec says planned) | 15-9 |
| `mission-types` / `mission-time-types` catalogue endpoints — `mission-time-types` is a **hardcoded TODO stub** in the controller (`MissionManagementController.cs:435-456`); `LookupManagement/mission-interview-types` doesn't exist yet | 15-3, 15-4, 15-5 |
| `#/missions/my-missions` route + `MyMissionsComponent` — legacy extra, not in the spec's route set; leave untouched | — |
| `markAsCompleted` PATCH / `reopenMission` PATCH `{id}/reopen` / `exportMissions` GET vs POST — dead frontend calls; each lands with its owning story | 15-9 etc. |

### References

- [Source: docs/Modules/20-UC-MSN-Missions.md#20.S.1] screen contract — 3 filters, 13 grid
  columns, 8 commands
- [Source: docs/Modules/20-UC-MSN-Missions.md#20.U.1] scenario — my-missions scoped to caller's
  charity and country
- [Source: _bmad-output/planning-artifacts/epics.md#3.15] US-MSN-01 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-13-office-development-projects/13-1-list-development-projects.md] the reviewed
  reference for: country pin (`ApplyCallerScope`, pin-never-widen), serial column formula,
  `PERMISSION_ROLES` wiring, alias-field-grid defect class, camelCase wire proof, tests-excluded
  decision
- [Source: Backend/src/IIROSA.Application/DTOs/MissionManagement/Missions.cs] current filter DTO
  keys (lines 127-141) — the wire mismatch side A
- [Source: Frontend/src/app/modules/missions/services/mission.service.ts] `buildHttpParams`
  (lines 235-282) — the wire mismatch side B

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

Live verification 2026-08-23 against `dotnet run` (profile https://localhost:60960) with the seeded
SuperAdmin account: unauthenticated `GET /api/MissionManagement/my-missions` → 401; authenticated →
200 `items/totalCount/page`; `missionType` / `assignedTo` / `entityName` populated; `dateFrom` and
`charityId` filters narrow the result. A temporary Console.WriteLine probe in the controller catch
was used to capture the `SqlException` behind the initial 500 (removed after the fix).

### Completion Notes List

- All nine audited defects fixed. `my-missions` is now the caller-scoped register read
  (`GetMyMissionsAsync(MissionFilterDto)` delegates to the fixed filter pipeline with
  `ApplyCallerScope` pin-never-widen); controller claim-parsing removed; all 13 §20.S.1 columns
  served and bound to real wire fields; charity dimension added (entity + config + migration
  `20260823135202_Epic15_Missions`); `PERMISSION_ROLES` gained `Missions.*` with Delete =
  SuperAdmin only; list screen rebuilt (filters, serial formula, badges, pagination, empty state).
- **Fix beyond the story, found live**: every `my-missions` request 500'd with
  `Invalid column name 'CharityId'/'CountryId'`. Root cause: `ApplicationUser` entered the app
  context via `Mission.AssignedUser` and convention-mapped to `dbo.ApplicationUser` — an empty
  duplicate of the users table that lacked the entity's newer CharityId/CountryId columns, while
  the live users sit in `identity.Users` (owned by `AppIdentityDbContext`, which had been migrated).
  Remapped the entity to `identity.Users` with `ExcludeFromMigrations()` in
  `ApplicationDbContext.OnModelCreating`, plus hand migration
  `20260823154500_MissionAssigneeToIdentityUsers` repointing the physical
  `FK_Mission_ApplicationUser_FK_UserId` (previously referencing the empty table, which would also
  have rejected any create-with-assignee). `assignedTo` now resolves real users.
- Legacy `#/missions/my-missions` route + `MyMissionsComponent` **removed**: dead extra outside the
  spec route set, unbuildable against the corrected service (called removed
  `markAsCompleted`/`getMissionStatusCounts`), referenced nowhere else.
- Lookup seeding: `MissionType`/`MissionTimeType` already held 3 legacy Arabic rows (2026-05-04) —
  preserved; spec-shaped rows seed on fresh DBs only (guards skip existing).
- Platform deviations honoured as recorded in Dev Notes (no `ApiResponse<T>` wrapper, bespoke grid
  + shared Pagination — matches every shipped module; `architecture.md` §10).

### File List

- `Backend/src/IIROSA.Application/Services/MissionService.cs` — scope + filter rewrite
- `Backend/src/IIROSA.Application/Interfaces/IMissionService.cs`
- `Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs`
- `Backend/src/IIROSA.Application/DTOs/MissionManagement/Missions.cs` — filter + list DTO keys
- `Backend/src/IIROSA.Application/Profiles/MissionProfile.cs` — list/detail mappings
- `Backend/src/IIROSA.Infrastructure/Data/Repository/MissionRepository.cs` — AssignedUser include
- `Backend/src/IIROSA.Domain/Entities/Mission.cs` — FK_CharityId + Charity navigation
- `Backend/src/IIROSA.Domain/Configurations/MissionConfiguration.cs`
- `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260823135202_Epic15_Missions.cs`
- `Backend/src/IIROSA.Infrastructure/Data/ApplicationDbContext.cs` — ApplicationUser remap
- `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260823154500_MissionAssigneeToIdentityUsers.cs`
- `Backend/src/IIROSA.Infrastructure/Data/SeedData/MissionLookupSeedData.cs`
- `Frontend/src/app/modules/missions/mission-list/mission-list.component.ts|.html`
- `Frontend/src/app/modules/missions/models/mission.model.ts`
- `Frontend/src/app/modules/missions/missions-routing.module.ts` (+ deleted `my-missions/`)
- `Frontend/src/app/core/services/auth.service.ts` — PERMISSION_ROLES
- `Frontend/src/assets/i18n/ar.json`, `en.json`

### Review Findings

- [x] [Review][Patch] Pagination bounds unclamped — `page` < 1 makes `Skip` go negative (throws),
      `pageSize` = 0 divides by zero in `TotalPages`, and `pageSize` has no upper bound
      [Backend/src/IIROSA.Application/Services/MissionService.cs — paged result mapping] —
      fixed: service clamps page ≥ 1 and pageSize to [1, 200] (and the client's `totalPages` was
      reading a wire field that never exists — now derived with a guarded division)
- [x] [Review][Patch] `dateTo` date-only value binds midnight — missions later on the end day fall
      outside the filter; needs an end-of-day inclusive bound
      [Backend/src/IIROSA.Application/Services/MissionService.cs — date predicate] —
      fixed: bound is now `< DateTo.Date + 1 day`
- [x] [Review][Patch] Deleting the last row of the last page leaves the list on a stale empty page —
      decrement `currentPage` when the page empties
      [Frontend/src/app/modules/missions/mission-list/mission-list.component.ts — delete handler] —
      fixed: steps back one page when the deleted row was the only one on a page > 1
- [x] [Review][Patch] Charity claim never applied to reads — `ApplyCallerScope` pins the country
      only, although §20.U.1 scopes the register to the caller's charity AND country; a
      charity-scoped caller currently sees other charities' rows (HQ callers unaffected — no claim)
      [Backend/src/IIROSA.Application/Services/MissionService.cs:637] —
      fixed: `ApplyCallerScope` and `IsWithinCallerScope` now pin/check both claims
- [x] [Review][Defer] §20.S.1's 8th command (استخراج البيانات / export) is unimplemented and
      unowned — 15-1 deferred it to "15-9 etc.", 15-9 closed without it; needs a board assignment
      [docs/Modules/20-UC-MSN-Missions.md §20.S.1] — deferred, unowned scope
- [x] [Review][Defer] Hand-migration snapshot drift — the next `dotnet ef migrations add` diffs
      against a stale snapshot and may emit a spurious `DropTable("ApplicationUser")`; the junk
      `dbo.ApplicationUser` table also remains
      [Backend/src/IIROSA.Infrastructure/Data/Migrations/20260823154500_MissionAssigneeToIdentityUsers.cs]
      — deferred, pre-existing

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created from `epics.md` US-MSN-01 and module spec §20.S.1 / §20.U.1; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implemented (review-and-complete pass over the copied vertical), verified live against the running API, status → review. |
| 2026-08-24 | Code review: 4 findings fixed (paging clamps + derived totalPages, inclusive `dateTo`, stale-page-after-delete, charity claim on reads); 2 deferred; status → done. |
