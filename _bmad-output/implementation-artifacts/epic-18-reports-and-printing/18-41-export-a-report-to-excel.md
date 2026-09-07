# Story 18-41: Export a report to Excel تصدير إلى إكسل

| Field | Value |
| --- | --- |
| Story key | `18-41-export-a-report-to-excel` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-41 — تصدير إلى إكسل |
| Priority / size | Could · 8 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.41 scenario — generic capability, no dedicated §23.S screen) |
| Route | capability inside `report-viewer` (the استخراج البيانات command) — no new route |
| Endpoint | none NEW — each report's existing JSON endpoint feeds the export; the legacy EPPlus + JSZip/FileSaver server pipeline is superseded — recorded deviation |
| Depends on | **18-1 landed** (`services/report-export.service.ts`); every data-report story landed by the time this runs (the audit sweeps them) |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to export a report to Excel تصدير إلى إكسل, so that the
data can be handed to the bank, the auditor or the donor in the format they expect.

## Acceptance Criteria

1. Given a signed-in user on any landed report screen, when the actor presses استخراج البيانات,
   then an `.xlsx` workbook is delivered via the browser save dialog. No stored data is changed.
2. Given the export runs, when the workbook opens, then the sheet is RTL (`rightToLeft = true`),
   the header row is styled and frozen, column headers come from the report's i18n keys, dates
   and numbers carry Excel-native formats, and the file is named
   `<reportKey>-<yyyyMMdd>.xlsx`.
3. Given the report's current filter returns no row, when the export is invoked, then the actor is
   told there is nothing to produce rather than receiving an empty file.
4. Given the export covers a multi-page result, when the actor exports, then the workbook fetches
   ALL matching pages (respecting the service's page-size cap) — never just the visible page
   without the actor knowing; if a cap truncates the extract, the workbook's manifest states the
   truncation (no silent caps).
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.
6. Given ANY landed report screen, when its استخراج command runs, then the SAME export engine
   serves it through that report's registered column set — zero per-component ExcelJS
   re-implementations remain in the reports module after this story's audit.

**Definition of done:** §23.U.41 passes end to end — every registered report exports through one
engine with one column-set registry; the audit (Task 4) lists every report key and confirms the
single code path; the seasonal-aid module's pre-existing ExcelJS usage is noted as precedent and
left alone (out of scope).

## Capability contract (the epic-wide Excel engine)

- `services/report-export.service.ts` (landed by 18-1 as a workbook builder) is hardened into the
  engine:
  - **Column-set registry**: map report key → ordered column definitions
    (`{ key, i18nLabel, type: text|number|date|boolean, width? }`) — the single place a report's
    export shape is declared.
  - **Sheet contract**: RTL sheet, styled + frozen header row, typed cells (dates as dates,
    numbers as numbers), autofilter on the header, footer row with total count; filename
    `<reportKey>-<yyyyMMdd>.xlsx` via file-saver.
  - **Paging contract**: an `exportAll(filter, fetchPage)` helper that walks `page … totalPages`
    under the cap and appends; truncation (if hit) is written into a manifest sheet.
- Reports call the engine with their filter + column set; the engine owns ExcelJS entirely —
  report components never import ExcelJS.

## Tasks / Subtasks

- [x] **Task 1 — Column-set registry + types** (AC 2, 6)
  - [x] Column-definition type + registry in `Frontend/src/app/modules/reports/models/
        report.models.ts` (or a sibling `report-columns.ts`); seed it with the column sets of
        every report landed so far (18-1's 44-column orphan set, 18-3/18-4/18-5's grids, 18-6's
        22 columns, 18-7/18-8's meza set, the landed 18-9…18-23 grids, 18-24/18-25's manifest
        sheets) — each keyed by its report key
- [x] **Task 2 — Engine hardening** (AC 1, 2, 4)
  - [x] Upgrade `services/report-export.service.ts` to the Sheet contract above (RTL, freeze,
        typed cells, autofilter, footer, filename) and add the `exportAll` paging walker with
        the truncation manifest; keep 18-24/18-25's image-manifest builder as a registered
        special-case producer (images are NOT grid columns — they keep their own sheet builder
        behind the same registry entry)
- [x] **Task 3 — Wire the shell command** (AC 1, 3, 6)
  - [x] The `report-viewer` shell's استخراج البيانات command calls the engine with the active
        report's registry entry + current filter; reports without a registered column set hide
        the command (honest absence); empty result → nothing-to-produce message, no file
- [x] **Task 4 — Anti-reinvention audit** (AC 6)
  - [x] Grep the reports module for direct `exceljs` imports outside `report-export.service.ts`
        and for hand-rolled workbook loops in report components; fold every hit into the engine
        and list the folded files in the Dev Agent Record
  - [x] Note (do NOT refactor): the seasonal-aid module's own ExcelJS usage
        (`campaign-list`/`campaign-report`) predates this epic and stays as-is — recorded as
        precedent; a unification pass is a maintenance backlog item, not this story
- [x] **Task 5 — i18n** — any engine-level strings (manifest sheet title, truncation notice,
      nothing-to-produce, toasts) under `reports.export.*` in **both** `ar.json` and `en.json`;
      column headers resolve through each report's existing keys
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] Export two contrasting reports (widest = 18-1's 44 columns; a date-typed one): RTL sheet,
        frozen styled header, typed cells, autofilter, correct filename; a multi-page filter
        exports every page (row count = `totalCount`) or the manifest declares the truncation
  - [x] Empty filter → message, no file; 401 path → login redirect
  - [x] `cd Frontend && npm run build` green (ng-serve stale-bundle grep caveat); backend
        untouched — `dotnet build` a formality; tests excluded per the standing user decision

## Dev Notes

### Supersession (recorded)

The legacy realisation is server-side EPPlus + the bundled `Scripts/FileSaver.js` +
`node_modules/jszip` — a server worksheet writer streaming bytes. The platform supersedes it
(recorded deviation, consistent with the epic-wide ruling and 9-x's export precedent): exports
are client-side ExcelJS workbooks from each report's JSON endpoint. No EPPlus enters the backend
(no licence burden), no server byte-streaming endpoints are added. The 8-point size is the audit
sweep + registry + engine hardening, not new surface area.

### Platform rules that bind this story

- No new backend endpoint, no migration — frontend capability only.
- ExcelJS + file-saver (both already in `package.json`); the engine is the ONLY ExcelJS importer
  in the reports module after the audit.
- i18n in both languages; lazy module; the command lives in the shared shell so every report
  inherits it (AC 6 is the point of this story).
- No silent caps: any truncation is declared (§23.U.41's "handed to the bank/auditor/donor" is a
  completeness-sensitive use).

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| PDF rendering / preview | 18-21 / 18-40 |
| The individual reports' data endpoints | their own stories |
| Seasonal-aid ExcelJS unification | maintenance backlog — recorded, not built |
| CSV export | none — backlog if asked |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.41] scenario — grid export for
  offline analysis; legacy EPPlus/JSZip/FileSaver realisation
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-41 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] the export service this
  story hardens into the engine + the 44-column seed set
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-24-export-orphan-photographs.md] the
  image-manifest special case that stays behind the registry
- [Source: Frontend/src/app/modules/seasonal-aid/] the pre-existing ExcelJS precedent noted in
  Task 4

## Dev Agent Record

### Agent Model Used

Claude (GLM-5 via Claude Code) — 2026-08-25.

### Debug Log References

- Frontend build: `/tmp/fe-build-1841.log` — exit 0, zero `Error:` lines. (One intermediate red build:
  `firstValueFrom` imported from `rxjs/operators` in cheque-statement — moved to `rxjs`, rebuilt green.)
- Backend formality build: `/tmp/be-build-1841.log` — exit 0, `0 Error(s)` (backend untouched by this story).
- i18n splice verification: node parse of both locales — `reports.export` (7 keys) sits directly after the
  `preview` anchor, `reports.chequeStatement.colCurrency` present, `reports.nothingToExport` intact.

### Completion Notes List

**Registry + engine (Tasks 1–2).** The column-set registry landed as the sibling file the story allowed:
`models/report-columns.ts` (not inside `report.models.ts` — keeps the model file free of export shape).
28 report keys registered; the serial column is ENGINE-owned (`#`, index+1) so sets never declare it.
`report-export.service.ts` is fully rewritten around two builders — `exportRows` (one payload) and
`exportAll` (the paging walker: walks every page under the endpoint's validator cap, default pageSize 100,
maxRows 5000, manifest sheet on ANY truncation: short delivery, cap hit, or maxRows). Sheet contract per AC 2:
RTL + frozen header, bold grey header, typed cells (number → JS number, date → JS Date with `dd/mm/yyyy`
/ `dd/mm/yyyy hh:mm`, boolean → `✓`/`''` or translated `booleanLabels`), autofilter over data rows only
(footer excluded — the total is never filtered), bold footer total row, filename `<reportKey>-<yyyyMMdd>.xlsx`
via file-saver. file-saver 2.x ships no typings → minimal ambient `Frontend/src/declarations.d.ts`
(the engine is its only consumer). The 15 existing per-report wrappers are preserved as thin `exportRows`
delegates; `exportOrphanFiles` (18-24/25 image manifest — `specialCase: 'image-manifest'`, empty columns)
and `exportCampaignReport` (seasonal-aid precedent) keep their own builders behind the registry.

**Shell wiring (Task 3).** Same degeneracy ruling as 18-40's `[previewProducer]`, recorded there: the shell
cannot know each screen's filter, so the binding is a PRODUCER — `[exportRequest]="buildExportRequest"`
returning `ReportExportRequest | null`, closing over the live filter. Seven screens ride the engine through
it: cheque-statement (cap 200, CheckManagement register precedent), new-beneficiaries (4 variant keys),
widows-sponsorship (cap 200), follow-up-sheets (3 variant keys), missing-outgoing-attachments (cap 100),
family-orphans-by-date (async fetchPage flattens family pages into repeated-cell rows), and family-follow-up
(folded below — hosts its own button, pre-18-1 shell screen). The shell's `runExport()` runs the engine when
a producer is bound (`engineExporting` flag, spinner, `reports.export.failed` toast on throw,
`reports.nothingToExport` info on zero rows → no file, AC 3) and falls back to the legacy `(export)` emit for
the 16 screens already engine-served through their per-report wrappers — AC 6 held either way. The five
newly-registered screens dropped `[showExport]="false"`; unregistered screens KEEP it (honest absence).

**Audit result (Task 4).** `grep exceljs` over the reports module now matches exactly ONE file —
`services/report-export.service.ts`. Folded: ONE file, `family-follow-up-report/family-follow-up-report.component.ts`
— its inline `import('exceljs')` block (hand-rolled workbook loop + anchor download) replaced by
`exportAll('family-follow-up', …)` walking the families follow-up endpoint instead of exporting just the
loaded grid page (AC 4 applied to a pre-shell screen). The 10 remaining `exceljs` hits in `src/app` are OUTSIDE
the reports module — periodic-orphan-reports ×6, incoming-outgoing ×3, seasonal-aid ×1 (`campaign-list`) —
noted as precedent and left alone per the story's own out-of-scope table (unification = maintenance backlog).

**Scope rulings (verified against story files, not comments).** 18-28 (orphans-without-payment) and 18-13
(family-orphans entries screens) list no export in their contracts → export stays hidden. 18-30's cheque
numbers are the print command in orphan-payments/orphan-payment-detail → no export there. 18-34's
distribution sheets live on seasonal-aid's distribution-record → fenced off by THIS story's out-of-scope
table (seasonal-aid stays as-is) → export deferred, not built here.

**Deviations & decisions (recorded).** (a) 18-39 flattening: in Excel the family cells REPEAT on every orphan
row — Excel has no rowspan and the autofilter demands consistent cells; print keeps its group-first banding.
(b) Cheque statement gains a NEW currency column (`reports.chequeStatement.colCurrency`) so amount exports
as a naked typed number (unambiguous, filterable) — the legacy sheet packed currency into the amount text.
(c) refused-reports state exports as `constantLabel` (every row is refused by definition). (d) missed-payments
appends one boolean column per batch, headers are the batch-number literals (translate of a missing key
returns the key — safe passthrough), values translated received/missed. (e) Filename convention
`<reportKey>-<yyyyMMdd>.xlsx` supersedes the old per-report literal names (e.g. `family_follow_up_*.xlsx`).

**Verification status.** Builds green (above). The two Task-6 live checks stay OPEN pending the consolidated
private-instance smoke of 18-30…18-41 (standing strategy: one smoke after all stories land —
`dotnet run --no-build --urls 127.0.0.1:60970`, user's live API untouched). Tests excluded per the standing
user decision.

### File List

- `Frontend/src/declarations.d.ts` — NEW: ambient `file-saver` module declaration
- `Frontend/src/app/modules/reports/models/report-columns.ts` — NEW: the column-set registry (28 keys) + engine request/result types + structural row shims
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — rewritten as the engine (`exportRows`/`exportAll`, sheet + manifest builders, file-saver download); 15 wrappers now delegate
- `Frontend/src/app/modules/reports/report-viewer/report-viewer.component.ts` / `.html` — `[exportRequest]` producer input, `runExport()` engine path, `engineExporting` state
- `Frontend/src/app/modules/reports/cheque-statement/cheque-statement.component.ts` / `.html` — `buildExportRequest()` (cap 200) + binding; `[showExport]="false"` dropped
- `Frontend/src/app/modules/reports/new-beneficiaries-report/new-beneficiaries-report.component.ts` / `.html` — variant→key map + `buildExportRequest()` + binding
- `Frontend/src/app/modules/reports/widows-sponsorship-report/widows-sponsorship-report.component.ts` / `.html` — `buildExportRequest()` (cap 200) + binding
- `Frontend/src/app/modules/reports/follow-up-sheets/follow-up-sheets.component.ts` / `.html` — variant→key map + `buildExportRequest()` + binding
- `Frontend/src/app/modules/reports/missing-outgoing-attachments/missing-outgoing-attachments.component.ts` / `.html` — `buildExportRequest()` (cap 100) + binding
- `Frontend/src/app/modules/reports/family-orphans-by-date-report/family-orphans-by-date-report.component.ts` / `.html` — `buildExportRequest()` with flattening fetchPage + binding
- `Frontend/src/app/modules/reports/family-follow-up-report/family-follow-up-report.component.ts` — audit fold: inline ExcelJS → `exportAll('family-follow-up', …)`
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.export.*` (7 keys) + `reports.chequeStatement.colCurrency`


### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

Both open live boxes evidenced in the browser (ng serve on 4299 + the private API, OsamaSuper):

- **Two contrasting exports captured as real downloads** (Playwright download events):
  `orphan-data-20260825.xlsx` (the widest set — 42 columns on the sheet) and
  `follow-up-orphans-20260825.xlsx` (the walker path through `[exportRequest]`, 8 columns).
  Filenames follow `<reportKey>-<yyyyMMdd>.xlsx`.
- **Sheet contract verified by opening both files with ExcelJS + raw sheet XML**:
  worksheet views `rightToLeft` in both; frozen header (`<pane ySplit="1" topLeftCell="A2"
  state="frozen"/>`); `<autoFilter ref="A1:AP11"/>` / `A1:H11` — the filter covers header + data
  rows ONLY, the footer total row (12) is excluded; engine serial column `#` as numbers; typed cells
  proven on populated rows (orphan-data: date of birth = JS Date with `dd/mm/yyyy` numFmt, age =
  number); bold footer `Total rows | 10` in both; data rows 10 = totalCount 10.
- **Multi-page clause**: no dev dataset exceeds one 100-row page (10 orphans / 2 families / 0
  outgoing letters), so "exports every page" is evidenced as row count = totalCount on single-page
  results; the truncation manifest path could NOT be exercised against live data (walker + manifest
  logic verified at build and code level). Recorded as a data-limited caveat, not silently claimed.
- **Empty filter → message, no file**: 0-row search disables استخراج (honest absence) AND the real
  producer returns null in that state — invoking the app's own `runExport()` on it toasts "There is
  no data to extract" (`reports.nothingToExport`) with a download watcher confirming NO file fired.
- **401 path**: corrupted access token → redirect to `#/auth/login` (verified in the 18-40 pass;
  the same interceptor/guard serves every export call).
- **Frontend build**: `npm run build` exit 0, zero errors, with the fix below in place
  (`fe-build-final.log`).

#### Defect found by the smoke and fixed in this pass (recorded)

The `[exportRequest]="buildExportRequest"` bindings carried the same unbound-`this` defect as
18-40's `[previewProducer]` (method reference → `this` = the viewer inside the producer → reads
`this.hasRun`/`this.totalCount` off the wrong instance → returned null → the engine never ran).
Fixed together with 18-40 by converting all 12 bound producers to arrow-function properties —
see 18-40's record for the full analysis. The export path was then re-verified live (both
downloads above went through the fixed bindings).
- `Frontend/src/app/modules/reports/cheque-statement/cheque-statement.component.ts` — smoke fix: `buildExportRequest` → arrow property (unbound-`this`, shared defect with 18-40)
- `Frontend/src/app/modules/reports/family-orphans-by-date-report/family-orphans-by-date-report.component.ts` — smoke fix: `buildExportRequest` → arrow property
- `Frontend/src/app/modules/reports/follow-up-sheets/follow-up-sheets.component.ts` — smoke fix: `buildExportRequest` → arrow property
- `Frontend/src/app/modules/reports/missing-outgoing-attachments/missing-outgoing-attachments.component.ts` — smoke fix: `buildExportRequest` → arrow property
- `Frontend/src/app/modules/reports/new-beneficiaries-report/new-beneficiaries-report.component.ts` — smoke fix: `buildExportRequest` → arrow property
- `Frontend/src/app/modules/reports/widows-sponsorship-report/widows-sponsorship-report.component.ts` — smoke fix: `buildExportRequest` → arrow property

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-41 and module spec §23.U.41; EPPlus/JSZip supersession, column-set registry, no-silent-caps rule and the anti-reinvention audit recorded. |
| 2026-08-25 | Implemented: registry (`report-columns.ts`, 28 keys), engine rewrite (`exportAll` walker + truncation manifest), shell `[exportRequest]` producer wiring (7 screens), audit fold (family-follow-up), i18n both locales. Builds green; live checks pending consolidated smoke. |
| 2026-08-25 | Consolidated live smoke passed: two real downloads inspected (RTL/frozen/typed/autofilter/footer, filename convention), empty-filter toast with no file, 401 redirect; multi-page manifest noted as data-limited. Export-producer this-binding defect fixed with 18-40. Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
