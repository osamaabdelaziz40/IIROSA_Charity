# Story 10.12: Record cheque number, date and collector — تسجيل رقم الشيك والمستلم

| Field | Value |
| --- | --- |
| Story | US-PAY-12 (UC-PAY-12) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.1, §15.U.12, §25.8) |
| Priority / size | Must · 5 points |
| Route | `#/orphan-payments/:id` (settle dialog on a row) |
| Endpoint | `POST /api/OrphanPayments/orphan-items` with `action=3|4` (endpoint from 10-9) |
| Depends on | 10-9 (endpoint/validator), 10-11 (receipt semantics) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity |

Status: ready-for-dev

## Story

As a charity user,
I want to be able to record cheque number, date and collector تسجيل رقم الشيك والمستلم,
so that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a caller with rights on a row, when the settle dialog saves with cheque number + print date + collector name, then `action=3` sets `IsGotIt`, `IsPrinted`, `ChiqueNum`, `Printdate`, `BenificiaryName` together on that row — full settlement (§25.8 main flow).
2. Given no print date, when saving, then `action=4` stores everything except `Printdate` (§25.8 A1) — the dialog's date field is optional and drives the action choice.
3. Given `ChiqueNum` or `BenificiaryName` is empty on a 3/4 save, then 400 with the offending field flagged (validator rule from 10-9).
4. Given a Charity caller on an out-of-scope row, then 403; a stopped row settles normally (BR-21 only forbids un-stopping); «Faliure» → message, no write.
5. Given a settled row, then it appears on the received list (10-18) and leaves the not-received list (10-19); the audit trail (cheque number, date, collector, stamps) is visible on the row.
6. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** one-step settlement dialog works per §25.8; property names on the wire are verbatim `chiqueNo` / `chiqueDate` / `sponserName` per §15.U.12 — see rulings for the DTO naming decision.

## What exists already

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint | Actions 3/4 branches on the 10-9 switch; `UpdateOrphanPaymentItemValidator` already requires ChiqueNum + BenificiaryName for 3/4 |
| Columns | `ChiqueNum`, `Printdate`, `BenificiaryName` from 10-2's migration |
| Grid | 10-8's grid + row-action pattern (10-9..10-11) |

## Tasks / Subtasks

- [ ] Task 1 — Service actions 3/4 (AC: 1, 2, 4): set flags + cheque fields + stamps in ONE UoW save; action 4 skips `Printdate`; scope check reused
- [ ] Task 2 — Settle dialog (AC: 1, 2, 3): رقم الشيك (required) · تاريخ الشيك/الطباعة (optional — drives 3 vs 4) · اسم المستلم (required); field errors from 400 payload; SweetAlert2 confirm
- [ ] Task 3 — Row rendering (AC: 5): cheque columns per §15.S.3 (رقم الشيك · تاريخ الشيك) show stored values; received state badge
- [ ] Task 4 — Smoke + i18n: action 3, action 4, missing field 400, out-of-scope 403, settled row appears in received view

## Dev Notes

### Platform rules that bind this story

- No new endpoint, no schema. Thin controller, raw envelope, camelCase wire; business refusals `InvalidOperationException` → 400; tests excluded per standing decision — smoke and record.

### Story-specific rulings

- **DTO naming**: §15.U.12's wire names (`chiqueNo`, `chiqueDate`, `sponserName`) are the legacy AngularJS spellings. The typed DTO uses the §15.1/§25.8 **column** spellings `ChiqueNum`, `Printdate` (as `chiqueDate` on the wire per §15.U.12), `BenificiaryName` (as `sponserName`→`benificiaryName`); keep the **entity column names verbatim** (they match the legacy table, easing any data migration), and map the dialog fields accordingly. Record the alias table in completion notes.
- Settlement is per-row only; bulk settlement via import lands in 10-16.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Grid-level bulk flag surface | 10-13 |
| Cheques issued via CheckManagement (epic 11, done) — this story only records numbers on rows | epic 11 |
| Cheque-numbers print list | 10-24 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.1] · [#15.U.12] · [#25.8 incl. BR-21/BR-22]

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
