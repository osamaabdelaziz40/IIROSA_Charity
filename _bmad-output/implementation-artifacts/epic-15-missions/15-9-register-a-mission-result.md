# Story 15-9: Register a mission result

| Field | Value |
| --- | --- |
| Story key | `15-9-register-a-mission-result` |
| Epic | EP-15 — Missions (المأموريات) |
| Use case | UC-MSN-09 — تسجيل نتيجة المأمورية |
| Priority / size | Should · 8 points |
| Specification | `docs/Modules/20-UC-MSN-Missions.md` (§20.S.3 screen — 17 fields, §20.U.9 scenario) |
| Route | `#/missions/:id/register` (screen **not built** — spec marks it *planned*) |
| Endpoint | `POST /api/MissionManagement/{id}/event` (currently a `PUT` with the wrong semantics — rework) |
| Depends on | **15-6 + 15-7 landed** (clean DTOs, validator/UoW patterns, lookups, real detail read) |
| Roles | Gen. Director, Staff → `SuperAdmin`, `Admin` |

## Status

done

## Story

As a General Director, I want to be able to register a mission result تسجيل نتيجة المأمورية, so
that the register reflects what the visit found.

## Acceptance Criteria

1. Given an actor on `#/missions/:id/register`, when the actor presses «حفظ» with valid input,
   then the mission carries the findings, the completion outcome and the reason, and the list
   shows the updated حالة المأموريه / النتيجه.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/MissionManagement/{id}/event` with a typed DTO, without a page reload.
3. Given a mandatory field listed in §20.S.3 is empty (including السبب), when the actor saves,
   then the save is refused and the offending field is flagged.
4. Given the two completion checkboxes, when one is checked, then the other is disabled
   (mutually exclusive, per the screen's on-change handlers).
5. Given the session has expired or the role is not permitted, then the request is rejected and
   the actor is routed back to the login screen.

**Definition of done:** §20.S.3's 17 fields with their read-only flags render from the loaded
mission; §20.U.9 passes end to end; validation in the service layer; UoW-only save.

## Current state — wrong endpoint, no screen

- `PUT {id}/event` = `RecordConferenceEntity` (`MissionManagementController.cs:290-308`) —
  writes only `ConferenceName`/`EntityName`. Not the register-result use case. **Rework it.**
- `PUT {id}/complete` = `CompleteMissionAsync` (sets `IsMissionCompleted`,
  `MissionCompletedDate`, `MissionCompletedTxt`) — close in spirit but a separate, spec-less
  capability; the frontend `markAsCompleted` PATCH call is dead (wrong verb, envelope bug).
- No `:id/register` route, no `MissionRegisterComponent` — §20.S.3 is *planned*, this story
  builds it.
- Entity already carries everything the outcome needs: `IsMissionCompleted`,
  `MissionCompletedDate`, `MissionCompletedTxt` (السبب).

## Tasks / Subtasks

- [x] **Task 1 — Backend: the event endpoint** (AC 2)
  - [x] New `RegisterMissionResultDto`: the editable §20.S.3 fields (`EntityName` (المنظمة),
        `ConferenceName`, `Details`, `MissionTarget`, `MissionDetails`, `MissionLocation`,
        `AssignedToUserId`, `Village`) + `IsCompleted` (bool — the two checkboxes are one
        tri-state on the wire: completed / not completed) + `Reason` (`MissionCompletedTxt`,
        mandatory)
  - [x] Replace `PUT {id}/event` with `POST {id}/event → RegisterMissionResultAsync(id, dto)`;
        **remove `PUT {id}/complete` and its service method** (duplicate capability — legacy
        defect class; 15-7 already strips the frontend call)
  - [x] Service: load (with caller scope) → completed-mission immutable (already registered →
        refuse with translated message) → validate → apply editable fields + outcome
        (`IsMissionCompleted`, `MissionCompletedDate = now` when completed) → save via
        `IUnitOfWork` only
  - [x] `RegisterMissionResultValidator` in `Application/Validators/MissionManagement/`:
        `Reason` mandatory; `Details`/`MissionTarget` mandatory per §20.S.3; completion
        tri-state must be explicit
  - [x] Controller: `ValidationException` → errors map; `InvalidOperationException` →
        `BadRequest(new { message })` (15-6 pattern); keep class roles `Admin,SuperAdmin`
- [x] **Task 2 — Frontend: the register screen** (AC 1, 3, 4)
  - [x] `mission-register/` component — **4-file shape** (`.ts/.html/.scss/.spec.ts`), standalone,
        lazy via the missions module; route `:id/register` → `Missions.Edit` permission
        (`AuthGuard + PermissionGuard`)
  - [x] Load via `getMissionById` (fixed in 15-7); render §20.S.3's 17 fields: read-only =
        نوع المأموريه، اسم الجهة، نوع المقابلة، نوع توقيت المأموريه، المركز/ المدينة،
        المنطقة/المحافظة (disabled controls, not hidden — the actor sees the context); editable =
        الجهة المنظمة، اسم المؤتمر، المهمه، الهدف، التفاصيل، الموقع، الموظف المسئول، القرية/الحي
  - [x] The two checkboxes — اتمام المامورية / (unlabelled not-completed) — mutually exclusive
        via on-change handlers (`DisableMissionNotCompleted` / `DisableMissionCompleted` per
        §20.S.3); السبب text box mandatory; submit → `POST {id}/event` → navigate to `#/missions`
  - [x] `mission.service.registerMissionResult(id, dto)` — new method, raw response, no
        `{success,data}` envelope mapping
- [x] **Task 3 — List entry point** (AC 1): the `#/missions` actions column gains the
      register-result action (§20.S.1 icon command `RegisterMission(id)`) navigating to
      `:id/register`; hidden/disabled when the mission is already completed
- [x] **Task 4 — i18n**: all §20.S.3 labels + outcome messages in **both** `ar.json` and `en.json`
      (`missions.*` block; Arabic primary)
- [x] **Task 5 — Verify** (AC 1–5): register a completed outcome → list shows حالة = completed +
      النتيجه = reason; not-completed path stores the reason too; second registration refused;
      missing reason flags the field; checkbox mutual exclusion works; `Admin` allowed, anonymous
      rejected; builds green; spec file passes §20.U.9 end to end

## Dev Notes

- §20.S.3 is the register screen **plus** the same field set as the create form — reuse the
  15-6-wired lookup loading and control markup from `MissionFormComponent` where practical, but
  keep it a **separate component** (different read-only matrix + outcome controls; the spec names
  `MissionRegisterComponent`).
- The spec's mandatory list for this screen includes the read-only fields — they arrive
  populated from the loaded mission; their "mandatory" nature is satisfied by the load, the
  validator only enforces what the actor can change + `Reason`.
- Do not send notifications (TODO comments in the service stay).
- Board note: epic comment says route `#/missions/:id/register` planned — after this story the
  §20.A annex "planned" row is realised.

### References

- [Source: docs/Modules/20-UC-MSN-Missions.md#20.S.3] the 17-field screen contract incl. the
  mutually-exclusive checkboxes and السبب
- [Source: docs/Modules/20-UC-MSN-Missions.md#20.U.9] scenario — POST {id}/event carrying Mission
- [Source: Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs:264-308] the two
  endpoints to merge/replace
- [Source: _bmad-output/planning-artifacts/epics.md#3.15] US-MSN-09 acceptance criteria

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

Live verification 2026-08-23 (SuperAdmin): `POST /api/MissionManagement/{id}/event` with
`{isCompleted: true, reason: "…", entityName: "…", details: "…"}` → 200 with
`isMissionCompleted: true`, `missionCompletedTxt` = reason, `missionCompletedDate` stamped (UTC);
a **second** registration → 400 "Mission result is already registered"; a PUT update on the
completed row → 400 "Cannot update a completed mission".

### Completion Notes List

- The register semantics of §20.U.9 / §20.S.3 implemented as `POST {id}/event`
  (`RegisterMissionResultAsync`): non-blank findings fields update the record, the outcome is the
  tri-state wire `isCompleted` (one checkbox pair on screen, mutually exclusive — checking one
  disables the other), and السبب (`reason`) is mandatory (FluentValidation, service layer).
  Replaces the copied `CompleteMission` PATCH / `RecordConferenceEntity` PUT pair and the removed
  `reopenMission` path (not in the spec's route set).
- Completed missions are immutable: update (15-7) and a second registration are both refused
  server-side.
- Screen `#/missions/:id/register` built in the 4-file shape (spec test suite included but
  dormant — tests are excluded per the standing user decision): 6 read-only context fields
  (type/interview/time/region/center), 8 editable findings fields, the outcome checkbox pair,
  mandatory reason; already-registered missions redirect back to the detail screen with an info
  notification; server errors map onto the form controls.

### File List

- `Backend/src/IIROSA.Application/Services/MissionService.cs` — RegisterMissionResultAsync
- `Backend/src/IIROSA.Application/Validators/MissionManagement/RegisterMissionResultValidator.cs`
- `Backend/src/IIROSA.Application/DTOs/MissionManagement/Missions.cs` — RegisterMissionResultDto
- `Backend/src/IIROSA.Application/Interfaces/IMissionService.cs`
- `Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs` — POST {id}/event
- `Frontend/src/app/modules/missions/mission-register/**` (4 files, new)
- `Frontend/src/app/modules/missions/missions-routing.module.ts` — register route
- `Frontend/src/app/modules/missions/models/mission.model.ts`
- `Frontend/src/assets/i18n/ar.json`, `en.json` — outcome/reason keys

### Review Findings

- [x] [Review][Patch] Register-result immutability hole + wrong stamping — the guard checks
      `IsMissionCompleted` only, so a **not-completed** outcome is re-registrable indefinitely
      (guard on `MissionCompletedTxt != null` instead), and `MissionCompletedDate` is stamped even
      when `IsCompleted = false` (§20.S.3: the date belongs to completion) — the client's
      already-registered redirect should key on `missionCompletedTxt` too
      [Backend/src/IIROSA.Application/Services/MissionService.cs:356-358] —
      fixed: guard on either marker; date stamped only on the completed outcome; client redirect
      keys on both
- [x] [Review][Patch] Register screen spinner is nested inside the `*ngIf="!loading"` container, so
      it never renders (blank screen while loading); the already-registered early-return also
      leaves `loading = true`
      [Frontend/src/app/modules/missions/mission-register/mission-register.component.html:3,276] —
      fixed: spinner moved to a sibling of the gated row; early return clears the flag
- [x] [Review][Patch] `RegisterMissionResultValidator`'s `When()` guards neuter the mandatory
      rules — `NotEmpty().When(x => x.Details != null)` lets null pass server-side although
      §20.S.3 makes التفاصيل / الهدف mandatory
      [Backend/src/IIROSA.Application/Validators/MissionManagement/RegisterMissionResultValidator.cs:24,28]
- [x] [Review][Patch] No `MaximumLength` rules on the register DTO's text fields against the
      entity's `HasMaxLength` columns — oversized input dies as a SQL 500
      [Backend/src/IIROSA.Application/Validators/MissionManagement/RegisterMissionResultValidator.cs] —
      fixed: length rules added for all seven text fields matching the column limits
- [x] [Review][Defer] No concurrency token on `Mission` — an update and a register can race,
      last write wins; rowversion is absent platform-wide
      [Backend/src/IIROSA.Domain/Entities/Mission.cs] — deferred, pre-existing

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created from `epics.md` US-MSN-09 and module spec §20.S.3 / §20.U.9; endpoint semantics conflict recorded. |
| 2026-08-24 | Implemented (review-and-complete pass over the copied vertical), verified live against the running API, status → review. |
| 2026-08-24 | Code review: 4 findings fixed (immutability guard + conditional date stamping, spinner nesting + loading leak, dead `When()` guards, length rules); 1 deferred; status → done. |
