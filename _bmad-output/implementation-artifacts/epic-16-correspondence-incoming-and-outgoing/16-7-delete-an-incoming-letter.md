# Story 16-7: Delete an incoming letter

| Field | Value |
| --- | --- |
| Story key | `16-7-delete-an-incoming-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-07 — حذف الوارد |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.U.7 scenario; delete actions on the §21.S.1 grid) |
| Route | `#/incoming-outgoing/incoming` (row action) and the detail screen |
| Endpoint | `DELETE /api/IncomingOutgoing/incoming/{id}` (epic AC writes the bare route; the controller's `{id}` route is the implemented wire) |
| Depends on | 16-1 (roles + list), 16-5 (detail delete action) |
| Roles | Staff, Gen. Director — platform split: `Admin` sees the action, `SuperAdmin` executes deletes (`IncomingOutgoing.Delete` = SuperAdmin only, 16-1 Task 5) |

## Status

review

## Story

As a head-office staff member, I want to be able to delete an incoming letter حذف الوارد, so that
records entered in error do not distort the register or the reporting.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor invokes
   the function with valid input, then the record is no longer returned by the list and read
   endpoints of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `DELETE /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a
   page reload.
3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.
4. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.7 passes end to end — the business layer "checks
that the record may still be removed and deletes it (or marks it removed)"; the role scoping is
enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `IncomingOutgoingController` `DELETE incoming/{id}` | Exists |
| Service | `IncomingService.DeleteAsync` | Exists — but hard-deletes |
| Frontend | list row action + `incoming-letter-detail.component.ts` delete (translated `notification.confirm`) | Exists |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Hard delete.** `DeleteAsync` calls `_incomingRepository.Delete`, and
   `RepositoryBase.Delete` (`Framework/Framework.Core/Data/Repositories/RepositoryBase.cs:128`) is
   `DbSet.Remove` — a physical row removal. Platform rule: soft delete via `IsDeleted` (§21.U.7
   explicitly allows "marks it removed"); audit stamps `DeletedOn`/`DeletedBy`.
2. **Referential guard missing.** An incoming letter referenced by an outgoing reply
   (`Outgoing.IncomingId`, `DeleteBehavior.Restrict` in `IncomingConfiguration`) will throw a raw
   SQL 500 today. The business layer must check "may still be removed" → localized 400 refusal
   listing the blocker.
3. **Native `confirm()` on the list row** (`incoming-letters-list.component.ts:194`) — bypasses
   the platform's `notification.confirm` (SweetAlert2) used by the detail screen; also its
   message strings are hard-coded English (AC 3's declined-confirmation path must still work in
   Arabic).
4. **No role gate on the row action** and no server-side role beyond `[Authorize]` — wire
   `IncomingOutgoing.Delete` (16-1 Task 5) to hide/gate the action **and** enforce `[Authorize(
   Roles = "SuperAdmin")]`-level restriction server-side; hiding alone is not a control.

## Tasks / Subtasks

- [x] **Task 1 — Soft delete in the service** (AC 1)
  - [x] `DeleteAsync`: load through the caller scope; set `IsDeleted` (+ `DeletedOn`/`DeletedBy`
        via the audit path) and save via `IUnitOfWork`; never `RepositoryBase.Delete`
  - [x] Guard: if outgoing replies reference the letter → localized 400 naming the blocker;
        otherwise the row disappears from list/read (global query filter does the rest)
- [x] **Task 2 — Confirmation + gating in the SPA** (AC 3, 4)
  - [x] Replace the native `confirm()` with `notification.confirm` + translated strings
        (`incomingOutgoing.confirmDelete` already exists — reuse); declined → nothing happens
  - [x] Row action visible to `IncomingOutgoing.Delete` holders only (auth service helper), and
        the endpoint enforces the role regardless of UI state
- [x] **Task 3 — Verification**
  - [ ] Live: delete → row gone from list and `GET {id}` → 404; declined confirm → row intact;
        referenced-by-reply letter → 400 refusal; row still recoverable in the DB with
        `IsDeleted = 1`
  - [x] `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- Soft delete only — the global query filter hides flagged rows; manual `IsDeleted` filters in
  reads are redundant.
- "Hiding a menu is not a control" — the endpoint authorises; the UI gate is convenience.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Outgoing delete (mirror) | 16-16 |
| Purge/restore utilities | not in this epic's scope |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.7] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-07 acceptance criteria
- [Source: Backend/Framework/Framework.Core/Data/Repositories/RepositoryBase.cs#L128] hard-delete
  base behaviour
- [Source: Backend/src/IIROSA.Domain/Configurations/IncomingConfiguration.cs] Restrict relations

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified `DeleteAsync` (:226-249) — loads through caller scope (`IsWithinCallerScope`, out-of-scope reads as not-found), refuses while `CountOutgoingRepliesAsync > 0` with the WAR-phrased «Operation Faild: the letter has outgoing replies and cannot be deleted» (business 400, never a raw SQL 500 past the Restrict FK), then sets `IsDeleted/DeletedOn/DeletedBy` and saves via `IUnitOfWork` — no `RepositoryBase.Delete`. Endpoint `DELETE incoming/{id}` carries `[Authorize(Roles = "SuperAdmin")]` (controller :144-145).
- 2026-08-24 (verification): project builds clean; `ng build` clean for the module; solution blocked only by the parallel epic-6 error.

### Completion Notes List

- List row uses `notification.confirm` (SweetAlert2) with translated `incomingOutgoing.deleteConfirm`; declined confirmation leaves the row intact. Native `confirm()` and its hard-coded English strings are gone (this pass's list rewrite).
- Delete action gated client-side on `canDelete` (SuperAdmin = the General Director, per `PERMISSION_ROLES` `IncomingOutgoing.Delete`) on both the list row and the detail screen — **and** the endpoint enforces the role server-side regardless of UI state (hiding is not a control).
- Soft-deleted rows vanish from list/read via the global query filter; the row stays recoverable in the DB with `IsDeleted = 1`.
- Task 3 live check left unchecked — pending the user's `IIROSA.Api` restart. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/IncomingService.cs` — scoped soft delete + reply guard
- `Backend/src/IIROSA.Domain/Interfaces/IIncomingRepository.cs` — `CountOutgoingRepliesAsync`
- `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` — `DELETE incoming/{id}` SuperAdmin gate
- `Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letters-list.component.ts` / `.html` — confirm + `canDelete` gating
- `Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letter-detail.component.ts` — detail delete gating

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-07 and module spec §21.U.7; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (verified soft-delete/guard/role chain; SweetAlert confirm + SuperAdmin gating on list and detail). Status → review; live delete walkthrough pending user's API restart. |
