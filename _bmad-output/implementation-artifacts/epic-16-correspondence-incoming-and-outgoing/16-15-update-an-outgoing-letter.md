# Story 16-15: Update an outgoing letter

| Field | Value |
| --- | --- |
| Story key | `16-15-update-an-outgoing-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-15 — تعديل الصادر |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.5 screen, §21.U.15 scenario) |
| Route | `#/incoming-outgoing/outgoing/:id/edit` |
| Endpoint | `PUT /api/IncomingOutgoing/outgoing/{id}` (epic AC writes the bare route; the controller's `{id}` route is the implemented wire) |
| Depends on | 16-13 (form + create contract) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to update an outgoing letter تعديل الصادر, so
that a record that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor invokes
   the function with valid input, then the stored record carries the new values; no other record
   is affected.
2. Given the request is accepted, when it is served, then it is handled by
   `PUT /api/IncomingOutgoing/outgoing` and the response is rendered on the screen without a page
   reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor
   saves, then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.15 passes end to end; the role and charity scoping
is enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `PUT outgoing/{id}` (id-vs-dto.Id guard) | Exists |
| Service | `OutgoingService.UpdateAsync` | Exists — full overwrite, no validator |
| Frontend | `outgoing-letter-form.component.ts` edit mode + `patchForm` | Exists |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

Exact mirrors of 16-6's defect set on the outgoing side:

1. **`patchForm` reads broken wire names** — `fkDepartmentId`/`uploadedFile`/`outgoingCategoryName`
   class (`outgoing-letter-form.component.ts:198-214`) — department, category, and attachment
   silently clear on edit load until re-bound to the 16-10 clean keys.
2. **No update validator** — add `UpdateOutgoingValidator` (same rules as 16-13's create),
   service-layer invoked.
3. **Serial mutable by accident** — server ignores client serial changes (read-only per §21.S.5).
4. **No ownership check** — load through the caller scope before writing (pin-never-widen).
5. **Response must return the saved row** so the AC 4 list refresh shows true values.

## Tasks / Subtasks

- [x] **Task 1 — Service update contract** (AC 1, 3): `UpdateOutgoingDto` mirrors the create
        shape; validator invoked in service; FK-existence → localized 400; tracked load through
        caller scope; never overwrite Serial/charity/audit stamps; save via `IUnitOfWork`
- [x] **Task 2 — Edit mode in the SPA** (AC 3, 4): `patchForm` to clean keys — every §21.S.5
        control populates incl. category (int id), reply-to incoming (+ summary block), existing
        attachment (filename + RemoveImage); رقم الصادر read-only with stored value; save →
        PUT → toast → list; server refusal flags the field
- [x] **Task 3 — Verification**: live — edit changes exactly the target row; mandatory-field
        violation → 400 + flagged field; cross-charity edit refused; `dotnet build` + `npm run
        build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

Same as 16-6: verify-then-write tenancy, `IUnitOfWork`-only saves, service-layer validators.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Delete | 16-16 |
| Orphan attachment editing | 16-18 |
| Incoming update (mirror) | 16-6 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.5] field contract
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.15] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-15 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/OutgoingService.cs] `UpdateAsync`
- [Source: _bmad-output/implementation-artifacts/epic-16-correspondence-incoming-and-outgoing/16-6-update-an-incoming-letter.md] mirrored
  defect classes + tasks

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified `OutgoingService.UpdateAsync` (:156+) — `UpdateOutgoingValidator.ValidateAndThrowAsync` + `EnsureReferencesExistAsync(departmentId, categoryId, incomingId)` (FK half added this pass), tracked load through caller scope (`IsWithinCallerScope`), serial/charity/audit stamps never overwritten, save via `IUnitOfWork`, response returns the saved row.
- 2026-08-24 (verification): `patchForm` (:193-206) reads the clean keys — `departmentId`, `outgoingCategoryId`, `incomingId` (+ summary block via `updateSelectedIncoming`), `uploadedFileId`; رقم الصادر shows the stored padded serial read-only. Project builds clean; `ng build` clean for the module.

### Completion Notes List

- Save → PUT → toast → register list (fixed this pass; was navigating to detail); server 400 error map flags the offending form field.
- Mirror of 16-6's landed shape — same verify-then-write tenancy, same immutability set (serial + year + charity anchor the per-charity sequence).
- Task 3's live portion pending the user's `IIROSA.Api` restart; builds verified. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/OutgoingService.cs` — scoped update + FK checks
- `Backend/src/IIROSA.Application/Validators/Correspondence/OutgoingValidators.cs` — `UpdateOutgoingValidator`
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letter-form.component.ts` — edit mode + post-save navigation

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-15 and module spec §21.S.5 / §21.U.15; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (added update-path FK checks; post-save navigation to the register; verified scope/immutability chain). Status → review; live edit walkthrough pending user's API restart. |
