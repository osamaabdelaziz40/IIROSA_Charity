# Story 16-6: Update an incoming letter

| Field | Value |
| --- | --- |
| Story key | `16-6-update-an-incoming-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-06 — تعديل الوارد |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.2 screen, §21.U.6 scenario) |
| Route | `#/incoming-outgoing/incoming/:id/edit` |
| Endpoint | `PUT /api/IncomingOutgoing/incoming/{id}` (epic AC writes the bare route; the controller's `{id}` route is the implemented wire) |
| Depends on | 16-4 (form + create contract; edit mode reuses it) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to update an incoming letter تعديل الوارد, so that
a record that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor invokes
   the function with valid input, then the stored record carries the new values; no other record
   is affected.
2. Given the request is accepted, when it is served, then it is handled by
   `PUT /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page
   reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor
   saves, then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.6 passes end to end; the role and charity scoping is
enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `IncomingOutgoingController` `PUT incoming/{id}` (id-vs-dto.Id guard exists) | Exists |
| Service | `IncomingService.UpdateAsync` | Exists — full-overwrite update, no validator |
| Frontend | `incoming-letter-form.component.ts` edit mode + `patchForm` | Exists |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Edit-mode patch reads broken wire names.** `patchForm` reads `letter.fkDepartmentId` /
   `letter.uploadedFile` (`:215, :219`) — after 16-1/16-4 the clean keys are `departmentId` /
   `uploadedFileId`; today the department and attachment silently clear on every edit load.
2. **Full overwrite with no validator.** `UpdateAsync` copies every DTO field; there is no
   `UpdateIncomingValidator`, so AC 3's mandatory-field refusal is client-only. Same class as
   16-4 defect 3 — the update validator must exist in the service layer.
3. **Serial is editable-by-accident.** The update path copies serial fields when present; كود
   الوارد is read-only (§21.S.2) — the server must ignore client serial changes (keep the
   original; re-derive only on year-affecting date change if the spec's sequence demands it —
   simplest correct behaviour: serial never changes after create).
4. **No ownership check.** Update does not verify the record belongs to the caller's charity (or
   an HQ caller) — a Guid-bearing charity user could edit another charity's letter. Read →
   verify-scope → write.
5. **Concurrency/staleness.** No optimistic check; `UpdatedOn` is maintained by the audit
   interceptor — acceptable, but the response must return the saved row so AC 4's list refresh
   shows true values.

## Tasks / Subtasks

- [x] **Task 1 — Service update contract** (AC 1, 3)
  - [x] `UpdateIncomingDto` mirrors 16-4's create shape (sans server-stamped fields); add
        `UpdateIncomingValidator` (same rules as create) invoked in the service; FK-existence
        violations → localized 400
  - [x] `UpdateAsync`: load tracked entity **through the caller scope** (404/403-shaped anonymous
        error when out of scope), never overwrite Serial/SerialTxt/CreatedBy/CreatedOn/charity;
        stamp `UpdatedBy`/`UpdatedOn` via the audit interceptor path; save through `IUnitOfWork`
- [x] **Task 2 — Edit mode in the SPA** (AC 3, 4)
  - [x] Fix `patchForm` to the clean wire keys; every §21.S.2 control populates incl. status,
        responsible employee, outgoing letter, and the existing attachment (shows filename +
        RemoveImage per §21.S.2 commands)
  - [x] Save → PUT `{id}` → success toast → navigate to the list; server refusal flags the field
  - [x] كود الوارد renders read-only with the stored value (no next-serial call in edit mode)
- [x] **Task 3 — Verification**
  - [ ] Live: edit changes exactly the target row (no other row affected — AC 1); mandatory-field
        violation → 400 + flagged field; charity user cannot edit another charity's record
  - [x] `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- Only `IUnitOfWork` saves; validators in the service layer; bare DTO returns.
- Tenancy: verify-then-write, pin-never-widen — the scope check is server-side, not a hidden UI
  affordance.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Delete | 16-7 |
| Outgoing update (mirror) | 16-15 |
| Employees attachment editing | 16-9 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.2] field contract
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.6] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-06 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/IncomingService.cs] `UpdateAsync`
- [Source: Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letter-form.component.ts#L203-222] `patchForm`

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified `UpdateAsync` (:182-220) — `UpdateIncomingValidator.ValidateAndThrowAsync` + `EnsureReferencesExistAsync` first (this pass added the FK half), then loads via `GetWithDetailsAsync` and refuses out-of-scope rows (`IsWithinCallerScope` → not-found-shaped refusal, no cross-charity leak), uniqueness re-check per charity+year with `excludeId`, then copies only the §21.S.2 mutable fields — Serial/SerialTxt/Year/CreatedBy/CreatedOn/FK_CharityId never touched. Save via `IUnitOfWork`; response returns the saved row.
- 2026-08-24 (verification): project builds clean; `ng build` clean for the module; solution blocked only by the parallel epic-6 error.

### Completion Notes List

- Serial immutability documented in code ("the serial, its year and the charity ownership are immutable — they anchor the per-charity sequence") — defect 3 resolved with the simplest correct behaviour: serial never changes after create, even on date change.
- Edit mode: `patchForm` reads the clean wire keys (`departmentId`, `uploadedFileId`, …); every §21.S.2 control populates incl. status, responsible employee, outgoing letter, existing attachment (filename + remove); كود الوارد shows the stored serial read-only — no next-serial call in edit mode. Save → PUT → toast → register list (fixed this pass; was navigating to detail).
- Server refusal (400 error map) flags the offending field on the form.
- Task 3 live check left unchecked — pending the user's `IIROSA.Api` restart. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/IncomingService.cs` — scoped update, immutable serial/charity, FK checks
- `Backend/src/IIROSA.Application/Validators/Correspondence/IncomingValidators.cs` — `UpdateIncomingValidator`
- `Backend/src/IIROSA.Application/DTOs/IncomingOutgoing/IncomingDto.cs` — `UpdateIncomingDto` shape
- `Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letter-form.component.ts` — edit mode + patch keys + post-save navigation

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-06 and module spec §21.S.2 / §21.U.6; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (added update-path FK checks; post-save navigation to the register; verified scope/immutability chain). Status → review; live edit walkthrough pending user's API restart. |
