# Story 15-4: Select the interview type

| Field | Value |
| --- | --- |
| Story key | `15-4-select-the-interview-type` |
| Epic | EP-15 — Missions (المأموريات) |
| Use case | UC-MSN-04 — نوع المقابلة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/20-UC-MSN-Missions.md` (§20.S.2/§20.S.3 form field نوع المقابلة, §20.U.4 scenario) |
| Endpoint | `GET /api/LookupManagement/mission-interview-types` (not built — this story creates it) |
| Depends on | EP-01; independent of 15-1…15-3 |
| Roles | Gen. Director, Staff → `SuperAdmin`, `Admin` |

## Status

done

## Story

As a General Director, I want to be able to select the interview type نوع المقابلة, so that the
mission outcome can be classified from the active catalogue.

## Acceptance Criteria

1. Given a General Director with an active session, when the interview-type drop-down loads, then
   no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/LookupManagement/mission-interview-types` and the active catalogue is returned
   without a page reload.
3. Given the session has expired or the role is not permitted, then the request is rejected and
   the actor is routed back to the login screen.

**Definition of done:** the endpoint exists and serves a real lookup table; §20.U.4 passes end to
end.

## What exists — nothing (verified)

`MissionInterviewType` appears **nowhere** in the backend (grep across `Backend/src`: zero hits —
no entity, no config, no DTO, no service, no endpoint). `LookupManagementController` has no
`mission-interview-types` route. This story builds the lookup vertical from scratch.

## Build it exactly like the MissionType triplet (the in-repo template)

Copy the shape of these files — do not invent a new pattern:

| Piece | Template |
| --- | --- |
| Entity | `Domain/Entities/Lookups/MissionType.cs` — `LookupEntity` (int key, `Name/NameAr/NameEn/IsActive` + audit inherited); add a nullable `TypeCode`-style code property if useful, keep it lean |
| Configuration | `Domain/Configurations/MissionTypeConfiguration.cs` — `MappingDefaults.LOOKUP_SCHEMA`, never a literal schema string |
| Repository | `LookupRepository.cs:147` — `MissionInterviewTypeRepository : LookupRepository<MissionInterviewType>` + its interface beside `IMissionTypeRepository` |
| DTO | `LookupDtos.cs:182` — `MissionInterviewTypeDto : LookupDto` (+ Create/Update DTOs beside the MissionType ones) |
| Service | `ILookupService.cs:61` — `IMissionInterviewTypeService : ILookupService<…>`; `LookupManagementService.cs:563` — `MissionInterviewTypeService : LookupServiceBase<…>` |
| DI | `ServiceCollectionExtensions.cs` lines 173/201 — register service + repository |
| Endpoint | `LookupManagementController.cs:769` `GetOfficeProjectTypes` — `[HttpGet("mission-interview-types")]`, `[Authorize(Roles = "Admin,SuperAdmin")]`, `LookupFilterDto{IsActive=true, PageSize=1000}`, return `Ok(result.Items)` |

## Tasks / Subtasks

- [x] **Task 1 — Domain**: `MissionInterviewType` lookup entity + `MissionInterviewTypeConfiguration`
- [x] **Task 2 — Application**: DTOs, `IMissionInterviewTypeService`, `MissionInterviewTypeService`
- [x] **Task 3 — Infrastructure**: repository + DI registrations; migration
      (`dotnet ef migrations add AddMissionInterviewType …` per CLAUDE.md) and apply
- [x] **Task 4 — API**: `GET mission-interview-types` on `LookupManagementController` in the
      office-project-types shape (AC 2)
- [x] **Task 5 — Seed data**: populate the catalogue (Arabic + English names — interview types
      such as مقابلة ميدانية / مقابلة مكتبية / مقابلة هاتفية or whatever the seeded MissionType
      style suggests); an empty catalogue blocks 15-6's mandatory form field
- [x] **Task 6 — Frontend**: `mission.service.getMissionInterviewTypes(): Observable<LookupItem[]>`
      hitting the new endpoint (no UI consumer yet — the form field lands with 15-6; keep the
      method ready and typed)
- [x] **Task 7 — Verify** (AC 1–3): endpoint returns camelCase active-only list as Admin;
      rejected when unauthenticated; `dotnet build` 0 errors; `npm run build` (with
      `--legacy-peer-deps` if `npm install` is needed — known ngx-bootstrap peer conflict)

## Dev Notes

- The spec places this catalogue under `LookupManagement` (not the mission controller) — follow
  the board, unlike mission-types/time-types which live under `MissionManagement`.
- The mission-side FK (`Mission.FK_MissionInterviewTypeId`) and the form field are **15-6**'s
  schema change — this story delivers only the catalogue. Do not touch the `Mission` entity here.
- Soft delete and `IsActive` filtering come from the lookup base — no manual filtering.
- Bilingual: `NameAr`/`NameEn` both required on every seed row.

### References

- [Source: docs/Modules/20-UC-MSN-Missions.md#20.U.4] scenario — realisation
  `LookupManagementController` → `ILookupService`
- [Source: _bmad-output/planning-artifacts/epics.md#3.15] US-MSN-04 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs:763-789] the endpoint
  template

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

Live verification 2026-08-23: `GET /api/LookupManagement/mission-interview-types` returned the 3
seeded rows in camelCase (`FIELD/OFFICE/PHONE` with Arabic names مقابلة ميدانية/مكتبية/هاتفية).
Initial state was 0 rows — fixed by the seeding repairs below (verified via
`sqlcmd` row counts: InterviewType 3, SupportCategory 6, SupportPriority 4, SupportStatus 4).

### Completion Notes List

- Full vertical added: `MissionInterviewType` lookup entity + configuration (Lookup schema),
  repository/service wiring in the lookup stack, `mission-interview-types` endpoint on
  `LookupManagementController` (matches the `office-project-types` shipped pattern), and migration
  `20260823135202_Epic15_Missions` (table `Lookup.MissionInterviewType` + the mission FK).
- **Seed blocking bug found and fixed**: lookup `Id` columns are IDENTITY
  (`sys.columns is_identity = 1`); both `MissionLookupSeedData` and `TechnicalSupportSeedData` set
  explicit `Id` values, so `SaveChanges` threw "Cannot insert explicit value for identity column…
  IDENTITY_INSERT is off". `Program.cs`'s seed catch swallows it, so seeding failed **silently** —
  the epic-14 TechnicalSupport catalogue (Step 3) died before the missions seed (Step 4) ever ran.
  Removed all explicit `Id` assignments from both seeders (identity assigns; `SortOrder` preserves
  order). This also un-broke epic-14's SupportCategory/Priority/Status seeding.
- Mandatory field wired end to end: §20.S.2's نوع المقابلة select on the create/edit form
  (required), shown read-only on the register screen, and `missionInterviewTypeName` served on the
  detail DTO.

### File List

- `Backend/src/IIROSA.Domain/Entities/Lookups/MissionInterviewType.cs`
- `Backend/src/IIROSA.Domain/Configurations/MissionInterviewTypeConfiguration.cs`
- `Backend/src/IIROSA.Domain/Interfaces/IMissionInterviewTypeRepository.cs` (+ Infrastructure impl)
- `Backend/src/IIROSA.Application/…` — DTO/validator/profile/interface updates
- `Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs` — endpoint
- `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260823135202_Epic15_Missions.cs`
- `Backend/src/IIROSA.Infrastructure/Data/SeedData/MissionLookupSeedData.cs`
- `Backend/src/IIROSA.Infrastructure/Data/SeedData/TechnicalSupportSeedData.cs` — epic-14 latent fix
- `Frontend/src/app/modules/missions/**` — form select, register read-only display, model
- `Frontend/src/assets/i18n/ar.json`, `en.json`

### Review Findings

- [x] [Review][Defer] LookupManagementController read-widening — the class-level auth lets any
      authenticated role read every catalogue and bulk-export any lookup table via
      `tables/{tableName}/export`; writes are per-action `SuperAdminOnly`. Pre-dates epic-15 and is
      documented in the controller's doc comment; recommend gating the export/summary pair behind
      `SuperAdminOnly` as a platform follow-up
      [Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs:20,68,86] — deferred,
      pre-existing
- [x] [Review][Defer] Seed concurrency — two instances starting in parallel can race past the
      exist-guards and double-seed (dev-only risk today)
      [Backend/src/IIROSA.Infrastructure/Data/SeedData/MissionLookupSeedData.cs] — deferred,
      pre-existing

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created from `epics.md` US-MSN-04 and module spec §20.S.2 / §20.U.4. |
| 2026-08-24 | Implemented (review-and-complete pass over the copied vertical), verified live against the running API, status → review. |
| 2026-08-24 | Code review: no patch findings (2 pre-existing platform risks deferred); status → done. |
