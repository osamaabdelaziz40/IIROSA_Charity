# Story 10.23: Identify orphans sponsored elsewhere — أيتام لهم كافل آخر

| Field | Value |
| --- | --- |
| Story | US-PAY-23 (UC-PAY-23) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.U.23) |
| Priority / size | Must · 2 points |
| Route | surfaced from the payments screens (HQ tool) |
| Endpoint | `POST /api/Reports/orphans-other-sponsor` (board endpoint, NEW) |
| Depends on | 10-19 (ReportsController + panel pattern), 10-7 (item model) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer (§15.U.23: HQ roles) |

Status: ready-for-dev

## Story

As a HQ role,
I want to be able to identify orphans sponsored elsewhere أيتام لهم كافل آخر,
so that I can find the record I need without leaving the system.

## Acceptance Criteria

1. Given a charity and batch, when the report runs, then `POST /api/Reports/orphans-other-sponsor` returns the batch rows whose orphan carries an indication of sponsorship by another body — so duplicate disbursement can be prevented.
2. Given rows, then each renders orphan identity, code, the other-sponsor indication, amount and stop state.
3. Given no row qualifies, then an empty result set (grid empty, zero pages).
4. Given a Charity-role caller, then 403 (§15.U.23 primary actor is HQ roles); given session expiry, then back to login.

**Definition of done:** the duplicate-disbursement watchlist works off a clearly-documented data predicate; §15.U.23 passes.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Root | `ReportsController` from 10-19 (or 9-14) |
| Data | `Orphan` carries sponsorship state: `SponsorId`/`SponsorshipStatus` (epic 8's model — status flips null/Pending→Unsponsored on first coding), items carry charity/amount (10-7 model) |
| Pattern | 10-19/10-20 report panels |

## Verified gaps this story must build

1. The "sponsored elsewhere" **predicate must be resolved against the live enum** — see rulings; implement, document the chosen predicate in code + completion notes.
2. Service query + thin action + HQ panel.

## Tasks / Subtasks

- [ ] Task 1 — Predicate resolution (AC: 1): inspect `SponsorshipStatus` (Application enums + Orphan entity usage) for a sponsored-elsewhere/other-sponsor member. If present → filter on it. If absent → use the documented fallback: rows whose orphan's sponsorship state is **not** one of the IIROSA-sponsored states (i.e. sponsored outside this system's sponsor register while still enrolled in the batch) — concretely: `SponsorId == null && SponsorshipStatus != Unsponsored && != null` per epic-8's semantics, adjusted to what the enum actually contains; record the exact clause
- [ ] Task 2 — Service + action (AC: 1, 2, 3): batch+charity scoped rows, projection with identity/indication/amount/stop-state; 403 Charity; POST body typed `{ charityId, batchNo? }`
- [ ] Task 3 — Panel + smoke + i18n: HQ-only surface, grid + ExcelJS export, empty state; smoke with a crafted row

## Dev Notes

### Platform rules that bind this story

- Thin action on the reports root; POST per WAR wire; raw envelope; HQ-only (403 for Charity — matrix "Bank files & disbursement tracking" HQ-only and §15.U.23's actor list); tests excluded per standing decision — smoke and record.

### Story-specific rulings

- **Data-source ruling**: WAR says "orphans who also appear under another sponsoring body" — the legacy system had no clean column for this either (it is a report-time judgment). This story MUST NOT invent a new entity/column (schema froze at 10-2/10-9). Resolve from the existing sponsorship state as in Task 1 and document the clause; if the review finds the predicate unrepresentable, log a correct-course request rather than widening schema silently.
- `batchNo` param aligns with the WAR signature (`charityId, batchNo`) — resolve batchNo→batch via 10-6's `by-batch-no`, or accept `paymentId` interchangeably; pick one, document it.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Printing this list | 10-24 |
| Sponsorship-state writes | EP-08 (orphan register) |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.23]
- [Source: docs/Modules/00-Overview-and-Common-Context.md §4.2 Sponsor/donor actor]
- [Source: _bmad-output/implementation-artifacts/epic-8-orphan-register-and-coding/8-6-assign-a-sponsorship-code-to-an-orphan.md] (SponsorshipStatus semantics)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
