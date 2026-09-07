# Story 10.17: Import exchange (execution) statuses — رفع حالات الصرف

| Field | Value |
| --- | --- |
| Story | US-PAY-17 (UC-PAY-17) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.U.17) |
| Priority / size | Must · 8 points |
| Route | `#/orphan-payments/:id/bank-file` (import tab) |
| Endpoint | `POST /api/OrphanPayments/{id}/import/exchange-status` (board endpoint, NEW) |
| Depends on | 10-15 (pattern), 10-2 (`ExchangeStatus` column) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer (HQ only) |

Status: ready-for-dev

## Story

As a Financial Director,
I want to be able to import exchange (execution) statuses رفع حالات الصرف,
so that data produced outside the system is carried in without manual re-keying.

## Acceptance Criteria

1. Given the bank's execution-status file uploaded, when the import runs, then each matched row records its status — `0 Pending · 1 Executed · 2 Failed` (10-2 enum) — and the response reports `appliedCount` / `unmatchedRows`.
2. Given a status value outside the known set, then that row is refused into `unmatchedRows` with a reason (not silently coerced), others apply.
3. Given a wrong layout, then nothing is written and the actor is told why (400).
4. Given a row has no `TransferNo` yet, when the file references it, then it still matches by orphan code and records the status (status import does not require the transfer reference).
5. Given a Charity-role caller, then 403; given session expiry, then back to login.

**Definition of done:** §25.7's closing post-condition holds ("after import, each row carries its bank transfer reference and execution status" — reference via 10-15, status via this story); §15.U.17 passes.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Column | `OrphanPaymentItem.ExchangeStatus int?` (10-2 migration: 0=Pending, 1=Executed, 2=Failed) |
| Pattern | 10-15's client-parse → typed rows → applied/unmatched contract — third instance |
| Grid | 10-8's grid renders حاله الصرف column once values exist |

## Tasks / Subtasks

- [ ] Task 1 — Endpoint + service (AC: 1, 2, 3, 4): `ImportExchangeStatusDto` (rows: orphanCode + status); validator (status ∈ 0..2); match by orphan code; partial-success contract; layout refusal 400
- [ ] Task 2 — Frontend (AC: 1, 2): third import-tab section; result panel; grid badge for the three states (localised); i18n ar+en
- [ ] Task 3 — Smoke (AC: 3, 5): all three statuses, unknown status → unmatched, wrong layout (nothing written), Charity 403

## Dev Notes

### Platform rules that bind this story

- Identical binding rules to 10-15/10-16 (client parses; server validates + applies; typed DTO; raw envelope; HQ-only; tests excluded — smoke and record).

### Story-specific rulings

- The enum values are fixed by the 10-2 migration — do not renumber or add values (a "pending" row is `0`, an untouched row is `null`; the grid distinguishes them).
- `ExchangeStatus` is informational — it drives no automatic flag changes (a Failed row is NOT auto-stopped; operators decide via 10-9). Record this in the grid tooltip copy.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Reports/views over statuses | 10-19, 10-22 |
| Anything PDF | 10-24 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.17] · [#25.7 post-conditions]
- [Source: _bmad-output/implementation-artifacts/epic-10-orphan-payments-and-disbursement/10-2-create-a-payment-batch.md] (ExchangeStatus column contract)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
