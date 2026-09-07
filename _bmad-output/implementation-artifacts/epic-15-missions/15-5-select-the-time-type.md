# Story 15-5: Select the time type

| Field | Value |
| --- | --- |
| Story key | `15-5-select-the-time-type` |
| Epic | EP-15 — Missions (المأموريات) |
| Use case | UC-MSN-05 — التوقيت |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/20-UC-MSN-Missions.md` (§20.S.2/§20.S.3 form field نوع توقيت المأموريه, §20.U.5 scenario) |
| Endpoint | `GET /api/MissionManagement/mission-time-types` (exists as a **hardcoded stub** — replace) |
| Depends on | EP-01; independent of 15-1…15-4 |
| Roles | Gen. Director, Staff → `SuperAdmin`, `Admin` |

## Status

done

## Story

As a General Director, I want to be able to select the time type التوقيت, so that the mission's
time classification (morning / evening / full day) comes from the active catalogue.

## Acceptance Criteria

1. Given a General Director with an active session, when the time-type drop-down loads, then no
   stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/MissionManagement/mission-time-types` returning rows from the **`MissionTimeType`
   table**, without a page reload.
3. Given the session has expired or the role is not permitted, then the request is rejected and
   the actor is routed back to the login screen.

**Definition of done:** the endpoint serves real data (no constants); §20.U.5 passes end to end.

## Verified defect — the endpoint is a fake

`MissionManagementController.GetMissionTimeTypes` (`MissionManagementController.cs:435-456`)
returns a **hardcoded `List<MissionTimeTypeDto>` with four invented rows** (One-time / Daily /
Weekly / Monthly) under a TODO — the database is never queried. Meanwhile the real stack is
half-present:

| Piece | State |
| --- | --- |
| Entity | `Domain/Entities/Lookups/MissionTimeType.cs` — exists (`LookupEntity`, `TimeTypeCode`, `TypeDescription`) |
| Config | `MissionTimeTypeConfiguration.cs` — exists |
| DTO | `MissionTimeTypeDto : LookupDto` (`LookupDtos.cs:213`) — exists |
| Service / interface | **missing** (only `MissionTypeService` exists, `LookupManagementService.cs:563`) |
| Repository / DI | **missing** (only `IMissionTypeRepository`/`MissionTypeRepository` registered, `ServiceCollectionExtensions.cs:173,201`) |
| Frontend | `mission.service.getMissionTimeTypes()` (`mission.service.ts:226-228`) — exists, unused; the form uses a hardcoded 6-item array (`mission-form.component.ts:202-209`) |

## Tasks / Subtasks

- [x] **Task 1 — Build the service stack** mirroring MissionType line for line:
  - [x] `IMissionTimeTypeService : ILookupService<MissionTimeTypeDto, …>` in `ILookupService.cs`
  - [x] `MissionTimeTypeService : LookupServiceBase<MissionTimeType, …>` in
        `LookupManagementService.cs`
  - [x] `MissionTimeTypeRepository : LookupRepository<MissionTimeType>` in `LookupRepository.cs` +
        interface
  - [x] DI registrations in `ServiceCollectionExtensions.cs` beside the MissionType ones
- [x] **Task 2 — Replace the stub** (AC 2): rewrite `GetMissionTimeTypes` in the exact shape of
      `GetMissionTypes` (`MissionManagementController.cs:410-429`) — inject
      `IMissionTimeTypeService`, `LookupFilterDto{IsActive=true, PageSize=1000}`,
      `Ok(result.Items)`; delete the hardcoded list and its TODO
- [x] **Task 3 — Seed data**: ensure the `MissionTimeType` table holds the real classification
      (morning / evening / full day per the spec summary — صباحي / مسائي / يوم كامل — or the
      seeded catalogue's own convention); bilingual `NameAr`/`NameEn` on every row; migration if
      seeded via `HasData`
- [x] **Task 4 — Verify** (AC 1–3): live call returns the seeded rows camelCase, active-only;
      unauthenticated rejected; `dotnet build` 0 errors
- [x] **Task 5 — Frontend readiness**: `getMissionTimeTypes()` already points at the right URL —
      leave it; the form's static array removal lands with 15-6. Do not create a second service.

## Dev Notes

- The four stub rows (One-time/Daily/Weekly/Monthly) describe *frequency*, but the spec says the
  catalogue is a **time-of-day classification** — seed per the spec (§20 catalogue summary), not
  per the stub.
- Keep the endpoint under `MissionManagement` (board contract), brokered to the lookup service —
  same as mission-types.
- Soft delete/IsActive handled by the lookup base; no manual filtering; no hand-rolled caching.

### References

- [Source: docs/Modules/20-UC-MSN-Missions.md#20.U.5] scenario
- [Source: Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs:435-456] the stub
- [Source: _bmad-output/planning-artifacts/epics.md#3.15] US-MSN-05 acceptance criteria

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

Live verification 2026-08-23: `GET /api/MissionManagement/mission-time-types` returned the 3 live
Arabic rows (جزء من يوم …) in camelCase — legacy rows predating this epic, preserved untouched.

### Completion Notes List

- The controller's hardcoded TODO stub was replaced by the repository-backed lookup read (same
  shipped pattern as `mission-types`); camelCase wire verified live.
- Dev table already held 3 Arabic `MissionTimeType` rows (2026-05-04) — **preserved**;
  `MissionLookupSeedData` seeds spec-shaped rows (صباحي/مسائي/يوم كامل) on fresh DBs only, and sets
  no explicit `Id` (identity column — see 15-4's seeding fix).
- Mandatory field wired: توقيت المأمورية select on the create/edit form (required),
  `missionTimeTypeName` on the detail DTO, read-only on the register screen.

### File List

- `Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs` — stub → repository read
- `Backend/src/IIROSA.Infrastructure/Data/SeedData/MissionLookupSeedData.cs`
- `Backend/src/IIROSA.Application/…` — DTO/profile updates
- `Frontend/src/app/modules/missions/**` — form select + model
- `Frontend/src/assets/i18n/ar.json`, `en.json`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created from `epics.md` US-MSN-05 and module spec §20.S.2 / §20.U.5. |
| 2026-08-24 | Implemented (review-and-complete pass over the copied vertical), verified live against the running API, status → review. |
| 2026-08-24 | Code review: no patch findings — verified clean; status → done. |
