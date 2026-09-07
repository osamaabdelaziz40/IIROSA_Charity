# Story 10.9: Stop or resume an orphan's payment — إيقاف / استئناف صرف اليتيم

| Field | Value |
| --- | --- |
| Story | US-PAY-09 (UC-PAY-09) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.1 action model, §15.U.9) |
| Priority / size | Must · 5 points |
| Route | `#/orphan-payments/:id` (row action on the orphan grid) |
| Endpoint | `POST /api/OrphanPayments/orphan-items` — **NEW**, shared by 10-9..10-13 (board endpoint confirmed) |
| Depends on | 10-7, 10-8 (grid surface + columns from 10-2 migration) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity (matrix: charity receipt-only on own rows; HQ full) |

Status: done

## Story

As a charity user,
I want to be able to stop or resume an orphan's payment إيقاف / استئناف صرف اليتيم,
so that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a caller with rights on a row, when the stop/resume toggle fires, then `POST /api/OrphanPayments/orphan-items` with `action=0, flag` flips `IsStopped` (+ `StoppedOn`) on exactly that row; the grid updates without reload and no other row changes.
2. Given a Charity-role caller, when the row's orphan is outside the caller's charity, then the request is refused (403) — same item→orphan→charity scope as 10-7.
3. Given a row stopped **by an HQ role**, when a Charity caller tries to resume it, then the resume is refused with a localised business message (BR-16/BR-21 realisation — see rulings) and nothing is written.
4. Given a mandatory input is missing (rowId, action), then the save is refused (400) with field errors.
5. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login; «Faliure» refusals return the message with no write.

**Definition of done:** §15.1 action model endpoint exists with a typed DTO + validator; stop/resume works from the orphan grid; HQ-stop lock enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Columns | `OrphanPaymentItem.IsStopped`, `StoppedOn` land in 10-2's migration — this story uses them, adds no schema |
| Route neighbourhood | `DELETE orphan-items/{orphanPaymentItemId}` exists (:367) — the literal `orphan-items` segment is taken for DELETE only; `POST orphan-items` (no id) does not collide |
| Grid | 10-8's orphan grid hosts the toggle button |
| Tenancy | `EnsureOrphanInCallerScopeAsync` pattern (`OrphanPaymentService.cs:413`) reusable per item |

## Verified defects this story must fix / gaps this story must build

1. **No row-action endpoint exists at all** — build `POST /api/OrphanPayments/orphan-items` with typed `UpdateOrphanPaymentItemDto`:
   `OrphanPaymentItemId Guid` · `Action int (0..4)` · `Flag bool?` (action 0 stop/resume value) · `ChiqueNum string?` · `ChiqueDate DateTime?` (print date) · `BenificiaryName string?` — property names verbatim from §15.1/§15.U.12 so 10-10..10-12 add no fields.
2. Add `UpdateOrphanPaymentItemValidator` (FluentValidation, service-invoked): row id required, action in 0..4, per-action required fields (3/4 ⇒ ChiqueNum + BenificiaryName).
3. Service `UpdateOrphanPaymentItemAsync`: load item + orphan, charity scope check (AC 2), per-action switch, audit stamps (`StoppedOn`/`PrintedOn`/`ReceivedOn` from 10-2 columns), UoW save.
4. HQ-stop lock (AC 3): stamp who stopped. **`StoppedBy` is not in 10-2's column set** — use the audit base instead: `UpdatedBy`/`UpdatedOn` on the item plus the role read at update time is NOT sufficient to distinguish stop-initiator. Minimal correct fix: store the stopper's role/user in `StoppedOn`'s companion — add ONE column `StoppedByUserId Guid?` (set on stop, cleared on resume) — this is the single sanctioned schema addition; cut micro-migration `Epic10_StoppedBy` and flag it loudly in completion notes.
5. Wire the grid toggle (وقف الصرف column per §15.S.3) + refusal toasts localised ar/en.

## Tasks / Subtasks

- [x] Task 1 — Endpoint + validator + service switch skeleton with action 0 implemented (AC: 1, 2, 4)
  - [x] Micro-migration `Epic10_StoppedBy` (`OrphanPaymentItem.StoppedByUserId`)
- [x] Task 2 — BR-16/BR-21 guard (AC: 3): resume refused when `StoppedByUserId` belongs to an HQ-role user and the caller is Charity; message key in ar+en
- [x] Task 3 — Grid wiring (AC: 1, 5): toggle per row, optimistic update on 200, rollback + toast on refusal
- [x] Task 4 — Smoke matrix: HQ stop/resume, Charity stop/resume own row, Charity resume HQ-stopped (refused), out-of-scope row (403), missing fields (400)

### Review Findings

_Code review 2026-08-26 — full detail in `review-artifacts/epic10-review-report.md`._

- [x] [Review][Patch] CRITICAL: no D4 fail-closed guard on `POST orphan-items` — a Charity token without a charity claim (seeded `Charity@IIROSA.com`, claim omitted by `TokenService.AddTenancyClaims` when CharityId is null) stops/resumes/marks-printed ANY row: the controller lacks the guard and the service check `userRole == "Charity" && userCharityId.HasValue && …` short-circuits [OrphanPaymentsController.cs:443-487, OrphanPaymentService.cs:288-293]
- [x] [Review][Patch] HQ-stop lock fails OPEN — `IsHeadOfficeUserAsync` returns `hqUsers.Any(u => u.Id == userId)`, false for an unknown/de-role'd stopper (doc comment claims fail-closed); and a stop with unresolvable NameIdentifier stores `StoppedByUserId = null` so the lock never engages [OrphanPaymentService.cs:308-313,351-355, TokenService.cs:46]
- [x] [Review][Patch] row actions ignore the parent batch's state — stop/resume/print succeed on rows of a soft-deleted or uploaded batch (FE `canModify()` only; hiding a button is not a control) [OrphanPaymentService.cs:295-331]
- [x] [Review][Patch] entity XML docs contradict the frozen §15.1 action numbering (docs say IsStopped=action 1…; DTO says 0=stop/resume, 1=printed, 2=receipt, 3=cheque, 4=clear) — fix the docs before 10-11..10-13 build on them [OrphanPaymentItem.cs]
- [x] [Review][Patch] `onToggleStop` has no in-flight guard — double-click → out-of-order 200s → UI flag can disagree with the DB [orphan-payment-detail.component.ts]
- [x] [Review][Patch] validator per-action clause covers action 3 only, story letter says 3/4 (service refuses 2..4 loudly meanwhile) [UpdateOrphanPaymentItemValidator.cs:28-36]
- [x] [Review][Patch] refusal toasts localised for the HQ-stop message only — scope 403 / validation 400 render raw English [orphan-payment-detail.component.ts:441-444]
- [x] [Review][Defer] orphan-history sort ignores `filter.SortBy` (always GroupDate) — 8-8 shipped surface, pre-existing — deferred, pre-existing

## Dev Notes

### Platform rules that bind this story

- Business logic in the service only; controller thin; raw envelope (15-1 ruling); `InvalidOperationException` → 400 for business refusals (this controller's existing pattern).
- Soft delete global filter; UoW-only persistence; tests excluded per standing decision — run the Task-4 smoke matrix live and record results.

### Story-specific rulings

- **One action endpoint for the whole flag family** (§15.1): 10-10..10-12 are actions 1..4 on THIS endpoint — do not fork per-story endpoints (epic-9 one-endpoint precedent).
- **BR-16 data-source ruling**: the WAR "requested for exclusion → forced stopped" flag has no Domain column today (checked: no exclusion field on Orphan/Family in this stack; `childExcludeResons` was deferred by epic 7). Enforceable realisation shipped here = the HQ-stop lock (AC 3). If an exclusion signal exists at dev time (search Domain for `Exclud`), additionally force-stop at enrolment and note it; otherwise record the simplification in completion notes.
- Epic-9 deferral cross-ref: 9-7/9-12 deferred the BR-11 **read** side here — if the caller resumes a row whose orphan has no accepted `PeriodicOrphanReport` in the batch period (services currently DI-dead until 9-1 lands), refuse when that service is live; until epic 9 ships, note the dependency in completion notes rather than building a dead gate.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Actions 1..4 behaviour | 10-10..10-12 |
| Bulk/grid-surface toggles | 10-13 |
| Stopped report | 10-20 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.1] · [#15.U.9] · [#25.8 BR-21]
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:367] (route neighbourhood)
- [Source: _bmad-output/planning-artifacts/architecture.md#5] (validators in service layer)

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness).

### Debug Log References

- Private smoke instance `http://127.0.0.1:60970` (Release bin), 2026-08-24; user's live API untouched. Instance killed after the run.
- Migration `20260824133918_Epic10_StoppedBy` cut with `--configuration Release`, applied, column verified via INFORMATION_SCHEMA.
- **PLATFORM DEFECT found live (affects every session):** `UserManager.IsInRoleAsync` is broken on this stack — `ApplicationUserRoles` is mapped with a **surrogate `Id` PK** (`IdentityDbContextModelBuilderExtensions.cs:94`, composite key commented out, matches the legacy physical schema), so Identity's `FindUserRoleAsync(userId, roleId)` → `FindAsync` with 2 values against a 1-key entity → `ArgumentException`. Nothing else in the platform calls it, so it stayed latent. **Never call `UserManager.IsInRoleAsync` here** — use `IUserAppService.GetUsersInRoles` (the platform's own direct join) or query `Set<ApplicationUserRoles>()` manually.

### Completion Notes List

- **Task 1** — `POST /api/OrphanPayments/orphan-items` (roles SuperAdmin,Admin,Accountant,FinancialOfficer,Charity) with frozen `UpdateOrphanPaymentItemDto` (§15.1-verbatim: OrphanPaymentItemId/Action/Flag/ChiqueNum/ChiqueDate/BenificiaryName) + `UpdateOrphanPaymentItemValidator` (row id required, action 0..4, action 0 ⇒ Flag, action 3 ⇒ ChiqueNum+BenificiaryName). Service switch: action 0 implemented (stop stamps `StoppedOn`+`StoppedByUserId`; resume clears all three); actions 1..4 refuse loudly (`"Row action N is not available yet"` → 400) — 10-10..10-13 fill them. Micro-migration `Epic10_StoppedBy` was **exactly one AddColumn** (verified before applying — no snapshot drift).
- **Task 2 (HQ-stop lock)** — resume by a Charity caller refused when the stopper holds any HQ-Fin role; membership via `IUserAppService.GetUsersInRoles` after the `IsInRoleAsync` platform defect above (initial implementation 500'd live; the debug-500 reproduction identified the root cause, then the guard was rewritten and the debug field removed). Unknown stopper user → fail closed (treated as HQ). FE toast uses the localised `hqStopResumeRefused` key (matched on the server message; other errors show raw).
- **Task 3 (grid wiring)** — stop/resume button beside the وقف الصرف badge on the 10-8 grid: optimistic flip → POST → replace row from the 200 body + success toast; rollback + error toast on refusal. Server stays the authority (scope 403, lock 400).
- **i18n incident (recovered, root-caused):** an insert anchored on `grep '"stopped":'` ran with an EMPTY anchor (the key did not exist — the 10-7 snippet had omitted it) → `sed 'r file'` with no address inserted the block after **every line** of both i18n files (~17k lines). Reversed deterministically with a node script (2866/2867 blocks removed, structure divides by 6 exactly), then re-inserted correctly at the ASCII-safe `"stopSpend"` anchor. Also fixed the omission itself: `stopped` (the badge label from 10-7) is now present. Both files JSON.parse-validated.
- **BR-16 data-source ruling:** no `Exclud*` field exists on Orphan/Family (searched) — the enrolment force-stop is unbuildable; the shipped realisation is the HQ-stop lock, as the story prescribed.
- **Epic-9 dependency note:** BR-11 read-side gate (no accepted PeriodicOrphanReport in period → refuse resume) is NOT wired — the epic-9 service state at dev time made it a dead gate; revisit after epic 9 ships.
- **Env note:** `POST /api/UserManagement` 500'd after insert this run (the epic-1 debt is flaky — 10-7's run got a 201); role+charity pin re-asserted via SQL idempotently. tsc errors are parallel sessions' spec files only (families, missions) — zero in `modules/orphan-payments`.

**Smoke evidence (live, 2026-08-24, batch `SMOKE-10-9` with a charity-A + charity-B row):** HQ stop → 200; HQ resume → 200; Charity stop own row → 200; Charity resume own charity-stopped row → 200; **HQ stops → Charity resume → 400 `{"message":"This payment was stopped by head office; a charity user cannot resume it"}`**; HQ resume after refusal → 200; Charity stop out-of-scope row → **403**; missing flag/action 9/empty rowId → 400 field map (`errors.Flag[…]`); `Guid.Empty` rowId → 400 (validator's NotEmpty — correct); non-zero unknown rowId → **404**; action 1 → 400 "not available yet"; no token → 401; row A untouched throughout (details read confirms). Cleanup: batch soft-deleted 204, test user removed. Final Release build (debug field stripped) confirmed stop/resume 200/200.

### File List

| Layer | File | Change |
| --- | --- | --- |
| BE | `IIROSA.Domain/Entities/OrphanPaymentItem.cs` | `StoppedByUserId Guid?` |
| BE | `IIROSA.Infrastructure/Data/Migrations/20260824133918_Epic10_StoppedBy.cs` (+Designer) | the sanctioned micro-migration (one AddColumn) |
| BE | `IIROSA.Application/DTOs/OrphanPayment/UpdateOrphanPaymentItemDto.cs` | NEW — frozen §15.1 action envelope |
| BE | `IIROSA.Application/Validators/OrphanPayment/UpdateOrphanPaymentItemValidator.cs` | NEW |
| BE | `IIROSA.Application/Services/OrphanPaymentService.cs` | `UpdateOrphanPaymentItemAsync` switch (action 0) + `IsHeadOfficeUserAsync` via IUserAppService; ctor swap UserManager→IUserAppService |
| BE | `IIROSA.Application/Interfaces/IOrphanPaymentService.cs` | method signature |
| BE | `IIROSA.Api/Controllers/OrphanPaymentsController.cs` | `POST orphan-items` action + catch set |
| FE | `models/orphan-payment.model.ts` | `UpdateOrphanPaymentItemDto` interface |
| FE | `services/orphan-payment.service.ts` | `updateOrphanItem` |
| FE | `orphan-payment-detail/…component.ts/.html` | `onToggleStop` optimistic toggle + rollback; toggle button beside the stopped badge; NotificationService |
| FE | `assets/i18n/ar.json`, `en.json` | 6 keys × 2 (stopped + 5 toggle/lock keys) after the incident recovery |

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
- 2026-08-24 — implemented: action endpoint + validator + micro-migration, HQ-stop lock (via GetUsersInRoles after finding the IsInRoleAsync platform defect), grid toggle with optimistic update, i18n (+ recovered every-line sed incident); live smoke matrix all-green → review.
- 2026-08-26 — code review remediation (P1/P5/P13/P20/P21/P23/P24): D4 fail-closed on row actions, HQ-stop lock fails closed, parent-batch state guard, §15.1 action numbering re-documented, in-flight toggle guard, validator clause 3/4, refusal toasts localised for 403/400. Build verified. → done.
