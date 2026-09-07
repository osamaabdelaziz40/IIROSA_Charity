# Story 10.24: Print disbursement documents — طباعة مستندات الصرف

| Field | Value |
| --- | --- |
| Story | US-PAY-24 (UC-PAY-24) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.U.24) |
| Priority / size | Should · 3 points |
| Route | print commands on the cheques/detail workbench (10-22) |
| Endpoint | `POST /api/Reports/payments-received/export/pdf` and the four sibling keys (board endpoints, NEW — payload contracts, see rulings) |
| Depends on | 10-18..10-23 (data surfaces), 10-22 (commands bar), 10-10 (IsPrinted) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity (§15.U.24: charity + HQ) |

Status: ready-for-dev

## Story

As a charity user,
I want to be able to print disbursement documents طباعة مستندات الصرف,
so that the paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given a print command (received list · not-received list · stopped list · cheque-numbers list · receipt cards), when invoked for a batch+charity, then the corresponding `/api/Reports/<key>` route returns the composed document payload and the client renders a printable document via jsPDF — the paper document is produced.
2. Given the received/not-received/stopped lists, then their payloads reuse the SAME projections as 10-18/10-19/10-20 (one dataset per list, no divergent numbers).
3. Given receipt cards (كروت التسليم), then one card section per row with orphan identity, guardian, amount, batch and a signature line; the cheque-numbers list renders row + `ChiqueNum`.
4. Given rows were printed, where the module records printing the row is flagged (`IsPrinted` via action 1 — batch-flagging of print runs, §15.1) with the actor's confirmation.
5. Given the selection returns no row, then the actor is told there is nothing to produce — no empty PDF; «Faliure» → message.
6. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** all five documents printable from the workbench; §15.U.24 + the AC-3 empty-selection rule pass.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| Root | `ReportsController` (10-19/9-14) — five more thin actions |
| Data | 10-18/19/20 projections; item cheque columns (10-12); summary (10-21) |
| Client | jsPDF is a project dependency (epic-9 9-17 precedent: client-side rendering, variant keys) |
| Commands | 10-22's commands bar hosts the print buttons (طباعة الموقوفين · طباعة ارقام الشيكات · طباعة كشف التسليم · طباعة غير المستلم · كروت التسليم) |

## Tasks / Subtasks

- [ ] Task 1 — Payload endpoints (AC: 1, 2): five routes on the reports root — `payments-received/export/pdf`, `payments-not-received/export/pdf`, `payments-stopped/export/pdf`, `cheque-numbers/export/pdf`, `receipt-cards/export/pdf` — each POST `{ paymentId, charityId? }` returning `{ variantKey, batch, charity, rows[], generatedOn }`; the first three delegate to the SAME service queries as 10-18/19/20; cheque-numbers and receipt-cards get minimal new queries over the item model
- [ ] Task 2 — Client rendering (AC: 1, 3): shared print service in the orphan-payments module — jsPDF document per variant (RTL layout, Arabic fonts per the app's existing print pattern from reviewed-done modules — inspect and reuse the missions/seasonal-aid print implementation; do not invent a new pipeline); A4 pagination with page subtotals on lists
- [ ] Task 3 — Print-run flagging (AC: 4): after a successful print of receipt cards / cheque list, offer "mark printed" (batch of action-1 calls, 10-13's bulk pattern)
- [ ] Task 4 — Empty-selection + smoke (AC: 5): every variant on a populated batch, empty batch (message, no file), Charity vs HQ scoping, i18n ar+en for all document labels

## Dev Notes

### Platform rules that bind this story

- **Binding precedent (9-17)**: the endpoint returns a composed payload + variant key — **never PDF bytes**; the client renders. The `/export/pdf` route suffixes are kept verbatim to honour the board's wire, with payload semantics documented in the controller XML comments. No backend PDF library (none exists; none added).
- Five routes collapse the legacy print family (epic-9 collapse precedent); do not build per-template matrix endpoints.
- Thin actions; raw envelope; claims tenancy; tests excluded per standing decision — smoke each variant and record.

### Story-specific rulings

- Receipt cards are the guardians' signed receipts (§4.2 overview: "signs receipt cards") — signature line for `BenificiaryName`; no cheque-stock positioning here (that is epic 11's printer-layout domain).
- Numbers must reconcile: the received list total equals 10-21's received amount for the same scope — assert in smoke.

### Out of scope

| Item | Story / owner |
| --- | --- |
| Cheque stock printing | epic 11 (done) |
| Family/orphan identification sheets | 5-14 (families epic) |
| Non-renewed-reports print | 9-x (epic 9) |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.24] · [#15.2 UC-PAY-24 row] · [#15.S.3 print commands]
- [Source: docs/Modules/00-Overview-and-Common-Context.md §4.2 Guardian/Report actors]
- [Source: _bmad-output/implementation-artifacts/epic-9-orphan-periodic-reports/9-17-*.md] (client-side print precedent, variant keys)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
