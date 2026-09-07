# Story 10.18: Report payments received — المستلمون

| Field | Value |
| --- | --- |
| Story | US-PAY-18 (UC-PAY-18) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.U.18) |
| Priority / size | Should · 3 points |
| Route | `#/orphan-payments/:id` (received view of the detail grid) |
| Endpoint | `GET /api/OrphanPayments/{id}/details?received=true` (additive query param on the 10-7 read) |
| Depends on | 10-7, 10-8, 10-11 (`IsGotIt` live) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity (charity sees own rows) |

Status: ready-for-dev

## Story

As a HQ role,
I want to be able to report payments received المستلمون,
so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given the received view is opened, when the read runs, then `{id}/details?received=true` returns only rows with `IsGotIt = true` (scoping per 10-7: charity-filtered for Charity callers, `charityId` filter for HQ) — no stored data changes.
2. Given the view, then rows render with receipt evidence: cheque number, print date, collector, received-on (§25.8 audit trail).
3. Given no row qualifies, then an empty grid + zero pages — not an error.
4. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** received list usable for reconciliation and as the data source of the received-list print (10-24); §15.U.18 passes.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Read | `{id}/details` with charity filtering + full row projection (10-7); adding a boolean filter param is additive |
| Columns | `IsGotIt`, `ReceivedOn`, `ChiqueNum`, `Printdate`, `BenificiaryName` all live |
| Grid | 10-8's orphan grid hosts the view toggle |

## Tasks / Subtasks

- [ ] Task 1 — Backend (AC: 1): add `bool? Received` to the details read path (service-level item predicate `IsGotIt == true` when set); additive only — no param ⇒ identical response to today (8-9 regression guard); unknown id → 404
- [ ] Task 2 — Frontend (AC: 2, 3): view toggle (تقرير بيانات المستلمين) on the detail grid; receipt-evidence columns; empty state; `trackBy`; i18n ar+en
- [ ] Task 3 — Smoke: HQ all-charities received, Charity own received, `?received=true` + charityId combined, no-param regression

## Dev Notes

### Platform rules that bind this story

- Additive read change only — the 8-9 byte-identical contract applies to the no-param path. Thin controller, raw envelope; tests excluded per standing decision — smoke and record.

### Story-specific rulings

- The §15.2 realisation lists both `?received=true` and the bare details call — one param, one endpoint; no fork. The cheques-screen command of the same name (`GetGotItPaymentDetails`, §15.S.3) is THIS view surfaced on 10-22 — no separate backend.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Not-received / stopped reports (separate Reports routes) | 10-19, 10-20 |
| Printing the received list | 10-24 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.18] · [#25.8 step 6]
- [Source: _bmad-output/implementation-artifacts/epic-10-orphan-payments-and-disbursement/10-7-view-payment-details-for-a-charity.md] (read contract + regression guard)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
