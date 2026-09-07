# Epic-18 review — chunk 4 (screens 18-20…18-41 + companion files) — Blind Hunter findings

- **Injected print stylesheet blanks every other print in the app** — `reports/services/report-pdf.service.ts` (`ensureStyles` + `printStyles`) permanently injects a `<style>` whose `@media print` block is `body > *:not(.report-print-sheet):not(.report-print-form):not(.report-print-cards) { display: none !important; }`. After any report print/preview runs once, printing any other screen (Ctrl+P / Save-as-PDF anywhere in the app) hides the entire body and yields blank pages; `afterprint` cleanup only removes the sheet and restores the title, never the stylesheet.

- **`family-follow-up-report` enables OnPush but never calls `markForCheck`** — sets `ChangeDetectionStrategy.OnPush` yet has no `ChangeDetectorRef`: async `run()`/`loadCharityOptions()`/print callbacks mutate `loading`, rows and options the view will never re-render. No `OnDestroy`/`destroy$` either — every subscription leaks.

- **The `family-follow-up-report` spec tests a component that does not exist** — spec sets `component.familyCode = ' FAM-9 '` and calls `printIdentificationSheets('guardians')`, asserting `toHaveBeenCalledWith('guardian-identification-sheets', { charityId: 'charity-1', familyCode: 'FAM-9' })`. The shipped class exposes `familyFilter`, `printIdentificationSheet` (singular) taking `familyId`, and no `familyCode` — the spec fails to compile/run against the component.

- **Recursive exporters page with a stale `totalCount` while re-reading the live filter** — every `fetchNext` engine computes `totalPages = Math.ceil(this.totalCount / exportPageSize)` from the *last search's* count but each page re-calls `buildFilter()` reading the *current* form values, so editing a filter after searching exports a silently truncated (or padded) workbook; most also lack a `hasRun` guard. Affected (14): `beneficiary-family-details`, `excluded-orphans-report`, `finished-sponsorship-report`, `missed-payments`, `charity-payment-tracking`, `batch-non-renewed-reports`, `orphan-data-report`, `orphan-files`, `orphans-missing-files`, `orphans-missing-reports`, `provider-sponsor-changes`, `refused-reports`, `registered-family-projects`, `reports-awaiting-approval`.

- **Printed/previewed documents cover only the loaded grid page while meta claims the whole selection** — `follow-up-sheets.printSheetCommand()`, `new-beneficiaries-report.printReport()`, `family-orphans-by-date-report.printSheetCommand()`, `cheque-statement.printStatement()` build sheets from `this.rows` (current page) yet print `totalCount` in the meta band — misleading partial documents, no truncation warning (only `family-follow-up-report` handles `payload.truncated`). `charity-payment-tracking.loadUpdates()` same defect in data form (one fetch at 500; >500 updates silently vanish).

- **`family-orphans-by-date` export feeds the engine mismatched paging metadata** — `buildExportRequest.fetchPage` returns flattened orphan `items` but the parent `totalCount` (family count), so the engine's page math is incoherent and the walked export cannot be trusted to collect the full set.

- **`charity-payment-tracking` ignores its own "HQ-only" rule** — calls `loadCharities()` unconditionally in `ngOnInit` and renders the charity dropdown without the `*ngIf="isHQ"` guard every sibling uses; injected `authService` is dead code. Non-HQ callers get a filter the rest of the epic hides.

- **`orphan-payment-detail` swallows every non-404 load failure into a blank screen** — error handler is `this.notFound = err?.status === 404;` with no else: a 500/403/timeout leaves `notFound = false`, no rows, no notification, no error state.

- **`orphan-payment-detail` row writes bypass the lock the delete action respects** — `onToggleStop`/`onMarkPrinted` buttons not gated by `canModify()` while the delete control is — users can toggle stop / mark printed on uploaded/locked batches.

- **`missed-payments` builds batch columns from the visible page only and sorts them lexicographically** — `batchColumns` is the union of `row.batchStates` keys across the current 20 rows, so the column set changes when paging (and differs from the export's all-pages union); `sort((a,b) => b.localeCompare(a))` orders "9" before "10". The `reason` column permanently renders '—'.

- **Variant switching keeps stale, wrong-shaped rows** — `follow-up-sheets.selectVariant()` and `new-beneficiaries-report.selectVariant()` swap `this.variant` (column set) without refetching or clearing `rows`, so the family/widow grid renders orphan-shaped data under mismatched headers until the user re-runs بحث; no warning.

- **Bootstrap 4 badge classes in a Bootstrap 5.3 codebase** — `orphans-without-payment-report.component.html` uses `badge badge-danger` / `badge badge-secondary` (removed in BS5, replaced by `text-bg-*`); sibling `refused-reports.component.html` correctly uses `text-bg-danger`. Gap-state badges render unstyled grey pills.

- **Whole columns ship permanently blank "by contract"** — `provider-sponsor-changes` (3 always-null prior-guardian columns — a *change report* showing no changes), `widows-sponsorship-report` (`mobileNumber2`, `developmentProject`), `reports-awaiting-approval` (`refuseReason`), `missed-payments` (`reason`), `missing-outgoing-attachments` (constant-0 count), `registered-family-projects` (`yearsOfExperience`, `hasExperience`). Users get a grid of em-dashes presented as a report.

- **`registered-family-projects` is a screen that can never show data** — class comment states the row source has no domain link ("empty with a recorded gap"), yet the full 14-column grid, charity filter, pager and export engine all ship around a permanently empty endpoint.

- **Filter dropdowns silently truncate their option lists** — `loadCharities()` single `pageSize: 500` call across all report screens; `orphans-without-payment-report.loadBatches()` caps batches at 100 with no paging: past those counts, charities/batches are unfilterable.

- **`orphans-without-payment` claims the query-param pre-selection waits for options but doesn't** — `ngOnInit` immediately `setValue`s on every `queryParamMap` emission with zero coordination with async `loadCharities()`; a late dropdown reset clobbers the pre-selection; re-fires on any later param change.

- **Synchronous `printing` flags can never paint** — `survey-questionnaire.print()`, `cheque-statement.printStatement()`, `follow-up-sheets.printSheetCommand()`, `new-beneficiaries-report.printReport()`, `family-orphans-by-date-report.printSheetCommand()` set and clear the flag around blocking `window.print()` in one tick — `[disabled]="printing"` never disables anything; the double-click guard is illusory.

- **`orphan-files` fires one image request per row with no concurrency cap** — `loadThumbs()` subscribes to `attachmentService.getImage()` for all 20 rows simultaneously (twice when switching grids), hammering the endpoint every page turn; per-image failures swallowed with an empty handler.

- **`new Date().toISOString().slice(0, 10)` exports the UTC date** — filenames (`orphans-missing-files`, `orphans-missing-reports`, `provider-sponsor-changes`, `refused-reports`, `registered-family-projects`, `reports-awaiting-approval`) and questionnaire footer stamp the previous day between local midnight and UTC+3 03:00; inconsistently mixed with local `dd/MM/yyyy` formatting elsewhere (`charity-payment-tracking.formatDate`).

- **`cheque-statement` fetches one 200-row page and pretends that's the result set** — `search()` runs page 1 at `pageSize: 200` with no pager bindings to the shell; rows beyond 200 unreachable. `loadBanks` casts response `{...} as any`; `family-orphans-entries` similarly multiplexes `mode` through `result as {...}` casts instead of typed DTOs.

- **Inconsistent client gating of the review jump between forked siblings** — `refused-reports` hides `openReview` behind `*ngIf="canReview"`, while `reports-awaiting-approval` (the declared pattern-holder of the same shared grid) shows the identical `/periodic-orphan-reports/{id}/review` button to every role and lets the route guard bounce the click.

- **The preview document is not actually "the SAME renderer"** — `report-pdf.service.standaloneDocument()` emits a document whose only CSS is `printStyles()`, where `.report-print-sheet` uses `font-family: inherit`; inside the blob-URL frame that resolves to the browser default font, so previewed documents diverge typographically from the in-page print path the comments claim is identical.

- **Every new screen ships a smoke-only spec** — `orphans-missing-files`, `orphans-missing-reports`, `orphans-without-payment-report`, `provider-sponsor-changes`, `refused-reports`, `registered-family-projects`, `reports-awaiting-approval`, `widows-sponsorship-report`, `survey-questionnaire` (and the rest) test nothing beyond `should create`; the one spec with real assertions (`family-follow-up-report`) is broken against the shipped API.

- **Hard-coded glyphs and a mislabelled checkbox** — `excluded-orphans-report` (`row.isExcluded ? '✓' : '—'`), `registered-family-projects` (`hasExperience ? '✓' : '—'`), `orphan-data-report` render literal glyphs instead of translated labels; `orphan-data-report.component.html` carries the HTML comment admitting the "العمر" (age) label actually binds `isFinishedSponsorship`, and `buildFilter()` derives `notExcluded` from the other two flags, making the visible `notExcluded` checkbox decorative.

- **Hand-rolled `setErrors` clobbers and under-reports** — `charity-payment-tracking` (`dateControl.setErrors({required:true})` / `setErrors(null)`) and `orphan-files` (`setErrors({ server: true })` without the server's message) clear coexisting errors and show generic "invalid" rather than the server's reason; the two server-error mappings in the chunk (`family-orphans-by-date` includes message, `orphan-files` doesn't) can't agree on the shape.
