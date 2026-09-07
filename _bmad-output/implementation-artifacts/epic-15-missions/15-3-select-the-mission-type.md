# Story 15-3: Select the mission type

| Field | Value |
| --- | --- |
| Story key | `15-3-select-the-mission-type` |
| Epic | EP-15 — Missions (المأموريات) |
| Use case | UC-MSN-03 — نوع المأمورية |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/20-UC-MSN-Missions.md` (§20.S.2 form field نوع المأموريه, §20.U.3 scenario) |
| Endpoint | `GET /api/MissionManagement/mission-types` |
| Depends on | EP-01; independent of 15-1/15-2 (list screen does not use this catalogue) |
| Roles | Gen. Director, Staff → `SuperAdmin`, `Admin` |

## Status

done

## Story

As a General Director, I want to be able to select the mission type نوع المأمورية, so that I can
classify the assignment from the active catalogue.

## Acceptance Criteria

1. Given a General Director with an active session, when the mission-form type drop-down loads,
   then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/MissionManagement/mission-types` and the active catalogue is returned as a list
   without a page reload.
3. Given the session has expired or the role is not permitted, then the request is rejected and
   the actor is routed back to the login screen.

**Definition of done:** the endpoint serves the real `MissionType` table (not constants); only
active rows are returned; §20.U.3 passes end to end.

## What exists already — DO NOT rebuild

The whole vertical exists and is registered in DI:

| Piece | Location |
| --- | --- |
| Endpoint | `MissionManagementController.GetMissionTypes` (`MissionManagementController.cs:410-429`) — `LookupFilterDto{IsActive=true, PageSize=1000}` → `_missionTypeService.GetLookupItemsAsync` → `Ok(result.Items)` |
| Service | `MissionTypeService : LookupServiceBase<MissionType,…>` (`LookupManagementService.cs:563`), `IMissionTypeService` (`ILookupService.cs:61`) |
| Repository | `MissionTypeRepository : LookupRepository<MissionType>` (`LookupRepository.cs:147`) |
| DTO | `MissionTypeDto : LookupDto` (`LookupDtos.cs:182`) |
| Entity / config | `Domain/Entities/Lookups/MissionType.cs` (`LookupEntity`, int key, `TypeCode`, `TypeDescription`), `MissionTypeConfiguration.cs` |
| DI | `ServiceCollectionExtensions.cs:173` (service) and `:201` (repository) |
| Frontend | `mission.service.getMissionTypes()` → `GET /api/missionmanagement/mission-types` (`mission.service.ts:218-220`) |

## Verified gaps

1. **The frontend never calls it.** `mission-form.component.ts loadLookupData()` (lines 189-218)
   fills the type drop-down with a **hardcoded 6-item array** (`missions.missionTypeFieldwork` …
   `missionTypeOther`) whose ids 1-6 are fiction — saving would write invalid FKs. Wiring the form
   to the real endpoint is **story 15-6's** form-rebuild task; this story only guarantees the
   endpoint serves the catalogue correctly.
2. **Catalogue content unverified.** Whether any `MissionType` rows exist in the database is
   unknown — an empty catalogue makes the mandatory form field unusable downstream.

## Tasks / Subtasks

- [x] **Task 1 — Verify the endpoint live** (AC 2)
  - [x] `GET /api/MissionManagement/mission-types` (as Admin) returns camelCase
        `[{id, name, nameAr, nameEn, …}]` — only `IsActive` rows, soft-deleted excluded (global
        query filter), ordered stably (name/NameAr)
  - [x] Confirm `[Authorize]` reaches it (class-level `Admin,SuperAdmin` on the controller)
- [x] **Task 2 — Guarantee catalogue data** (AC 2)
  - [x] Check the table; if empty, seed the mission types via the existing lookup management
        facility (or a `HasData` seed in `MissionTypeConfiguration` + migration, matching how other
        lookups were seeded) — Arabic + English names mandatory (`NameAr`/`NameEn`)
- [x] **Task 3 — Trim dead weight** (guard against reinvention)
  - [x] `Frontend mission.model.ts` carries `MissionTypeEnum` (static string enum, lines 246-253)
        and the form's static type array — mark/keep them out of the live path; full removal lands
        with 15-6's form rebuild. Do **not** build a second type service on the frontend.

## Dev Notes

- The board's realisation maps this to `IMissionService.MissionTypes`; in this codebase the
  catalogue lives in the lookup stack (`IMissionTypeService`) and the mission controller only
  brokers it — that is the shipped pattern (`office-project-types` in
  `LookupManagementController.cs:769`), keep it.
- Bilingual rule: dropdowns display `NameAr ?? NameEn` (profile convention across the platform).
- Do not add caching by hand — `ICacheService` is the only sanctioned cache, and only if the
  lookup stack doesn't already handle it; check before adding.

### References

- [Source: docs/Modules/20-UC-MSN-Missions.md#20.U.3] scenario
- [Source: Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs:410-429] endpoint
- [Source: Backend/src/IIROSA.Application/Services/LookupManagementService.cs:563] service
- [Source: _bmad-output/planning-artifacts/epics.md#3.15] US-MSN-03 acceptance criteria

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

Live verification 2026-08-23: `GET /api/MissionManagement/mission-types` returned the 3 live
Arabic rows (`مقابلة / اجتماع`, …) in camelCase with `name` resolved — the table was already
populated (2026-05-04 legacy data), so no re-seed was needed or performed.

### Completion Notes List

- Endpoint verified against real data; class-level `[Authorize(Roles = "Admin,SuperAdmin")]`
  covers it; soft-deleted/`IsActive=false` rows excluded by the lookup stack.
- Seeding posture: the dev table already held 3 Arabic `MissionType` rows predating this epic —
  **preserved as-is** (decision: never overwrite live lookup data);
  `MissionLookupSeedData` seeds spec-shaped rows on fresh databases only (existence-guarded), and
  no longer sets explicit `Id` values (the lookup `Id` columns are IDENTITY — explicit ids made
  `SaveChanges` throw "Cannot insert explicit value for identity column", which the seed
  initializer swallowed silently).
- Create/edit form's نوع المأموريه select binds this catalogue
  (`getMissionTypes()` → `MissionLookupItem`); static `MissionTypeEnum`/hardcoded arrays kept out
  of the live path.

### File List

- `Backend/src/IIROSA.Infrastructure/Data/SeedData/MissionLookupSeedData.cs` — id-less seeding
- `Frontend/src/app/modules/missions/mission-form/mission-form.component.ts|.html`
- `Frontend/src/app/modules/missions/models/mission.model.ts` — MissionLookupItem

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created from `epics.md` US-MSN-03 and module spec §20.S.2 / §20.U.3. |
| 2026-08-24 | Implemented (review-and-complete pass over the copied vertical), verified live against the running API, status → review. |
| 2026-08-24 | Code review: no patch findings — verified clean; status → done. |
