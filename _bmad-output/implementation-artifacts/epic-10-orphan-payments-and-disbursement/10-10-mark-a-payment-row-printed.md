# Story 10.10: Mark a payment row printed — تعليم كمطبوع

| Field | Value |
| --- | --- |
| Story | US-PAY-10 (UC-PAY-10) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.1, §15.U.10) |
| Priority / size | Should · 3 points |
| Route | `#/orphan-payments/:id` (row action) |
| Endpoint | `POST /api/OrphanPayments/orphan-items` with `action=1` (endpoint from 10-9) |
| Depends on | 10-9 (endpoint + validator skeleton) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity (§15.U.10: Charity) |

Status: ready-for-dev

## Story

As a charity user,
I want to be able to mark a payment row printed تعليم كمطبوع,
so that the paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given a caller with rights on a row, when the mark-printed action fires, then `action=1` sets `IsPrinted` (+ `PrintedOn`) on that row only, without a page reload.
2. Given a row already printed, when marked again, then the operation is idempotent (no error, no timestamp churn).
3. Given a Charity caller on an out-of-scope row, then 403; given missing input, then 400 field errors.
4. Given the selection for a **print run** returns no row, then the actor is told there is nothing to produce rather than receiving an empty file (print runs themselves are 10-24).
5. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** §15.S.3 تمت الطباعة column reflects server state; marking excludes the row from the next print run selection (10-24 consumes the flag).

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint | `POST /orphan-items` action switch — built in 10-9; action 1 branch is this story |
| Columns | `IsPrinted`, `PrintedOn` from 10-2's migration |
| Grid | 10-8's grid + 10-9's row-action wiring pattern |

## Tasks / Subtasks

- [ ] Task 1 — Service action-1 branch (AC: 1, 2): idempotent set + timestamp; scope check reused from 10-9; unit of stamping = one row
- [ ] Task 2 — Grid column + toggle (AC: 1, 3): تمت الطباعة column, mark action, toasts localised
- [ ] Task 3 — Smoke: mark, re-mark idempotent, out-of-scope 403, i18n ar+en

## Dev Notes

### Platform rules that bind this story

- No new endpoint, no new DTO fields, no schema — action 1 on the 10-9 switch only. Thin controller; raw envelope; tests excluded per standing decision — smoke and record.

### Story-specific rulings

- Print-run semantics (selecting all unprinted rows of a batch/charity to print, then flagging them) belong to 10-24; this story is the per-row flag only (§15.1 action 1).

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Receipt confirmation (action 2) | 10-11 |
| Settlement with cheque data (actions 3/4) | 10-12 |
| Print runs / documents | 10-24 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.1] · [#15.U.10]

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
