# Story 10.21: View batch summary pages — ملخص الدفعة

| Field | Value |
| --- | --- |
| Story | US-PAY-21 (UC-PAY-21) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.U.21) |
| Priority / size | Must · 2 points |
| Route | surfaced from the payments screens (ملخص صفحات كشف الصرف command per §15.S.3) |
| Endpoint | `GET /api/Dashboard/payment-summary` (board endpoint, NEW — **founds `DashboardController`**) |
| Depends on | 10-7/10-8 (item model), 10-11/10-12 (receipt states) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer (HQ roles per §15.U.21) |

Status: ready-for-dev

## Story

As a HQ role,
I want to be able to view batch summary pages ملخص الدفعة,
so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a batch (+ optional charity filter), when the summary is requested, then `GET /api/Dashboard/payment-summary?paymentId=&charityId=&page=&pageSize=` returns the paginated summary: totals (row count, total amount, received amount/count, not-received count, stopped count) and the per-page row breakdown used as the disbursement file's cover sheet.
2. Given paging, then each page lists its rows' subtotals so the printed cover sheet matches the physical bundle pages.
3. Given a Charity-role caller, then 403 (§15.U.21 primary actor is HQ roles — matrix gives charity "receipt only", not summaries).
4. Given an unknown batch, then 404; given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** the summary panel renders totals + page breakdown; `api/Dashboard` exists for epic 7 (dashboard) to grow into; §15.U.21 passes.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Dashboard | **Nothing** — no `DashboardController`, no payment-summary, no frontend reference (verified by grep). Greenfield. |
| Data | Everything needed is live: items with amounts (10-2), flags (10-9..10-12), charity scoping model (10-7) |
| Pattern | 10-19/10-20 report-panel pattern; SignalR dashboard hub exists elsewhere but is irrelevant here |

## Tasks / Subtasks

- [ ] Task 1 — Found `DashboardController` (AC: 1, 3, 4): `Backend/src/IIROSA.Api/Controllers/DashboardController.cs` — `ControllerBase`, `[Route("api/Dashboard")]`, `[Authorize]`, thin; raw envelope house shape; roles HQ-Fin; document in the class header that EP-07 (dashboard) grows here
- [ ] Task 2 — Service summary (AC: 1, 2): `GetPaymentSummaryAsync(paymentId, charityId?, page, pageSize)` over the item model — totals once + page slice with per-page amount subtotal; 404 unknown batch; pagination clamps (`page >= 1`, `pageSize ∈ [1,200]` — 15-1 review convention)
- [ ] Task 3 — Frontend panel (AC: 1, 2): ملخص صفحات كشف الصرف command opens the summary (totals cards + page table); i18n ar+en; empty state
- [ ] Task 4 — Smoke: totals reconcile with 10-18/10-19 counts for the same batch/charity; HQ filter; Charity 403; unknown batch 404

## Dev Notes

### Platform rules that bind this story

- Thin controller; business logic in the Application layer; raw envelope + anonymous error object (15-1 ruling); claims-based tenancy; tests excluded per standing decision — smoke and record.

### Story-specific rulings

- The §15.U.21 realisation row (`AuthController`/`IDashboardService`) is a legacy artifact of the WAR routing — the board endpoint (`api/Dashboard`) is the contract; a dedicated dashboard controller is the correct home (epic 7's dashboard will extend it).
- Totals must be computed server-side in one pass (no client aggregation of paged rows).
- Amount basis = the row `Amount` column (snapshot) — consistent with 10-14's bank file; exchange-rate application follows the same documented formula (cross-ref 10-14 Task 1).

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Cheques/transfers view | 10-22 |
| Printing the cover sheet | 10-24 (reuses this payload) |
| Epic-7 dashboard widgets | EP-07 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.21] · [#15.S.3 GetPaymentSummeryPages]
- [Source: docs/Modules/00-ROUTING-MAP.md §1.2] (API conventions)
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-1-list-missions.md] (pagination clamps, envelope ruling)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
