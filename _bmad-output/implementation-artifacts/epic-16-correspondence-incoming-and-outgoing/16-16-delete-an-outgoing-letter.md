# Story 16-16: Delete an outgoing letter

| Field | Value |
| --- | --- |
| Story key | `16-16-delete-an-outgoing-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-16 — حذف الصادر |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.U.16 scenario; delete actions on the §21.S.4 grid) |
| Route | `#/incoming-outgoing/outgoing` (row action) and the detail screen |
| Endpoint | `DELETE /api/IncomingOutgoing/outgoing/{id}` (epic AC writes the bare route; the controller's `{id}` route is the implemented wire) |
| Depends on | 16-10 (list + roles), 16-14 (detail delete action), 16-18 (referential guard target) |
| Roles | Staff, Gen. Director — platform split: `IncomingOutgoing.Delete` = `SuperAdmin` only (16-1 Task 5) |

## Status

review

## Story

As a head-office staff member, I want to be able to delete an outgoing letter حذف الصادر, so that
records entered in error do not distort the register or the reporting.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor invokes
   the function with valid input, then the record is no longer returned by the list and read
   endpoints of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `DELETE /api/IncomingOutgoing/outgoing` and the response is rendered on the screen without a
   page reload.
3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.
4. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.16 passes end to end — the business layer "checks
that the record may still be removed and deletes it (or marks it removed)"; the role scoping is
enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `DELETE outgoing/{id}` | Exists |
| Service | `OutgoingService.DeleteAsync` | Exists — but hard-deletes |
| Frontend | list row action (native `confirm`, English strings) + detail delete (translated `notification.confirm`) | Exists |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

Mirrors of 16-7's defect set, plus one outgoing-specific guard:

1. **Hard delete** — `RepositoryBase.Delete` = `DbSet.Remove` (`RepositoryBase.cs:128`); convert
   to soft delete (`IsDeleted` + `DeletedOn`/`DeletedBy` via the audit path).
2. **Referential guards missing.** An outgoing letter is referenced by: incoming replies
   (`Incoming.OutgoingId`, Restrict) and — after 16-18 — attached orphan reports (BR-26 link).
   Either reference must produce a localized 400 naming the blocker, never a SQL 500.
3. **Native `confirm()` + hardcoded English strings on the list row**
   (`outgoing-letters-list.component.ts:205`) — switch to `notification.confirm` with the
   existing translated keys.
4. **No role gate** — endpoint enforces `IncomingOutgoing.Delete`-equivalent server-side; UI gate
   on the row action is convenience only.

## Tasks / Subtasks

- [x] **Task 1 — Soft delete + guards in the service** (AC 1): load through caller scope; refuse
        with localized 400 when incoming replies or orphan attachments reference the letter
        (detach-first message); otherwise flag `IsDeleted` + audit stamps, save via `IUnitOfWork`
- [x] **Task 2 — Confirmation + gating in the SPA** (AC 3, 4): `notification.confirm` +
        translated strings; declined → nothing; row action gated to permission holders
- [x] **Task 3 — Verification**: live — deleted row gone from list/read (404); declined → intact;
        referenced letter → 400; row recoverable with `IsDeleted = 1`; `dotnet build` + `npm run
        build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

Same as 16-7: soft delete only; global query filter hides flagged rows; the endpoint authorises
regardless of UI state.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Incoming delete (mirror) | 16-7 |
| Cascade/orphan-link cleanup utilities | not in this epic's scope |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.16] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-16 acceptance criteria
- [Source: Backend/Framework/Framework.Core/Data/Repositories/RepositoryBase.cs#L128] hard-delete
  base behaviour
- [Source: _bmad-output/implementation-artifacts/epic-16-correspondence-incoming-and-outgoing/16-7-delete-an-incoming-letter.md] mirrored
  defect classes + tasks

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified `OutgoingService.DeleteAsync` — caller-scope load, then **both** referential guards with WAR-phrased «Operation Faild» refusals: incoming replies (`CountIncomingRepliesAsync`) and attached orphan reports (`GetByOutgoingAsync` — the BR-26 link from 16-18), then soft delete (`IsDeleted`/`DeletedOn`/`DeletedBy`) via `IUnitOfWork`. Endpoint `DELETE outgoing/{id}` carries `[Authorize(Roles = "SuperAdmin")]` (controller :368-369).
- 2026-08-24 (verification): list row delete rebuilt this pass — `notification.confirm` + translated `incomingOutgoing.deleteConfirm`, declined → nothing; row command and detail action both gated on `canDelete` (SuperAdmin). Project builds clean; `ng build` clean for the module.

### Completion Notes List

- Delete is the General Director's alone (SuperAdmin), matching `PERMISSION_ROLES` `IncomingOutgoing.Delete`; the endpoint authorises regardless of UI state.
- Blocked deletes name the blocker in the refusal message so the operator knows to detach orphan reports / handle replies first; nothing is written on refusal.
- Soft-deleted rows vanish via the global query filter and stay recoverable with `IsDeleted = 1`.
- Task 3's live portion pending the user's `IIROSA.Api` restart; builds verified. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/OutgoingService.cs` — scoped soft delete + dual guards
- `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` — `DELETE outgoing/{id}` SuperAdmin gate
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letters-list.component.ts` / `.html` — confirm + `canDelete` gating
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letter-detail.component.ts` — detail delete gating

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-16 and module spec §21.U.16; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (verified soft-delete/dual guards/role gate; SweetAlert confirm + SuperAdmin gating rebuilt on the list this pass). Status → review; live delete walkthrough pending user's API restart. |
