# Story 5-10: Approve a guardian-change request

| Field | Value |
| --- | --- |
| Story key | `5-10-approve-a-guardian-change-request` |
| Epic | EP-05 — Family Register (سجل الاسر) |
| Use case | UC-FAM-10 — اعتماد تعديل العائل |
| Priority / size | Must · 8 points |
| Specification | `docs/Modules/10-UC-FAM-Family-Register.md` (§10.U.10 scenario; §10.S.4 queue screen — the موافقه action) |
| Route | `#/families/provider-requests` (built by 5-9) |
| Endpoint | `POST /api/Families/provider-requests/{id}/approve` |
| Depends on | **5-9 landed** (`GuardianChangeRequest` entity, queue screen, service exist) |
| Legacy reference | `EnsureUpdate(parent.Id)` (old system) — the queue's action icon |
| Roles | General Director → `SuperAdmin` |

## Status

done

## Story

As a General Director, I want to be able to approve a guardian-change request اعتماد تعديل العائل,
so that head office keeps control of what is accepted into the sponsorship cycle.

## Acceptance Criteria

1. Given a General Director reviewing a pending request, when the actor records an approval with
   valid input, then the guardian change is applied to the family file, and the request carries its
   new state, the deciding user and the decision date, and moves out of the pending queue.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Families/provider-requests/{id}/approve` with a typed DTO and the response is
   rendered on the screen without a page reload.
3. Given the decision is a **refusal**, when no reason is given, then the refusal is not accepted
   (validator: `RejectionReason` mandatory when `IsApproved == false`).
4. Given the decision is recorded, when the charity opens the item, then it sees the new state and,
   on refusal, the reason.
5. Given the request has already been decided (approved or rejected), when approve is invoked
   again, then it is refused (idempotency guard — legacy «Faild Operation» behaviour) and nothing
   further is written.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the approval applies the new guardian to the family and the request state
transition in ONE transaction; refusals store the reason; the queue refreshes without a page reload.

## Current state — what exists and what is missing (verified 2026-08-24)

Backend (`Backend/src/`):

- After 5-9: `GuardianChangeRequest` entity (with `Status`, `DecidedBy?`, `DecidedOn?`,
  `RejectionReason?` already declared), `IGuardianChangeRequestService`, queue endpoint. **This
  story adds the decision method + endpoint.**
- Precedent: `SupportTicketsController.MarkTicketAsSolved` (`POST {id}/mark-solved`,
  `[Authorize(Roles = "SuperAdmin,Admin")]`, passes `GetCurrentUserId()` into the service) — copy
  this shape.
- Applying the change touches `FamilyService`'s provider path — the family's `Provider` record
  (or `ProviderType` designation) is what the snapshot replaces. `FamilyService` has
  `AddProviderToFamilyAsync` / `UpdateProviderAsync`-style methods to reuse for the write — read
  them first; do not write a parallel provider-update path.

Frontend (`Frontend/src/app/modules/families/`):

- After 5-9: the queue screen renders the موافقه column (empty/disabled). **This story wires it**:
  decision modal (approve / reject + reason), confirmation, service call, row removal/refresh.
- `notification.confirm()` (SweetAlert2) is the house pattern for decisions.

## Tasks / Subtasks

- [x] **Task 1 — Application: DTO + validator** (AC: 2, 3)
  - [x] `DTOs/Family/ApproveGuardianChangeRequestDto.cs` — `bool IsApproved`, `string?
        RejectionReason` (max 500). *(Delivered inside `GuardianChangeRequestDtos.cs` — the
        aggregate's DTOs live in one file; 5-9's file, same namespace.)*
  - [x] `Validators/Family/ApproveGuardianChangeRequestValidator.cs` — when `IsApproved == false`,
        `RejectionReason` NotEmpty; invoked in the service.
- [x] **Task 2 — Service: decision method** (AC: 1, 4, 5)
  - [x] `IGuardianChangeRequestService.DecideRequestAsync(Guid requestId,
        ApproveGuardianChangeRequestDto dto, string decidedBy, string? userRole)`:
        1. Load request (`!IsDeleted`); `NotFoundException` if missing; **refuse if
           `Status != Pending`** (`BusinessException` — the idempotency guard of AC 5).
        2. `BeginTransactionAsync`:
           - **Approve:** apply the snapshot to the family — update/replace the family's guardian
             record (new guardian name, national ID, relationship) via `IProviderRepository`
             *inside this transaction*; do not touch orphans or sponsorship.
           - **Reject:** store `RejectionReason`.
           - Stamp `Status`, `DecidedBy` (from claims), `DecidedOn` (UtcNow); `Commit`.
- [x] **Task 3 — API endpoint** (AC: 2, 6)
  - [x] `POST provider-requests/{id}/approve` on `FamiliesController` —
        `[Authorize(Roles = "SuperAdmin")]` (General Director only per spec — recorded mapping:
        `SuperAdmin`), thin bind→delegate→`ApiResponse` with a localized outcome message;
        validation → 400 field-map.
- [x] **Task 4 — Frontend: decision modal on the queue** (AC: 1, 2, 3, 4)
  - [x] Enable the موافقه action icon on pending rows (hidden for decided rows); open a modal:
        approve («موافقة») / reject («رفض») choice, rejection-reason textarea shown+required when
        rejecting, submit «تم» after `notification.confirm()`.
  - [x] `family.service.decideProviderRequest(id, dto)`; on success toast + refresh the grid (row
        leaves the pending queue when the default filter is pending); surface server refusals
        (already-decided) as an error toast.
  - [x] Charity visibility (AC 4): decided requests show their state and, when rejected, the reason
        — render state/reason columns in the queue for non-pending rows (charity role sees its own
        requests there).
  - [x] i18n keys under `families.providerRequests.*` (decision labels, reason label, toasts) in
        **both** `ar.json` and `en.json`.
- [x] **Task 5 — Verify** (AC: 1–6): approve a pending request → family guardian updated + request
      Approved with decider/date, row leaves queue; reject without reason → 400 flagged field;
      reject with reason → reason stored and visible to the charity; re-decide an already-decided
      request → refused; non-SuperAdmin call → 403; `dotnet build` + `npm run build` green.
      *(Static verification done — build/tsc/i18n green; the live walkthrough is batched into the
      epic-5 sweep with 5-6/5-7/5-8/5-9 because the running API predates the pending migration
      that carries the GuardianChangeRequest table.)*

### Review Findings (code review 2026-08-24)

- [x] [Review][Decision] Approve/reject race — the status guard is check-then-act outside the transaction with no concurrency token and no conditional UPDATE; two SuperAdmin sessions can interleave into "Rejected with a reason while the family's guardian was actually replaced" (or the reverse). Pick the mechanism: conditional `UPDATE … WHERE Status = Pending`, rowversion token, or app-level serialization (one design with 5-9's unique-index decision) — GuardianChangeRequestService.cs:196-232 *(applied 2026-08-24: status re-checked inside the transaction with a fresh read — the pre-tx window is closed; full serialization still awaits the platform-wide concurrency-token decision (deferred-work))*
- [x] [Review][Decision] Sibling pendings stay approvable — after one request for a family is decided, a second pending for the same family (race artifact or legacy row) can still be approved, silently re-applying a different guardian; void or refuse sibling pendings inside the approve transaction — GuardianChangeRequestService.cs:205-215 *(applied 2026-08-24: approve voids sibling pendings for the family inside the same transaction — Rejected, "Superseded by an approved request for the same family")*
- [x] [Review][Decision] `DecidedBy` stores the display name (`User.Identity?.Name`), not the user id the Dev Notes specified — ratify the documented deviation (name collisions/renames break the audit trail) or fix to id + resolved display — GuardianChangeRequestService.cs (decide path) *(applied 2026-08-24: controller passes the NameIdentifier claim id, falling back to the name only when the claim is absent)*
- [x] [Review][Patch] Approve never re-validates the family — no `family.IsDeleted` check (approves onto a soft-deleted family) and no `EnsureCanAddAsync()`, although the same changeset runs that guard on every other guardian-seat write (UC-FAM-12) — GuardianChangeRequestService.cs:196-206,246-266 *(applied 2026-08-24: approving onto a soft-deleted family is refused ("refuse the request instead of approving it"); `EnsureCanAddAsync` is deliberately NOT wired — the guard exempts head-office callers by design and this endpoint is SuperAdmin-only, so it would be dead code (recorded))*
- [x] [Review][Patch] Parent-as-provider families left inconsistent — approval writes only the Provider row; `Family.ProviderType` stays "Father"/"Mother" while every other provider-add path stamps "Other" — GuardianChangeRequestService.cs:249-262 vs FamilyService.cs:687 *(applied 2026-08-24: insert path stamps `ProviderType = "Other")*
- [x] [Review][Patch] `Family.HeadOfFamily` mirror not kept in step after approval — the same changeset enforces the mirror on every other guardian write (lists and the follow-up report show the old guardian) — GuardianChangeRequestService.cs:249-262 *(applied 2026-08-24 — on both the update and insert paths)*
- [x] [Review][Patch] Stale-approval overwrite — approval never compares the live provider's national ID against `OldGuardianNationalId`; a provider corrected while the request pended (member-control, refugee edits) is silently clobbered. Refuse on mismatch — GuardianChangeRequestService.cs:246-266 *(applied 2026-08-24: live-vs-snapshot national-ID comparison; an empty seat (provider removed via 5-13 while the request pended) remains a legitimate re-attach)*
- [x] [Review][Patch] `deciding` flag is set only after `await notification.confirm()` — a double-click stacks two confirms/two POSTs; set it before the await — provider-request-list.component.ts:128-157 *(applied 2026-08-24 — target captured before the await too)*
- [x] [Review][Patch] `userRole` parameter of `DecideRequestAsync` is accepted and never used — the service layer places no decide gate at all; add the gate or drop the parameter — IGuardianChangeRequestService.cs:54-58 *(applied 2026-08-24: service refuses a Charity-role caller — defense in depth, same rule as member control / 5-13)*
- [x] [Review][Defer] English decision messages (Task 3's "localized outcome message") — deferred, pre-existing (platform-wide)

Dismissed as noise: recorded at 5-9 (the chunk's dismissals are story-level there).

## Dev Notes

- **One transaction is the whole point** — guardian applied + status stamped together or not at
  all. A request marked Approved with an un-applied family (or vice versa) is the disaster this
  story exists to prevent.
- Reuse the existing provider write path in `FamilyService` to apply the snapshot; if the service
  boundary makes injection awkward, inject `IFamilyService` into
  `GuardianChangeRequestService` (Application→Application is allowed) rather than duplicating
  provider-update logic.
- The deciding user comes from claims (`CurrentUserId`), never the payload; `DecidedOn` is stamped
  server-side (`DateTime.UtcNow`).
- The «Faild Operation» legacy literal becomes a meaningful localized message; keep the behaviour
  (refuse + nothing written), not the string.
- If 5-9 has not landed, STOP — this story cannot be implemented independently.
- Platform invariants: `ApiResponse` envelope, platform exceptions, typed DTOs, validators in the
  service, `IUnitOfWork`-only saves, soft-delete filters, claims-based identity.
- **Build note:** MSB3021/3027 on `dotnet build` = the user's live API locking outputs; never kill
  it — the compile is clean, retry later.

### References

- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.U.10] scenario — refusal reason mandatory, item leaves queue
- [Source: _bmad-output/planning-artifacts/epics.md#3.5] US-FAM-10 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/SupportTicketsController.cs:341-377] decision-endpoint precedent
- [Source: Backend/src/IIROSA.Application/Services/FamilyService.cs] provider write path to reuse
- [Source: _bmad-output/planning-artifacts/architecture.md#5.1] platform exceptions

## Dev Agent Record

### Agent Model Used

claude-sonnet-4.5 (Claude Code, BMAD dev-story workflow)

### Debug Log References

- `dotnet build IIROSA.Api.csproj` → **Build succeeded** (full chain — Domain/Application/Api
  compile with the decision method, endpoint and provider apply; the user's running API was not
  locking outputs this pass).
- `npx tsc --noEmit -p tsconfig.json` filtered to this story's files → no errors.
- `node JSON.parse` + key-diff on both i18n files → valid, 47/47 `families.providerRequests.*`
  keys matched between `ar.json` and `en.json`.

### Completion Notes List

- **Provider write path — deliberate deviation from the letter of Task 2:** the story suggested
  reusing `FamilyService`'s provider methods, but the viable candidate
  (`AddProviderToFamilyInternalAsync`, FamilyService.cs:1361) is **private and self-saving**
  (calls `SaveChanges` itself), so it cannot join this story's one-transaction guarantee without
  saving early. Instead `ApplyGuardianToFamilyAsync` writes the provider row through
  `IProviderRepository` inside `DecideRequestAsync`'s transaction — updating the family's existing
  row in place (name, national ID, relationship) or creating one when the family has none. The
  duplicate rule is reused from the same repository (`IsNationalIdExistsAsync` with
  `excludeId: currentProvider.Id`, throwing the literal «أحد المعيلين مكرر من قبل أكثر من مرة»).
  Guardian application + status stamp commit together or not at all — the story's whole point.
- `DecidedBy` stores the deciding user's **name** (`User.Identity?.Name`), matching the entity's
  display-name design already used by `RequestedByName` (5-9) — the story's "user id" wording was
  written before the entity settled on a display-name column.
- The «Faild Operation» idempotency guard became a meaningful English message
  ("This request has already been decided") surfaced as an error toast — behaviour kept, string
  modernized, per Dev Notes.
- The queue's موافقه cell now has two faces: pending rows → the decision action (enabled for
  SuperAdmin only — the endpoint is `[Authorize(Roles = "SuperAdmin")]`, and the button stays
  disabled for Admin/Charity viewers); decided rows → status badge + rejection reason subline
  (AC 4 — the charity reads the outcome in the same grid via the status filter).
- The client mirrors the refusal-reason rule (empty-reason refusal blocked with the same message
  the server's validator would produce) — belt and braces, the server stays authoritative.
- Live walkthrough (approve → provider applied + row leaves queue; reject without/with reason;
  re-decide refusal; non-SuperAdmin 403) is batched into the epic-5 sweep — the running API
  predates the pending migration that carries this table.

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/GuardianChangeRequestDtos.cs` — added
  `ApproveGuardianChangeRequestDto` (`IsApproved`, `RejectionReason`) and the decided-state columns
  (`DecidedBy`, `DecidedOn`, `RejectionReason`) on the queue row DTO.
- `Backend/src/IIROSA.Application/Validators/Family/ApproveGuardianChangeRequestValidator.cs` —
  new: `RejectionReason` NotEmpty when refusing, max 500.
- `Backend/src/IIROSA.Application/Interfaces/IGuardianChangeRequestService.cs` — added
  `DecideRequestAsync` with the full exception contract.
- `Backend/src/IIROSA.Application/Services/GuardianChangeRequestService.cs` — decision method:
  validate → load → Pending-only guard → one transaction (apply provider / store reason + stamp
  decided fields) → rollback on failure; private `ApplyGuardianToFamilyAsync` (in-place provider
  update or insert, duplicate-national-ID guard).
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` — `POST provider-requests/{id}/approve`
  (`SuperAdmin` only, thin bind→delegate→`ApiResponse`, full exception ladder).
- `Frontend/src/app/modules/families/models/family.model.ts` — `decidedBy/decidedOn/rejectionReason`
  on the queue row + `DecideGuardianChangeRequest` payload.
- `Frontend/src/app/modules/families/services/family.service.ts` — `decideProviderRequest(id, dto)`.
- `Frontend/src/app/modules/families/provider-request-list/provider-request-list.component.ts` —
  decision modal state + `openDecision/closeDecision/submitDecision` (confirm → call → toast →
  refresh, page-step-back when the last row of a page leaves), `canDecide` (SuperAdmin),
  `isApproved/isRejected` row predicates.
- `Frontend/src/app/modules/families/provider-request-list/provider-request-list.component.html` —
  wired موافقه cell (action on pending rows / state badge + reason on decided rows) + decision
  modal (approve/refuse radios, conditional reason textarea).
- `Frontend/src/app/modules/families/provider-request-list/provider-request-list.component.spec.ts`
  — decision-modal opens, SuperAdmin-only gating, empty-reason refusal blocked.
- `Frontend/src/assets/i18n/ar.json` + `en.json` — 15 new decision keys under
  `families.providerRequests.*`; `approvePending` title updated (was "wired in the approval
  story").
- `Backend/src/IIROSA.Domain/Configurations/ProviderConfiguration.cs` — `IX_Provider_FamilyId`
  made a filtered unique (`[IsDeleted] = 0`): the 1:1-convention plain unique index blocked
  every empty-seat re-attach insert with 2601 (found live, epic-5 battery).
- `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824172643_Epic05_ProviderSeatSoftDeleteIndex.cs`
  — the index-swap migration, applied to the dev DB.
- `Backend/src/IIROSA.Application/Services/GuardianChangeRequestService.cs` — approve's
  create branch clears the outgoing Father/Mother `IsProvider` designation (5-13's ruling
  made it a first-class seat; vacating it belongs to the approval too).

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-FAM-10 and module spec §10.U.10; scoped on top of 5-9's aggregate. |
| 2026-08-24 | Implemented end to end: decision DTO/validator/service/endpoint, provider applied in-transaction, queue decision modal + decided-state columns, i18n. Provider-write deviation and DecidedBy-as-name decision recorded. Status → review. |
| 2026-08-24 | Live walkthrough (private instance + dev DB) walked BOTH approve branches and surfaced two real defects, both fixed and re-verified live: **(1)** `IX_Provider_FamilyId` — the unique index the 1:1 Family↔Provider map creates — spanned soft-deleted rows, so the empty-seat re-attach insert (this story's create branch, and 5-13's remove→re-attach post-condition) died with duplicate-key 2601 after ANY prior provider removal; replaced with a filtered unique (`[IsDeleted] = 0`) via migration `20260824172643_Epic05_ProviderSeatSoftDeleteIndex` (applied). **(2)** Approving onto a parent-designated family left the outgoing Father/Mother `IsProvider` flag set (a vacated mother would linger on 5-14's widow sheet as a false guardian of record) — the create branch now clears the outgoing designation. Verified live: update-in-place approve overwrote name/NID/relationship and Approved the request; create-on-empty-seat built the provider, stamped `ProviderType='Other'` and cleared the father flag; a mid-walkthrough failure (the 2601) rolled the transaction back with the request still pending and nothing written. Status → done. |
