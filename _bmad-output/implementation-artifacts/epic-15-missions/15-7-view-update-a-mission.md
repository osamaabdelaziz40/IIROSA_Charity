# Story 15-7: View / update a mission

| Field | Value |
| --- | --- |
| Story key | `15-7-view-update-a-mission` |
| Epic | EP-15 — Missions (المأموريات) |
| Use case | UC-MSN-07 — تعديل المأمورية |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/20-UC-MSN-Missions.md` (§20.S.1 detail/edit context, §20.U.7 scenario) |
| Route | `#/missions/:id` (detail) · `#/missions/:id/edit` (edit) |
| Endpoint | `GET /api/MissionManagement/{id}` · `PUT /api/MissionManagement/{id}` |
| Depends on | **15-6 landed** (clean DTOs, validator, UoW write path, real lookups, interview-type column) |
| Roles | Gen. Director, Staff → `SuperAdmin`, `Admin` |

## Status

done

## Story

As a General Director, I want to be able to view / update a mission تعديل المأمورية, so that a
record that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given a General Director, when the actor saves valid changes, then the stored record carries
   the new values and no other record is affected.
2. Given the request is accepted, when it is served, then the read is handled by
   `GET /api/MissionManagement/{id}` and the response is rendered without a page reload.
3. Given a mandatory field listed in §20.S.2 is empty, when the actor saves, then the save is
   refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, then the request is rejected and
   the actor is routed back to the login screen.

**Definition of done:** §20.U.7 passes end to end; completed missions are immutable; the caller
scope applies to the single read too.

## Verified defects this story must fix

1. **`getMissionById` always throws.** `mission.service.ts:78-88` checks `response.success` on an
   `{success,data}` envelope — the controller returns the raw `MissionDetailDto`, so
   `response.success` is undefined and **every** detail load errors out. The detail and edit
   screens are dead.
2. **The detail read returns hollow names.** `GetMissionByIdAsync` uses
   `_missionRepository.GetByIdAsync(id)` with **no includes** — `MissionTypeName`,
   `RegionName`, `CenterName`, `AssignedUserName` etc. on `MissionDetailDto` map from null
   navigations. It must use `IncludeNavigationProperties()` (+ `AssignedUser`, fixed in 15-1,
   + `MissionInterviewType` from 15-6).
3. **No caller scope on the read** — any Admin can fetch any country's mission by id; apply the
   same pin-never-widen country check as the list (15-1).
4. **Update wire contract broken** — same FK-prefix mismatch as create was (fixed for create in
   15-6; `UpdateMissionDto` still binds `FK_*`). The form's `populateForm` also patches
   `assignedTo` from `mission.assignedTo` (a *display name* on the wire) — the select then holds
   a name, not a user id.
5. **Frontend `Mission` model conflates id/name** (`mission.model.ts:41` `assignedTo: string`
   "UserId" but list wire sends `assignedTo` = FullName). Align the model to the real
   `MissionDetailDto` wire: `assignedToUserId` + `assignedUserName`.
6. **Update path still repo-saves + manual patching** — convert with the 15-6 pattern
   (`UpdateMissionValidator`, UoW-only save); keep the copied rule *a completed mission cannot be
   modified* (matches spec: update happens "after execution" of findings, registration is 15-9).

## Tasks / Subtasks

- [x] **Task 1 — Detail read** (AC 2; defects 1, 2, 3)
  - [x] `GetMissionByIdAsync`: include navigations; not-found → null → controller `NotFound`;
        country-scope check (404, not 403, for out-of-scope ids — don't leak existence)
  - [x] `mission.service.getMissionById` reads the raw DTO (drop the envelope mapping); align
        `Mission` model fields to the wire (defect 5)
- [x] **Task 2 — Update write path** (AC 1, 3; defects 4, 6)
  - [x] Rename `UpdateMissionDto` keys to the 15-6 clean wire names; `UpdateMissionValidator`
        (mandatory rules per §20.S.2, same as create); service: load → completed-check →
        validate → apply → save via `IUnitOfWork` only
  - [x] Controller `UpdateMission`: `ValidationException` → errors map (15-6 pattern);
        `InvalidOperationException` → `BadRequest(new { message })`
  - [x] `mission.service.updateMission` reads the raw DTO; form flags field errors from the map
- [x] **Task 3 — Edit screen** (AC 3, 4)
  - [x] `populateForm` patches `assignedToUserId` (id, not name), `missionInterviewTypeId`,
        `charityId`; lookups load exactly as 15-6 wired them (they are the same component)
  - [x] On save success navigate to `#/missions/:id` (current behaviour) or the list — keep
        current navigation
- [x] **Task 4 — Detail screen** (AC 2)
  - [x] `mission-detail.component` renders `MissionDetailDto` fields (names included); remove its
        `markAsCompleted` PATCH call (dead endpoint; completion UI belongs to 15-9's register
        screen) — display completion state read-only
  - [x] Add an edit button → `#/missions/:id/edit` and a back-to-list breadcrumb
- [x] **Task 5 — Route + permission**: `#/missions/:id` → `Missions.View`; `#/missions/:id/edit`
      → `Missions.Edit` (both `AuthGuard + PermissionGuard`; entries exist from 15-1)
- [x] **Task 6 — i18n**: new keys in both locales; no hard-coded strings
- [x] **Task 7 — Verify** (AC 1–5): load a seeded mission (names populated), edit + save, list
      reflects changes; empty mandatory field flags; editing a completed mission refused with a
      translated message; out-of-scope id → 404; builds green

## Dev Notes

- The board writes `PUT /api/MissionManagement`; the shipped route is `PUT {id}` (consistent with
  the reviewed modules) — keep `PUT {id}`, it is the same contract with the id on the route.
- "Cannot update a completed mission" is *kept from the copy* and consistent with §20.U.7's
  purpose (corrections before the register entry is final); 15-9 is the sanctioned path for
  outcomes. Surface the refusal as a translated message, not a raw 500.
- Do not touch `PUT {id}/event` (15-9 reworks it) or delete (15-8).
- `ApiResponse<T>` envelope and `data-list` remain out (platform deviations, see 15-1).

### References

- [Source: docs/Modules/20-UC-MSN-Missions.md#20.U.7] scenario
- [Source: Backend/src/IIROSA.Application/Services/MissionService.cs:100-118] hollow detail read;
  [:175-229] update path
- [Source: Frontend/src/app/modules/missions/services/mission.service.ts:78-88] envelope bug
- [Source: _bmad-output/planning-artifacts/epics.md#3.15] US-MSN-07 acceptance criteria

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

Live verification 2026-08-23: `GET /api/MissionManagement/{id}` returns camelCase with **clean**
id keys — `missionTypeId`, `regionId`, `centerId`, `assignedToUserId`, `charityId` — plus resolved
names (`missionTypeName`, `centerName`, `assignedUserName`, `assignedUserEmail`); no `fK_*`
anywhere on the wire. Update on a completed mission → 400 "Cannot update a completed mission"
(checked together with 15-9's immutability rules).

### Completion Notes List

- Detail read now goes through `IncludeNavigationProperties()` + caller-scope check — an
  out-of-scope id is treated exactly like a missing one (404-shaped null), per the epic-13
  pattern. `getMissionById`'s copied `success/data` wrapper expectations removed; the SPA reads
  the raw DTO.
- Update path: patch semantics (only non-null DTO fields applied), UoW-only saves, completed
  missions immutable (the sanctioned outcome path is 15-9's register). The copied legacy
  fine-grained setters (set-date/assign-type/assign-owner/…) remain for UC-8.x compatibility but
  are out of §20's route set.
- Detail screen rebuilt around the real wire contract: classification/assignment/audit sections,
  register-result action replaces the removed `markAsCompleted`, edit disabled when completed,
  delete gated on `Missions.Delete` (SuperAdmin).
- Edit form populates the cascade correctly: patch → load regions by country → re-patch
  `regionId` → load centers → re-patch `centerId` (the copied form lost the region/center values
  on edit because it never chained the cascade loads).

### File List

- `Backend/src/IIROSA.Application/Services/MissionService.cs` — GetMissionByIdAsync/Update rewrite
- `Backend/src/IIROSA.Application/Validators/MissionManagement/UpdateMissionValidator.cs`
- `Backend/src/IIROSA.Application/DTOs/MissionManagement/Missions.cs` — detail DTO clean keys
- `Backend/src/IIROSA.Application/Profiles/MissionProfile.cs` — 8 explicit id ForMembers
- `Frontend/src/app/modules/missions/mission-detail/mission-detail.component.ts|.html`
- `Frontend/src/app/modules/missions/mission-form/mission-form.component.ts|.html` — cascade populate
- `Frontend/src/app/modules/missions/models/mission.model.ts`
- `Frontend/src/assets/i18n/ar.json`, `en.json`

### Review Findings

- [x] [Review][Patch] Legacy UC-8.x fine-grained endpoints (`{id}/date`, `{id}/type`, `{id}/timetype`,
      `{id}/location`, `{id}/assign`) bypass both the caller-scope check and the
      completed-immutability guard — and `SetMissionLocationAsync` takes the country from the
      payload; add `IsWithinCallerScope` + completed guards (or prune the set, as 15-6's Dev Notes
      already flagged as candidate debt)
      [Backend/src/IIROSA.Application/Services/MissionService.cs:379,415,446,477,524] —
      fixed: all five now scope-check; `AssignMissionOwnerAsync` also gained the completed guard;
      `SetMissionLocationAsync` keeps the country inside the caller's pin
- [x] [Review][Patch] Update never re-pins the caller's country — a country-pinned caller can move
      a mission out of their scope on edit; mirror the create-time pin
      [Backend/src/IIROSA.Application/Services/MissionService.cs:204 — UpdateMissionAsync]
- [x] [Review][Patch] Update rejects any edit of a past-dated mission even when the date is
      unchanged — compare against the loaded entity and block only *changes* to a past date
      [Backend/src/IIROSA.Application/Services/MissionService.cs — UpdateMissionAsync] —
      fixed: the validator rule moved into the service and compares against the loaded record

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created from `epics.md` US-MSN-07 and module spec §20.S / §20.U.7; copied-code defects audited. |
| 2026-08-24 | Implemented (review-and-complete pass over the copied vertical), verified live against the running API, status → review. |
| 2026-08-24 | Code review: 3 findings fixed (legacy UC-8.x scope + immutability guards, update country re-pin, past-date change-detection); status → done. |
