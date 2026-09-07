# Story 10.11: Confirm receipt of a payment — تأكيد الاستلام

| Field | Value |
| --- | --- |
| Story | US-PAY-11 (UC-PAY-11) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.1, §15.U.11) |
| Priority / size | Must · 5 points |
| Route | `#/orphan-payments/:id` (row action — إستلم per §15.S.3) |
| Endpoint | `POST /api/OrphanPayments/orphan-items` with `action=2` (endpoint from 10-9) |
| Depends on | 10-9 |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity (matrix: receipt is the charity's core right on own rows) |

Status: ready-for-dev

## Story

As a charity user,
I want to be able to confirm receipt of a payment تأكيد الاستلام,
so that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a caller with rights on a row, when receipt is confirmed, then `action=2` sets `IsGotIt` (+ `ReceivedOn`) on that row only — the row "closes" (§15.U.11).
2. Given the row was stopped, when receipt is confirmed, then the operation still succeeds (stopped ≠ not-received; the row simply appears on both lists) — no implicit unstop.
3. Given a Charity caller on an out-of-scope row, then 403; missing input → 400; «Faliure» → message, no write.
4. Given the save succeeds, then the row moves between the received/not-received views (10-18/10-19 consume the flag) and the grid updates without reload.
5. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** simple receipt confirmation works from the orphan grid; BR-22 visibility (charity's confirmation visible to HQ) is satisfied by the read side (10-7 HQ full view).

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint | Action 2 branch on the 10-9 switch |
| Columns | `IsGotIt`, `ReceivedOn` from 10-2's migration |
| Grid | إستلم column in §15.S.3 — 10-8's grid hosts it |

## Tasks / Subtasks

- [ ] Task 1 — Service action-2 branch (AC: 1, 2, 3): set + timestamp, idempotent re-confirm, scope check, no unstop side-effect
- [ ] Task 2 — Grid wiring (AC: 4): إستلم toggle, optimistic update, refusal toasts
- [ ] Task 3 — Smoke + i18n (ar + en): confirm, re-confirm, stopped-row confirm, out-of-scope 403

## Dev Notes

### Platform rules that bind this story

- No new endpoint/fields/schema — action 2 only. Thin controller, raw envelope; tests excluded per standing decision — smoke and record.

### Story-specific rulings

- Action 2 is receipt WITHOUT cheque details; the with-cheque settlement is action 3/4 (10-12) — §25.8 A2 vs main flow. Keep the split.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Cheque-number settlement | 10-12 |
| Received / not-received report views | 10-18, 10-19 |
| Print of receipt cards | 10-24 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.1] · [#15.U.11] · [#25.8 A2, BR-22]

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
