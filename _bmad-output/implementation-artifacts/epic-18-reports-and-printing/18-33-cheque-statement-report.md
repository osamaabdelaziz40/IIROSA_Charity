# Story 18-33: Cheque statement report بيان الشيكات

| Field | Value |
| --- | --- |
| Story key | `18-33-cheque-statement-report` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-33 — بيان الشيكات |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.33 scenario — no dedicated §23.S screen) |
| Route | `#/reports/cheque-statement` (thin print screen — board assigns no route; decision recorded) |
| Endpoint | data: **EXISTING** `GET /api/CheckManagement/report` (UC-CHQ-09, `CheckManagementController.cs:97`, returns `CheckStatementDto`); document: client-side via 18-21's PDF service. The board/legacy `POST /api/Reports/cheque-statement/export/pdf` is superseded — recorded deviation |
| Depends on | **18-21 landed** (`services/report-pdf.service.ts`); cheque vertical (EP-11) live with the statement endpoint; 18-1's reports module shell |
| Roles | Fin. Director → `SuperAdmin`, `Admin` (`Reports.View`; the data endpoint's own roles already include `Accountant`, `FinancialOfficer` — the screen gates on `Reports.View`, the endpoint stays the control) |

## Status

done

## Story

As a Financial Director, I want to be able to cheque statement report بيان الشيكات, so that the
paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given a Financial Director with an active session on `#/reports/cheque-statement`, when the
   actor sets the bank / date range / cheque-type filters and presses طباعة, then the bank
   reconciliation statement is produced for print/save. No stored data is changed.
2. Given the data is fetched, when the statement renders, then the rows + per-currency totals come
   from the EXISTING `GET /api/CheckManagement/report` (`CheckStatementDto`) — no new Reports
   endpoint, no fork of the cheque service.
3. Given the selection returns no row, when the document is produced, then the actor is told that
   there is nothing to produce rather than receiving an empty file.
4. Given a required parameter is missing (e.g. no bank chosen where the statement requires one),
   when طباعة is pressed, then the request is refused with a `message` and the offending field is
   flagged.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** §23.U.33 passes end to end — filters (bank, date range, cheque type),
statement grid + totals, and the printable document via the epic's client-side PDF path; the
charity/country scope of the underlying cheque read is whatever EP-11 enforces (recorded, not
re-cut here).

## Screen contract (derived from §23.U.33 — bankId, dateFrom, dateTo, checkType)

| Field | DTO key (of the existing `CheckFilterDto`) | Control | Mandatory | Source |
| --- | --- | --- | --- | --- |
| البنك | bank filter key | Drop-down | **Yes** | `GET /api/LookupManagement/banks` |
| من تاريخ | date-from key | Date picker | No | |
| الى تاريخ | date-to key | Date picker | No · ≥ from | |
| نوع الشيك | cheque-type key | Drop-down | No | the cheque vertical's own type options — read the live `CheckFilterDto` first; NO hardcoded arrays |

Task 1 audits `CheckFilterDto` and `CheckStatementDto` and renames this table's keys to the real
wire keys — the contract above records INTENT, the audit records the binding.

## Tasks / Subtasks

- [x] **Task 1 — Contract audit (binding first step)** (AC 2, 4)
  - [x] Read `Backend/src/IIROSA.Application/DTOs/CheckManagement/Checks.cs`
        (`CheckFilterDto`, `CheckStatementDto`) and `CheckManagementController.cs:93-127`; map
        §23.U.33's four params onto the REAL filter keys (bank, date range, cheque type) and the
        statement's row/totals shape onto the document's columns; record the mapping in the Dev
        Agent Record
- [x] **Task 2 — Thin screen** (AC 1, 4, 5)
  - [x] `Frontend/src/app/modules/reports/cheque-statement/` thin 4-file component in 18-1's
        shell: filter panel (bank from `GET /api/LookupManagement/banks`, date pickers, type
        select from the audited source), a statement grid with the per-currency totals row,
        `trackBy`, empty state; route `cheque-statement`, `AuthGuard + PermissionGuard`,
        `data.permission: 'Reports.View'`; OnPush omitted (list-screen precedent)
  - [x] Data call: the module's cheque service or a small `getStatement` wrapper calling
        `GET /api/CheckManagement/report` with the typed query — reuse the existing
        `checks`/general-checks module service if one already wraps it (check
        `modules/general-checks`); do NOT duplicate a cheque HTTP client
- [x] **Task 3 — Printable document** (AC 1, 3)
  - [x] A `cheque-statement-document` builder feeding 18-21's `report-pdf.service.ts`: A4
        landscape if the row count demands, RTL header (charity/bank/range), the statement rows,
        per-currency totals footer, labels through `reports.chequeStatement.*` i18n in **both**
        `ar.json` and `en.json`; empty result → nothing-to-produce message, no file
  - [x] Produce-and-preview via the PDF service's standard flow (18-40's viewer when landed)
- [x] **Task 4 — Verification** (build subtask; live-check subtask below stays open for the consolidated epic smoke) (AC 1–5)
  - [x] Live check: anonymous → 401; missing bank (where the audit made it required) → 400 with
        the field flagged; empty range → message, no file; rows + totals match the cheque
        register's own screen for the same filter; `dotnet build` (expected: backend untouched)
        + `npm run build` green (ng-serve stale-bundle grep); tests excluded per the standing
        user decision

## Dev Notes

### Reuse + supersession (recorded)

- The cheque statement's DATA endpoint already exists — EP-11 shipped UC-CHQ-09 as
  `GET /api/CheckManagement/report` with `CheckStatementDto` (register filters + per-currency
  totals, "rendered for printing" per its own doc comment). This story is the reports-module
  SURFACING of that statement plus the epic's client-side PDF rendering. The board/legacy
  `POST /api/Reports/cheque-statement/export/pdf` route is superseded — building it would fork a
  live endpoint (the exact duplication 18-27 guards against).
- Role note: the data endpoint's `ReadRoles` include `Accountant`/`FinancialOfficer`; the reports
  screen gates on `Reports.View` for menu visibility only. The ENDPOINT remains the authorisation
  control — no client-side-only gating.
- The legacy «Faliure» rule maps to AC 4's missing-parameter refusal — there is no write to fail.

### Platform rules that bind this story

- No new backend endpoint, no DTO change, no migration — audit + frontend + PDF builder only.
- Client-side PDF only (jsPDF via 18-21's service); server EPPlus/Crystal superseded.
- i18n in both languages; bespoke grid; lazy module; no hardcoded option arrays.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Cheque register CRUD/filters themselves | EP-11 (landed) |
| Generic in-browser preview modal | 18-40 |
| Excel export of the statement | 18-41 (generic engine) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.33] scenario — bank/date/type
  filters, reconciliation statement
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-33 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/CheckManagementController.cs:93-127] the LIVE
  statement endpoint this story reuses (UC-CHQ-09)
- [Source: Backend/src/IIROSA.Application/DTOs/CheckManagement/Checks.cs] `CheckFilterDto` /
  `CheckStatementDto` — the audited contract
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-21-family-update-tracking.md] the PDF service
  the document renders through

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Implemented this session (frontend-only — backend untouched, as the story cuts it). Task 1's
  contract audit completed FIRST and is recorded below. `npm run build` → exit 0
  (`NODE_TLS_REJECT_UNAUTHORIZED=0` for the Google-Fonts inline step behind the corporate
  proxy); `dotnet build` not re-run — no backend file changed (expected per Task 4).
- Live-check matrix (anon 401 on the data endpoint / rows + totals vs the cheque register's own
  §16.S.3 screen for the same filters / empty range message / date-order refusal): deferred to
  the epic-18 consolidated smoke at the end of the 18-30…18-41 batch — recorded here when run.

### Completion Notes List

- **Task 1 contract audit (binding, recorded)**: `CheckFilterDto` real keys =
  `charityId?`, `bankId?` (int), `dateFrom?`, `dateTo?`, `chequeType?` ("Orphans"/"Individuals";
  null = both), `searchText?`, `page`/`pageSize` — the story table's four §23.U.33 params map
  verbatim (`bankId`/`dateFrom`/`dateTo`/`chequeType`). `CheckStatementDto` = `items:
  CheckListDto[]` (checkNumber, checkDate, beneficiaryName, amount, currency, chequeType,
  bankName, flag columns), `totalCount`, `totalPages`, `totalByCurrency: {code: total}`,
  `generatedOn`. **Audit finding on mandatory fields: NONE** — every `CheckFilterDto` field is
  nullable and the service attaches no NotEmpty validator to `bankId`; the screen-contract
  table's "Bank: Yes" is therefore NOT enforced (code-wins): AC 4's refusal path reduces to the
  client-side date-order guard (`dateOrderInvalid`), and the server's 400 `{message,errors}`
  ladder (observed live in the controller) covers any future validator.
- **Reuse honoured**: data via the general-checks module's own
  `GeneralChecksService.getStatement` (GET /api/CheckManagement/report) — no forked cheque HTTP
  client, no new Reports endpoint. Cross-module service import follows the reports screens'
  precedent (orphan-data-report imports OrphanPaymentService/CharityService). Bank options from
  `LookupManagementService.getBanks` — the same lookup the cheque register uses; type options
  are the audited wire values ("Orphans"/"Individuals" + both), not a decorative array.
- **Scope note (recorded, not re-cut)**: the charity/country scope of the cheque read is
  whatever EP-11's `GetStatementAsync` enforces — this screen sends no `charityId` (the story's
  four params only; the §16.S.3 register screen owns the HQ charity narrow).
- **Print shape**: 18-21's browser-print `printSheet` — totals ride the meta band (bank, range,
  type label, count + per-currency totals joined), columns = the audited core set + type.
  Empty result → info toast, no sheet (AC 3). Bespoke طباعة button in the screen's command arm
  with its own spinner (the viewer's export arm is hidden via `[showExport]="false"` — its
  استخراج label would mislabel a print command).
- **Route**: `#/reports/cheque-statement`, `AuthGuard + PermissionGuard`,
  `data.permission: 'Reports.View'` (menu-visibility gate only; the endpoint's own roles —
  which include Accountant/FinancialOfficer — remain the authorisation control).
- **Page cap recorded**: one statement read at `pageSize: 200` (page 1) — the register's own
  §16.S.3 statement screen's exact cap; the print sheet renders that pass.

### File List

- Frontend/src/app/modules/reports/cheque-statement/cheque-statement.component.ts · .html · .scss · .spec.ts — NEW (4-file shape)
- Frontend/src/app/modules/reports/reports-routing.module.ts — the `cheque-statement` route
- Frontend/src/assets/i18n/ar.json, en.json — `reports.chequeStatement.*` (24 keys each)

### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

- Anonymous → 401.
- Bank omitted → 200, per this story's OWN audit ruling (bank optional — the box's original "400
  where the audit made it required" expectation was superseded inside the story; the live behaviour
  matches the audited contract).
- Empty range → message, no file; rows + totals cross-checked against the cheque register for the
  same filter.
- Browser pass (18-41): the screen's search/export/preview shell rides this endpoint; a wide date
  range returned the register's rows and the export filename convention held.
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-33 and module spec §23.U.33; cut to a surfacing story over the LIVE `GET /api/CheckManagement/report` (UC-CHQ-09) with the legacy Reports-route supersession and audit-first task recorded. |
| 2026-08-25 | Implemented: Task 1 audit recorded (no server-required filter; keys map verbatim), thin screen in the report-viewer shell over the general-checks service, browser-print sheet + i18n ×2 locales; `npm run build` green, backend untouched. Live-check deferred to the consolidated epic smoke. Status → in-progress pending that smoke. |
| 2026-08-25 | Live smoke passed (auth, bank-optional per the story audit, empty-range message, register cross-check). Status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
