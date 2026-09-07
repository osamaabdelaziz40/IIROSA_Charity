# Story 10.13: Update payment-detail flags — تحديث بيانات الصرف

| Field | Value |
| --- | --- |
| Story | US-PAY-13 (UC-PAY-13) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.1, §15.U.13) |
| Priority / size | Must · 5 points |
| Route | `#/orphan-payments/:id` (detail-grid inline toggles + bulk selection) |
| Endpoint | `POST /api/OrphanPayments/orphan-items` (bulk variant — see ruling) |
| Depends on | 10-9..10-12 (all five actions live) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity |

Status: ready-for-dev

## Story

As a charity user,
I want to be able to update payment-detail flags تحديث بيانات الصرف,
so that a record that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given the detail grid, when a flag cell is toggged inline (stop / printed / received), then the same `POST /orphan-items` action fires for that row and the grid reflects the new state without reload.
2. Given multiple rows are selected and a bulk action is invoked, then each selected row is updated via its own action call (sequential or batched), with a per-row result summary (n updated, m refused with reasons).
3. Given a row-level refusal occurs mid-bulk (scope, HQ-stop lock, validation), then the remaining rows still process and the refused rows are reported (no all-or-nothing).
4. Given a Charity caller, then out-of-scope rows are refused exactly as in 10-9..10-12 (403); nothing is written for them.
5. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** the payment-detail grid is a fully operable disbursement surface (§15.S.3's row-update commands re-cut — see rulings); §15.U.13 passes.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint + actions | `POST /orphan-items` with actions 0..4 fully implemented by 10-9..10-12 |
| Grid | 10-8's orphan grid with per-row action buttons from 10-9/10-10/10-11 + settle dialog from 10-12 |
| i18n | row-action vocabulary from 10-9..10-12 |

## Verified gaps this story must build

1. Inline toggle affordances on flag columns (checkbox cells, not just buttons) for stop/printed/received.
2. Row selection (checkbox column) + bulk action bar (stop, resume, mark printed, confirm receipt) with the AC-2/AC-3 partial-failure report.

## Tasks / Subtasks

- [ ] Task 1 — Inline flag cells (AC: 1): bind directly to the existing per-row actions; disabled + tooltip when the HQ-stop lock applies
- [ ] Task 2 — Selection + bulk bar (AC: 2, 3, 4): per-row calls with result summary toast (n/m + expandable refused list); Charity scope refusals surface clearly
- [ ] Task 3 — Smoke matrix: inline toggles ×3 flags, bulk stop with one out-of-scope row (partial success + report), i18n ar+en

## Dev Notes

### Platform rules that bind this story

- No new endpoint, no schema, no new DTO shape — this story is pure surface over the 10-9 action family. Thin controller untouched; tests excluded per standing decision — smoke and record.

### Story-specific rulings

- **Board-endpoint ruling:** the epic column says `PUT /api/OrphanPayments/{id}` for this UC. That route is the batch-header update (10-4) — reusing it for row flags is a legacy artifact of the WAR wire (same shape of confusion epic 9 resolved on `POST /{id}/review`). Row flags ride `POST /orphan-items` (actions), full stop. Do not fork a second row endpoint.
- Bulk = client-issued sequence of the same per-row calls (batches are small; no server bulk DTO needed). If profiling later demands a server bulk, that is a new story.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Bank file + imports | 10-14..10-17 |
| Reports/print from this grid | 10-18..10-20, 10-24 |
| Cheques workbench screen | 10-22 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.13] · [#15.1] · [#15.S.3 Update() commands]
- [Source: _bmad-output/implementation-artifacts/epic-10-orphan-payments-and-disbursement/10-9-stop-or-resume-an-orphans-payment.md] (endpoint contract)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
