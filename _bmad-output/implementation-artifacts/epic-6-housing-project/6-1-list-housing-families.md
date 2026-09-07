# Story 6-1: List housing families

| Field | Value |
| --- | --- |
| Story key | `6-1-list-housing-families` |
| Epic | EP-06 — Housing Project (مشروع الاسكان) |
| Use case | UC-HOU-01 — قائمة الأسر الساكنة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/11-UC-HOU-Housing-Project.md` (§11.S.1 screen, §11.U.1 scenario) |
| Route | `#/housing-projects` |
| Endpoint | `GET /api/Families?familyType=Housing&charityId=` |
| Depends on | EP-01 (authentication and role resolution) — nothing in this epic precedes it |
| Roles | Charity user + HQ roles → `Charity`, `Admin`, `SuperAdmin` |
| Recommended build order | 6-1 → 6-2 → 6-5 → 6-3 → 6-4 → 6-6 → 6-7 → 6-8 |

## Status

done

## Story

As a charity user, I want to be able to list housing families قائمة الأسر الساكنة, so that I can
find the record I need without leaving the system.

## Acceptance Criteria

1. Given a charity user with an active session on the screen at `#/housing-projects`, when the
   actor opens the screen, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Families?familyType=Housing&charityId=` and the response is rendered on the screen
   without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §11.S.1 are implemented with their mandatory flags and
lookups; the scenario of §11.U.1 passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Reality check: the existing `housing-projects` vertical is an INVENTED module — re-cut it, do not extend it

**The board marked this story `done` by endpoint/route NAME matching only.** A semantic audit
(2026-08-24, this context pass) found the vertical that exists under the `housing-projects` name
is a **construction-project tracker** that appears nowhere in chapter 11:

| What exists | What it actually is | What UC-HOU-01 needs |
| --- | --- | --- |
| `HousingProjectsController` (`Backend/src/IIROSA.Api/Controllers/HousingProjectsController.cs`, 20+ actions) | Construction projects: `/projects/active`, `/budget`, `/progress`, `/complete`, `/statistics` | A register of FAMILIES housed in organisation-owned buildings |
| `HousingProject` entity (`Backend/src/IIROSA.Domain/Entities/HousingProject.cs`) | `ProjectType` (New Construction/Renovation/…), `TotalBudget`, `DonorName`, `CompletionPercentage`, `GPSCoordinates` | `Family` rows with a housing discriminator + building/flat allocation |
| `HousingProjectService` + DTOs + repository | Construction lifecycle, no `ICurrentUserService` | `IFamilyService` family-register reads (board crosswalk names `GetAllHousingFamilyById`) |
| Angular `modules/housing-projects` (list/form/detail) | Construction CRUD screens | §11.S.1 housing-families list (3 filter fields, 7-column grid) |

Nothing in chapter 11 has construction projects — capital projects belong to **epic 13 / chapter 18
Office Development Projects**, already delivered on its own `OfficeProjectManagement` stack. The
invented tracker duplicates that capability, which the PRD names a legacy defect (§7: "duplicate
capability variants — one implementation per capability").

**Ruling (epic-11 precedent, `epic-11-closeout.md`):** re-cut both sides to the WAR.IIROSA scope.
Invented construction endpoints/screens/columns are **dropped, not preserved**. This story lands
the re-cut foundation; 6-3 re-cuts the controller, 6-3/6-4 re-cut the form screens.

What genuinely exists and MUST be reused (verified):

| Exists | Where | Use |
| --- | --- | --- |
| Generic family list endpoint | `FamiliesController.GetFamilies` (`FamiliesController.cs:39`) — `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`, takes `[FromQuery] FamilyFilterDto`, calls `GetFamiliesAsync(filter, userCharityId, userRole)` | extend with `FamilyType` — do NOT build a parallel endpoint |
| Charity pinning from the token | `FamiliesController.GetUserCharityId()` (`FamiliesController.cs:843`) reads `IiroSaClaimTypes.CharityId` | AC 3/4 mechanism — keep, verify |
| `FamilyFilterDto` | `DTOs/Family/FamilyFilterDto.cs` — has `SearchTerm` (Code/Address/Father/Mother), `CharityId`, `CountryId`, paging/sort | add `FamilyType` (+ `SearchType` lands in 6-2) |
| Family-file machinery | `modules/families` — `family-list`, `family-form`, father/mother/provider/orphan subforms, `family.service.ts` (43 methods) | the housing form (6-3/6-4) mirrors this module |
| `ICurrentUserService` | `Application/Interfaces/ICurrentUserService.cs` — `.CharityId`, `.CountryId`, `.IsHeadOffice` | service-level scoping if the parameter path proves insufficient |
| ApplyCallerScope reference shape | `OfficeProjectService.cs:384`, `CharityService.cs:570` | pin-never-widen pattern (13-1/15-1 precedent) |
| Sidebar entry + i18n block | `main-layout.component.html:235-259` (مشاريع الإسكان), `ar.json:1278-1400` / `en.json:1266-1388` | keep the entry, rewrite the block's construction keys |

## Epic-wide entity design (decided here, binding on 6-2 … 6-8)

1. **`Family` gains a `FamilyType` discriminator** — enum `FamilyType { Regular = 1, Housing = 2, Refugee = 3 }`
   (`Backend/src/IIROSA.Domain/Enums/` beside the existing enums), stored as `int` on `Family`,
   default `Regular`. The migration **backfills existing rows to `Regular`**. This is a
   cross-epic column: epic 7 (Refugee) and epic 8 stories already assume
   `GET /api/Families?familyType=…`. One column, one migration — do not model housing as a
   separate entity.
2. **Housing allocation columns on `Family`** (land in **6-5** with the lookup tables, consumed by
   6-3/6-4): `FK_HousingBuildingId int?` + `FK_HousingFlatId int?`.
3. **`PeriodicOrphanReport` gains housing-beneficiary support** — a `ChildOrParent`
   discriminator (the spec names it: §11.U.6 "the ChildOrParent discriminator selects which") +
   `FK_HousingFamilyId` nullable link. The **column migration lands in 6-6** (the read filters on
   it); the **create branch lands in 6-8**. Design detailed in those stories.
4. **The invented `HousingProject` construction entity/table is dropped** in the 6-3 migration
   (controller re-cut). Dev DB holds seed/test data only — confirm before dropping (recorded as an
   open question on the epic, not a blocker).

**Roles decision:** UC-HOU's primary actor is the **charity user**; the existing module gates
Admin/SuperAdmin only — wrong for this module. Register reads/writes:
`[Authorize(Roles = "SuperAdmin,Admin,Charity")]` (the `FamiliesController` precedent); delete and
cross-charity operations stay HQ-only.

## Tasks / Subtasks

- [x] **Task 1 — Domain: the `FamilyType` discriminator** (AC 2)
  - [x] `Backend/src/IIROSA.Domain/Enums/FamilyType.cs` — `Regular = 1, Housing = 2, Refugee = 3`
  - [x] `Family` gains `public FamilyType FamilyType { get; set; } = FamilyType.Regular;`
        (audit fields inherited; do not touch the existing `HousingTypeId` — that is the
        living-condition lookup, NOT the register discriminator; leave it alone)
  - [x] `FamilyConfiguration` — map the column beside the existing mappings,
        `MappingDefaults.IIROSA_SCHEMA` (never a literal string); index on `(FamilyType)` only if
        the configuration convention indexes filters (check a sibling first)
- [x] **Task 2 — Migration** (AC 2)
  - [x] `dotnet ef migrations add Epic06_HousingFamilyType --project
        Backend/src/IIROSA.Infrastructure/IIROSA.Infrastructure.csproj --startup-project
        Backend/src/IIROSA.Api/IIROSA.Api.csproj` — verify the generated migration backfills
        `FamilyType = 1` for existing rows (add `migrationBuilder.Sql` if EF leaves it NULL on a
        non-nullable column) — then `dotnet ef database update` (same form)
  - [x] NOTE: if the build fails with MSB3021/3027 on copy steps, the user's live `IIROSA.Api` is
        locking outputs — the compile is clean; do NOT kill their process; coordinate
- [x] **Task 3 — Application: filter + read** (AC 2, 3, 4)
  - [x] `FamilyFilterDto` gains `public string? FamilyType { get; set; }` (string on the wire —
        `?familyType=Housing` binds case-insensitively; parse with `Enum.TryParse<
        FamilyType>` in the service, ignore invalid values)
  - [x] `FamilyService.GetFamiliesAsync` — when `FamilyType` resolves, filter
        `f.FamilyType == parsed`; verify the charity pinning path: a charity caller's
        `userCharityId` must clamp `filter.CharityId` (pin-never-widen), an HQ caller keeps the
        explicit `charityId` (AC 4). If the existing parameter-based pinning does not clamp,
        inject `ICurrentUserService` and apply the `OfficeProjectService.cs:384` shape
  - [x] `FamilyListDto` — expose `familyType` so the screen can badge/branch later
- [x] **Task 4 — API: no new controller** (AC 2, 5)
  - [x] `GET /api/Families?familyType=Housing&charityId=` is served by the EXISTING
        `GetFamilies` action — no route change, no new endpoint; `[Authorize]` already admits
        `Charity` (`FamiliesController.cs:41`)
  - [x] Do NOT re-cut `HousingProjectsController` in this story — that lands in 6-3
- [x] **Task 5 — Frontend: re-cut the list screen to §11.S.1** (AC 1, 2)
  - [x] Replace `housing-project-list` grid + filters with §11.S.1: filter row — الجمعية
        (charity drop-down, HQ roles only, options `lookup: Charities` + كافة الجهات, on-change
        reload), searchValue text box, searchType drop-down (البحث عن طريق: إسم الاب / إسم الام /
        إسم الطالب الابن / الرقم القومي / كود الطالب الابن / الهاتف — the searchType WIRE lands
        in 6-2; ship the control disabled or plain-text search first)
  - [x] Grid columns in §11.S.1 order: الرقم (row serial, `(currentPage-1)*pageSize + i + 1`,
        continuous across pages — 13-1 formula) · إسم الأب · إسم الأم · الأبناء · الهواتف ·
        كود العائله · الجمعية — sourced from `FamilyListDto` (father/mother names come from the
        family aggregate; if the list DTO lacks them, extend the projection, do not invent a new
        endpoint)
  - [x] Point `housing-project-list.component.ts` at `familyService.getFamilies({ familyType:
        'Housing', charityId, page, pageSize })` — the families module's service
        (`modules/families/services/family.service.ts:103`) is the reference call shape; DELETE
        the construction-model bindings from the component and `housing-project.service.ts`
        list method
  - [x] Keep: shared `Pagination`, `PageHeader`/`Breadcrumb`, `trackBy` (add `trackByFamilyId`),
        `takeUntil(this.destroy$)` unsubscribe pattern already in the file; add `OnPush`
  - [x] الاجراءات column: view icon → `#/housing-projects/:id` (6-4 makes it meaningful), add
        icon → `#/housing-projects/:id/edit` add mode (disabled until 6-3 wires save) —
        §11.S.1 lists them; render disabled, wire later
  - [x] Route `#/housing-projects` stays registered (`app-routing.module.ts:66` lazy-loads the
        module; module route table `housing-projects-routing.module.ts:19` keeps the path) —
        change the module routes' role data from `['Admin','SuperAdmin']` to include `Charity`,
        and the sidebar gate `hasAnyRoles(['Admin','SuperAdmin'])`
        (`main-layout.component.html:236`) likewise
  - [x] Add `PERMISSION_ROLES` entries in `auth.service.ts` (~line 60): `HousingProjects.View` →
        `['SuperAdmin','Admin','Charity']` (+ `HousingProjects.Create`/`.Edit`/`.Delete` now so
        later stories only wire routes) — today NO `HousingProjects.*` keys exist, so
        `PermissionGuard` fails open with a console warning (`auth.service.ts:369-385`)
- [x] **Task 6 — i18n** — rewrite the `housingProjects` block in BOTH `ar.json` (1278-1400) and
      `en.json` (1266-1388): drop the construction keys (projectTypes/housingTypes/
      projectStatuses/projectStages/budget…), add the §11.S.1 keys (title قائمة الأسر الساكنة,
      7 column headers, filter labels, empty state); no hard-coded UI strings
- [x] **Task 7 — Verification** (AC 1–5)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; migration applied
  - [x] Live: `GET /api/Families?familyType=Housing` → 200 camelCase, only housing rows (seed one
        via SQL or a Regular row flipped to `FamilyType = 2`); unauthenticated → 401; charity
        token without `charityId` sees only its own rows
  - [x] `cd Frontend && npm run build` — 0 errors. If a UI fix shows no effect under `ng serve`,
        grep the served chunk for the new key before trusting it — the dev server can serve a
        dead-watcher build forever
  - [x] Tests: excluded per the standing user decision (no test project under `Backend/tests`)

### Review Findings

Code review 2026-08-24 (3 adversarial layers; blind+edge+auditor):

- [x] [Review][Patch] Register listing keeps soft-deleted families — no `!f.IsDeleted` in `GetFamiliesAsync` and no global query filter exists (`SetGlobalQueryFilters` body commented out) → the housing register lists deleted families forever [Backend/src/IIROSA.Application/Services/FamilyService.cs:1118] — **applied**: `!f.IsDeleted` added to the register predicate; build green, and the 35-check review battery's create→list cycles surface no deleted rows
- [x] [Review][Defer] Invalid `familyType` silently drops the discriminator filter [FamilyService.cs:1187] — deferred, recorded cross-epic binding ("string on wire, invalid ignored" — 6-1/epic-7 shared decision); UI always sends Housing

## Dev Notes

### Platform rules that bind this story

- Wire is camelCase; DTO property names must not start with `FK_` (Newtonsoft emits `fK_…`) —
  the entity column may be `FK_HousingBuildingId`, the DTO property is `HousingBuildingId`.
- Soft delete is a **global query filter** (`Framework.Core/Data/ModelBuilderExtensions.cs:83`) —
  never add manual `IsDeleted` checks.
- Response envelope: raw result + try/catch with anonymous `{ message }` — NOT `ApiResponse<T>`
  (recorded platform deviation, 15-1 Dev Notes; architecture.md §10 "code wins").
- Only `IUnitOfWork` saves; no business logic in controllers; FluentValidation in the service
  layer (this story writes nothing, so no new validator).
- Bilingual lookup labels: `NameAr ?? NameEn`. Routes are hash-based; module stays lazy-loaded.
- The existing `Family` has BOTH `CharityId` and `FK_CharityId` (pre-existing duplicate debt) —
  do not "fix" it in this epic; touch neither.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| `searchType` server-side handling (6 search fields) | 6-2 |
| `HousingBuilding`/`HousingFlat` lookups + allocation columns + `GET /api/LookupManagement/housing-buildings` | 6-5 |
| `POST /api/HousingProjects/projects` controller re-cut + §11.S.2 form + dropping the construction entity | 6-3 |
| `GET /api/HousingProjects/projects/{id}` + edit mode | 6-4 |
| Reports list screen + by-orphan with `ChildOrParent` | 6-6 |
| Beneficiary code lookup | 6-7 |
| `POST /api/PeriodicOrphanReports` housing branch + report form | 6-8 |

### References

- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.S.1] screen contract — 3 filter fields,
  7-column grid + 5 commands
- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.U.1] scenario — charity-scoped register read
- [Source: _bmad-output/planning-artifacts/epics.md#3.6] US-HOU-01 acceptance criteria
- [Source: _bmad-output/planning-artifacts/architecture.md#10] code-wins reconciliations; §9 build order
- [Source: Backend/src/IIROSA.Api/Controllers/FamiliesController.cs#L39] the endpoint to extend
- [Source: Backend/src/IIROSA.Application/DTOs/Family/FamilyFilterDto.cs] filter to extend
- [Source: _bmad-output/implementation-artifacts/epic-11-closeout.md] re-cut-the-invented-module
  precedent (same defect class, general cheques)
- [Source: _bmad-output/implementation-artifacts/epic-13-office-development-projects/13-1-list-development-projects.md] serial formula,
  PERMISSION_ROLES wiring, camelCase proof, tests-excluded decision
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-1-list-missions.md] envelope/data-list platform
  deviations

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code session, 2026-08-24).

### Debug Log References

- `dotnet build Backend/src/IIROSA.Api/IIROSA.Api.csproj -c Efmig` → Build succeeded, 0 errors
  (196 pre-existing warnings). The Efmig configuration sidesteps the live-API bin lock
  (MSB3021/3027 on the default Debug copy step).
- `npm run build` → the only TS errors are in `modules/incoming-outgoing/import-wizard/`
  (a parallel session's in-flight correspondence work); zero errors in any housing-projects file.

### Completion Notes List

- **Migration content caveat:** `Epic06_HousingFamilyType` (20260824063442) also carries the
  pending correspondence-module model sync that was sitting unmigrated in the shared working tree
  (Incoming/Outgoing `FK_CharityId` + indexes, `FamilyCharityTransfer`, `IncomingEmployee`,
  `OutgoingOrphanReport`, `ChildOutGoing` retirement). The parallel epic-16/17 session removed its
  own duplicate migrations after this snapshot landed — do NOT regenerate migrations assuming this
  file only contains the FamilyType column. Documented in the migration's `<remarks>`.
- **defaultValue fix:** EF scaffolded `FamilyType` with `defaultValue: 0` (invalid — not an enum
  member); hand-corrected to `1` (`Regular`) so existing rows backfill correctly.
- **Migration NOT applied to the DB.** It drops `ChildOutGoing`, which the user's live `IIROSA.Api`
  process may still reference; applying is deferred to user coordination (`dotnet ef database
  update --configuration Efmig --context ApplicationDbContext` once the API can be restarted).
  Consequence: live endpoint verification (AC 2/3/4 against a running server) could not run —
  the running API predates this code. Compile-level verification only.
- `FamilyConfiguration` needed no change: EF maps the enum to `int` automatically and the
  configuration convention does not index filter columns (only `Code`/`CharityId` are indexed).
- Charity pinning verified in `GetFamiliesAsync`: the `userRole == "Charity"` branch precedes the
  `filter.CharityId` branch, so charity callers are pinned (never widened) and HQ keeps the
  explicit filter — AC 3/4 hold through the existing parameter path; no `ICurrentUserService`
  injection needed.
- Frontend list re-cut is template-driven (missions-list precedent) with `OnPush`; the searchType
  selector is fully wired because 6-2's backend landed in the same pass.
- `HousingProjectListResponse`/`PagedApiResponse`/`getHousingProjects`/`exportHousingProjects`
  removed from `housing-project.service.ts`; the remaining construction methods are retired in
  6-3/6-4 when the form/detail screens are re-cut.
- i18n: `housingProjects` blocks rewritten in both `ar.json` and `en.json` (list keys only for now;
  form/report keys land with 6-3/6-6/6-8). JSON validated after edit.

### File List

- `Backend/src/IIROSA.Domain/Enums/FamilyType.cs` (new)
- `Backend/src/IIROSA.Domain/Entities/Family.cs` (+`FamilyType`, +using)
- `Backend/src/IIROSA.Application/DTOs/Family/FamilyFilterDto.cs` (+`FamilyType`, +`SearchType` [6-2])
- `Backend/src/IIROSA.Application/DTOs/Family/FamilyListDto.cs` (+`FamilyType`)
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` (discriminator filter, list projection,
  typed-search branch [6-2])
- `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824063442_Epic06_HousingFamilyType.cs`
  (+`.Designer.cs`) — defaultValue fix + remarks
- `Frontend/src/app/modules/housing-projects/housing-project-list/housing-project-list.component.ts` (re-cut)
- `Frontend/src/app/modules/housing-projects/housing-project-list/housing-project-list.component.html` (re-cut)
- `Frontend/src/app/modules/housing-projects/services/housing-project.service.ts` (construction list/export methods removed)
- `Frontend/src/app/modules/housing-projects/housing-projects-routing.module.ts` (roles +Charity)
- `Frontend/src/app/modules/families/models/family.model.ts` (+`searchType` [6-2])
- `Frontend/src/app/core/services/auth.service.ts` (+`HousingProjects.*` permission entries)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` (sidebar gate +Charity)
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` (housingProjects block rewrite)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-HOU-01 and module spec §11.S.1 / §11.U.1. Semantic audit found the existing `housing-projects` vertical is an invented construction tracker (board `done` was endpoint-name matching only) — story re-scoped as the re-cut foundation; epic-wide entity design recorded. |
| 2026-08-24 | Implemented: FamilyType discriminator (enum, entity, filter, projection), migration (backfill=1, carries pending correspondence model sync — see Dev Agent Record), §11.S.1 list re-cut with typed-search wiring, roles/PERMISSION_ROLES/sidebar widened to Charity, i18n rewrite. Status → review; live verification deferred (API restart + database update required). |
| 2026-08-24 | Review closed: soft-delete filter patch applied; migrations applied (FamilyType chain + review backing indexes); live verification completed in the epic-6 review battery (35/35) against a private smoke instance on the migrated DB. All findings resolved (1 patch, 1 recorded defer). Status → done. |
