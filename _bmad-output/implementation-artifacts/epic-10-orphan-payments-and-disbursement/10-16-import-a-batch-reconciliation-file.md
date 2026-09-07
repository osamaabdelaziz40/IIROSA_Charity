# Story 10.16: Import a batch reconciliation file — رفع ملف الدفعة

| Field | Value |
| --- | --- |
| Story | US-PAY-16 (UC-PAY-16) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.U.16) |
| Priority / size | Must · 8 points |
| Route | `#/orphan-payments/:id/bank-file` (import tab) |
| Endpoint | `POST /api/OrphanPayments/{id}/import/bank-file` (board endpoint, NEW) |
| Depends on | 10-15 (parse/tab pattern), 10-11/10-12 (flags it may set), 10-2 (`IsBatchUploaded`) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer (HQ only) |

Status: ready-for-dev

## Story

As a Financial Director,
I want to be able to import a batch reconciliation file رفع ملف الدفعة,
so that data produced outside the system is carried in without manual re-keying.

## Acceptance Criteria

1. Given the batch file uploaded, when the import runs, then matched rows carry the imported values and the response reports `appliedCount` / `unmatchedRows`.
2. Given a row can't be matched, then it is untouched and reported; the rest apply.
3. Given a wrong layout, then nothing is written and the actor is told why (400).
4. Given the import completes successfully, then the batch header's `IsBatchUploaded` flips to true with `UploadDate` stamped (the as-built mark-uploaded semantics) — visible on the 10-1 list.
5. Given a Charity-role caller, then 403; given session expiry, then back to login.

**Definition of done:** §15.U.16 passes; the reconciliation round trip (file → rows → uploaded-flag) works; mark-uploaded is also manually triggerable.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Mark-uploaded | `POST /{id}/mark-uploaded` → `MarkAsUploaded(MarkAsUploadedDto)` (`OrphanPaymentsController.cs:431`) + `MarkAsUploadedAsync` (real) — exists; frontend calls it with **PATCH (405)** and `unmark-uploaded` doesn't exist |
| Columns | All item columns live (10-2); header `IsBatchUploaded`/`UploadDate` exist |
| Pattern | 10-15's client-parse → typed-rows → partial-success contract — reuse verbatim |

## Design this story must build

- The legacy `InesrtValuesIntoDb` (epic crosswalk) = "apply the batch CSV's contents to the rows". Concretise: the reconciliation file carries per-row updates — amounts received, receipt flags, cheque data (whichever columns are present). Wire: `POST {id}/import/bank-file` body `{ charityId?, rows: [{ key/orphanCode, isGotIt?, chiqueNum?, chiqueDate?, benificiaryName?, amount? }] }` → same applied/unmatched contract as 10-15. **Only provided columns are applied** (null = untouched).
- Column application reuses the action semantics of 10-11/10-12 (a row with `isGotIt` + cheque fields settles like action 3/4) — implement by calling the same service paths, not by duplicating flag logic.

## Tasks / Subtasks

- [ ] Task 1 — Endpoint + service (AC: 1, 2, 3, 4): typed DTO + validator; match by orphan code; partial apply through the 10-9 action paths; on ≥1 applied row → `IsBatchUploaded=true` + `UploadDate` (same UoW); layout refusal → 400, nothing written
- [ ] Task 2 — Frontend (AC: 4): import-tab section (alongside 10-15's), result panel; fix `markAsUploaded` PATCH→POST; drop the dead `unmark-uploaded` call or add a real unset path via the existing endpoint (`IsUploaded:false`) — choose and record
- [ ] Task 3 — Smoke: mixed file, wrong layout (DB untouched), uploaded flag on list, manual mark/unmark, Charity 403

## Dev Notes

### Platform rules that bind this story

- Same binding rules as 10-15 (client parses, server validates/applies; typed DTO; raw envelope; HQ-only). Tests excluded per standing decision — smoke and record.

### Story-specific rulings

- Import-applied flags respect the same guards as manual actions: HQ-stop-locked rows and out-of-scope rows land in `unmatchedRows` with reasons — the import must not bypass 10-9's rules.
- The 10-2 create-form file inputs (legacy) are NOT restored — this tab is the only upload surface.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Exchange-status import | 10-17 |
| Reports on imported state | 10-18..10-20, 10-22 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.16]
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:431-460] · [OrphanPaymentService.cs:289-311]

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
