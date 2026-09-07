# Story 10.5: Delete a payment batch — حذف الدفعة

| Field | Value |
| --- | --- |
| Story | US-PAY-05 (UC-PAY-05) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.S, §15.U.5) |
| Priority / size | Should · 2 points |
| Route | `#/orphan-payments` (row action on the list) |
| Endpoint | `DELETE /api/OrphanPayments/{id}` (as-built) |
| Depends on | 10-1 |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer (WAR §15.U.5: Gen. Director + Fin. Director) |

Status: done

## Story

As a General Director,
I want to be able to delete a payment batch حذف الدفعة,
so that records entered in error do not distort the register or the reporting.

## Acceptance Criteria

1. Given an HQ-Fin role and a batch created in error, when deletion is confirmed, then `DELETE /api/OrphanPayments/{id}` soft-deletes the batch **and its rows**, and the record is no longer returned by list/read endpoints.
2. Given the confirmation dialog is declined, then nothing is deleted and no request is sent.
3. Given a batch that has already been disbursed (any row with `IsGotIt`/`TransferNo`/`ChiqueNum` set), when deletion is attempted, then it is refused with a business message and nothing is written (§25.6 A1 limits delete to pre-disbursement batches).
4. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** SweetAlert2 confirm→delete→row-drop flow on the list; disbursement guard enforced server-side; soft delete only.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint | `DELETE /{id}` → `DeletePaymentGroup` (`OrphanPaymentsController.cs:220`, roles **SuperAdmin only**) |
| Service | `DeletePaymentGroupAsync` (real: loads with items, deletes items then group — verify soft-delete path, `OrphanPaymentService.cs:660-669`) |
| Frontend | List row delete icon + `DeleteAnViewModel(id)` handler exist |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **No disbursement guard** — a settled batch can be deleted today; add the AC-3 business refusal in the service (`InvalidOperationException` with a localised message).
2. Roles SuperAdmin-only vs WAR (GenDir+FinDir) — widen to the HQ-Fin set.
3. Verify `RemoveOrphanFromGroupAsync`/`DeletePaymentGroupAsync` go through the soft-delete path (global filter) — if any raw `Delete` hard-deletes, fix to soft delete.
4. Confirm dialog must be decline-safe (AC 2) — verify the handler doesn't fire the HTTP call before confirmation.

## Tasks / Subtasks

- [x] Task 1 — Service guard (AC: 1, 3): refuse delete when any item has `IsGotIt`, `TransferNo`, or `ChiqueNum` set; widen roles; generic 500 message on this action
- [x] Task 2 — List flow (AC: 1, 2): SweetAlert2 confirm (localised, ar+en) → DELETE → drop row; decline = no-op
- [x] Task 3 — Smoke: create (10-2) → delete; settle a row (10-12 blocked until it lands — verify the guard with a manual DB flag flip and record it)

### Review Findings

_Code review 2026-08-26 — full detail in `review-artifacts/epic10-review-report.md`._

- [x] [Review][Patch] soft-deleted batches remain WRITABLE — update / mark-uploaded / add-orphans / assign-batch-number / set-exchange-rate / lock all fetch via `FindAsync` (bypasses IsDeleted) and mutate deleted batches; update even persists then throws "Failed to retrieve updated payment group" — filter `!IsDeleted` on the write-path fetches [OrphanPaymentService.cs:361-429,166-170]
- [x] [Review][Patch] row removal (`DELETE orphan-items/{id}`) bypasses the disbursement guard the group delete enforces — settled rows (`IsGotIt`/`TransferNo`/`ChiqueNum`) deletable one at a time [OrphanPaymentService.cs:875-883]

## Dev Notes

### Platform rules that bind this story

- Soft delete only — `IsDeleted` via the base repository/global filter; never a hard `DELETE`.
- Thin controller, raw envelope; tests excluded per standing decision — smoke and record.
- `PeriodicOrphanReport.OrphanPaymentId` FK points at batches (snapshot) — check referential impact before delete: reports referencing the batch should block deletion too (same guard, second condition) unless the FK is nullable and cleared; record what you find.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Row removal from a live batch | existing `DELETE orphan-items/{orphanPaymentItemId}` (10-2 audit) |
| Everything else | 10-6..10-24 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.5] · [#25.6 A1]
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:220-247] · [OrphanPaymentService.cs:660-669]

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness).

### Debug Log References

- Private smoke instance `http://127.0.0.1:60970` (Release bin, user's live API on 60961 untouched), 2026-08-24.
- sqlcmd `IIROSA_Db_Dev` — row-level verification of soft-delete persistence and guard test rows.

### Completion Notes List

- **Guards (AC 3, both conditions)** — `DeletePaymentGroupAsync` refuses before any write: (a) any item with `IsGotIt`/`TransferNo`/`ChiqueNum` → 400 "Cannot delete a payment batch with disbursed rows (received, transferred or cheque-issued)"; (b) any `PeriodicOrphanReport.OrphanPaymentId == id` → 400 "Cannot delete a payment batch referenced by periodic orphan reports" (nullable snapshot FK — referencing reports block the delete; FK is never cleared).
- **Soft delete (AC 1, defect 3)** — `RepositoryBase.Delete` is a `DbSet.Remove` **hard** delete; rewrote `DeletePaymentGroupAsync` to the platform convention (`IsDeleted = true` + `Update` + `SaveChanges` for items then header). `RemoveOrphanFromGroupAsync` was already converted in the same pass.
- **Read filters (defect found in smoke)** — `GetPaymentGroupDetailsAsync`, service `GetByIdAsync` and the delete fetch used unfiltered `FindAsync`/`FirstOrDefault`; deleted batches still returned 200. All three now carry `!IsDeleted`. Repeat-delete answers 404, not a second pass.
- **Roles** — DELETE widened SuperAdmin-only → `SuperAdmin,Admin,Accountant,FinancialOfficer` (WAR §15.U.5 GenDir+FinDir → HQ-Fin set); `InvalidOperationException` → 400 `{message}` caught ahead of the generic 500.
- **FE** — SweetAlert2 confirm (`notificationService.confirm`) → DELETE → success toast + reload; decline sends nothing (AC 2). Server refusal messages map to `deleteDisbursedError` / `deleteReferencedError`; unknown → verbatim/`common.operationFailed`.
- **Migration dance (root cause of the first smoke 500)** — `PeriodicOrphanReport.OrphanPaymentId` existed in the EF model snapshot but **no migration ever emitted the column** (permanent snapshot drift: `migrations add` diffs model vs snapshot, so the column could never materialise). Two-step dance: comment property+navigation → cut `TmpReportLinkDance` (drops phantom from snapshot, deleted unapplied) → restore → cut `20260824120618_Epic10_ReportBatchLink` (AddColumn+index+FK, **applied**, column verified in DB). 4 in-flight references in `PeriodicOrphanReportService`/`OrphanPaymentService` were temporarily commented and restored byte-identical.
- **Cross-epic finding (not 10-5 scope, blocks epic-9 reads)** — the `IIROSA.PeriodicOrphanReport` table itself is table-scale drifted: DB has 41 legacy columns (`SubmissionDate`, `SubmittedBy`, `ReviewDate`, `ReviewedBy` …) while the model maps ~90 (`ReportNo`, `ReportPeriodFrom/To`, `Reviewed`, `Locked`, `Active`, `IsAccepted` … are missing from the DB). Any full-entity SELECT on that table 500s (e.g. `GET /api/PeriodicOrphanReports`). Same disease, bigger blast radius; needs the same dance from the epic-9 owner. My FK guard is unaffected (EF `EXISTS` references only the predicate column — proven by the live 400s).
- **AC 4** — role/auth failures already handled platform-wide (401 → AuthGuard redirect); smoke ran under SuperAdmin.

**Smoke evidence (all live, 2026-08-24):** create → id; DELETE empty batch → **204**, read-after → **404**, repeat DELETE → **404**; item with `IsGotIt=1` linked → DELETE → **400 disbursed message**, batch intact (200); report row linked via `OrphanPaymentId` → DELETE → **400 referenced message**, batch intact (200); unlink → DELETE → **204**. Test rows and batches cleaned up. Debug build of `IIROSA.Application` 0 errors; FE `tsc --noEmit` exit 0. Tests excluded per standing decision.

### File List

| Layer | File | Change |
| --- | --- | --- |
| BE | `IIROSA.Application/Services/OrphanPaymentService.cs` | dual delete guard; soft-delete rewrite; `!IsDeleted` read filters (details/by-id/delete fetch); ctor `IRepository<PeriodicOrphanReport>` |
| BE | `IIROSA.Api/Controllers/OrphanPaymentsController.cs` | DELETE roles widened; 400 mapping for `InvalidOperationException`; `[ProducesResponseType(400)]` |
| BE | `IIROSA.Domain/Entities/PeriodicOrphanReport.cs` | `OrphanPaymentId` + navigation (restored after dance — net zero) |
| BE | `IIROSA.Infrastructure/Data/Migrations/20260824120618_Epic10_ReportBatchLink.cs` (+Designer) | AddColumn+index+FK, applied |
| BE | `IIROSA.Application/Services/PeriodicOrphanReportService.cs` | dance-window comment/restore of 3 `OrphanPaymentId` refs — net zero |
| FE | `modules/orphan-payments/orphan-payment-list/…component.ts` | SweetAlert2 confirm/delete/error-map flow |
| FE | `assets/i18n/ar.json`, `en.json` | deleteGroupTitle/Success, deleteDisbursedError, deleteReferencedError (+10-4 keys) |

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
- 2026-08-24 — implemented: guards, soft-delete, roles, read filters, SweetAlert2 flow, i18n; `Epic10_ReportBatchLink` migration cut+applied (snapshot-drift dance); live smoke all-green → review.
- 2026-08-26 — code review remediation (P4/P12): all write paths fetch live batches only (GetLivePaymentGroupAsync), settled-row removal refused. Build verified. → done.
