# Story 15-6: Create a mission

| Field | Value |
| --- | --- |
| Story key | `15-6-create-a-mission` |
| Epic | EP-15 — Missions (المأموريات) |
| Use case | UC-MSN-06 — تسجيل المأمورية |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/20-UC-MSN-Missions.md` (§20.S.2 screen — 15 fields, §20.U.6 scenario) |
| Route | `#/missions/create` and `#/missions/:id/edit` (one `MissionFormComponent`, add mode here) |
| Endpoint | `POST /api/MissionManagement` |
| Depends on | **15-1 landed** (charity dimension, service scope, permission entries); 15-3/15-4/15-5 catalogues live |
| Roles | Gen. Director, Staff → `SuperAdmin`, `Admin` |

## Status

done

## Story

As a General Director, I want to be able to create a mission تسجيل المأمورية, so that the
register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a General Director on `#/missions/create`, when the actor presses «حفظ» with valid
   input, then a new record exists, owned by the caller's charity claim (stamped server-side),
   and appears in the list screen.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/MissionManagement` with a typed DTO and the response is rendered without a page
   reload.
3. Given a mandatory field listed in §20.S.2 is empty, when the actor saves, then the save is
   refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to `#/missions`, then the record appears there
   with the values just entered.
5. Given the session has expired or the role is not permitted, then the request is rejected and
   the actor is routed back to the login screen.

**Definition of done:** §20.S.2 fields with their mandatory flags and lookups; §20.U.6 passes end
to end; validation runs in the service layer (FluentValidation); only the UnitOfWork saves.

## Verified defects this story must fix

1. **The form's lookups are all fake** (`mission-form.component.ts:189-218`): mission types and
   time types are hardcoded arrays with fictional ids 1-6; `countries/regions/centers/users` are
   **empty arrays under TODOs** — so the mandatory الموظف المسئول field can never be filled and
   **the form can never be validly submitted**. The create path is dead in practice.
2. **Create wire contract broken.** The form posts `CreateMissionRequest{missionTypeId,
   missionTimeTypeId, countryId, regionId, centerId, assignedTo, …}` but `CreateMissionDto` binds
   `FK_MissionTypeId, FK_MissionTimeTypeId, FK_CountryId, FK_RegionId, FK_CenterId, FK_UserId, …`
   — the FK fields bind to default(0/Guid.Empty) and the mission saves with garbage keys.
3. **`mission.service.createMission` expects an `{success, data}` envelope**
   (`mission.service.ts:95-105`) — the controller returns the raw DTO via `CreatedAtAction`, so
   `response.success` is undefined and the observable **throws on a successful save**.
4. **No interview type anywhere.** §20.S.2 makes نوع المقابلة (`MissionInterviewTypeId`)
   mandatory; `Mission` has no such column and the form has no such control.
5. **Persistence rule violated.** `MissionService.CreateMissionAsync` calls
   `_missionRepository.AddAsync` + `_missionRepository.SaveChangesAsync()` — repositories must
   never save; only `IUnitOfWork` does (CLAUDE.md; the reviewed OfficeProjectService is the
   reference).
6. **No FluentValidation validator** — business rules are inline `if/throw` in the service;
   platform rule is validators in `Application/Validators/` invoked by the service layer
   (reference: `CreateOfficeProjectValidator`).
7. **Cascade logic is placeholder** — `onCountryChange` filters `regions.filter(r => r.id ===
   countryId)` (filters regions by *country id*, not `region.countryId`).

## Tasks / Subtasks

- [x] **Task 1 — Schema: add the interview-type column** (AC 1, defect 4)
  - [x] `Mission`: `int? FK_MissionInterviewTypeId` + `virtual MissionInterviewType?
        MissionInterviewType` navigation (lookup from 15-4)
  - [x] `MissionConfiguration`: FK + relationship, `MappingDefaults.IIROSA_SCHEMA`; migration
        (`dotnet ef migrations add AddMissionInterviewType …`) and apply
  - [x] Include the navigation in `MissionRepository.IncludeNavigationProperties()`; map
        `MissionInterviewTypeName` on `MissionDetailDto` (`NameAr ?? NameEn` convention)
- [x] **Task 2 — DTO + validator + UoW on the write path** (AC 2, 3; defects 2, 5, 6)
  - [x] Rename `CreateMissionDto` keys to the clean wire names the SPA sends: `MissionTypeId,
        MissionTimeTypeId, MissionInterviewTypeId, CountryId, RegionId, CenterId,
        AssignedToUserId, EntityName, ConferenceName, MissionTarget, MissionDetails, Details,
        MissionLocation, Village, MissionDate` — typed DTO only, never untyped
  - [x] `Application/Validators/MissionManagement/CreateMissionValidator.cs` (new folder),
        modelled on `CreateOfficeProjectValidator`: mandatory per §20.S.2 — MissionDate,
        MissionTypeId, EntityName, MissionInterviewTypeId, ConferenceName, Details (المهمه),
        MissionTarget, MissionDetails, MissionTimeTypeId, MissionLocation, AssignedToUserId,
        Village, CenterId, RegionId; keep the copied rules worth keeping (date not in past,
        center→region→country cascade) as validator rules
  - [x] Service: inject `IUnitOfWork` + the validator; validate → map → `Add` → save **through
        the UoW only**; drop `_missionRepository.SaveChangesAsync()`
  - [x] Stamp ownership server-side: `FK_CharityId` from the caller's charity claim via
        `ICurrentUserService` (HQ callers without a claim save null) — never from the payload
  - [x] Controller `CreateMission`: `catch (FluentValidation.ValidationException)` →
        `BadRequest(new { message, errors = … })` in the `OfficeProjectManagementController.cs:89-101`
        shape so the client gets a field→messages map
- [x] **Task 3 — Wire the form to real lookups** (AC 1, 3; defects 1, 7)
  - [x] mission types → `getMissionTypes()` (15-3); time types → `getMissionTimeTypes()` (15-5);
        interview types → `getMissionInterviewTypes()` (15-4)
  - [x] countries → `GET /api/LookupManagement/countries`; regions →
        `regions/by-country/{countryId}`; centers → `centers/by-region/{regionId}` (all exist);
        fix the cascade to filter by the parent's real key
  - [x] users (الموظف المسئول, §20.S.2 `allEmployees`) → the existing employees endpoint
        (`GET /api/EmployeeManagement`, epic 4) — reuse the employees service if one exists;
        option label = employee full name, value = user id
  - [x] Add the نوع المقابلة control (mandatory); delete the hardcoded arrays and their i18n
        constants from the load path
- [x] **Task 4 — Fix the service call + navigation** (AC 2, 4; defect 3)
  - [x] `mission.service.createMission` reads the raw DTO (drop the `success/data` mapping);
        surface validator errors (field → message) to the form and flag fields
  - [x] On success navigate to `#/missions` (spec post-condition); on failure stay and flag
- [x] **Task 5 — Route + permission**: `#/missions/create` guarded `AuthGuard + PermissionGuard`
      with `data.permission: 'Missions.Create'` (entry exists in `PERMISSION_ROLES` from 15-1)
- [x] **Task 6 — i18n**: every new label/error in `missions.*` of **both** `ar.json` and `en.json`
- [x] **Task 7 — Verify** (AC 1–5): create via the UI → row appears in `#/missions` with correct
      type/interview/time/assigned names; mandatory-field omission flags the field; invalid FK
      rejected; `dotnet build` + `npm run build` green

## Dev Notes

- **§20.S.2 spec anomaly (recorded deviation):** the screen lists اسم الجهة and اسم الجهة
  المنظمة **both bound to `Mission.EntityName`**. The entity models one field — ship ONE control
  labelled اسم الجهة المنظمة; the list grid's الجهه column reads the same `EntityName`. Do not
  add a second column.
- The copied controller's fine-grained endpoints (`PUT {id}/date`, `{id}/type`, `{id}/timetype`,
  `{id}/location`, `{id}/assign`) are beyond the 9 specified use cases — legacy capability-variant
  smell. Out of scope here; prune only if they block the build (candidate debt for the epic retro).
- Notifications TODOs in the service (notify assigned user) — leave the TODOs; notifications are
  not in this epic's scope.
- Edit mode of the same component is **15-7**'s story — implement create cleanly, leave
  `checkEditMode`/`populateForm` working but don't polish edit semantics here.
- `AssignedTo` on the frontend `Mission` model conflates id and display name — 15-7 fixes the
  model; here only the *create* payload's `assignedToUserId` matters.

### References

- [Source: docs/Modules/20-UC-MSN-Missions.md#20.S.2] the 15-field screen contract
- [Source: docs/Modules/20-UC-MSN-Missions.md#20.U.6] scenario — stamps owning charity + user
- [Source: Backend/src/IIROSA.Application/Services/MissionService.cs:123-170] current create path
  (manual validation, repo SaveChanges)
- [Source: Backend/src/IIROSA.Api/Controllers/OfficeProjectManagementController.cs:89-101]
  ValidationException → errors-map pattern
- [Source: _bmad-output/planning-artifacts/epics.md#3.15] US-MSN-06 acceptance criteria

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

Live verification 2026-08-23 (SuperAdmin): `POST /api/MissionManagement` with the clean-key payload
→ **201** with the full detail echo — Arabic round-trips correctly when sent as UTF-8 (a payload
file proved the API stores/returns real Arabic; the `?????` seen with inline curl bodies is a
Git-Bash console codepage artifact, not an app defect). Incomplete payloads → 400
`{message: "One or more fields are invalid", errors: {RegionId: […], …}}` (PascalCase keys the
client maps onto controls). The created row's `assignedUserName` resolved through the repointed
`identity.Users` join (see 15-1) — the physical FK no longer rejects an assignee.

### Completion Notes List

- Write path rebuilt to platform rules: FluentValidation `CreateMissionValidator` invoked in the
  service via `ValidateAndThrowAsync`; **only `IUnitOfWork` saves** (the copied code called
  `_missionRepository.SaveChangesAsync()`); charity ownership stamped server-side from the caller's
  claim (never from the payload); caller's country pinned onto the record when the token carries
  one. Replaces the copied legacy pair `CreateMission`/`AddMission`.
- All §20.S.2 fields mandatory, including the spec's anomaly of a single الجهة المنظمة control
  (`entityName`) — recorded in the story analysis; the old system's dual entity/conference JSON
  pair was replaced by plain columns.
- **Wire-casing fix**: `MissionDetailDto`'s id properties renamed from `FK_*` to clean names
  (`MissionTypeId`, `CountryId`, `AssignedToUserId` …) with explicit `ForMember` maps in
  `MissionProfile`. Reason: Newtonsoft's camelCase policy turns `FK_MissionTypeId` into
  `fK_MissionTypeId` (FixCasing lowercases the leading F, then stops at the underscore) — inbound
  binding is case-insensitive, so only the outbound contract needed clean names (13-3 precedent).
- Create form rebuilt (4-file shape): all-mandatory fields with `*` markers, country→region→center
  cascade with disabled gating, employees select (`AssignedToUserId`), server-error mapping onto
  controls, i18n for every label — the copied form's static arrays and dual-entity controls are
  gone.

### File List

- `Backend/src/IIROSA.Application/Services/MissionService.cs` — CreateMissionAsync rewrite
- `Backend/src/IIROSA.Application/Validators/MissionManagement/CreateMissionValidator.cs`
- `Backend/src/IIROSA.Application/DTOs/MissionManagement/Missions.cs` — CreateMissionDto + detail keys
- `Backend/src/IIROSA.Application/Profiles/MissionProfile.cs`
- `Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs` — validation error contract
- `Frontend/src/app/modules/missions/mission-form/mission-form.component.ts|.html|.scss|.spec.ts`
- `Frontend/src/app/modules/missions/models/mission.model.ts`
- `Frontend/src/assets/i18n/ar.json`, `en.json`

### Review Findings

- [x] [Review][Patch] Edit-mode selects never preselect — `[value]` stringifies option keys while
      the patched controls hold numeric ids; switch all seven selects to `[ngValue]` (the charity
      filter on the list screen already does)
      [Frontend/src/app/modules/missions/mission-form/mission-form.component.html:91,110,129,180,201,221,282]
- [x] [Review][Patch] Create allows back-dating — the copied "date not in past" rule was dropped
      from `CreateMissionValidator` although 15-6 Task 2 said keep it
      [Backend/src/IIROSA.Application/Validators/MissionManagement/CreateMissionValidator.cs — MissionDate]
- [x] [Review][Patch] Bogus FK ids reach SQL and 500 instead of 400 field errors — no existence
      checks for missionType / timeType / interviewType / country / region / center / assignee
      [Backend/src/IIROSA.Application/Services/MissionService.cs — CreateMissionAsync] —
      fixed: `ValidateForeignKeysAsync` on create AND update (assignee via `IEmployeeService`)
- [x] [Review][Patch] "Country must be specified" message sits on the `RegionId` rules (and is
      duplicated) — fix the text to name the field actually validated
      [Backend/src/IIROSA.Application/Validators/MissionManagement/CreateMissionValidator.cs:73-77]
- [x] [Review][Patch] No `MaximumLength` rules against the entity's `HasMaxLength` columns —
      oversized create input survives validation and dies as a SQL 500
      [Backend/src/IIROSA.Application/Validators/MissionManagement/CreateMissionValidator.cs] —
      **verified on fix**: create's limits already matched `MissionConfiguration` exactly; the real
      gap was register-side (fixed under 15-9)
- [x] [Review][Patch] Catalogue/cascade load errors swallowed — a failed lookup load leaves empty
      dropdowns with no notification
      [Frontend/src/app/modules/missions/mission-form/mission-form.component.ts — loadCatalogues / populateForm] —
      fixed: every catalogue and cascade load toasts `missions.catalogueLoadFailed`
- [x] [Review][Defer] Catalogue reads cap at `PageSize=1000` (mission-types, interview-types,
      employees select) — silently truncates past 1000 rows; a platform-wide pattern, not
      missions-specific [Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs] —
      deferred, pre-existing

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created from `epics.md` US-MSN-06 and module spec §20.S.2 / §20.U.6; copied-code defects audited. |
| 2026-08-24 | Implemented (review-and-complete pass over the copied vertical), verified live against the running API, status → review. |
| 2026-08-24 | Code review: 6 findings fixed (`[ngValue]` selects, past-date rule restored, FK existence checks → 400, cascade message typo, create length limits verified, catalogue error toasts); 1 deferred; status → done. |
