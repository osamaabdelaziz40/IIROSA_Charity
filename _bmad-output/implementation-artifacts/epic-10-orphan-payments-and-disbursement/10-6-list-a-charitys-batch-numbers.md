# Story 10.6: List a charity's batch numbers — أرقام الدفعات للجمعية

| Field | Value |
| --- | --- |
| Story | US-PAY-06 (UC-PAY-06) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.S, §15.U.6) |
| Priority / size | Must · 2 points |
| Route | selector on every payment/reporting screen (no dedicated route) |
| Endpoint | `GET /api/OrphanPayments/batch-numbers` (primary, as-built) · `GET /api/OrphanPayments/by-batch-no/{batchNo}` (resolve-one, as-built) |
| Depends on | 10-1 |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity (charity sees only batches it participates in) |

Status: done

## Story

As a charity user,
I want to be able to list a charity's batch numbers أرقام الدفعات للجمعية,
so that I can find the record I need without leaving the system.

## Acceptance Criteria

1. Given a Charity-role caller, when the batch-number list is requested, then `GET /api/OrphanPayments/batch-numbers` returns only batches in which that charity has enrolled orphans — scoped from the JWT claim, never a payload value.
2. Given an HQ role, when an explicit `charityId` query param is supplied, then the list is restricted to that charity; without it, all batch numbers return.
3. Given `by-batch-no/{batchNo}` is called, then the matching batch header returns (or 404) — kept as the resolve-one lookup.
4. Given no batch matches, then an empty list returns (grid renders empty, zero pages) — «Faliure» refusal only for genuinely invalid input.
5. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** the batch selector on the payments screens (and later reporting surfaces) is fed by this endpoint with correct scoping; board-endpoint ruling recorded below.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint | `GET /batch-numbers` → `GetBatchNumbers(charityId?)` (`OrphanPaymentsController.cs:71`, roles SuperAdmin,Admin,Charity) returning `IEnumerable<BatchNumberDto>` — **uncommitted UC-ORP-11 work, treat as shipped** · `GET /by-batch-no/{batchNo}` → `GetByBatchNo` (:509, roles SuperAdmin,Admin) |
| Service | `GetBatchNumbersAsync(userCharityId, userRole, charityId)` — real, scopes via `GetGroupIdsByCharityAsync` (item join) · `GetByBatchNoAsync` (:480-505, :625-669) |
| DTO | `BatchNumberDto { batchNo, latestGroupDate }` (new file, uncommitted) |
| Frontend | `getBatchNumbers` service method exists and matches |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. Role sets: `batch-numbers` lacks Accountant/FinancialOfficer; `by-batch-no` lacks them **and** Charity — align both with the Roles row (charity on `by-batch-no` must still scope: a batch number outside the charity's participation → 404, not exposure).
2. `by-batch-no` for a Charity caller currently has no scope check — add the participation check (charity has ≥1 item in that batch).
3. Wire `batch-numbers` into the batch selector(s) of the payments screens (list filter/form Batch dropdown per §15.S.2, detail/cheques selectors come with 10-7/10-22 — this story wires at least the form's رقم الدفعة selector).
4. Generic 500 message on both actions if not already applied.

## Tasks / Subtasks

- [x] Task 1 — Backend (AC: 1, 2, 3, 4): align roles; charity participation check on `by-batch-no`; verify `GetGroupIdsByCharityAsync` filters soft-deleted items
- [x] Task 2 — Frontend (AC: 1): batch selector on the form fed by `batch-numbers` (HQ also gets the charity selector `الجمعية` per §15.S.1/§15.S.2, feeding `charityId`); i18n ar + en
- [x] Task 3 — Smoke: Charity scoped list; HQ all + filtered; by-batch-no resolve + 404

### Review Findings

_Code review 2026-08-26 — full detail in `review-artifacts/epic10-review-report.md`._

- [x] [Review][Patch] no D4 fail-closed guard on `batch-numbers` + `by-batch-no` — a claim-less Charity token (seeded `Charity@IIROSA.com`) enumerates every charity's batch numbers and resolves any header [OrphanPaymentsController.cs:77-92,609-632, OrphanPaymentService.cs:650-675,808-823]
- [x] [Review][Patch] `by-batch-no` participation check is header-level only — a participating charity receives the FULL DTO with every charity's rows and batch counts via unscoped `GetByIdAsync` [OrphanPaymentService.cs:1289-1305]
- [x] [Review][Patch] Trim inconsistency — picker groups by `Trim()`, uniqueness/lookup exact-match: an offered number 404s on click; visually identical duplicates coexist [OrphanPaymentService.cs:667-670 vs OrphanPaymentRepository.cs:27-43]
- [x] [Review][Patch] BatchNo charset unvalidated — `/`/`%` values make `by-batch-no/{batchNo}` unreachable (route segment / decoding) [Create/Update validators, OrphanPaymentsController.cs:609]

## Dev Notes

### Platform rules that bind this story

- Tenancy: `GetUserCharityId()` claim → service scope; explicit `charityId` param is an **HQ-only** filter (same pin-never-widen pattern as 13-1's `ApplyCallerScope`).
- Thin controller, raw envelope, camelCase; tests excluded per standing decision — smoke and record.

### Story-specific rulings

- **Board-endpoint ruling:** the epic's endpoint column says `GET /api/OrphanPayments/by-batch-no/{batchNo}` only. That endpoint exists but serves resolve-one; the UC's realisation ("batch numbers in which the charity participates … used as the selector on every screen") is the `batch-numbers` list shipped with UC-ORP-11. Both are this story's surface. Do not "fix" stories toward the board column.
- `BatchNumberDto` participates in no paging — the list is small; keep it unpaged (as-built).

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Charity-level batch details read | 10-7 |
| Selector reuse on cheques/bank-file/report screens | 10-14, 10-18..10-22 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.6] · [#15.2 use-case table UC-PAY-06]
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:71-91,509-536] · [OrphanPaymentService.cs:480-505]
- [Source: _bmad-output/implementation-artifacts/epic-8-orphan-register-and-coding/8-11-*.md] (batch-numbers endpoint origin, UC-ORP-11)

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness).

### Debug Log References

- Private smoke instance `http://127.0.0.1:60970` (Release bin), 2026-08-24; user's live API on 60961 untouched.
- sqlcmd `IIROSA_Db_Dev` — throwaway Charity-role test user (`smoke106@test.local`) pinned to each charity in turn; removed after the run.

### Completion Notes List

- **Roles** — `batch-numbers` + Accountant, FinancialOfficer; `by-batch-no` → full HQ-Fin set + Charity (defect 1).
- **Participation check (defect 2)** — `GetByBatchNoAsync(batchNo, userCharityId, userRole)`: a Charity caller resolves only batches in `GetGroupIdsByCharityAsync(charityId)` — outside participation reads as 404, never exposure. Repo method already filters `!IsDeleted` items (verified, defect-free — Task 1's third clause).
- **HQ charityId param** — remains an HQ-only narrowing filter; the charity claim always pins (pin-never-widen).
- **FE (Task 2)** — form's رقم الدفعة is now a `<select>` fed by `getBatchNumbers()` (auto-generate option = empty value; manual entry preserved via the existing toggle, now with a back-to-list button); edit-mode echo appends the saved BatchNo if the list lags (10-4's locked-batch lesson). HQ-only الجمعية filter (`standalone` ngModel — it filters the picker, never the payload) reloads options server-side; charity users see no selector (claim-pinned). i18n ar+en: `batchNumberAutoOption`, `backToBatchList`, `allCharities` (`orphanPayments.charity` reused).
- **Env notes** — identity tables live in schema `[identity]` (`Users.UserRoles.Roles`); `POST /api/UserManagement` currently 500s **after** inserting the user row but **before** assigning roles (empty roles → 403s downstream) — pre-existing epic-1 debt worth a defect ticket, worked around in the smoke by direct role insert; `Charity@IIROSA.com` has `CharityId = NULL` (claim-less charity user — the scope pin silently no-ops; same ticket).

**Smoke evidence (live, 2026-08-24):** batch `SMOKE-10-6` created with an orphan of charity B enrolled. HQ: all-list contains it; `?charityId=B` contains; `?charityId=A` → `[]`; `by-batch-no/SMOKE-10-6` → 200; `by-batch-no/NO-SUCH-BATCH` → 404. Charity user pinned to A (no participation): list `[]`, resolve → **404** (not-leak). Same user re-pinned to B: list contains `SMOKE-10-6`, resolve → 200. Cleanup: batch soft-deleted (204), test user removed. Debug `IIROSA.Application` build 0 errors; FE `tsc --noEmit` exit 0 (full `ng build` noted below on completion). Tests excluded per standing decision.

### File List

| Layer | File | Change |
| --- | --- | --- |
| BE | `IIROSA.Application/Services/OrphanPaymentService.cs` | `GetByBatchNoAsync` caller-context + participation check |
| BE | `IIROSA.Application/Interfaces/IOrphanPaymentService.cs` | signature |
| BE | `IIROSA.Api/Controllers/OrphanPaymentsController.cs` | roles on both actions; charity claim passed to resolve |
| FE | `orphan-payment-form/…component.ts` | batch options + HQ charity filter + edit-echo |
| FE | `orphan-payment-form/…component.html` | batch `<select>` + manual-entry toggle + HQ الجمعية selector |
| FE | `assets/i18n/ar.json`, `en.json` | 3 keys × 2 languages |

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
- 2026-08-24 — implemented: roles aligned, participation 404, form selector + HQ filter, i18n; live smoke all-green → review.
- 2026-08-26 — code review remediation (P6/P14/P15/P26): D4 fail-closed guards on batch-numbers/by-batch-no, by-batch-no rows scoped to the caller charity, trim-consistent batch matching, BatchNo charset validation. Build verified. → done.
