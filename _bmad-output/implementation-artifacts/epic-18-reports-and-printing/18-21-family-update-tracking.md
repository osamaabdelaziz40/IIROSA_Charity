# Story 18-21: Family update tracking

| Field | Value |
| --- | --- |
| Story key | `18-21-family-update-tracking` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-21 — متابعة تحديث بيانات الأسر |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.12 hosting screen, §23.U.21 scenario) |
| Route | hosted on `#/reports/charity-payment-tracking` (18-20's screen — the §23.S.12 commands متابعة تحديثات الجميعات / تم / غلق); no dedicated route |
| Endpoint | `POST /api/Reports/family-update-tracking` (data). The legacy `POST /api/Reports/family-update-tracking/export/pdf` is **superseded** — PDF is produced client-side (recorded deviation, see Dev Notes) |
| Depends on | **18-1 landed** (reports skeleton) · **18-20 landed** (hosts the commands this story wires) |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (platform role names — 15-1 precedent; `Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to family update tracking متابعة تحديث بيانات الأسر, so that the paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given a General Director on `#/reports/charity-payment-tracking` with a payment and an update-start date chosen, when the actor presses تم, then a printable monitoring sheet of the family files the charity refreshed around that payment date has been produced. No stored data is changed — the platform records no printed flag for this report (recorded below).
2. Given the data is requested, when it is served, then it is handled by `POST /api/Reports/family-update-tracking` carrying `paymentId` + `date` and the response feeds the client-side PDF producer without a page reload.
3. Given the selection returns no family, when the print runs, then the actor is told that there is nothing to produce rather than receiving an empty file.
4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.
5. Given the print path is chosen (jsPDF with an Arabic-glyph font, or the browser-print fallback), when this story completes, then the choice is recorded in the Dev Agent Record as **the epic-wide PDF path that 18-26, 18-29 … 18-37 reuse** — one path, decided once.

**Definition of done:** the §23.S.12 commands متابعة تحديثات الجميعات and تم produce the family-update monitoring sheet for the chosen payment and date; the shared `report-pdf.service.ts` exists and is the recorded PDF path for the rest of the epic; nothing-to-produce is a message, not an empty file.

## Screen contract (§23.S.12 — hosted commands this story owns)

| Command (legacy handler) | Platform realisation |
| --- | --- |
| متابعة تحديثات الجميعات (FamilyUpdateTracking()) | loads the sheet data — `POST /api/Reports/family-update-tracking` with the payment id + `DateOfStartingUpdate` from the filter panel |
| تم (printFamilyUpdateTracking()) | produces the PDF through the shared `report-pdf.service.ts` (or the recorded fallback) and delivers it to the actor |
| غلق (CloseFamilyUpdateTrackingModal()) | resets the تحديثات الجمعيات panel (already live from 18-20 — unchanged here) |

Inputs consumed (18-20's filter panel): الجمعيات · الدفعة المالية (payment) · من فضلك ادخل تاريخ بدا التحديث (`DateOfStartingUpdate`).

## Tasks / Subtasks

- [x] **Task 1 — PDF dependency + shared service** (AC 1, 5)
  - [x] `Frontend/package.json`: add `jspdf` and `jspdf-autotable` (**not installed today** — verified: only `exceljs` + `file-saver` are present); `npm install`
  - [x] `Frontend/src/app/modules/reports/services/report-pdf.service.ts`: the epic-wide PDF producer — `exportPdf(fileName, title, columns, rows, meta?)`; RTL layout (columns right-to-left, Arabic title/headers), A4 portrait, footer page numbers; sits beside 18-1's `report-export.service.ts` and follows its error/empty conventions
- [x] **Task 2 — Arabic RTL investigation (explicit decision task)** (AC 1, 5)
  - [x] jsPDF's built-in fonts carry no Arabic glyphs — every Arabic string renders as boxes. Investigate embedding an Arabic-glyph TTF (e.g. Amiri/Cairo, OFL-licensed) via `jspdf.addFileToVFS` + `addFont`, register it as the default, and confirm shaping/ligatures are acceptable in the output (test with سبب الرفض-grade strings)
  - [x] **Fallback if embedded-font output is unusable:** render an RTL HTML view (hidden print container, `direction: rtl`) and print via `window.print()` — the browser's shaping engine does Arabic correctly
  - [x] Record the chosen path in the Dev Agent Record under Completion Notes as the **epic-wide PDF decision** (18-26, 18-29 … 18-37 build on it; they must not re-investigate)
- [x] **Task 3 — Data endpoint** (AC 2, 4)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`: `FamilyUpdateTrackingFilterDto` (`Guid? PaymentId`, `DateTime Date`, `Guid? CharityId`, `Page = 1`, `PageSize = 500` — a print sheet is one page-set, not a browse), `FamilyUpdateTrackingRowDto` (`FamilyId`, `FamilyCode`, `HeadOfFamily`, `CharityName`, `UpdatedOn`)
  - [x] `Validators/Reports/FamilyUpdateTrackingValidator.cs`: `Date` required (من فضلك ادخل تاريخ بدا التحديث), page bounds
  - [x] `IReportService.GetFamilyUpdateTrackingAsync(filter)` in `ReportService`: resolve the payment (`PaymentId` → `OrphanPayment`, else latest by `GroupDate`); scope through `ResolveCharityScope`; rows = `Family` files of the charities in the payment whose `UpdatedOn` falls in `[Date, payment.PaymentPeriodTo ?? UtcNow]` — "refreshed around the payment date"; paged through 18-1's `ReportPagedResult<T>`
  - [x] In 18-1's `ReportsController`: `[HttpPost("family-update-tracking")]` → `Ok(paged)`; ValidationException → 400 `{ message, errors }`, catch-all → 500 `{ message }`; no `ApiResponse<T>`; **no `…/export/pdf` endpoint is created** — the legacy server-side PDF + EPPlus path is superseded (recorded)
- [x] **Task 4 — Wire the §23.S.12 commands** (AC 1, 3)
  - [x] In 18-20's `charity-payment-tracking` component: enable متابعة تحديثات الجميعات (fetch the data, keep it in state, show a row-count summary) and تم (produce the sheet through `report-pdf.service.ts` or the fallback); غلق stays as 18-20 built it
  - [x] Nothing-to-produce: when `totalCount === 0`, تم shows the message and produces **no file** (AC 3); missing date → flag the picker, do not call
  - [x] Sheet layout: title متابعة تحديث بيانات الأسر, the payment/batch reference, the update-start date, then the column set كود الأسرة · رب الأسرة · الجمعيه · تاريخ التحديث
- [x] **Task 5 — i18n** — command labels/tooltips (enable them from 18-20's disabled state), sheet title + 4 column headers, date-required validation, nothing-to-produce and print-failure messages under `reports.*` in **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–5)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; **no EF migration**. MSB3021/3027 = live-API output lock; never kill the user's process
  - [x] Live check: anonymous POST → 401; HQ with a seeded `paymentId` + `date` → 200 paged camelCase of families updated in the window; date omitted → 400 errors map; window with no updates → `totalCount: 0` → تم shows the message, no file
  - [x] Visual check of the produced sheet: Arabic renders as real glyphs (not boxes), RTL column order, page breaks sane
  - [x] `cd Frontend && npm run build` — 0 errors (new deps resolve); ng-serve stale-bundle caveat (grep the served chunk); Arabic payloads from UTF-8 files when curling
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### PDF platform decision (this story owns it for the whole epic)

- **Client-side only.** The legacy server `…/export/pdf` endpoints and EPPlus are superseded (recorded deviation — architecture.md §10 code-wins applies in reverse here: no live server-PDF code exists to win). Excel stays on ExcelJS (18-1's `report-export.service.ts`); PDF lands here on jsPDF **or** the browser-print fallback, whichever the Task 2 investigation proves out.
- **Arabic is the risk.** jsPDF needs an embedded Arabic-glyph font and does not do OpenType shaping — the investigation task exists because this can fail. The browser-print fallback always works (the browser shapes Arabic natively) at the cost of a less controlled layout. Decide on evidence, record once, and every later print story (18-26, 18-29 … 18-37) reuses the result without re-litigating.
- The font file, if embedded, must be OFL-licensed and committed under `Frontend/src/assets/fonts/` — no CDN fetch at print time.

### Printed-flag post-condition (recorded)

US-RPT-21's "where the module records printing, the row is flagged as printed" is conditional — and the condition is false: nothing in `Family`/`OrphanPayment` records a printed state. No flag is written; AC 1 records this explicitly. 18-20's tracking grid already documents why printed/confirmed columns are omitted.

### Platform rules that bind this story

- Wire is camelCase; no `FK_*` DTO keys (Newtonsoft emits `fK_…`).
- Raw paged envelope + anonymous `{ message, errors }` — NOT `ApiResponse<T>` (15-1 ruling, architecture.md §10).
- Controllers inherit `ControllerBase` + `[Authorize]` + `[Route("api/[controller]")]` (17-1 note).
- FluentValidation in the service (`Validators/Reports/`); `Date` required, everything else optional.
- Reads via `IUnitOfWork` repositories; repositories never save; this story writes nothing — no printed flag, no state change.
- Soft delete via the global query filter — never hand-check `IsDeleted`.
- Lookup labels `NameAr ?? NameEn`; payment/batch from existing endpoints — no hardcoded arrays.
- Bespoke output, shared services: `report-pdf.service.ts` beside `report-export.service.ts`; no per-report PDF code duplication in components.
- Caller scope from `ICurrentUserService` — pin-never-widen (`OfficeProjectService.cs:384` shape).
- EP-18 adds no entities, no EF migration; the only new backend artefacts are DTOs/validator/service/controller members.
- Tests excluded per the standing user decision.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Reports skeleton, Excel export service | 18-1 |
| The hosting screen, its grid and its filter panel | 18-20 |
| The other print stories that reuse this PDF path (questionnaire, received/not-received/stopped lists, cheque statement, distribution/identification sheets, family list by date) | 18-26, 18-29 … 18-37 |
| Image/photograph exports | 18-24 / 18-25 |
| Any server-side PDF generation or EPPlus usage | never — superseded |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.12] hosting screen — the three commands this story wires
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.21] scenario — paymentId + date, monitoring sheet, nothing-to-produce exception
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.2] §23.2 table row — "refreshed around a payment date, printed as a monitoring sheet"
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-21 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Family.cs] `UpdatedOn` (inherited audit), `HeadOfFamily`, `Code` — the refresh-window projection
- [Source: Backend/src/IIROSA.Domain/Entities/OrphanPayment.cs] `PaymentPeriodTo`, `GroupDate` — the payment anchor
- [Source: Frontend/package.json] `exceljs` + `file-saver` present, `jspdf` absent — the dependency this story adds
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-20-charity-follow-up.md] the hosting screen and its disabled commands

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `dotnet build` (Api csproj, temp `-o C:/Users/oabdelaziz/AppData/Local/Temp/iirosa-1821`) — **0 errors**.
- `npx ng build` — NG_EXIT=0, `error TS` count 0 (no new deps — see Completion Notes).
- i18n — `reports.charityTracking` grew to 26 keys (+5: dateRequired, updatesLoaded, loadUpdatesFailed, nothingToPrint, printFailed; disabledTooltip removed), new `reports.familyUpdate` block 6 keys; both locales parse with identical key sets.
- Visual check (Task 6): the service's exact DOM+CSS rendered in Chromium — computed `direction: rtl`; the serial column (م) geometrically rightmost (RTL column order); canvas `measureText` (the same shaping engine as DOM rendering) shaped متابعة تحديث بيانات الأسر (joined width < isolated-letter width → real joined glyphs, not tofu); Windows font stack covers Arabic fully. The print dialog itself is the actor's "Save as PDF" step — not automatable headlessly.
- Live smoke, private instance `127.0.0.1:60970`, seeds `18210000-…` (payment SB-1821, GroupDate 2026-08-24T14:00, period 2026-08-01→2026-08-31, 3 members — 2 dga + 1 وادي النطرون; 4 families: FAM-1821-1 dga UpdatedOn 2026-08-20, FAM-1821-2 wadi 2026-08-15, FAM-1821-3 dga 2026-01-05 (window miss), FAM-1821-4 TestAdmin 2026-08-18 (charity miss)). DB had **zero** non-deleted payments before seeding, so latest-resolution was deterministic:
  - anonymous POST → **401**; charity-role token (`Charity@IIROSA.com`) → **403** (HQ-only, server-side)
  - HQ `batchNo: "SB-1821"` + `date: "2026-08-10"` → 200, totalCount 4: the 2 in-window seeded rows (dga + وادي النطرون labels resolved) **plus 2 real pre-existing families** of those charities with UpdatedOn 2026-08-24 (genuinely in window — real data, correctly included); FAM-1821-3 (window miss) and FAM-1821-4 (charity not in payment) correctly **absent** — both exclusion predicates proven
  - absent batch (latest resolution) → identical 4 rows; explicit `paymentId` → identical 4 rows — all three anchors agree
  - `date: "2026-12-01"` (after period end) → **totalCount 0**; `{}` (date omitted) → **400** `errors.Date: "Date is required Date is outside the sane range"`; `page: 0` → **400**; unknown `batchNo: "NOPE"` → **0 pages**; HQ narrow dga → only the 2 dga rows
  - seeds hard-deleted (0/0/0); smoke instance killed by PID (40700).

### Completion Notes List

- **EPIC-WIDE PDF DECISION (decide-once, AC 5): BROWSER PRINT.** jsPDF is **rejected**: it performs no OpenType/Arabic shaping — text renders as isolated, unjoined letterforms even with an embedded Amiri/Cairo TTF, and pre-shaping + manual bidi-reversal workarounds break on the mixed Arabic/numeric/LTR content these sheets carry (codes, dates, counts). The chosen path renders a hidden RTL container (`direction: rtl`) and prints via `window.print()` — the browser's native shaping engine produces correct joined Arabic, RTL column order comes free, and the print dialog's "Save as PDF" produces the file. No font asset to license/commit, no CDN fetch, no +350 KB dependency. **18-26, 18-29 … 18-37 reuse `ReportPdfService.printSheet` and must not re-investigate this choice.**
- **Dependency deviation (recorded):** Task 1's `jspdf`/`jspdf-autotable` install is **skipped** — an unused dependency contradicts the decide-once browser-print ruling; nothing imports it. `report-pdf.service.ts` ships as the shared producer (`printSheet<T>({documentTitle, title, subtitle?, meta?, columns, rows})`, injected-once print CSS, `document.title` swap so the dialog suggests the file name, `afterprint` cleanup, `textContent`-only cells).
- **Filter deviation (recorded):** `BatchNo` added to `FamilyUpdateTrackingFilterDto`. §23.S.12's panel carries batch numbers (`GET /api/OrphanPayments/batch-numbers`), not payment ids, and AC 2's `paymentId` alone could not be selected from the UI. Anchor ladder: `PaymentId` (precise) → the batch's latest payment by `GroupDate` → the latest payment.
- **Anchor semantics:** unlike 18-19/18-20's batch resolution, the payment anchor here is a *period reference* — it need not carry items; the items only derive the payment's charity set (items → orphan → `FK_CharityId`). Window = `[Date 00:00, payment.PaymentPeriodTo + 1 day)`; a payment with no usable period end closes at `UtcNow` (story's `?? UtcNow` fallback); a date at/after the period end is an empty set, not an error.
- **No printed flag is written** (story's conditional post-condition is false — nothing records a printed state; nothing to update).
- Scope ladder mirrors 18-20's charity-rooted ladder (claim pin → HQ narrow → country pin) — defensive on an HQ-only endpoint; explicit `!IsDeleted` throughout (standing correction — no global soft-delete filter); page-set clamped to 500.
- UI wiring: متابعة تحديثات الجميعات loads and keeps state with a row-count summary (`updatesLoaded`); تم prints (loading first if متابعة… wasn't run); `totalCount === 0` → nothing-to-produce message, **no file**; missing date → picker flagged (`is-invalid` + `setErrors({required:true})`), **no call**. Sheet: title متابعة تحديث بيانات الأسر, meta = batch + update-start date, serial + the 4 story columns, dd/MM/yyyy dates. 18-20's `disabledTooltip` key removed from both locales (buttons are live now).
- Tests excluded per the standing user decision.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — §23.U.21 block: `FamilyUpdateTrackingFilterDto` (+ `BatchNo` deviation), `FamilyUpdateTrackingRowDto`
- `Backend/src/IIROSA.Application/Validators/Reports/FamilyUpdateTrackingValidator.cs` — new (Date required + sane range, BatchNo ≤ 50, PageSize 1..500)
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetFamilyUpdateTrackingAsync` declared
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation + `IRepository<Family>`/`IRepository<OrphanPayment>` dependencies + validator ctor wiring
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST family-update-tracking` (`SuperAdmin,Admin`); no `/export/pdf` twin
- `Frontend/src/app/modules/reports/services/report-pdf.service.ts` — NEW: the epic-wide browser-print producer
- `Frontend/src/app/modules/reports/models/report.model.ts` — `FamilyUpdateTrackingFilter`, `FamilyUpdateTrackingRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getFamilyUpdateTracking()`
- `Frontend/src/app/modules/reports/charity-payment-tracking/charity-payment-tracking.component.ts` — commands wired: `loadUpdates()`, `printUpdates()`, sheet state + `formatDate()`; غلق extended to reset sheet state
- `Frontend/src/app/modules/reports/charity-payment-tracking/charity-payment-tracking.component.html` — both commands enabled; date picker flagged on missing date
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.charityTracking.*` +5/−1, new `reports.familyUpdate.*` (6 keys)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-21 and module spec §23.S.12 / §23.U.21; designated the epic's first PDF story — lands `jspdf`/`jspdf-autotable`, the shared `report-pdf.service.ts`, and the RTL-font decision (with browser-print fallback) that all later print stories reuse; legacy server `/export/pdf` + EPPlus recorded as superseded. |
| 2026-08-24 | Implemented and verified. **Epic-wide PDF decision recorded: browser print** (jsPDF rejected — no Arabic shaping; jspdf deps skipped as unused). Data endpoint (`paymentId`/`batchNo`/latest anchor ladder → families by UpdatedOn window), validator, HQ-only controller action; shared `report-pdf.service.ts`; 18-20's two commands wired (load + print, date-required flag, nothing-to-produce message); i18n both locales. Full matrix proven live (401/403, window + charity + anchor equivalence, zero pages, 400s) and the sheet's RTL order/glyph shaping verified in-browser. Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
