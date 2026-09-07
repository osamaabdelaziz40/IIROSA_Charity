# Story 10.8: List orphans in a batch — الأيتام في الدفعة

| Field | Value |
| --- | --- |
| Story | US-PAY-08 (UC-PAY-08) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.S.3, §15.U.8) |
| Priority / size | Must · 2 points |
| Route | `#/orphan-payments/:id` (orphan-level view of the batch) |
| Endpoint | `GET /api/OrphanPayments/{id}/details` (same read as 10-7 — by design) |
| Depends on | 10-7 (charity filtering + row projection) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity |

Status: done

## Story

As a charity user,
I want to be able to list orphans in a batch الأيتام في الدفعة,
so that I can find the record I need without leaving the system.

## Acceptance Criteria

1. Given a caller in the module, when the orphan-level view loads, then `{id}/details` items render one row per enrolled orphan — identity, code, amount, flags — scoped exactly as 10-7 rules (charity sees own rows only).
2. Given the grid, then rows support paging and sorting (code/name/amount) client-side over the returned items, and no stored data changes.
3. Given a search box (اسم اليتيم / كود اليتيم), then the visible rows filter without a new endpoint (client filter over the loaded page) — «Faliure» only for invalid batch id (404).
4. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** orphan-level grid usable as the base surface the receipt/printing screens (10-11/10-12/10-24) build on; empty state clean.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Read | Everything from 10-7: `{id}/details` with charity filtering, `OrphanPaymentItemDto` projection incl. disbursement columns |
| Frontend | `orphan-payment-detail/` renders header; `add-orphans-to-group/` has a working orphan grid pattern to mirror; shared `app-pagination` component in use |
| i18n | `orphanPayments` block has row/flag vocabulary |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. No orphan-level grid surface on the detail screen today (header only) — this story builds it on the 10-7 data.
2. Dead `getGroupOrphans` (`GET {id}/orphans`) call sites still present — the orphan list comes from `{id}/details` items; remove the dead method.
3. `trackBy` + OnPush on the new grid; no `*ngFor` without trackBy (CLAUDE.md frontend rule).

## Tasks / Subtasks

- [x] Task 1 — Orphan grid component section (AC: 1, 2, 3): rows from `{id}/details`; client paging (shared `app-pagination`), sort by code/name/amount; search box; empty state
- [x] Task 2 — Cleanup (AC: 1 — single data path): remove dead `getGroupOrphans` service method + call sites
- [x] Task 3 — Smoke + i18n (AC: 4; ar + en for all new labels)

### Review Findings

_None — clean pass (2026-08-26 code review; grid math, trackBy, OnPush, i18n all verified)._

## Dev Notes

### Platform rules that bind this story

- Reuse the module's bespoke-grid + `app-pagination` pattern (data-list is not used by shipped modules — 15-1 ruling); RTL-first; every string via ngx-translate; tests excluded per standing decision.

### Story-specific rulings

- **Shared endpoint by design**: 10-7 and 10-8 are the same read with different surfaces (§15.2 lists both UCs against `GET {id}/details`). No new endpoint, no query param fork.
- Server-side paging of items is NOT required — batches are bounded (hundreds of rows); client paging over the full item set is acceptable. If a batch exceeds ~1000 rows in practice, note it for a follow-up.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Row action buttons/columns behaviour | 10-9..10-13 (grid gains the buttons there) |
| Received-filtered variant | 10-18 |
| Cheques screen | 10-22 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.8] · [#15.2 UC-PAY-08]
- [Source: Frontend/src/app/modules/orphan-payments/] (grid patterns, dead calls)

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness).

### Debug Log References

- Endpoint evidence reuses 10-7's live smoke (private `http://127.0.0.1:60970` instance, 2026-08-24) — this story adds **no endpoint by design** (§15.2 lists UC-PAY-08 against the same `GET {id}/details`).
- `npx tsc --noEmit` + `npm run build` (`/tmp/ngbuild-108.log`), 2026-08-24.

### Completion Notes List

- **Task 1 (grid surface)** — the 10-7 §15.S.3 grid on `orphan-payment-detail` gains the orphan-level view: client search box (اسم اليتيم / كود اليتيم, `[(ngModel)]` → `onSearchTermChange`, resets page), sortable headers on كود اليتيم / اسم اليتيم / المبلغ (`sortBy` toggles direction; Arabic `localeCompare('ar')`; null amounts sort last), and client paging via the shared `app-pagination` (`pageNumber`/`pageSize=10`/`filteredCount`) — all over the loaded item set, no second call, no stored-data change. State recomputes in one `applyGridState()` (filter → sort → clamp page → slice) called from every mutation point; OnPush + `markForCheck()` + `trackOrphan` trackBy throughout. Empty state distinguishes "no orphans enrolled" from "no search matches" (`noOrphansInGroup` vs new `noOrphansMatch`).
- **Task 2 (dead call)** — `getGroupOrphans` was already removed by the 10-3 pass (only the explanatory comment remains at `orphan-payment.service.ts:177`); grep confirms zero call sites. Pre-satisfied, recorded rather than re-done.
- **Task 3 (i18n + smoke)** — 2 keys × 2 languages (`searchOrphanHint`, `noOrphansMatch`) after the `noOrphansInGroup` anchor; both files JSON.parse-validated. AC-4 evidence carries over from 10-7's live smoke (no token → 401; unknown batch → 404 — the endpoint is byte-identical). AC-1's row rendering is proven by the 10-7 wire capture (identity/code/amount/flags on every row).
- **Env note** — the long-running 10-6 `ng build` background task was stale (hung watcher, no output for the whole 10-7 window); killed and re-run fresh — this build now covers the 10-5..10-8 FE delta. Its bundle-generation phase completed with **zero errors** (AOT accepted every template change; the two `tsc`-only spec errors are a parallel session's untracked `refugee-family-detail` file, excluded from `ng build`'s tsconfig).
- Batches are bounded (client paging ruling stands); no batch in dev data approaches the ~1000-row follow-up threshold.

### File List

| Layer | File | Change |
| --- | --- | --- |
| FE | `orphan-payment-detail/…component.ts` | grid state (`searchTerm`/`sortColumn`/`pageNumber`/`pageSize`/`filteredOrphans`/`pagedOrphans`) + `applyGridState`/`onSearchTermChange`/`sortBy`/`onPageChange`; `PaginationComponent` import |
| FE | `orphan-payment-detail/…component.html` | search toolbar in card header; clickable sortable `th` with chevrons; `*ngFor` over `pagedOrphans`; two-state empty row; `app-pagination` under the table |
| FE | `orphan-payment-detail/…component.scss` | `.cursor-pointer` rule |
| FE | `assets/i18n/ar.json`, `en.json` | 2 keys × 2 languages |

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
- 2026-08-24 — implemented: client search/sort/paging over the 10-7 rows, shared-pagination, two-state empty message, i18n; dead-call cleanup verified pre-satisfied → review.
- 2026-08-26 — code review: clean pass, no findings. → done.
