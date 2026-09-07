# Story 7-1: List refugee families

| Field | Value |
| --- | --- |
| Story key | `7-1-list-refugee-families` |
| Epic | EP-07 — Refugee Families (الاسر اللاجئة) |
| Use case | UC-REF-01 — قائمة الأسر اللاجئة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/12-UC-REF-Refugee-Families.md` (§12.S.1 screen, §12.U.1 scenario) |
| Route | `#/families/refugees` |
| Endpoint | `GET /api/Families?familyType=Refugee` |
| Depends on | EP-01 (authentication and role resolution); the live families vertical built by 5-1…5-5; shares the `FamilyType` discriminator binding with 6-1 (either may land it first — see below) |
| Roles | Charity, HQ roles → `SuperAdmin`, `Admin`, `Charity` (existing `FamiliesController` role set) |

## Status

review

## Story

As a charity user, I want to be able to list refugee families قائمة الأسر اللاجئة, so that I can find
the record I need without leaving the system.

## Acceptance Criteria

1. Given a charity user with an active session on the screen at `#/families/refugees`, when the
   actor opens the screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Families?familyType=Refugee` and the response is rendered on the screen without a page
   reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity (and
   country) are returned; given an HQ role, when an explicit charity id is supplied, then the
   function operates on that charity's data.
4. Given no refugee family matches, when the screen loads, then the grid renders empty and the
   paging control reports zero pages.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §12.S.1 are implemented with their mandatory flags and
lookups (the الجمعية selector); the scenario of §12.U.1 passes end to end; the role and charity
scoping is enforced server-side, not only in the menu.

## Reality check: BROWNFIELD extension of the live families vertical

The whole family vertical is DONE and live on both sides (5-1…5-5): `FamiliesController`
(`Backend/src/IIROSA.Api/Controllers/FamiliesController.cs`), `FamilyService.GetFamiliesAsync`
(`Backend/src/IIROSA.Application/Services/FamilyService.cs:491`), and the Angular
`modules/families` module (list/form/detail + father/mother/relatives subcomponents +
`services/family.service.ts`). **Nothing refugee-specific exists anywhere** (verified): no
`FamilyType` on the `Family` entity, no `familyType` filter parameter, no `#/families/refugees`
route, no i18n keys, no menu entry.

This story adds a **type discriminator + a refugee-scoped list screen** on top of that machinery.
It must NOT fork the families module, create a `RefugeeFamiliesController`, or duplicate the list
service — the routing map fixes refugee families as *child routes of `families`* with the same
`api/Families` controller (`docs/Modules/00-ROUTING-MAP.md` rows 12 and 102).

What already exists and MUST be reused, not rebuilt:

| Exists | Where | Use |
| --- | --- | --- |
| Paged list endpoint | `FamiliesController.GetFamilies` (`FamiliesController.cs:42`) — `Ok(new { result.Items, result.TotalCount })`, camelCase wire `items`/`totalCount` | extend with the `familyType` filter param — do not fork |
| Charity scoping | `FamilyService.GetFamiliesAsync(filter, userCharityId, userRole)` (`FamilyService.cs:491`) — Charity role pinned to own `FK_CharityId`, HQ may pass `filter.CharityId` | unchanged semantics for refugee rows |
| Filter DTO | `Backend/src/IIROSA.Application/DTOs/Family/FamilyFilterDto.cs` | add `FamilyType` |
| List DTO | `Backend/src/IIROSA.Application/DTOs/Family/FamilyListDto.cs` | add `PhoneNumber` (grid needs الهواتف) |
| Frontend service | `family.service.ts:102` `getFamilies(searchRequest): Observable<FamilyPagedResult>` | extend the request model with `familyType` |
| Exemplar screen | `family-list/` + `seasonal-aid/campaign-list/` | bespoke Bootstrap grid + shared `Pagination` — NOT `data-list` |
| Charities selector source | `GET /api/Charities` (HQ-only control, prepend كافة الجهات) | الجمعية dropdown |

## Binding design: the `FamilyType` discriminator (SHARED with 6-1 — do not fork)

Epic 6's context pass already bound this discriminator for the housing variant and named it as
"unlocking epics 7/8" (`6-1-list-housing-families.md`, binding #1). **Follow that binding
verbatim; do not re-decide it here:**

1. **Enum on the entity, string on the wire.** `Backend/src/IIROSA.Domain/Enums/FamilyType.cs` —
   `enum FamilyType { Regular = 1, Housing = 2, Refugee = 3 }`;
   `Family` gains `public FamilyType FamilyType { get; set; } = FamilyType.Regular;`.
   `FamilyFilterDto.FamilyType` is `string?` — the service parses with
   `Enum.TryParse<FamilyType>` and **ignores invalid values** (falls through to unfiltered).
   Do NOT create a `RefugeeFamily` entity, a `FamilyTypes` string-constants class, or a
   `RefugeeFamiliesController` — the board and routing map both name
   `api/Families?familyType=Refugee`.
2. **One migration, owned by whichever epic lands first.** 6-1 names
   `dotnet ef migrations add Epic06_HousingFamilyType` with default `1` and a
   `migrationBuilder.Sql` backfill if EF leaves existing rows NULL. If 6-1 has already run, 7-1
   reuses the landed column/migration and skips Task 1's migration step entirely; if 7-1 runs
   first, it lands the same migration under the 6-1 name and 6-1 then has nothing to add. Never
   create two discriminator migrations.
3. **Backfill safety**: existing families are all `Regular` (1); with no `familyType` param the
   query is unfiltered, so 5-1…5-5 behaviour is byte-identical after the change.

## Tasks / Subtasks

- [x] **Task 1 — Domain + migration** (AC 2, 3)
  - [x] FIRST: check whether 6-1 has already landed `Enums/FamilyType.cs`, the `Family.FamilyType`
        column, and the `Epic06_HousingFamilyType` migration — if yes, this task reduces to
        verifying the enum carries `Refugee = 3` and moving to Task 2
  - [x] `Backend/src/IIROSA.Domain/Enums/FamilyType.cs` — `Regular = 1, Housing = 2, Refugee = 3`;
        `Family.cs` gains the enum-typed property defaulting to `Regular` (no audit fields, no
        `DbSet` — auto-discovery, architecture.md §3.3). Leave the existing `LivingConditionId`
        living-condition lookup alone
  - [ ] Configuration: enum-to-int column, default `1`, `MappingDefaults.IIROSA_SCHEMA` (never a
        literal schema string); index on `FamilyType` per the 6-1 binding — **column + default 1
        verified; the index is deferred into 7-3's `Epic7_RefugeeContract` migration** (see
        completion notes)
  - [x] Migration `Epic06_HousingFamilyType` (6-1's name — shared) + `dotnet ef database update`;
        backfill existing rows to `1` via `migrationBuilder.Sql` if EF leaves them NULL; verify
        `GET /api/Families` output is unchanged — migration minted AND applied by the concurrent
        6-x session (initial `defaultValue: 0` defect corrected to `1` + backfill note); DB
        verified: column exists, `Family` table holds 0 rows so no backfill was needed
- [x] **Task 2 — Filter plumbing** (AC 2, 3)
  - [x] `FamilyFilterDto`: add `string? FamilyType` (string on the wire — 6-1 convention)
  - [x] `FamilyService.GetFamiliesAsync`: after the charity-scope block, parse with
        `Enum.TryParse<FamilyType>(filter.FamilyType, out var type)` and, when it resolves, add
        `query = query.Where(f => f.FamilyType == type);` — invalid/absent values are ignored,
        never a 500
  - [x] `FamilyListDto`: add `string? PhoneNumber` and map it from the entity (grid column الهواتف)
- [x] **Task 3 — Frontend service + model** (AC 2)
  - [x] `models/family.model.ts`: add `familyType?: string` to `FamilySearchRequest`;
        `phoneNumber?: string` to the list-item model (new `FamilyListItemDto` interface typed
        against the real `FamilyListDto` wire shape — the legacy `FamilyDto` list typing had
        drifted)
  - [x] `family.service.ts`: no new method — `getFamilies` already forwards the request object as
        query params; confirm `familyType: 'Refugee'` serialises into the query string
        (`buildHttpParams` sets every non-empty key)
- [x] **Task 4 — Refugee list screen** (AC 1, 2, 4)
  - [x] `Frontend/src/app/modules/families/refugee-family-list/` — component `.ts`/`.html`/`.scss`
        (`RefugeeFamilyListComponent`, standalone like every families screen); no `.spec.ts` —
        the families module has no spec convention and tests are excluded per the standing
        decision
  - [x] **Route order landmine:** in `families-routing.module.ts` the `':id'` route is declared
        before any new sibling — `path: 'refugees'` MUST be inserted ABOVE `':id'` or the router
        will match `refugees` as an id. Add `refugees` → list (permission `Families.View`),
        `refugees/create` + `refugees/:id` + `refugees/:id/edit` as placeholders are NOT added here
        (they land with 7-3/7-4)
  - [x] Filter row per §12.S.1: الجمعية drop-down (options `GET /api/Charities`, prepend كافة الجهات
        empty option; HQ users only — hide for Charity role) + the search box and البحث عن طريق
        selector render but stay inert until 7-2 wires them
  - [x] Grid columns in §12.S.1 order: الرقم (row serial `(page-1)*pageSize + i + 1`, continuous
        across pages) · إسم الأب (`fatherName`) · إسم الأم (`motherName`) · الأبناء
        (`orphansCount`) · الهواتف (`phoneNumber`) · كود العائله (`code`) · الجمعية
        (`charityName`); `trackBy: trackByFamilyId`; empty state when `totalCount === 0`
  - [x] Shared `Pagination` component (`currentPage`/`pageSize`/`totalCount` + `pageChange`);
        reload resets to page 1 when the charity filter changes
  - [x] `OnPush`; load via `family.service.getFamilies({ familyType: 'Refugee', ... })`
- [x] **Task 5 — Menu + permissions + i18n** (AC 5)
  - [x] `auth.service.ts` `PERMISSION_ROLES` (line ~60): **`Families.*` entries are missing today**
        (verified — routes reference `Families.View/Create/Edit` while the registry has none, so
        `PermissionGuard` falls back to warn-and-allow). Add `Families.View`, `Families.Create`,
        `Families.Edit` → `['SuperAdmin', 'Admin', 'Charity']`. This fixes the existing gap for
        the whole families module, not just refugees — intended.
  - [x] Menu: in `layouts/main-layout` families dropdown, add الأسر اللاجئة + اضافة أسرة لاجئة
        entries (`routerLink="/families/refugees"`); gate with `hasPermission('Families.View')` —
        the dropdown gate was corrected from lowercase `families.view` to the registry's
        `Families.View`
  - [x] i18n: `families.refugee*` keys (menu labels, page title, 7 column headers, empty state,
        charity filter label + the 7 search-type option labels so the selector renders) in
        **both** `assets/i18n/ar.json` and `en.json`; also restored the ar `IIROSA` block's
        missing `families`/`allFamilies`/`addFamily` menu keys (the Arabic menu showed raw keys)
        and added `refugeeFamilies`/`addRefugeeFamily` in both languages; no hard-coded strings
- [x] **Task 6 — Verification** (AC 1–5)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors (MSB3021/3027 on copy steps = the user's
        live `IIROSA.Api` locking outputs, not a compile failure — never kill it); apply the
        migration — applied by the concurrent 6-x session, confirmed via
        `dotnet ef migrations list --no-build`
  - [x] Live check: `GET /api/Families?familyType=Refugee` → 200 with `items/totalCount` camelCase
        and zero rows (none seeded yet); same call unauthenticated → 401; without the param →
        pre-existing behaviour unchanged (regression guard for 5-1/5-2); `?familyType=Nonsense`
        → 200 ignored (no 500). Logged in with the seeded SuperAdmin account.
  - [x] Charity-role check: code-verified at `FamilyService.GetFamiliesAsync` (Charity pinned to
        own `FK_CharityId` before any filter) — live observation impossible with an empty table
        (both roles return the same empty envelope)
  - [x] `cd Frontend && npm run build` — **0 errors in families/refugee files** (verified by
        filtering the compiler output); the whole-tree build still fails in modules being edited
        by concurrent sessions (housing-projects list, outgoing-letters model exports, employees
        date-pipes) — outside this story's scope, recorded in the completion notes
  - [x] Tests: excluded per the standing user decision (no test project under `Backend/tests`)

### Review Findings (code review 2026-08-24)

- [x] [Review][Patch] (fixed 2026-08-24) [High] `charityName` null for every family created via `POST /api/Families` — create stamps only `FK_CharityId`, never the `CharityId` mirror the `Charity` navigation binds to; §12.S.1 الجمعية column cannot populate [Backend/src/IIROSA.Application/Services/FamilyService.cs:177]
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] List loads: no `takeUntil` on `getFamilies`/`getCharities`, no request sequencing (stale-response race), failed reload leaves stale rows rendered [Frontend/src/app/modules/families/refugee-family-list/refugee-family-list.component.ts:139-183]
- [x] [Review][Patch] (fixed 2026-08-24) [Low] Hard-coded English `'Failed to load refugee families'` in the list error path [refugee-family-list.component.ts:~183]
- [x] [Review][Patch] (fixed 2026-08-24) [Low] `IX_Family_FamilyType` index (Task 1 deliverable, deferred to 7-3's absorbed migration) never minted — additive migration, coordinate with the concurrent-session migration chain
- [x] [Review][Defer] Lookup catalogue GETs hard-cap `PageSize = 1000` and silently truncate — deferred, cross-module (LookupManagementController), pre-existing pattern

## Dev Notes

### Platform rules that bind this story

- Wire is camelCase; DTO names must not start with `FK_` (Newtonsoft degrades `FK_CharityId` to
  `fk_CharityId` — pre-existing on the entity; the service scopes on `FK_CharityId`, keep using it
  consistently and keep it OFF the DTOs).
- Response envelope: this controller already returns `Ok(new { items, totalCount })` — do NOT wrap
  in `ApiResponse<T>` (recorded platform deviation, 15-1 ruling; architecture.md §10 "code wins").
- **Do NOT use `data-list`** — used by zero shipped modules; the bespoke grid + shared
  `Pagination` is the platform pattern (13-1/15-1/16-1/17-1 precedent). The shared components that
  DO exist: `app-input-text`, `app-drop-down`, `app-attachment-input`, `Pagination`.
- Soft delete is a global query filter — never add manual `IsDeleted` checks.
- `FamiliesController` inherits `ControllerBase` (not the project `ApiController` base) — pre-existing;
  do not churn the base class in this story. New/edited actions keep
  `[Authorize(Roles = "SuperAdmin,Admin,Charity")]` — server-side authorisation is the control,
  the menu gate is convenience only.
- Caller identity reaches the service via `GetUserCharityId()`/`GetUserRole()` from claims —
  never from the request body.
- `FamilyRepository` filters `!IsDeleted` in places while the global filter also applies — harmless;
  don't "fix" it here.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| `searchValue` + `searchType` wiring (`GET …&search=&searchType=`) | 7-2 |
| Refugee household columns (region/center/rent/ownership/…), lookups, `POST /api/Families` refugee path, refugee form screen `refugees/create` | 7-3 |
| `refugees/:id` + `refugees/:id/edit` routes, view/update flow, list row icons | 7-4 |
| Add/View row-command icons on the grid — render disabled placeholders only | 7-3 / 7-4 |
| `familyType=Housing` reads (epic 6) — the discriminator is shared, the housing screens are not yours | epic 6 |

### References

- [Source: docs/Modules/12-UC-REF-Refugee-Families.md#12.S.1] screen contract — 3 filter fields,
  7-column grid, 5 commands
- [Source: docs/Modules/12-UC-REF-Refugee-Families.md#12.U.1] scenario — caller-scoped paged read
- [Source: _bmad-output/planning-artifacts/epics.md#3.7] US-REF-01 acceptance criteria
- [Source: docs/Modules/00-ROUTING-MAP.md#L77] refugees are child routes of `families` on
  `api/Families?familyType=Refugee`; [#L102](…) decision #5 — no routes of their own
- [Source: Backend/src/IIROSA.Application/Services/FamilyService.cs#L491] `GetFamiliesAsync`
  scoping + filter shape to extend
- [Source: Backend/src/IIROSA.Api/Controllers/FamiliesController.cs#L42] list action + wire envelope
- [Source: Frontend/src/app/modules/families/families-routing.module.ts] route table — `:id`
  ordering landmine
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-1-list-housing-families.md] the SHARED
  `FamilyType` enum discriminator binding (Regular/Housing/Refugee) and its migration — follow it,
  do not fork a string-constants variant
- [Source: _bmad-output/implementation-artifacts/epic-16-correspondence-incoming-and-outgoing/16-1-list-incoming-letters.md] brownfield list
  story precedent (envelope, PERMISSION_ROLES, verification style)

## Dev Agent Record

### Agent Model Used

glm-5 (Claude Code harness, dev-story pass 2026-08-24)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` → 0 errors (after fixing a CS0266/CS0029 blocker the
  concurrent UC-ORP edit introduced in `FamilyService` — an orphan query typed as
  `IIncludableQueryable` then reassigned; retyped to `IQueryable<Orphan>`, no semantic change).
- `npm run build` → 0 errors touching families/refugee (grep-filtered compiler output);
  whole-tree build still red in concurrently-edited modules (housing-projects, outgoing-letters,
  employees date-pipes) — not this story's files.
- Live (seeded SuperAdmin login, `https://localhost:60960`):
  `GET /api/Families?familyType=Refugee` → 200 `{"items":[],"totalCount":0}` ·
  `GET /api/Families` → 200 unchanged · `?familyType=Nonsense` → 200 ignored ·
  unauthenticated → 401.
- DB (`sqlcmd`, IIROSA_Db_Dev): `[IIROSA].[Family].FamilyType` column exists; table holds 0
  rows, so the discriminator backfill question is moot for this database.

### Completion Notes List

- **This was a review-and-complete pass**: the user had implemented the backend discriminator
  (via the shared 6-1 binding) before the story ran; the frontend refugee screen did not exist.
  The shared pieces (enum, entity column, filter DTO, service branch, `Epic06_HousingFamilyType`
  migration) were verified, not rebuilt. The migration initially shipped with `defaultValue: 0`
  (not a valid enum value); the concurrent 6-x session corrected it to `1` — confirmed applied.
- The `FamilyType` index from the 6-1 binding is still missing (no `IX_Family_FamilyType`);
  deferred into 7-3's `Epic7_RefugeeContract` migration rather than racing the concurrent
  session with a one-column migration.
- `FamilyListDto` carries `FamilyType`/`PhoneNumber`; the grid's row icons render disabled —
  their targets land with 7-3/7-4.
- `families.module.ts` declares nothing (the module is a routing shell; families screens are
  standalone components) — the refugee list follows that convention; no `.spec.ts` (module has
  no spec convention; tests excluded per standing decision).
- The 7 search-type option labels were added in this story (not 7-2) because the §12.S.1
  selector is rendered here; 7-2 wires the behaviour.
- Restored the ar `IIROSA` menu block's missing `families`/`allFamilies`/`addFamily` keys —
  without them the Arabic menu showed raw translation keys for the existing entries too.
- Concurrent-session coordination: epics 6/16/17 were landing in the same working tree during
  this pass; every edit re-read its anchor first. `FamilySearchRequest.searchType` (6-2's
  shared binding) arrived via that lane and is reused as-is by 7-2.

### File List

Backend (edited):
- `Backend/src/IIROSA.Application/DTOs/Family/FamilyListDto.cs` — added `PhoneNumber`
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — map `PhoneNumber` in the list
  projection; retype the UC-ORP orphan query to `IQueryable<Orphan>` (compile fix for a
  concurrent edit, no logic change)

Backend (verified, landed via the shared 6-1 binding / concurrent lane — not edited here):
- `Backend/src/IIROSA.Domain/Enums/FamilyType.cs`, `Family.cs` (enum column),
  `FamilyFilterDto.cs` (`string? FamilyType`), `FamilyService.GetFamiliesAsync`
  (`Enum.TryParse` filter branch),
  `Data/Migrations/20260824063442_Epic06_HousingFamilyType.cs` (applied)

Frontend (new):
- `Frontend/src/app/modules/families/refugee-family-list/refugee-family-list.component.ts` /
  `.html` / `.scss`

Frontend (edited):
- `Frontend/src/app/modules/families/families-routing.module.ts` — `refugees` route ABOVE `':id'`
- `Frontend/src/app/modules/families/models/family.model.ts` — `familyType` on
  `FamilySearchRequest`; new `FamilyListItemDto`
- `Frontend/src/app/core/services/auth.service.ts` — `Families.View/Create/Edit` in
  `PERMISSION_ROLES`
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — refugee menu entries;
  dropdown gate `families.view` → `Families.View`
- `Frontend/src/assets/i18n/ar.json`, `en.json` — `families.refugee*` blocks + `IIROSA`
  refugee menu keys (+ restored missing ar families menu keys)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-REF-01 and module spec §12.S.1 / §12.U.1; brownfield status verified against the live families vertical; `FamilyType` discriminator design bound for the whole epic. |
| 2026-08-24 | Review-and-complete dev pass: backend discriminator verified as landed (shared 6-1 binding, migration applied), `PhoneNumber` added to the list DTO, full refugee list screen + route + permissions + menu + i18n delivered; backend 0-error build, live endpoint checks green; status → review. |
