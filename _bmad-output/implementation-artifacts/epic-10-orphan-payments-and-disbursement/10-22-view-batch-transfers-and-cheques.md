# Story 10.22: View batch transfers and cheques — حوالات وشيكات الأيتام

| Field | Value |
| --- | --- |
| Story | US-PAY-22 (UC-PAY-22) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.S.3, §15.U.22) |
| Priority / size | Must · 2 points |
| Route | `#/orphan-payments/:id/cheques` — **planned → build** (disbursement workbench) |
| Endpoint | `GET /api/OrphanPayments/{id}/details` (rows/transfers) + `GET /api/CheckManagement?orphanPaymentId=` (cheques — extend the live epic-11 vertical) |
| Depends on | 10-7..10-13 (grid + actions), 10-15 (`TransferNo`), 10-18..10-21 (commands) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity |

Status: ready-for-dev

## Story

As a HQ role,
I want to be able to view batch transfers and cheques حوالات وشيكات الأيتام,
so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given the cheques screen opens, when the batch + charity are selected, then `{id}/details` renders the rows with transfer number, cheque number/date, exchange status and all §15.S.3 grid columns (the 13-column grid), scoped per caller.
2. Given the cheques tab, when queried, then cheques issued against this batch's orphan payments list via `GET /api/CheckManagement?orphanPaymentId=` — the live epic-11 vertical extended with an `orphanPaymentId` filter (it has none today — verified).
3. Given the commands bar, then the report/print commands of §15.S.3 navigate to the surfaces built by 10-18..10-21 and 10-24 (متابعة الايتام · المستلمين · غير المستلمين · ملخص الصفحات · الموقوفين · كشف التحويلات) — no dead buttons.
4. Given a Charity caller, then only its rows render and cheque actions follow epic-11's own role rules; given session expiry, then back to login.

**Definition of done:** the legacy 22-command cheques screen exists as a coherent workbench: rows grid + row actions (10-9..10-13 wiring) + working commands + cheque list; §15.U.22 passes.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Rows | `{id}/details` full projection + actions (10-7..10-13) |
| Cheques | `CheckManagementController` (epic 11, **done**) — live vertical, but **no `orphanPaymentId` parameter exists** (verified by grep) |
| Report surfaces | 10-18 (received view), 10-19/10-20 (reports), 10-21 (summary), 10-24 (prints) |
| Screen | `#/orphan-payments/:id/cheques` is planned — nothing to reuse |

## Verified gaps this story must build

1. New `OrphanPaymentChequesComponent` (+ route `cheques`, permission `OrphanPayments.Disburse`, literal `cheques` route registered above any `:id`-pattern conflicts — epic-7 route-order rule).
2. `orphanPaymentId` filter on the live CheckManagement list endpoint (additive query param on the existing read — epic-11's wire, minimal diff; epic 11 is done: additive-only, regression-safe, note in File List for the epic-11 record).
3. Commands bar wiring to 10-14/10-18..10-21/10-24 surfaces.

## Tasks / Subtasks

- [ ] Task 1 — Workbench screen (AC: 1, 3): batch/charity selectors (10-6), 13-column grid per §15.S.3 (وقف الصرف · رقم الشيك · كود اليتيم · اسم اليتيم · اسم المعيل · المبلغ الاجمالى · المبلغ المستلم · تمت الطباعة · إستلم · تاريخ الشيك · تم الصرف · تاريخ الصرف — drop the duplicated رقم الشيك column, render once), row-action wiring reused from 10-9..10-13, sort by father/mother/children count (client-side)
- [ ] Task 2 — Cheques tab (AC: 2): extend CheckManagement list with `orphanPaymentId?`; frontend tab calls it; cheque rows render batch name + cheque no/date + amount + delivered (this also feeds 7-4's refugee cheque grid deferral — note in completion notes)
- [ ] Task 3 — Commands bar (AC: 3): every §15.S.3 command routes to a live surface or the 10-24 print keys; NO new report endpoints here
- [ ] Task 4 — Smoke + i18n: HQ vs Charity scoping, cheque tab populated for a batch with cheque data, every command navigates somewhere real

## Dev Notes

### Platform rules that bind this story

- **Trim ruling (epic-6 precedent)**: the legacy screen's cheque-issuance pieces (`AddOrhCheck`, `AddOrhCheckEgypt`, `ViewModalOfPrintChecks`, first-cheque-number/bank/count/date fields — §15.S.3's own field list) are **epic 11's delivered capability**, not this epic's — link out to the epic-11 cheque screens instead of rebuilding. Only the row/transfer/report/cheque-list views are 10-22 scope. Record the trim.
- Additive-only change to CheckManagement (done epic — regression risk); everything else new inside the orphan-payments module. Thin controllers, raw envelope; tests excluded per standing decision — smoke and record.

### Story-specific rulings

- Transfers view = the rows grid with `TransferNo`/`ExchangeStatus` columns (imported by 10-15/10-17); there is no separate transfers entity — do not invent one.
- المبلغ المستلم (received amount) = `Amount` for rows with `IsGotIt`, else 0; المبلغ الاجمالى = `Amount` — document in the grid footer totals.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Print commands' documents | 10-24 |
| Cheque issuance/printing | epic 11 (done) |
| Other-sponsor report | 10-23 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.S.3] · [#15.U.22] · [#15.A route table]
- [Source: Backend/src/IIROSA.Api/Controllers/CheckManagementController.cs] (no orphanPaymentId — extension point)
- [Source: _bmad-output/implementation-artifacts/epic-7-refugee-families/7-4-view-update-a-refugee-family.md:101,146] (cheque-grid columns owed by EP-10)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
