# Story 10.14: Generate the bank transfer file — كشف التحويلات البنكية

| Field | Value |
| --- | --- |
| Story | US-PAY-14 (UC-PAY-14) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.S.4, §15.U.14, §25.7) |
| Priority / size | Must · 8 points |
| Route | `#/orphan-payments/:id/bank-file` — **planned → build** |
| Endpoint | `GET /api/OrphanPayments/{id}/export` (as-built route, re-cut semantics — see rulings) |
| Depends on | 10-7 (charity item filtering), 10-9 (`IsStopped` live), 10-2 (`Amount`, `TransferNo`) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer (matrix: bank files — HQ only, no Charity) |

Status: ready-for-dev

## Story

As a Financial Director,
I want to be able to generate the bank transfer file كشف التحويلات البنكية,
so that the data can be handed to the bank, the auditor or the donor in the format they expect.

## Acceptance Criteria

1. Given a Financial Director on `#/orphan-payments/:id/bank-file`, when «كشف التحويلات» is pressed for a charity, then the endpoint returns the composed row set for that charity's batch and the browser delivers an .xlsx **built client-side** (ExcelJS) — no stored data is changed.
2. Given the plain variant, then one line per beneficiary row: guardian identity, account/card number, amount — **stopped rows excluded** (BR-19).
3. Given the merged variants (كرت ميزة / تحويلات), then rows consolidate per guardian into a single line with the summed amount (BR-20); the full-child-name variant additionally carries the orphan's full name.
4. Given no payable row matches, then the actor is told there is nothing to produce — no empty file.
5. Given the batch's exchange rate, then amounts respect `ExchangeRate`/`DontRemoveRate` semantics (rate applied unless locked-removed — reuse the 10-4 rules; document the formula applied).
6. Given a Charity-role caller, then the endpoint is refused (403 — matrix excludes charity from bank files); given session expiry, then back to login; «Faild Operation» → 400 message.

**Definition of done:** three variants downloadable for a charity+batch; §25.7 main flow passes to the "file returned and saved locally" step; the invented as-built export signature is gone.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint | `GET /{id}/export` → `ExportPaymentGroup` (`OrphanPaymentsController.cs:461`) — exists but **stubbed**: `ExportPaymentGroupAsync` returns `Array.Empty<byte>()` with a TODO (`OrphanPaymentService.cs:506`); controller signature `format="Excel", includePhotos=false, groupBy="None"` is **invented** (nothing in WAR) and 400s "not yet implemented" |
| Columns | `Amount`, `TransferNo`, `ExchangeRate`, `DontRemoveRate`, `IsStopped` all live after 10-2/10-9 |
| Frontend | No bank-file screen exists; `exportGroup` service method POSTs to the GET endpoint (405) — replace |
| i18n | `orphanPayments.export*` keys exist (incl. invented `includePhotos` — retire that key's usage) |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. Export stub + invented signature — replace with the payload contract below.
2. Frontend verb mismatch (POST→GET) and the whole bank-file screen is missing (planned route).
3. Roles on export currently SuperAdmin,Admin — widen to HQ-Fin set but **exclude Charity** (matrix).
4. 500 leak pattern on this action.

## Tasks / Subtasks

- [ ] Task 1 — Payload contract (AC: 1, 2, 3, 5)
  - [ ] Re-cut `GET {id}/export?charityId={guid}&variant=Plain|MergedGuardian|MergedFullChildName` returning `{ batch, charity, exchangeRate, rows[] }` — `row`: guardian name, guardian national id, bank account/card number (from the guardian/payment-card source — inspect `Family`/`Provider` for the card/account field; §25.7 pre-condition "guardians have a registered payment card (UC-RPT-07)"; record the exact property used), amount (post-rate), orphan full name (variant 3 only), row count, generated-on
  - [ ] Service: charity item filtering (10-7 model), exclude `IsStopped` (BR-19), merge per guardian (BR-20), rate formula; empty set → typed empty result (AC 4), never a bare 200 with zero rows ambiguity
- [ ] Task 2 — Bank-file screen `#/orphan-payments/:id/bank-file` (AC: 1, 4)
  - [ ] Batch/charity selectors (10-6), three variant buttons (كشف التحويلات · بعد الدمج (كرت ميزه) · بعد الدمج (تحويلات)), ExcelJS workbook build + download, "nothing to produce" empty state
  - [ ] Route registered with `data.permission: 'OrphanPayments.BankFile'` (map from 10-1); lazy module child route — literal `bank-file` sits ABOVE `:id`-consuming patterns if any conflict (epic-7 route-order landmine rule)
- [ ] Task 3 — Cleanup + smoke (AC: 6): drop invented format/includePhotos params + i18n usage; smoke three variants + stopped-exclusion + empty batch + Charity 403

## Dev Notes

### Platform rules that bind this story

- **Client-side rendering ruling (binding, epic-9 precedent)**: the server NEVER produces Excel/PDF bytes — it returns composed JSON payloads; the client (ExcelJS, already a dependency) builds the workbook (9-14/9-17 ruling: "endpoint returns composed payload + variant key, never PDF bytes"; no backend Excel library exists and none is to be added).
- Thin controller, raw envelope; tenancy: `charityId` param is an HQ-only filter; Charity role → 403 here (matrix override of the generic read rule).
- Tests excluded per standing decision — smoke the variants live and record outputs.

### Story-specific rulings

- **Endpoint-shape ruling**: keep the as-built `GET {id}/export` route (satisfies the board) but the legacy three-controller variant family (getBankFile / AfterMerge / AfterMerge_FullChildName) collapses into the `variant` query param (epic-9 collapse precedent). The invented `includePhotos`/`groupBy` export features appear nowhere in chapter 15 — dropped (epic-6 re-cut precedent).
- Missing card/account (§25.7 A1): rows without bank details are **excluded from the file and reported in the payload** (`missingBankDetails[]`) so the operator can route those guardians to cheque disbursement — do not emit rows with blank accounts.
- Transfer numbers are written back by import (10-15), not here.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Transfer-number / status imports | 10-15, 10-17 |
| Batch reconciliation import + mark-uploaded | 10-16 |
| Cheque printing (epic 11 owns) | epic 11 |
| PDF print lists | 10-24 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.S.4] · [#15.U.14] · [#25.7 incl. BR-19/BR-20]
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:461-508] · [OrphanPaymentService.cs:506]
- [Source: docs/Modules/00-Overview-and-Common-Context.md §4.3 matrix "Bank files & CSV reconciliation"]
- [Source: _bmad-output/implementation-artifacts/epic-9-orphan-periodic-reports/9-14-*.md / 9-17-*.md] (client-side rendering precedent)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
