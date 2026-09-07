# Story 10.20: Report stopped payments — الموقوفون

| Field | Value |
| --- | --- |
| Story | US-PAY-20 (UC-PAY-20) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.U.20) |
| Priority / size | Should · 3 points |
| Route | surfaced from the payments detail/cheques screens |
| Endpoint | `POST /api/Reports/payments-stopped` (board endpoint, NEW) |
| Depends on | 10-19 (ReportsController + report-panel pattern), 10-9 (`IsStopped`, stop stamps) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity |

Status: ready-for-dev

## Story

As a HQ role,
I want to be able to report stopped payments الموقوفون,
so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given report criteria, when the report runs, then `POST /api/Reports/payments-stopped` returns the batch rows with `IsStopped = true` for the scoped charity (charity own / HQ filter) — reason context included.
2. Given rows, then each renders orphan identity, amount, stopped-on, stopped-by (user id resolved to name where available), and stop reason where recorded.
3. Given no row qualifies, then an empty result set; «Faliure»-class refusals return the message with no write (read-only operation).
4. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** HQ follow-up list (§15.U.20 summary) works; Excel export client-side; §15.U.20 passes.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Root | `ReportsController` from 10-19 (or 9-14) — one more thin action |
| Data | `IsStopped`, `StoppedOn`, `StoppedByUserId` (10-9) live; item notes may carry reason text |
| Pattern | 10-19's report panel + grid + ExcelJS export |

## Tasks / Subtasks

- [ ] Task 1 — Service query (AC: 1, 2): stopped rows with stop stamps; resolve `StoppedByUserId` → user name via the identity store (batch lookup, no N+1); reason = item `Notes` if present (no dedicated reason column exists — see rulings)
- [ ] Task 2 — Controller action + frontend panel (AC: 2, 3): تقرير الموقوفين command (§15.S.3), grid, export, empty state; i18n ar+en
- [ ] Task 3 — Smoke: HQ all / HQ+charityId / Charity own / empty

## Dev Notes

### Platform rules that bind this story

- Same rules as 10-19 (thin action on the reports root, POST per WAR wire, raw envelope, claims tenancy). Read-only. Tests excluded per standing decision — smoke and record.

### Story-specific rulings

- **Reason-context ruling**: §15.U.20 asks for "reason context". The Domain has no stop-reason column; the enforceable sources are stop stamps + item `Notes` + (later) the epic-9 exclusion/report state. If a dedicated stop-reason is wanted, that is a backlog change request — do not invent a column here (schema freeze after 10-2/10-9).
- Stop stamps exist only for rows stopped through 10-9+ (not for legacy/manual DB states) — render blank-when-null, never fake.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Printing the stopped list | 10-24 |
| Other-sponsor report | 10-23 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.20]
- [Source: _bmad-output/implementation-artifacts/epic-10-orphan-payments-and-disbursement/10-9-stop-or-resume-an-orphans-payment.md] (stop stamps contract)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
