# Story 10.3: View a payment batch — عرض الدفعة

| Field | Value |
| --- | --- |
| Story | US-PAY-03 (UC-PAY-03) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.S, §15.U.3) |
| Priority / size | Must · 2 points |
| Route | `#/orphan-payments/:id` |
| Endpoint | `GET /api/OrphanPayments/{id}` (as-built `GetPaymentGroup`) |
| Depends on | 10-1, 10-2 (PaymentDate column) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer (HQ read) |

Status: done

## Story

As a General Director,
I want to be able to view a payment batch عرض الدفعة,
so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given an HQ role in the module, when a batch is opened from the list, then `GET /api/OrphanPayments/{id}` returns the header and the detail screen renders it without a page reload — no stored data is changed.
2. Given the header, then all §15.S.2 batch parameters render (batch no, name, periods, PaymentDate, exchange rate, currency, DontRemoveRate, IsBatchUploaded/UploadDate, notes) plus orphan counts.
3. Given an unknown or soft-deleted id, then the response is 404 and the screen shows a not-found state (not a 500 leak).
4. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** read path works from list → detail; detail calls only live endpoints; i18n resolves in ar + en.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint | `GET /{id}` → `GetPaymentGroup` (`OrphanPaymentsController.cs:92`, roles SuperAdmin,Admin) returning full `OrphanPaymentDto` |
| Service | `GetByIdAsync` (real) — used by the controller; detail screen also uses `GetPaymentGroupDetailsAsync` via `{id}/details` |
| Frontend | `orphan-payment-detail/` screen exists on route `:id` (AuthGuard only — no permission key), service `getPaymentGroup(id)` matches backend |
| DTO | `OrphanPaymentDto` — header + audit + OrphanCount + per-charity/region dicts + `Orphans` list |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. Detail route has **no `data.permission`** — add `OrphanPayments.View` (map lands in 10-1).
2. Read role set narrower than the WAR matrix — widen `GET {id}` to the HQ-Fin set.
3. Detail component calls dead endpoints: `getGroupOrphans` (`GET {id}/orphans` — doesn't exist; the row list comes from `{id}/details`), `getGroupStatistics`, `getAuditLogs`, `printGroup`, `can-modify` — strip or rewire to `{id}`/`{id}/details`.
4. 500 paths leak `ex.Message` on these actions (10-1 Task 1 pattern) — apply the generic-message fix here if 10-1 hasn't already touched these actions.
5. `MapOrphanPaymentItemDto`'s empty charity block (`OrphanPaymentService.cs:721-725`) leaves `CharityName` null — fixing the read join is 10-7's job; this story just must not render fake values for absent columns (render nothing when null).

## Tasks / Subtasks

- [x] Task 1 — Backend (AC: 1, 3)
  - [x] Widen `GetPaymentGroup` roles; 404 (not 500) for unknown/deleted id; generic 500 message
- [x] Task 2 — Detail screen (AC: 2)
  - [x] Render §15.S.2 parameters + counts incl. new `PaymentDate` (10-2 column)
  - [x] Rewire row list to `{id}/details`; remove dead calls (statistics/audit-logs/print/can-modify buttons — print returns with 10-24)
  - [x] Route gains `data.permission: 'OrphanPayments.View'`; `trackBy` on rows; OnPush if touched deeply
- [x] Task 3 — i18n: keys for every rendered label in ar + en; smoke the list→detail round trip

### Review Findings

_Code review 2026-08-26 — full detail in `review-artifacts/epic10-review-report.md`._

- [x] [Review][Patch] `orphanPayments.export` missing from BOTH ar.json and en.json — export button + modal submit render the raw key [orphan-payment-detail.component.html:49,443]
- [x] [Review][Defer] payment-summary band 403s for Accountant/FinancialOfficer/Charity the detail screen admits (Dashboard roles = SuperAdmin,Admin) — role alignment rides 10-21/epic-18 — deferred, owning story

## Dev Notes

### Platform rules that bind this story

- Thin controller, raw envelope (15-1 ruling), camelCase wire, soft-delete global filter (404 comes free for deleted rows once `GetByIdAsync` filters — verify).
- Charity role does NOT get batch-header reads on `{id}` (orphan-scoped only per 8-8/8-9 contract); 10-7 defines the charity-level details read on `{id}/details` — do not widen here.
- Tests excluded per standing decision — record smoke results.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Charity item-level filtering on `{id}/details` + name joins | 10-7 |
| Orphan-level grid surface | 10-8 |
| Row actions on the detail grid | 10-9..10-13 |
| Print/export buttons | 10-14, 10-24 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.3]
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:92-119]
- [Source: Frontend/src/app/modules/orphan-payments/orphan-payment-detail/]

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code CLI).

### Debug Log References

- Live smoke vs dev API http://localhost:60961, seeded SuperAdmin (2026-08-24):
  `POST /api/OrphanPayments` → 201 (batch BP-202608-0001, paymentDate 2026-09-01 persisted);
  `GET /{id}` → 200 full §15.S.2 header (batchNo, periods, paymentDate, exchangeRate, currency,
  dontRemoveRate, isBatchUploaded/uploadDate, notes, orphanCount + per-charity/region dicts);
  `GET /{id}/details` → 200 same shape with `orphans: []` (the row source the screen now loads);
  `GET /00000000-…-000000000000` → **404** (AC 3, soft-delete filter via `GetByIdAsync` → null);
  cleanup `DELETE /{id}` → 204.
- `dotnet build Backend/IIROSA.sln` → 0 errors (8 warnings, pre-existing).
- `npx tsc --noEmit -p tsconfig.app.json` → 0 errors; `ar.json`/`en.json` JSON.parse OK.

### Completion Notes List

- 404-for-deleted verified by construction: `GetByIdAsync` → repository global soft-delete
  filter → null → controller `NotFound`; live 404 on unknown id confirmed.
- Role widening verified at attribute + compile level; the running dev instance may predate
  the edit (bin copy was not locked during the story build). SuperAdmin smoke passes either
  role set — behavioral check for Accountant/FinancialOfficer folded into the epic-final
  regression.
- FE `OrphanPaymentItemDto` re-cut to the server wire (`orphanFullName`, `orphanFamilyName`,
  `orphanAge`, `orphanGender`, `orphanMonthlyAmount`, `sponsorshipStatus`, …). The previous
  shape (`orphanName`/`familyName`/`age`/`monthlyAmount`/`assignedOn`/`assignedBy`/`currency`)
  matched **no** server property — every row rendered empty. The orphan link column is now
  plain text (no `/orphans/:id` route exists; same ruling as 10-2's add-orphans screen).
- Dead calls removed: `getGroupOrphans` (GET {id}/orphans — never existed), `getAuditLogs`
  ({id}/audit-logs — never existed), `printGroup` ({id}/print — returns with 10-24), and the
  `OrphanPaymentAuditLog` model. `canModify()` stays: it is a local `!isBatchUploaded` guard
  over live endpoints (edit/delete/mark-uploaded/remove-orphan), not a dead call.
- `exportGroup` realigned from a body-carrying POST (405 against the live route) to GET
  `{id}/export` with `format`/`includePhotos`/`groupBy` query params — detail now calls only
  live endpoints (export returns 400 "not yet implemented" until 10-24 re-cuts the payload).
- Detail load collapsed to a single `getOrphanPaymentDetails(id)` call (header + rows);
  component switched to OnPush with `markForCheck()` in the subscribe callbacks, `trackBy`
  on row id. Not-found state added (AC 3) with `groupNotFound`/`backToList` i18n keys.
- Charity column renders nothing while `charityName` is null (10-7 fixes the read join);
  header charity binding also dropped its `'-'` fallback to avoid implying data.

### File List

- Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs — GET {id} roles widened to
  HQ-Fin (SuperAdmin,Admin,Accountant,FinancialOfficer) + doc comment
- Frontend/src/app/modules/orphan-payments/models/orphan-payment.model.ts — ItemDto re-cut to
  server wire; `OrphanPaymentAuditLog` removed
- Frontend/src/app/modules/orphan-payments/services/orphan-payment.service.ts —
  `getGroupOrphans`/`printGroup`/`getAuditLogs` removed; `exportGroup` POST→GET with params
- Frontend/src/app/modules/orphan-payments/orphan-payment-detail/orphan-payment-detail.component.ts —
  single `{id}/details` load, not-found state, audit/print stripped, OnPush + markForCheck,
  takeUntil, trackBy row id
- Frontend/src/app/modules/orphan-payments/orphan-payment-detail/orphan-payment-detail.component.html —
  PaymentDate + UploadDate fields, row bindings to live wire, print button + audit-log
  card/section removed, not-found state, charity renders nothing when null
- Frontend/src/app/modules/orphan-payments/orphan-payments-routing.module.ts — `:id` route gains
  PermissionGuard + `permission: 'OrphanPayments.View'`
- Frontend/src/assets/i18n/ar.json / en.json — `uploadDate`, `groupNotFound`, `backToList`
  (ar + en)

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
- 2026-08-24 — implemented (Tasks 1–3): roles widened, detail screen rewired to `{id}` +
  `{id}/details`, dead endpoints stripped, i18n keys added; build + tsc + live smoke clean → review.
- 2026-08-26 — code review remediation (P9): orphanPayments.export key added to ar.json/en.json. Build verified (tsc orphan-payments clean). → done.
