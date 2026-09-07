# Story 13-5: Delete a development project

| Field | Value |
| --- | --- |
| Story key | `13-5-delete-development-project` |
| Epic | EP-13 — Office Development Projects |
| Use case | UC-OFP-05 — حذف المشروع التنموي |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/18-UC-OFP-Office-Development-Projects.md` (§18.S.5 screen, §18.U.5 scenario) |
| Route | `#/office-development-projects` (row action + detail screen) |
| Endpoint | `DELETE /api/OfficeProjectManagement/{id}` |
| Depends on | 13-1 (list), 13-4 (country scope on the record) |

## Status

done

## Story

As a General Director, I want to be able to delete a development project حذف المشروع التنموي, so
that records entered in error do not distort the register or the reporting.

## Acceptance Criteria

1. Given a General Director with an active session in the module, when the actor invokes the
   function with valid input, then the record is no longer returned by the list and read endpoints
   of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `DELETE /api/OfficeProjectManagement` and the response is rendered on the screen without a page
   reload.
3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.
4. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §18.S are implemented with their mandatory flags and
lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Tasks / Subtasks

- [x] **Task 1 — Restrict delete to the General Director role** (AC 4, DoD)
  - [x] The controller is class-level `[Authorize(Roles = "Admin,SuperAdmin")]`; delete is the one
        action that must be **`SuperAdmin` only** (spec actor: Gen. Director; permission matrix:
        Office development projects F/F/–/–/–). Split the class attribute or apply a per-action
        `[Authorize(Roles = "SuperAdmin")]`
  - [x] Frontend: hide the delete action for non-SuperAdmin (UX only — the endpoint is the control)
- [x] **Task 2 — Remove the spec-less `IsFinished` delete-block** (AC 1)
  - [x] Covered with 13-4 Task 4: a finished project entered in error must still be deletable
- [x] **Task 3 — Soft delete only, verified through the reads**
  - [x] Delete flows through the soft-delete path (`IsDeleted` + global query filter); after
        delete, list and detail no longer return the row (AC 1) — re-verify live
- [x] **Task 4 — Confirmation declined → nothing happens** (AC 3)
  - [x] Verified in both entry points: list row action and detail screen both use a confirm
        dialog; cancelling leaves the record untouched
- [~] **Task 5 — Tests** — EXCLUDED FROM SCOPE by user decision (same standing decision as epic 3;
      no test project exists under `Backend/tests`)

## Dev Notes

### Findings from the review that produced these tasks (2026-08-19)

- `epics.md`'s realisation table records the spec's `DELETE /api/OfficeProjectManagement` (carrying
  the id) as the route-keyed `DELETE /{id}` that shipped — the realised form is kept; no contract
  change needed, only the role split.
- Today `Admin` can delete. §18.S.5/§18.U.5 name only the General Director, and the permission
  matrix gives Staff (Admin) no delete in this module — the class-level authorisation is too wide
  for exactly one action.
- The `IsFinished` guard blocks deletion of finished projects; combined with mark-complete being
  one click, an accidental completion makes the record undeletable. Not in the spec.
- Country scope from 13-4 Task 5 also covers delete: a pinned caller cannot delete outside their
  country.

## Dev Agent Record

### Implementation Plan

1. Per-action `[Authorize(Roles = "SuperAdmin")]` on delete; keep class-level for the rest.
2. Confirm the soft-delete + confirmation flows live once 13-4 lands.

### Debug Log

- 2026-08-19: verified with the seeded accounts — `Admin@IIROSA.com` gets 403 on DELETE;
  `OsamaSuper@IIROSA.com` gets 200. The verification record (created during 13-3's live check)
  was then deleted through the endpoint and read back as 404.

### Completion Notes

- **Role split**: the controller keeps its class-level `Admin,SuperAdmin`; the delete action
  carries its own `[Authorize(Roles = "SuperAdmin")]`. The `PERMISSION_ROLES.Delete` entry and
  the list's `*ngIf="canDelete"` gate mirror it (UX only — the endpoint is the control).
- **`IsFinished` delete-block removed** with 13-4: a finished project entered in error is
  deletable again; completion is data, not a lock (§18.U.04/05).
- **Soft delete verified through the reads**: after DELETE, GET `{id}` returns 404 and the list
  no longer contains the row — the global `IsDeleted` query filter does its job; the record stays
  in the table for audit.
- **Country scope** (from 13-4) covers delete: a pinned caller cannot delete outside their
  country — the scope check runs before the soft delete.
- Both entry points confirm before deleting; cancelling leaves the record untouched.
- Tests excluded by user decision.

## File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Api/Controllers/OfficeProjectManagementController.cs` | Per-action `[Authorize(Roles = "SuperAdmin")]` on delete (UC-OFP-05) |
| `Backend/src/IIROSA.Application/Services/OfficeProjectService.cs` | `IsFinished` block removed; caller-country scope; soft delete persisted through `IUnitOfWork` |
| `Frontend/src/app/modules/office-development-projects/project-list/project-list.component.html` | Delete action + divider gated `*ngIf="canDelete"` |
| `Frontend/src/app/modules/office-development-projects/project-list/project-list.component.ts` | `canDelete` getter via `AuthService.hasRole('SuperAdmin')` |
| `Frontend/src/app/core/services/auth.service.ts` | `OfficeDevelopmentProjects.Delete` → `SuperAdmin` only |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-OFP-05 and module spec §18.S.5 / §18.U.5; audit findings recorded. |
| 2026-08-19 | Tasks 1–4 implemented; 403-as-Admin / 200-as-SuperAdmin / 404-read-back verified live. |
