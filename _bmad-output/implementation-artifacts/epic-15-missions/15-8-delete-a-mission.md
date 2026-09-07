# Story 15-8: Delete a mission

| Field | Value |
| --- | --- |
| Story key | `15-8-delete-a-mission` |
| Epic | EP-15 — Missions (المأموريات) |
| Use case | UC-MSN-08 — حذف المأمورية |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/20-UC-MSN-Missions.md` (§20.S.1 commands, §20.U.8 scenario) |
| Endpoint | `DELETE /api/MissionManagement/{id}` (board shorthand `DELETE /api/MissionManagement`) |
| Depends on | **15-1 landed** (list screen + `Missions.Delete` permission entry, SuperAdmin-only) |
| Roles | **Gen. Director only → `SuperAdmin`** (UC-MSN-08 — narrower than the module default) |

## Status

done

## Story

As a General Director, I want to be able to delete a mission حذف المأمورية, so that records
entered in error do not distort the register or the reporting.

## Acceptance Criteria

1. Given a General Director, when the actor confirms deletion, then the record is no longer
   returned by the list and read endpoints of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `DELETE /api/MissionManagement/{id}` without a page reload.
3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.
4. Given the session has expired or the role is **Staff** (`Admin`), then the request is rejected
   server-side and the actor is routed back to the login screen.

**Definition of done:** §20.U.8 passes end to end; the delete is **soft** (`IsDeleted`); the
SuperAdmin-only rule is enforced on the endpoint, not the menu.

## Verified defects this story must fix

1. **Role gap: delete is not Gen.-Director-only.** The controller's class-level
   `[Authorize(Roles = "Admin,SuperAdmin")]` covers `DeleteMission` — an `Admin` (Staff) can
   delete today, violating UC-MSN-08's primary actor. Needs an endpoint-level
   `[Authorize(Roles = "SuperAdmin")]`.
2. **`mission.service.deleteMission` checks `response.success`** (`mission.service.ts:130-135`) —
   the controller returns `Ok(new { message })`, so success maps to `false` and the UI treats
   every successful delete as a failure.
3. **Soft-delete path unverified + repo-saves** — confirm the repository `Remove`/soft-delete
   semantics through the audit interceptor, and convert the save to `IUnitOfWork` (15-6 pattern).

## Tasks / Subtasks

- [x] **Task 1 — Endpoint authorization** (AC 4; defect 1)
  - [x] `[Authorize(Roles = "SuperAdmin")]` on `DeleteMission` (narrowing the class default)
- [x] **Task 2 — Service** (AC 1; defect 3)
  - [x] Not-found → typed failure; soft delete via the entity's `IsDeleted` machinery through
        `IUnitOfWork` only — never a hard `DELETE`
  - [x] Caller scope: same country pin-never-widen check as the reads; out-of-scope id → 404
- [x] **Task 3 — Frontend** (AC 2, 3; defect 2)
  - [x] `mission.service.deleteMission` reads the raw response (drop the `success` mapping)
  - [x] List screen delete action (§20.S.1 command): confirm dialog first (SweetAlert2 is the
        platform convention; plain `confirm()` currently used — align with whichever the done
        modules use, e.g. technical-support), then call, then reload the page of results
  - [x] Gate the action on the `Missions.Delete` permission (SuperAdmin-only entry added to
        `PERMISSION_ROLES` in 15-1) — hide for `Admin` **and** rely on the endpoint's 403
- [x] **Task 4 — i18n**: confirm title/body/success/failure keys in both locales
- [x] **Task 5 — Verify** (AC 1–4): delete as SuperAdmin → row vanishes from `my-missions`,
      `GET {id}` → 404, row still in the table with `IsDeleted=1` (soft); decline → nothing;
      as Admin → 403; builds green

## Dev Notes

- Soft delete is a **global query filter** — once `IsDeleted` is set, every read in the module
  excludes the row automatically; no manual filtering anywhere.
- The list's "حفظ → DeleteIncoming()" command label in §20.S.1 is a legacy mislabel; the shipped
  UI uses an icon action with a confirm dialog — keep that, don't reproduce the mislabel.
- `Admin` receiving 403 must land on a sensible message, not a raw error — mirror the done
  modules' forbidden handling.

### References

- [Source: docs/Modules/20-UC-MSN-Missions.md#20.U.8] scenario — Gen. Director only
- [Source: Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs:127-145] current
  delete action (class-level roles only)
- [Source: _bmad-output/planning-artifacts/epics.md#3.15] US-MSN-08 acceptance criteria

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

Live verification 2026-08-23 (SuperAdmin): `DELETE /api/MissionManagement/{id}` → 200
"Mission deleted successfully" (on both a fresh row and a completed row); the register read
afterwards dropped from 2 to 1 then to 0 — the soft-deleted rows disappear from reads with no
manual `IsDeleted` predicate (global query filter does it).

### Completion Notes List

- Delete is soft (entity's `IsDeleted` machinery), saved through the UnitOfWork only, and
  caller-scope checked before the delete — an out-of-scope id is reported exactly like a missing
  one. `Missions.Delete` is SuperAdmin-only in `PERMISSION_ROLES`; the UI's delete action gates on
  `auth.hasPermission('Missions.Delete')` behind a `notification.confirm` dialog (SweetAlert2), and
  the endpoint itself is authorised server-side — hiding the button is not the control.
- A completed mission can be deleted (the register entry is final for edits, but the Gen.
  Director's delete power is unconditional per UC-MSN-08) — verified live.

### File List

- `Backend/src/IIROSA.Application/Services/MissionService.cs` — DeleteMissionAsync
- `Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs`
- `Frontend/src/app/modules/missions/mission-list/mission-list.component.ts|.html` — delete action
- `Frontend/src/app/core/services/auth.service.ts` — Missions.Delete role map
- `Frontend/src/assets/i18n/ar.json`, `en.json` — confirmDelete/deleteFailed keys

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created from `epics.md` US-MSN-08 and module spec §20.S.1 / §20.U.8. |
| 2026-08-24 | Implemented (review-and-complete pass over the copied vertical), verified live against the running API, status → review. |
| 2026-08-24 | Code review: no patch findings — verified clean (delete-of-completed-row kept per UC-MSN-08, documented deviation); status → done. |
