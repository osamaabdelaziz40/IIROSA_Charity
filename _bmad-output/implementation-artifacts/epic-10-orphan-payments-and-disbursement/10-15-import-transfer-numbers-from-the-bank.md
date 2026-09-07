# Story 10.15: Import transfer numbers from the bank — رفع أرقام الحوالات

| Field | Value |
| --- | --- |
| Story | US-PAY-15 (UC-PAY-15) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.U.15) |
| Priority / size | Must · 8 points |
| Route | `#/orphan-payments/:id/bank-file` (import tab — screen from 10-14) |
| Endpoint | `POST /api/OrphanPayments/{id}/import/transfer-numbers` (board endpoint, NEW) |
| Depends on | 10-14 (screen), 10-2 (`TransferNo` column) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer (matrix: CSV reconciliation — HQ only) |

Status: ready-for-dev

## Story

As a Financial Director,
I want to be able to import transfer numbers from the bank رفع أرقام الحوالات,
so that data produced outside the system is carried in without manual re-keying.

## Acceptance Criteria

1. Given the bank's returned file uploaded on the import tab, when the import runs, then the matching payment rows of the batch carry their transfer numbers; the response reports `appliedCount` and `unmatchedRows`.
2. Given a row cannot be matched (unknown orphan code / guardian key), then that row is left untouched, reported back, and the rest still apply.
3. Given the file layout is wrong (missing columns / unparseable), then NOTHING is written and the actor is told why (400 with a layout message).
4. Given rows belonging to a different charity than the selection, then they are treated as unmatched (scope: one import applies to the batch's rows for the selected/derived charity).
5. Given a Charity-role caller, then 403 (matrix); given session expiry, then back to login.

**Definition of done:** §25.7 step 6 ("the operator imports the transfer numbers") works; §15.U.15 main flow + both exception flows pass.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Column | `OrphanPaymentItem.TransferNo` (10-2 migration) |
| Screen | `#/orphan-payments/:id/bank-file` from 10-14 — this story adds the import tab + file input (the legacy create-form file inputs were dropped in 10-2; the upload surface lives HERE) |
| Frontend parsing | ExcelJS is a dependency; no other file parsing exists in the app for this module |
| Backend | No Excel/CSV parser in the solution (verified — no EPPlus/ClosedXML); `Framework.Core` has only attachment helpers |

## Design this story must build

- **Parsing ruling (binding, epic-9 precedent)**: the client parses the uploaded .xlsx/.csv with ExcelJS and POSTs a **typed row array**; the endpoint validates, matches and writes server-side. No multipart file handling, no backend Excel package. The wire:
  `POST {id}/import/transfer-numbers` body `{ charityId?, rows: [{ key, orphanCode?, guardianName?, transferNo }] }` → `{ appliedCount, unmatchedRows: [{ key, reason }] }`.
- Match key: orphan code within the batch (primary); guardian-name fallback only if the bank file lacks codes — implement code-first, record which the live bank files actually carry.
- Write path: one UoW save for all matched rows; `TransferNo` set only (no flag changes).

## Tasks / Subtasks

- [ ] Task 1 — Endpoint + service (AC: 1, 2, 4): typed `ImportTransferNumbersDto`; validator (rows non-empty, transferNo required per row); matching against batch items in the caller/charity scope; partial-success result contract; empty-batch/charity mismatch → unmatched reporting
- [ ] Task 2 — Client parse + tab (AC: 1, 3): ExcelJS read → row array; layout validation client-side AND server-side (AC 3 is a server guarantee); progress + result panel (applied / unmatched expandable); i18n ar+en
- [ ] Task 3 — Smoke: happy path, unmatched mix, wrong layout (nothing written — verify DB untouched), Charity 403

## Dev Notes

### Platform rules that bind this story

- Business rules (match, apply, report) in the service — the client's parse is untrusted input; the endpoint re-validates everything (typed DTO + FluentValidation, service-invoked).
- Raw envelope, camelCase; `InvalidOperationException` → 400 for layout refusals; HQ-only `charityId` filter; tests excluded per standing decision — smoke and record (including the nothing-written check).

### Story-specific rulings

- The legacy `UpdateTransferNo` service name (epic crosswalk) is not a contract — the typed endpoint above is.
- Re-import behaviour: a second import overwrites `TransferNo` for re-matched rows and reports as applied again (bank corrections); idempotence is not required — record this in the result panel copy.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Batch reconciliation import / mark-uploaded | 10-16 |
| Exchange-status import | 10-17 |
| Transfers view | 10-22 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.15] · [#25.7 step 6]
- [Source: docs/Modules/00-Overview-and-Common-Context.md §4.2 Bank actor, §4.3 matrix]
- [Source: _bmad-output/implementation-artifacts/epic-9-orphan-periodic-reports/9-14-*.md] (client-side file handling precedent)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
