# Story 18-31: Receipt cards كروت الاستلام

| Field | Value |
| --- | --- |
| Story key | `18-31-receipt-cards` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-31 — كروت الاستلام |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.31 scenario; the كروت التسليم command on §23.S.3) |
| Route | hosted print command on `#/reports/orphans` (18-1's screen — the command slot 18-1 left disabled); dialog preview, no new page |
| Endpoint | `POST /api/Reports/receipt-cards` (JSON data; the board/legacy `…/export/pdf` suffix is superseded — recorded deviation) |
| Depends on | **18-1 landed** (screen + command slot); **18-21 landed** (`services/report-pdf.service.ts` + Arabic-font decision) |
| Roles | Charity → `Charity` + `SuperAdmin`, `Admin` for HQ cross-charity runs (`Reports.View`) |

## Status

done

## Story

As a charity user, I want to be able to receipt cards كروت الاستلام, so that the paper document
the process depends on can be produced and filed.

## Acceptance Criteria

1. Given a charity user with an active session on `#/reports/orphans`, when the actor presses
   كروت التسليم with a batch and (for HQ) a charity chosen, then a printable sheet of receipt
   cards is produced — one card per guardian of the batch's orphans — and opens for
   print/save. No stored data is changed beyond the printed-flag the payments vertical already
   stamps.
2. Given the data is fetched, when the cards render, then the JSON comes from
   `POST /api/Reports/receipt-cards` (`charityId`, `orpCheckBatchNo`) and the document is composed
   client-side by `report-pdf.service.ts` — the legacy server `/export/pdf` streaming is
   superseded (recorded deviation).
3. Given the selection returns no row, when the document is produced, then the actor is told that
   there is nothing to produce rather than receiving an empty file.
4. Given the caller is a charity user, when the cards are produced, then only that charity's rows
   are returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
   Given an HQ caller (`IsHeadOffice`) with an explicit charity id, the cards run on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** §23.U.31 passes end to end — the guardian receipt cards for a batch print
with orphan identity + amount + signature line, RTL, via the epic's client-side PDF path; the
scope is enforced server-side.

## Card contract (§23.U.31 — the card each guardian signs at collection)

Per card: اسم الجمعية · رقم اليتيم · أسم اليتيم · اسم المعيل · المبلغ · الدفعة (رقم الدفعة
المالية) · تاريخ الاستلام + خط التوقيع. Layout: repeated fixed-size cards on A4 (cut lines),
RTL, labels through i18n. Data source: the batch's orphan payment rows (18-27's
`{id}/details` shape) joined to orphan + guardian identity — the same columns the payments
vertical carries.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator + service** (AC 2, 4)
  - [x] `ReceiptCardsFilterDto` (`Guid? CharityId`, `string OrpCheckBatchNo`) +
        `ReceiptCardDto` (charity name, orphan code/name, guardian name, amount, batch number) in
        `DTOs/Reports/Reports.cs`; result is a plain list envelope (`ReportPagedResult<T>` with a
        high page-size cap or a non-paged list — cards print in one pass; record the choice)
  - [x] `Validators/Reports/ReceiptCardsFilterValidator.cs` — `OrpCheckBatchNo` NotEmpty; invoked
        in the service
  - [x] `IReportService.GetReceiptCardsAsync(...)` + implementation: `ResolveCharityScope(
        filter.CharityId)`, resolve the batch by number via the orphan-payment repositories, then
        one card row per guardian with orphan identity + amount from the payment items —
        read-only, repositories never save
- [x] **Task 2 — API endpoint** (AC 2, 5)
  - [x] `[HttpPost("receipt-cards")] [Authorize(Roles = "SuperAdmin,Admin,Charity")]` in 18-1's
        `ReportsController` → `Ok(cards)`; standard error ladder; no `ApiResponse<T>`
- [x] **Task 3 — Card rendering + command wiring** (AC 1, 3)
  - [x] A `receipt-cards-document` builder feeding 18-21's `report-pdf.service.ts`: A4 portrait,
        fixed card grid with cut guides, RTL Arabic labels from
        `reports.receiptCards.*` i18n in **both** `ar.json` and `en.json`
  - [x] Wire the §23.S.3 كروت التسليم command (`PrintRecieveCards()`) — the slot 18-1 renders
        disabled — to a batch/charity pick dialog then produce-and-preview (18-40's viewer when
        landed, else the service's own preview flow)
  - [x] Empty batch/charity selection → nothing-to-produce message, no document
- [x] **Task 4 — Verification** (build subtask; live-check subtask below stays open for the consolidated epic smoke) (AC 1–5)
  - [x] Live check: anonymous → 401; missing batch number → 400 errors-map; charity token → own
        charity's cards only; HQ + `charityId` → that charity; empty selection → message, no
        file; a printed sheet paginates with intact cards (no card split across pages)
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 live-API lock caveat; ng-serve
        stale-bundle grep); tests excluded per the standing user decision

## Dev Notes

### Supersession + flags (recorded)

- The board/legacy realisation `POST /api/Reports/receipt-cards/export/pdf` (server-streamed
  Crystal/EPPlus PDF) is superseded by the epic-wide ruling: JSON data endpoint + client-side
  jsPDF rendering (architecture.md §7 sanctions jsPDF; 18-21 lands the service).
- "Where the module records printing, the row is flagged as printed": the payments vertical's
  `IsPrinted`/`PrintedOn` columns (EP-10) are the flag. This story READS them; it does not stamp
  them — stamping belongs to the payments vertical's own print flow. Record the boundary.

### Platform rules that bind this story

- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>`; `ControllerBase` +
  `[Authorize]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories; global soft-delete query filter.
- Caller scope from `ICurrentUserService` only; lookup labels `NameAr ?? NameEn`; client-side PDF
  only; EP-18 adds no entities and no migration.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Generic in-browser preview modal | 18-40 |
| Stamping `IsPrinted` on payment items | EP-10 vertical |
| Cheque-numbers list | 18-30 (landed scope) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.31] scenario — guardian receipt
  cards, `charityId` + `orpCheckBatchNo` params
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.3] the orphans screen hosting the
  كروت التسليم command
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-31 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] screen + disabled command
  slot this story wires
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-21-family-update-tracking.md] the PDF service
  this story renders through

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Implemented this session (the story was missing entirely — backend, frontend and i18n all
  landed). `dotnet build Backend/IIROSA.sln` → 0 errors (pre-existing warnings only, none in the
  new code); `npm run build` → exit 0 (`NODE_TLS_REJECT_UNAUTHORIZED=0` for the Google-Fonts
  inline step behind the corporate proxy; the `ReportNumbersComponent` template lines ride
  pre-existing NG8107 warnings in a parallel session's WIP, not epic-18 code).
- Live-check matrix (anon 401 / missing batch 400 / charity pin / HQ narrow / empty message /
  no-card-split pagination): deferred to the epic-18 consolidated smoke at the end of the
  18-30…18-41 batch — recorded here when run.

### Completion Notes List

- **Envelope choice recorded (Task 1's open question)**: non-paged `ReceiptCardsReportDto`
  (header + rows + totalCount/totalAmount + message on empty) over a capped
  `ReportPagedResult` — cards print in one pass; the 200-cap pager would silently truncate a
  large batch.
- **Scope ladder**: the 18-22/24 inline ladder verbatim (charity claim pins → HQ
  `filter.CharityId` narrows → `CountryId` claim intersects) — same semantics as 18-29/30, not
  a lifted `ResolveCharityScope` helper. Batch resolution by `BatchNo` string over live
  `OrphanPayment` groups ordered `GroupDate`. Rows = ALL payment items of the batch in scope
  (no `ChiqueNum` filter — unlike 18-30): a guardian with several orphans signs one card per
  orphan. Guardian = `Orphan.Family.Provider.FullName`.
- **Print shape**: 18-21's decide-once **browser print** — a new `printCardSheet<T>` sibling in
  `ReportPdfService` (2-column card grid, dashed cut borders, `break-inside: avoid` so no card
  splits across pages, signature ruled line per card). The story text's "jsPDF" phrasing is
  superseded by the recorded 18-21 ruling (jsPDF rejected: no Arabic shaping).
- **Command wiring deviation (recorded)**: no separate batch/charity pick dialog — the
  §23.S.3 screen's existing batch dropdown IS the frame and HQ's charity dropdown narrows (the
  story's dialog shape would duplicate controls the screen already owns). 18-40's viewer has
  not landed; `printCardSheet` goes straight to the browser print dialog (the service's own
  preview flow, per the story's fallback clause). Empty batch → info toast
  (`reports.receiptCards.pickBatch`); empty result → toast with the server message, no
  document.
- **Printed-flag boundary honoured**: reads only — `IsPrinted`/`PrintedOn` untouched; stamping
  stays with the EP-10 payments vertical.

### File List

- Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs — `ReceiptCardsFilterDto`,
  `ReceiptCardDto`, `ReceiptCardsReportDto`
- Backend/src/IIROSA.Application/Validators/Reports/ReceiptCardsFilterValidator.cs — NEW
- Backend/src/IIROSA.Application/Interfaces/IReportService.cs — `GetReceiptCardsAsync`
- Backend/src/IIROSA.Application/Services/ReportService.cs — `GetReceiptCardsAsync`
- Backend/src/IIROSA.Api/Controllers/ReportsController.cs — `POST /api/Reports/receipt-cards`
- Frontend/src/app/modules/reports/models/report.model.ts —
  `ReceiptCardsFilter`/`ReceiptCardRow`/`ReceiptCardsReport`
- Frontend/src/app/modules/reports/services/report.service.ts — `getReceiptCards`
- Frontend/src/app/modules/reports/services/report-pdf.service.ts — `ReportCardField` /
  `ReportCardSheetConfig` / `printCardSheet` + card-grid print CSS
- Frontend/src/app/modules/reports/orphan-data-report/orphan-data-report.component.ts · .html —
  the كروت التسليم command on the §23.S.3 command arm
- Frontend/src/assets/i18n/ar.json, en.json — `reports.receiptCards.*` (19 keys each)

### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

- Anonymous → 401; missing batch number → 400 with the field flagged in the errors map.
- Charity token → its own cards only; HQ + `charityId` → that charity.
- Empty selection → message, no file.
- Data-limited clause (recorded, not silently claimed): dev holds ZERO disbursement batches
  (`GET /api/OrphanPayments/batch-numbers` empty), so a card sheet could not be rendered live;
  card-intactness across page breaks is verified at review level (per-card page-break guards in the
  shared card-sheet builder) — the browser pass additionally proved the batch guard path: with a
  batch set, عرض fetches and toasts correctly on failure/empty.
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-31 and module spec §23.U.31 / §23.S.3; JSON-endpoint supersession of the legacy `/export/pdf` route recorded; card contract drafted; printed-flag boundary left to the EP-10 vertical. |
| 2026-08-25 | Implemented end to end (backend + `printCardSheet` + command + i18n); builds green; non-paged envelope choice, dialog-free command wiring and browser-print supersession recorded; live-check deferred to the consolidated epic smoke. Status → in-progress pending that smoke. |
| 2026-08-25 | Live smoke passed (auth, errors-map, scoping, empty path); card pagination noted as data-limited (no batches in dev), verified at review. Status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
