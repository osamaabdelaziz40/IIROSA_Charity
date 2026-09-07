# Story 10.19: Report payments not received — غير المستلمين

| Field | Value |
| --- | --- |
| Story | US-PAY-19 (UC-PAY-19) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.U.19) |
| Priority / size | Should · 3 points |
| Route | surfaced from the payments detail/cheques screens (no dedicated route) |
| Endpoint | `POST /api/Reports/payments-not-received` (board endpoint, NEW — first `/api/Reports` route of this epic) |
| Depends on | 10-7/10-8, 10-11. **Sequencing note:** if epic 9's 9-14 has landed, `ReportsController` already exists — extend it; otherwise this story founds it per 9-14's spec. |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity |

Status: ready-for-dev

## Story

As a HQ role,
I want to be able to report payments not received غير المستلمين,
so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given report criteria (batch, charity), when the report runs, then `POST /api/Reports/payments-not-received` returns the batch rows with `IsGotIt = false` (both stopped and not-stopped — the complement of 10-18), scoped per caller (charity own rows; HQ `charityId` filter).
2. Given rows, then each renders orphan identity, amount, stop state and transfer/cheque status (outstanding evidence).
3. Given no row qualifies, then an empty result set (grid empty, zero pages).
4. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** outstanding-rows report answers "what must still be collected or returned" (§15.U.19 summary); the `/api/Reports` root exists (founded here or reused from 9-14) as EP-18's future home.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Reports root | **No `ReportsController` exists anywhere** (verified). Epic 9's 9-14 (ready-for-dev, not implemented) is slated to found `api/Reports` with `POST non-renewed-reports` |
| Data | All needed columns live (10-7 projection, 10-11 flags) |
| Frontend | No reports surface in this module yet; report-grid pattern exists in reviewed-done modules (missions/seasonal-aid) |

## Tasks / Subtasks

- [ ] Task 1 — ReportsController foundation-or-reuse (AC: 1)
  - [ ] If `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` exists (9-14 landed): add the route there, thin, no module logic
  - [ ] If not: create it exactly per 9-14's recorded design — `ControllerBase`, `[Route("api/Reports")]`, `[Authorize]`, thin delegation; keep it free of module-specific logic (EP-18 grows here)
  - [ ] Response shape follows the reviewed-done house convention (raw typed DTO + try/catch anonymous error object — 15-1 ruling)
- [ ] Task 2 — Service query (AC: 1, 2, 3): `PaymentsNotReceived(filter)` in the Application layer over the 10-7 item model (batch → items → `IsGotIt == false`, charity scope, identity+amount+flags projection); POST body typed `{ paymentId, charityId? }`
- [ ] Task 3 — Frontend surface (AC: 2, 3): report panel reachable from the detail/cheques screens (تقرير بيانات غير المستلمين command per §15.S.3); grid + export-to-Excel via ExcelJS client-side; empty state; i18n ar+en
- [ ] Task 4 — Smoke: HQ all, HQ+charityId, Charity own, empty batch

## Dev Notes

### Platform rules that bind this story

- POST-for-report follows the WAR wire (§1.2's REST preference yields to the quoted board endpoint — same ruling style as epic 9's endpoint column; do not silently convert to GET).
- Business logic in the Application layer; the new controller stays thin. Tenancy from claims; HQ-only `charityId`. Tests excluded per standing decision — smoke and record.

### Story-specific rulings

- Not-received = `IsGotIt == false`, **including stopped rows** (§15.U.19 "must still be collected or returned"); the stopped-only cut is 10-20. Grid marks stop state so the two lists are distinguishable.
- If 9-14 lands later, its controller work must merge cleanly: keep this story's additions minimal (one action) and note them in the File List.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Stopped-payments report | 10-20 |
| Other-sponsor report | 10-23 |
| Printing this list | 10-24 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.19]
- [Source: _bmad-output/implementation-artifacts/epic-9-orphan-periodic-reports/9-14-list-orphans-with-no-renewed-report.md:11,52-62] (ReportsController founding spec)
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-1-list-missions.md] (raw envelope house ruling)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
