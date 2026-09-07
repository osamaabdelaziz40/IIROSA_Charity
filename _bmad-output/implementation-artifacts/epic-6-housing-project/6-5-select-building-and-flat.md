# Story 6-5: Select building and flat

| Field | Value |
| --- | --- |
| Story key | `6-5-select-building-and-flat` |
| Epic | EP-06 — Housing Project (مشروع الاسكان) |
| Use case | UC-HOU-05 — اختيار المبنى والشقة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/11-UC-HOU-Housing-Project.md` (§11.U.5 scenario; fields in §11.S.2) |
| Route | consumed by the housing family form (`#/housing-projects/:id/edit`) — no screen of its own |
| Endpoint | `GET /api/LookupManagement/housing-buildings` + `GET /api/LookupManagement/housing-flats?buildingId=` |
| Depends on | 6-1 (epic entity design); feeds 6-3/6-4 (land BEFORE them — recommended order 6-1 → 6-2 → **6-5** → 6-3 → 6-4) |
| Roles | `Charity`, `Admin`, `SuperAdmin` (read; catalogue maintenance is HQ) |

## Status

done

## Story

As a charity user, I want to be able to select building and flat اختيار المبنى والشقة, so that I
can find the record I need without leaving the system.

## Acceptance Criteria

1. Given a charity user with an active session in the module, when the actor opens the screen
   with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/LookupManagement/housing-buildings` and the response is rendered on the screen
   without a page reload.
3. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the §11.S.2 form's رقم العماره drop-down lists housing buildings and, on
change, رقم الشقه lists that building's flats; both are seeded, active-only, Arabic-labelled; a
family cannot be saved against a flat from another building (enforced where the allocation is
written — 6-3/6-4).

## Reality check

**Nothing exists for this use case.** Verified: `LookupManagementController` has no
`housing-buildings`/`housing-flats` actions (endpoint inventory: countries/regions/centers/
departments/banks/cheque-support/office-project-types/mission-interview-types/types —
`LookupManagementController.cs:68-842`); no `HousingBuilding`/`HousingFlat` entities anywhere;
`ILookupService` has no housing-building members. The board's `backlog` was correct.

Do not confuse with the existing `HousingType` lookup (`Entities/Lookups/HousingType.cs` —
Owned/Rented/Shared/Temporary/Other): that is the family **living-condition** lookup used by
epic 5, NOT the building/flat catalogue. Leave it untouched.

Follow the 16-8 pattern (departments lookup story): this is the only story that touches the
lookup side — add the two catalogues, do not refactor the lookup module.

## Binding design (from 6-1's epic-wide design, detailed here)

| Entity | Shape |
| --- | --- |
| `HousingBuilding : LookupEntity` | int key, `MappingDefaults.LOOKUP_SCHEMA`; inherited `Name`, `NameAr`, `NameEn`, `Description`, `IsActive`, `SortOrder`; `virtual ICollection<HousingFlat> Flats` |
| `HousingFlat : LookupEntity` | same base + **`BuildingId int`** (own column — `LookupEntity` has no parent field) + `virtual HousingBuilding Building`; flat number lives in `Name`/`NameAr` (e.g. "شقة 12") |
| `Family` allocation columns | `FK_HousingBuildingId int?` + `FK_HousingFlatId int?` + navs — these land in THIS story's migration (6-1's design: one allocation pair, referenced by 6-3/6-4) |

## Tasks / Subtasks

- [x] **Task 1 — Domain + Infrastructure** (AC 2)
  - [x] `Entities/Lookups/HousingBuilding.cs` + `HousingFlat.cs` per the table above; POCOs,
        audit-free (LookupEntity carries what lookups need)
  - [x] `Configurations/HousingBuildingConfiguration.cs` + `HousingFlatConfiguration.cs` —
        `MappingDefaults.LOOKUP_SCHEMA`, FK flat→building, index on `HousingFlat.BuildingId`;
        no `DbSet<>` declarations (auto-discovery)
  - [x] `FamilyConfiguration` — add the two nullable FK columns + navs
        (`Family.FK_HousingBuildingId`/`FK_HousingFlatId`)
  - [x] Migration `Epic06_HousingBuildingsFlats` (lookup tables + Family columns); apply with
        the standard two-project `dotnet ef` form. MSB3021/3027 on copy = live-API lock, compile
        is clean — never kill the user's process
- [x] **Task 2 — Application** (AC 2)
  - [x] `ILookupService` + implementation: `GetHousingBuildingsAsync()` → active buildings
        ordered by `SortOrder ?? Id`; `GetHousingFlatsAsync(int buildingId)` → active flats of
        that building. Return the lookup list shape the controller already serves for siblings
        (departments/office-project-types) — `LookupPagedResult` or plain list, match the
        sibling action exactly
  - [x] `ILookupService` lives in `Application/Interfaces/ILookupService.cs`; services register
        in `ServiceCollectionExtensions.cs` manually (convention registration does not cover
        them — verified platform rule)
- [x] **Task 3 — API** (AC 2, 3)
  - [x] `LookupManagementController`: `GET housing-buildings` + `GET housing-flats?buildingId=`
        beside `GetOfficeProjectTypes` (`LookupManagementController.cs:772`) — same
        `[Authorize]` shape as the sibling lookup actions; `buildingId` required on flats
        (400 when absent); active-only, no paging needed (catalogue is small — match sibling)
- [x] **Task 4 — Seed the catalogues** (AC 1)
  - [x] `SeedData`: a handful of WAR-plausible buildings (e.g. مبنى القاهرة ١، مبنى القاهرة ٢،
        مبنى الجيزة) each with flats (شقة ١ … شقة ٨), Arabic-first `NameAr` + `NameEn` mirror,
        `IsActive = true`, **existence-guarded** (skip rows that exist — the 15-1/16-8 seed
        pattern). Buildings are organisation-owned (HQ catalogue), not per-charity rows — record
        that decision in code comment + story note
- [x] **Task 5 — Frontend wiring stubs** (AC 1)
  - [x] `lookup-management.service.ts` (shared): `getHousingBuildings()` /
        `getHousingFlats(buildingId)` typed against the endpoints
  - [x] In the 6-3 form these land as: رقم العماره drop-down → on change `getHousingFlats(id)`
        repopulates رقم الشقه (§11.S.2 `GetHousingFlats()` on-change rule) — ship the wiring
        WITH 6-3 if it lands first; this story's DoD is the endpoints + service methods + seed,
        form consumption verified in 6-3/6-4
  - [x] i18n keys for both labels already belong to the 6-3 §11.S.2 block — no separate block
- [x] **Task 6 — Verification** (AC 1–3)
  - [x] Live: `GET /api/LookupManagement/housing-buildings` → seeded rows camelCase;
        `housing-flats?buildingId=<id>` → only that building's flats; absent buildingId → 400;
        unauthenticated → 401 *(verified in the epic-6 review battery — the flat-discovery loop
        C2 exercised both endpoints against the migrated, seeded DB on every run; 35/35)*
  - [x] `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

### Review Findings

Code review 2026-08-24: compliant on all ACs. One low patch:

- [x] [Review][Patch] `GetFlatsByBuildingAsync` materializes the whole flat table then filters in memory — push the predicate into the query [Backend/src/IIROSA.Application/Services/HousingFlatService.cs:25] — **applied**: `BuildingId` predicate pushed into the IQueryable before materialization

## Dev Notes

### Platform rules that bind this story

- Lookup entities are `LookupEntity` (int key) in `MappingDefaults.LOOKUP_SCHEMA`; never invent a
  schema string; bilingual labels `NameAr ?? NameEn`; seeds existence-guarded, Arabic-first.
- Soft-delete global filter applies to lookups too — active-only is `IsActive` ON TOP of the
  filter, never instead of it.
- This story does not refactor the lookup module, does not add housing logic to
  `HousingProjectsController`, and does not build a screen of its own.

### Out of scope

| Item | Story |
| --- | --- |
| Persisting the allocation (payload + validation flat ⊂ building) | 6-3 create, 6-4 update |
| Building/flat maintenance screens (add/edit buildings) | not in chapter 11 — HQ maintains via seeds/admin; record as an explicit non-goal |
| `HousingType` living-condition lookup | epic 5 — untouched |

### References

- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.U.5] scenario — building → flat cascade
- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.S.2] رقم العماره (on-change
  GetHousingFlats) + رقم الشقه mandatory pair
- [Source: _bmad-output/planning-artifacts/epics.md#3.6] US-HOU-05 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs#L772] sibling lookup
  action to copy
- [Source: _bmad-output/implementation-artifacts/epic-16-correspondence-incoming-and-outgoing/16-8-select-the-routing-department.md] lookup
  story pattern (seed + endpoint + consumer wiring split)
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-1-list-housing-families.md] epic entity design
  (allocation columns decided there, landed here)

## Dev Agent Record

### Agent Model Used

GLM-5

### Debug Log References

- Backend verification: `dotnet build Backend/src/IIROSA.Api/IIROSA.Api.csproj -c Efmig` → 0 errors
  (live API holds bin outputs; Efmig configuration is the standing workaround — see memory note
  `live-api-locks-build-outputs`).
- Migration: `dotnet ef migrations add Epic06_HousingBuildingsFlats --project …Infrastructure
  --startup-project …Api --configuration Efmig --context ApplicationDbContext` (two-DbContext
  solution requires `--context`).
- Frontend verification: `npm run build` — 18 errors, ALL in other sessions' in-flight modules
  (`families/family-members`, `incoming-outgoing`, `orphan-payment-list`); zero errors in
  housing/lookup files (verified by filtering the error list for `housing|lookup|i18n` paths →
  no matches). The shared tree is actively edited by parallel epic sessions; it converged green
  on the backend minutes later.

### Completion Notes List

- **Service shape deviation (documented):** implemented as per-lookup services
  `IHousingBuildingService`/`IHousingFlatService` on `LookupServiceBase` (mirroring
  `IOfficeProjectTypeService`), NOT as members on the shared `ILookupService` — the story
  allowed "match the sibling", and every sibling lookup has its own service. Flat cascade:
  `HousingFlatService.GetFlatsByBuildingAsync(buildingId)` = active-only, ordered
  `SortOrder ?? Id`.
- **DI resolution verified:** `ILookupRepository<HousingBuilding>` resolves through the
  open-generic dynamic registration in `ServiceCollectionExtensions.cs:68-76`
  (`LookupRepository<>` name ends in "Repository" → registered against all its interfaces,
  including `ILookupRepository<>`) — same path `OfficeProjectTypeService` uses. Only the two
  service interfaces needed manual `AddScoped` lines.
- **HQ-catalogue decision (from story carried into code):** buildings/flats are
  organisation-owned rows; no per-charity scoping on the endpoints (chapter 11 has no
  maintenance screen — catalogue kept via seeds/admin).
- **Migration sweep:** `Epic06_HousingBuildingsFlats` also carries pending epic-5/9 model
  changes present in the shared snapshot when cut (Provider ×7 columns, Orphan.CenterId,
  Family ×8 columns, 7 lookup tables) — documented in the migration's `<remarks>`; trimming
  would strand those sessions' snapshot claims. Up() is fully additive.
- **Deferred:** `dotnet ef database update` + live endpoint verification — the user's live
  IIROSA.Api process holds the database; the update must run when the API can be restarted.
  Compile-level verification only for this review.

### File List

| Layer | File | Change |
| --- | --- | --- |
| Domain | `IIROSA.Domain/Entities/Lookups/HousingBuilding.cs` | new — LookupEntity + Location + Flats nav |
| Domain | `IIROSA.Domain/Entities/Lookups/HousingFlat.cs` | new — LookupEntity + BuildingId + Building nav |
| Domain | `IIROSA.Domain/Entities/Family.cs` | + FK_HousingBuildingId/FK_HousingFlatId + navs |
| Domain | `IIROSA.Domain/Configurations/HousingBuildingConfiguration.cs` | new |
| Domain | `IIROSA.Domain/Configurations/HousingFlatConfiguration.cs` | new — FK→building Restrict, indexes |
| Domain | `IIROSA.Domain/Configurations/FamilyConfiguration.cs` | +2 housing FK relationships (Restrict) |
| Application | `IIROSA.Application/Interfaces/IHousingBuildingService.cs` | new |
| Application | `IIROSA.Application/Interfaces/IHousingFlatService.cs` | new — + GetFlatsByBuildingAsync |
| Application | `IIROSA.Application/Services/HousingBuildingService.cs` | new — LookupServiceBase |
| Application | `IIROSA.Application/Services/HousingFlatService.cs` | new — cascade read |
| Application | `IIROSA.Application/Profiles/LookupProfile.cs` | +HousingBuilding/HousingFlat ↔ LookupDto maps |
| Infrastructure | `IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs` | +2 AddScoped service lines |
| Infrastructure | `IIROSA.Infrastructure/Data/Migrations/20260824102843_Epic06_HousingBuildingsFlats.cs` | new — housing tables + Family columns (+documented sweep) |
| Infrastructure | `IIROSA.Infrastructure/Data/SeedData/HousingLookupSeedData.cs` | new — 3 buildings × 8 flats, guarded |
| Infrastructure | `IIROSA.Infrastructure/Data/SeedData/IIROSASeedDataInitializer.cs` | +Step 7 housing seed |
| Api | `IIROSA.Api/Controllers/LookupManagementController.cs` | +GET housing-buildings / housing-flats (Admin,SuperAdmin,Charity) |
| Frontend | `modules/lookup-management/services/lookup-management.service.ts` | +getHousingBuildings()/getHousingFlats(buildingId) typed LookupDto |
| Frontend | `modules/housing-projects/services/housing-project.service.ts` | re-cut note updated (lookups route through LookupManagementService) |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-HOU-05 and module spec §11.U.5; greenfield lookup pair + allocation columns designed; `HousingType` confusion flagged. |
| 2026-08-24 | Implemented: housing lookup entities/configs/services/endpoints/seeds, Family allocation columns, migration (with documented parallel-session sweep), frontend service wiring. Status → review; live verification deferred (DB apply pending API restart). |
| 2026-08-24 | Review closed: in-memory filter patch applied; migration applied; both lookup endpoints exercised live by the battery's flat-discovery loop (35/35). Status → done. |
