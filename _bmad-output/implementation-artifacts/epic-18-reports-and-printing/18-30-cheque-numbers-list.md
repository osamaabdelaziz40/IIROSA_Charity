# Story 18-30: Cheque numbers list

| Field | Value |
| --- | --- |
| Story key | `18-30-cheque-numbers-list` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-30 — Cheque numbers list أرقام الشيكات |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.3 financial-reports row UC-RPT-30, §23.U.30 scenario) |
| Route | Hosted print — أرقام الشيكات command on the orphan-payments batch screen (10-22's commands bar); rendered through the shared `report-viewer` shell |
| Endpoint | `POST /api/Reports/cheque-numbers` (board key; legacy `POST /api/Reports/cheque-numbers/export/pdf` — superseded, see Dev Notes) |
| Depends on | **18-1 landed** (Reports skeleton), **18-21 landed** (`report-pdf.service.ts`); cheque numbers on rows land with **10-12** (EP-10) |
| Roles | Charity → `Charity` (+ `SuperAdmin`, `Admin`) (`Reports.View`) |

## Status

done

## Story

As a charity user, I want to be able to cheque numbers list أرقام الشيكات, so that the paper
document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given a charity user with an active session on the orphan-payments batch screen, when the
   actor invokes the cheque-numbers command for a batch and charity, then no stored data is
   changed — the operation is a read.
2. Given the request is accepted, when it is served, then the data is fetched from
   `POST /api/Reports/cheque-numbers` and the document is rendered client-side without a page
   reload — no server-side PDF is produced.
3. Given the caller is a charity user, when the list is served, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
   Given an HQ caller (`IsHeadOffice`) with an explicit charity id, then the report operates on
   that charity's data; the country claim pins the country.
4. Given the batch has no cheque recorded, when the document is produced, then the actor is told
   that there is nothing to produce rather than receiving an empty file.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.
6. Given a business rule refuses the operation (legacy «Faliure»), when it is attempted, then the
   refusal surfaces as an anonymous `{ message }` response and nothing is written.

**Definition of done:** the cheque-numbers sheet for a batch in a charity is printable for
handover and reconciliation; §23.U.30 passes end to end; scoping is enforced server-side, not
only in the menu.

## Tasks / Subtasks

- [x] **Task 1 — DTOs + validator** (AC 2, 3)
  - [x] Extend `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`:
        `ChequeNumbersFilterDto` (`Guid? CharityId`, mandatory `string OrpCheckBatchNo`),
        `ChequeNumbersRowDto` (serial, orphan code/name, guardian, amount, currency,
        `chiqueNo`, print date, collector), `ChequeNumbersReportDto` (charity/batch header,
        `rows`, `totalCount`, totals). No `FK_*` wire keys; camelCase.
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/ChequeNumbersFilterValidator.cs` —
        `OrpCheckBatchNo` NotEmpty; `CharityId` optional (HQ-only honouring decided in the
        service).
- [x] **Task 2 — Service projection** (AC 1–4)
  - [x] `IReportService.GetChequeNumbersAsync(...)` in
        `Backend/src/IIROSA.Application/Services/ReportService.cs`: resolve the batch by `BatchNo`
        (the same resolution `GET /api/OrphanPayments/by-batch-no/{batchNo}` uses), then
        `ResolveCharityScope(filter.CharityId)` (18-1's helper: charity caller pinned to
        `ICurrentUserService.CharityId`; `IsHeadOffice` may narrow; `CountryId` claim pins
        country).
  - [x] Rows = the batch's payment rows **with a cheque number recorded** (`ChiqueNum` from
        10-12's settlement) plus orphan identity and amount; rows without a cheque are not on
        this sheet.
  - [x] **No bank filter here** — bank/date/type filtering is 18-33's dimension; unknown batch or
        zero recorded cheques → `rows: []` + message (AC 4).
- [x] **Task 3 — API endpoint** (AC 2, 5)
  - [x] `[HttpPost("cheque-numbers")]` in
        `Backend/src/IIROSA.Api/Controllers/ReportsController.cs`; `[Authorize]` roles per the
        header; `Ok(reportDto)`; catch `FluentValidation.ValidationException` FIRST → 400
        `{ message, errors }`; catch-all → 500 `{ message }`. Raw envelope, NOT `ApiResponse<T>`.
- [x] **Task 4 — Frontend host + print layout** (AC 1–4)
  - [x] Command أرقام الشيكات on
        `Frontend/src/app/modules/orphan-payments/orphan-payment-detail/` (extend the existing
        4-file component).
  - [x] `Frontend/src/app/modules/reports/services/report.service.ts` — `getChequeNumbers(filter)`;
        preview in the shared `report-viewer` grid with `trackBy`.
  - [x] `Frontend/src/app/modules/reports/services/report-pdf.service.ts` (18-21) — list layout:
        serial, orphan, guardian, amount, cheque number, print date, collector; charity + batch
        header; totals row; RTL Arabic per 18-21's font decision.
  - [x] Empty result → i18n nothing-to-produce toast; no empty PDF.
- [x] **Task 5 — i18n** — `reports.chequeNumbers.*` (command label, title, column headers,
      totals, empty message) in **both** `assets/i18n/ar.json` and `en.json`.
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors (MSB3021/3027 = live-API output lock; never
        kill the user's process).
  - [x] Live check: anonymous POST → 401; charity token → only own charity's rows; HQ with
        explicit `charityId` → that charity's rows; batch with no cheques → empty + message
        (Arabic payloads from UTF-8 files, not inline curl bodies).
  - [x] `cd Frontend && npm run build` — 0 errors; ng-serve stale-bundle caveat (grep the served
        chunk before trusting a no-effect fix).
  - [x] Tests: excluded per the standing user decision.

## Dev Notes

### PDF/Excel ruling (epic-wide — recorded for every print story)

Client-side rendering only. The legacy realisation — `POST /api/Reports/cheque-numbers/export/pdf`
and its Crystal Reports sheet — is **superseded**: the data comes from the JSON endpoint above and
the document is rendered client-side by `report-pdf.service.ts` (jsPDF, RTL-Arabic font per
18-21's decision). No server-side PDF, no EPPlus, no `.rpt` files are ported.

### Platform rules that bind this story

- EP-18 adds **no entities and no migration** — read-only projection.
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10). Controller inherits `ControllerBase` + `[Authorize]` +
  `[Route("api/[controller]")]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories; global soft-delete query filter.
- Charity/country scope from `ICurrentUserService` in the service — never the payload.
- Bespoke grid — **not** `data-list`; lookup labels `NameAr ?? NameEn` where names render.

### Data-source ruling (recorded — verified against the code)

There is **no FK between `Check` and `OrphanPayment`** (verified: `Check` carries `BankId`,
`CharityId`, dates, type — no batch link). The per-batch cheque numbers therefore come from the
**payment rows' recorded `ChiqueNum`** (landed by 10-12's settlement, printed the same way by
10-24's AC 3), not from a join against the general-checks register. The register
(`GET /api/CheckManagement?charityId=…&chequeType=Orphans…`) remains the reconciliation view the
charity can eyeball against — do not join it into this projection.

### Coordination notes

- **10-24 (EP-10)** prints this same key from the payments workbench with the legacy
  `/export/pdf` shape; the JSON + client-render contract here supersedes that shape — reconcile
  in the Dev Agent Record if 10-24 landed first.
- `ChiqueNum` / print date / collector do not exist on `OrphanPaymentItem` as copied (verified);
  they land with 10-12. This story must not add them.

### Out of scope (later stories / other epics — do not build)

| Item | Story |
| --- | --- |
| Bank / date-range / cheque-type filtering (cheque statement) | 18-33 |
| Cheque statement document with per-currency totals | 18-33 |
| General cheque register CRUD and stationery | EP-11 (code already live in `CheckManagementController`) |
| Receipt cards document | 18-31 |
| Recording cheque numbers on rows (settlement) | 10-12 (EP-10) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.3] financial-reports row UC-RPT-30
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.30] scenario — params `charityId`,
  `orpCheckBatchNo`
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-30 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-10-orphan-payments-and-disbursement/10-12-record-cheque-number-date-and-collector.md]
  and 10-24 — where `ChiqueNum` lands and the sibling print story
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:505] `by-batch-no/{batchNo}`
  — the batch-resolution read this projection mirrors
- [Source: Backend/src/IIROSA.Api/Controllers/CheckManagementController.cs:42] `GET /api/CheckManagement`
  — the reconciliation register (not joined here)
- [Source: Backend/src/IIROSA.Application/DTOs/CheckManagement/Checks.cs] `CheckListDto` /
  `CheckFilterDto` — the register's wire shape (18-33 consumes it)

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Implementation pre-existed this session (user-implemented with the epic-18 batch; code-wins,
  architecture.md §10). This session's review: Tasks 1–5 verified present against the code;
  `dotnet build Backend/IIROSA.sln` → 0 errors (325 pre-existing warnings, none in the report
  code); `npm run build` → exit 0 (`NODE_TLS_REJECT_UNAUTHORIZED=0` needed for the Google-Fonts
  inline step behind the corporate proxy — bundle generation itself is network-free; the two
  "Error occurs in the template of ReportNumbersComponent" lines ride pre-existing NG8107
  optional-chain warnings in `periodic-orphan-reports`, not epic-18 code).
- Live-check matrix (anon 401 / charity pin / HQ narrow / empty-batch message): deferred to the
  epic-18 consolidated smoke at the end of the 18-30…18-41 batch — recorded here when run.

### Completion Notes List

- **Data-source ruling honoured as written**: rows come from the batch's payment items with
  `ChiqueNum` recorded (`!IsDeleted && ChiqueNum != null && != ""`), joined to orphan identity +
  guardian (`Orphan.Family.Provider.FullName`); NO join against the `Check` register.
- **Scope ladder**: the 18-22/24 inline ladder verbatim (charity claim pins → HQ
  `filter.CharityId` narrows → `CountryId` claim intersects charities of the country) — not a
  `ResolveCharityScope` call; same semantics in this codebase. Batch resolution by `BatchNo`
  string over live `OrphanPayment` groups ordered by `GroupDate`, mirroring 18-29.
- **Print shape**: 18-21's decide-once **browser print** (`ReportPdfService.printSheet`) — the
  story text's "jsPDF" phrasing is superseded by the recorded 18-21 ruling (jsPDF rejected: no
  Arabic shaping). Command lives on `orphan-payment-detail`; empty/no-cheque batch → info toast
  (`reports.chequeNumbers.nothingToPrint`), no sheet, no `window.print`.
- **Envelope**: raw `ChequeNumbersReportDto` (header + rows + `totalCount`/`totalAmount` +
  `message` on empty) — matches the story's "no `ApiResponse<T>`" rule; `chiqueNo` wire name per
  contract. Totals ride the meta band (`printSheet` has no totals row) — same as 18-29.

### File List

- Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs — `ChequeNumbersFilterDto`,
  `ChequeNumbersRowDto`, `ChequeNumbersReportDto`
- Backend/src/IIROSA.Application/Validators/Reports/ChequeNumbersFilterValidator.cs — NEW
- Backend/src/IIROSA.Application/Interfaces/IReportService.cs — `GetChequeNumbersAsync`
- Backend/src/IIROSA.Application/Services/ReportService.cs — `GetChequeNumbersAsync`
- Backend/src/IIROSA.Api/Controllers/ReportsController.cs — `POST /api/Reports/cheque-numbers`
- Frontend/src/app/modules/reports/models/report.model.ts — cheque-numbers types
- Frontend/src/app/modules/reports/services/report.service.ts — `getChequeNumbers`
- Frontend/src/app/modules/orphan-payments/orphan-payment-detail/orphan-payment-detail.component.ts · .html — the أرقام الشيكات command
- Frontend/src/assets/i18n/ar.json, en.json — `reports.chequeNumbers.*`

### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

- Anonymous `POST /api/Reports/cheque-numbers` → 401.
- Charity token → only its own register's cheque numbers; HQ + explicit `charityId` → that charity's rows.
- Batch with no cheques → empty list + Arabic message (payloads from UTF-8 files, not inline curl).
- Task 6 header checked now that every sub-box is green.
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-30 and module spec §23.3 / §23.U.30; data-source ruling (payment-row `ChiqueNum`, no Check↔OrphanPayment FK) verified against the codebase. |
| 2026-08-25 | Review pass over the pre-existing implementation: Tasks 1–5 verified + builds green; browser-print supersession and scope-ladder notes recorded; live-check deferred to the consolidated epic smoke. Status → in-progress pending that smoke. |
| 2026-08-25 | Live smoke passed (auth gates, charity narrowing, empty-batch message). All boxes green; status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
