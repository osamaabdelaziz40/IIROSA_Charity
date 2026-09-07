# Story 18-40: Display a report in the browser عرض التقرير

| Field | Value |
| --- | --- |
| Story key | `18-40-display-a-report-in-the-browser` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-40 — عرض التقرير |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.40 scenario — generic capability, no dedicated §23.S screen) |
| Route | capability inside `report-viewer` (a preview modal) — no new route |
| Endpoint | none NEW — each report's existing JSON endpoint feeds the preview; the board/legacy `POST /api/Reports/{reportKey}/export/pdf` generic route is superseded — recorded deviation |
| Depends on | **18-21 landed** (`services/report-pdf.service.ts` — the renderer); at least two data-report stories landed (18-1 + one more) to verify against |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to display a report in the browser عرض التقرير, so that I
can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a signed-in user on any landed report screen, when the actor presses the report's عرض
   (preview) command, then the generated document opens in an on-screen preview — an inline frame
   fed by a blob URL — for review before print/save, without leaving the screen.
2. Given the preview is open, when the actor presses طباعة (or حفظ), then the browser's native
   print/save handles the framed document — one interaction, no duplicate generation.
3. Given the report behind the preview returns no row, when the preview is requested, then the
   actor is told there is nothing to display rather than seeing an empty frame.
4. Given the preview is requested while the report's data call fails, when the error returns, then
   the `{ message }` envelope surfaces as a toast and the modal closes — never a blank modal.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.
6. Given ANY landed report screen, when its preview command runs, then the SAME preview component
   and renderer serve it — no per-screen preview implementations (the anti-reinvention AC).

**Definition of done:** §23.U.40 passes end to end — the preview works identically on at least
two landed report screens (verified in the story's Dev Agent Record); the capability lives once
in the shell; no new backend endpoint exists for it.

## Capability contract (§23.U.40 — review before print or save)

- A shared `report-preview` modal component in `Frontend/src/app/modules/reports/report-viewer/`
  (beside the shell): input = a document producer (the PDF service call for the report's current
  filter + variant), output = nothing (side-effect only: preview/print/save).
- Flow: actor presses عرض → the screen's current filter drives its document builder →
  `report-pdf.service.ts` produces the blob → modal opens `<iframe [src]="blobUrl">` (the
  browser's PDF viewer renders inline) → طباعة triggers `iframe.contentWindow.print()`; حفظ
  re-uses the same blob (no regeneration); revoke the object URL on close.
- The grid already rendered by the screen stays the data source of truth (§23.U.40's own main
  flow ends "rendered in the on-screen grid, from where it can be printed or exported") — the
  preview is the print/preview affordance over that grid.

## Tasks / Subtasks

- [x] **Task 1 — Preview modal** (AC 1, 2)
  - [x] `report-viewer/report-preview/` 4-file component: producer-input API, iframe + blob URL,
        طباعة/حفظ/إغلاق actions, RTL dialog styling consistent with the module; empty-result
        guard (AC 3) and error-path guard (AC 4) before the iframe mounts
  - [x] Object-URL hygiene: create on open, `revokeObjectURL` on close — no leaks across previews
- [x] **Task 2 — Shell wiring** (AC 1, 6)
  - [x] Extend the `report-viewer` shell with the عرض command slot (disabled when the current
        report has no document builder registered — see Task 3); the shell passes the active
        report's producer to the modal
- [x] **Task 3 — Document-producer registry** (AC 6)
  - [x] A small registry in the reports module (map: report key → document producer) so each
        landed print story (18-21/18-26/18-31/18-33/18-34/18-35/18-39 …) registers its builder
        instead of building its own preview; register the builders of every landed story at the
        time this story runs and list them in the Dev Agent Record
  - [x] Screens whose report is grid-only (no printable document yet) keep عرض disabled — honest
        absence, no dead button that errors
- [x] **Task 4 — i18n** — `reports.preview.*` (title, print, save, close, nothing-to-display,
      error toast) in **both** `ar.json` and `en.json`
- [x] **Task 5 — Verification** (AC 1–6)
  - [x] On two landed report screens: preview opens with real content; طباعة opens the native
        dialog on the framed doc; حفظ downloads without regeneration; empty result → message;
        failed data call → toast + closed modal; 401 path → login redirect
  - [x] Blob URLs revoked (no `blob:` accumulation in `about:blank` after 3 previews)
  - [x] `cd Frontend && npm run build` green (ng-serve stale-bundle grep caveat); backend
        expected untouched — `dotnet build` a formality; tests excluded per the standing user
        decision

## Dev Notes

### Supersession (recorded)

The legacy/board realisation `POST /api/Reports/{reportKey}/export/pdf` — one server route that
renders any report key to PDF bytes — is superseded by the epic-wide ruling: reports already
return JSON from their own endpoints, and documents are composed client-side (jsPDF via 18-21).
A generic server PDF route would resurrect the server-rendering stack (EPPlus/Crystal) the
platform retired. The registry in Task 3 is the platform's replacement for the `reportKey` axis.
If a future story genuinely needs server-rendered PDFs (e.g. mass generation), that is a
correct-course decision, not this story.

### Platform rules that bind this story

- No new backend endpoint, no migration — frontend capability only.
- Client-side rendering only through 18-21's `report-pdf.service.ts`; no parallel renderer.
- i18n in both languages; lazy module; the preview lives in the shared shell so every report
  inherits it (reuse over reinvention — AC 6 is the point of this story).

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| The PDF renderer + Arabic font | 18-21 |
| Excel export engine | 18-41 |
| Server-side PDF generation of any report | none — correct-course only |
| Individual reports' document builders | their own stories (registered here, not built here) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.40] scenario — inline-frame review
  before print/save; grid as the render surface
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-40 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] the shell this capability
  extends
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-21-family-update-tracking.md] the renderer
  the preview frames
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] platform conventions
  reference

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5).

### Debug Log References

- `npm run build` → exit 0, zero `Error:` lines (`/tmp/fe-build-1840.log`)
- `dotnet build Backend/IIROSA.sln` → exit 0, `0 Error(s)` (`/tmp/be-build-1840.log` — formality; backend untouched)

### Completion Notes List

- **Blob content adaptation (recorded):** the contract assumes a jsPDF blob; 18-21's ruling
  (decide-once) is the BROWSER print pipeline — there is no JS PDF library. The preview frames a
  standalone RTL **HTML document** built by the SAME renderer: `report-pdf.service` gained
  `sheetSource` / `formSource` / `cardsSource`, which wrap the print paths' extracted element
  builders (`buildSheetElement` / `buildFormElement` / `buildCardsElement` — verbatim, no behavior
  change to printSheet/printForm/printCardSheet) into `<!DOCTYPE html>` documents with the SAME
  inlined stylesheet + a visibility override. طباعة runs the frame's own `print()` — the browser's
  print dialog, "Save as PDF" included; حفظ downloads the same blob as `.html` (one generation,
  no regeneration). No parallel renderer was introduced (AC-compliant).
- **Producer registry (recorded shape):** the story sketches a static map (report key →
  producer); producers must close over LIVE component state (grid rows, filters, loaded flags),
  which a static key-map cannot. The registry's binding is the shell's `[previewProducer]` input —
  sync or async (`Promise<ReportPreviewSource | null>`), the shell resolves both. AC 6 holds: ONE
  modal (`report-preview`) + ONE renderer serve every screen.
- **Registered producers at story time (7):**
  1. `#/reports/new-beneficiaries` (18-35) — sync sheet, the print builder previewed
  2. `#/reports/follow-up-sheets` (18-36) — sync sheet, variant-aware
  3. `#/reports/cheque-statement` (18-33) — sync sheet
  4. `#/reports/family-orphans-by-date` (18-39) — sync sheet, grouped/flat
  5. `#/reports/orphan-data` receipt cards (18-31 §23.U.31) — ASYNC fetch → cardsSource
  6. `#/reports/charity-tracking` updates sheet (18-21/22 era) — sync over the loaded updates set
  7. `#/reports/family-orphans` identification sheets (18-37) — ASYNC fetch → sheetSource; the
     modal is hosted DIRECTLY in that screen (it predates the 18-1 shell — no `app-report-viewer`
     there), with its own عرض button beside the print command
- **Honest absence (Task 3):** screens with no printable document keep عرض disabled — e.g.
  `missing-outgoing-attachments` (18-38, grid-only by story), and the reports still on the legacy
  `exportSheet` server-payload pipeline (e.g. 18-21's tracking sheet, 18-26's استبانة) until their
  own stories port them (out-of-scope table).
- Object-URL hygiene: create on `open()`, `revokeObjectURL` on close/Escape/backdrop AND before a
  re-preview while open; the blob is kept so حفظ re-uses it (AC 2). Guards: empty/absent source →
  nothing-to-display toast, frame never mounts (AC 3); async producer failure → error toast in the
  shell, modal never opens (AC 4 — verified by construction; live browser pass rides the
  consolidated smoke).
- The عرض button sits beside بحث / استخراج in the shell's command bar, disabled until the
  producer exists AND the grid has run with rows.
- Refactors kept behavior-neutral: `renderIdentificationSheet` and `renderReceiptCards` split into
  a print wrapper + a shared config builder; `printUpdates` likewise. The form's blank-cell
  non-breaking space and all print styles are untouched (styles extracted to `printStyles()` —
  one source of truth for both pipelines).
- Live checks (two screens end to end, blob-URL non-accumulation) deferred to the consolidated
  smoke; boxes stay open. No migration; tests excluded per standing decision.

### File List

Shared capability:
- `Frontend/src/app/modules/reports/services/report-pdf.service.ts` — `ReportPreviewSource` + `sheetSource`/`formSource`/`cardsSource` + element-builder extraction + `printStyles()` + `standaloneDocument()`
- `Frontend/src/app/modules/reports/report-viewer/report-preview/` — new 4-file shared modal (blob-URL iframe, طباعة/حفظ/إغلاق, Escape/backdrop, URL hygiene)
- `Frontend/src/app/modules/reports/report-viewer/report-viewer.component.ts/.html` — `[previewProducer]` input + عرض command + modal host + sync/async resolve with toasts

Producer registrations:
- `Frontend/src/app/modules/reports/new-beneficiaries-report/` — `previewDocument()` + binding
- `Frontend/src/app/modules/reports/follow-up-sheets/` — `previewDocument()` + binding
- `Frontend/src/app/modules/reports/cheque-statement/` — `previewDocument()` + binding
- `Frontend/src/app/modules/reports/family-orphans-by-date-report/` — `previewDocument()` + binding
- `Frontend/src/app/modules/reports/orphan-data-report/` — `previewReceiptCards()` (async) + config extraction + binding
- `Frontend/src/app/modules/reports/charity-payment-tracking/` — `previewDocument()` + config extraction + binding
- `Frontend/src/app/modules/reports/family-follow-up-report/` — `previewIdentificationSheet()` (async, direct modal host) + config extraction + عرض button

i18n:
- `Frontend/src/assets/i18n/ar.json` · `en.json` — `reports.preview` (7 keys ×2)

### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

Both open live boxes are evidenced end to end in the browser (ng serve on 4299 + the private API,
signed in as OsamaSuper, English locale):

- **Two screens, real content**: `#/reports/follow-up-sheets` (10 rows) and
  `#/reports/family-orphans-by-date` (2 families / 4 flattened orphan rows). عرض opens the shared
  modal and mounts an RTL framed document with the correct title and rows matching each grid.
- **طباعة**: the unstubbed click opened the NATIVE print dialog on the framed document (the click
  blocked until it was dismissed — the real dialog, not a stub); a rerun with `frameWindow.print`
  stubbed recorded exactly one call — the frame's own print pipeline, no regeneration (AC 2).
- **حفظ**: the download anchor carries `${documentTitle}.html` and the SAME `blob:` URL the frame
  renders (captured: "Follow-up and Handover Sheets - Follow-up.html", href `blob:http://…`).
- **Empty result → message**: a 0-row search disables عرض (honest absence) and shows "No data matches
  the selected filters"; the defensive `open(null)` path toasts "Nothing to display" and the modal
  stays closed.
- **Failed data call → toast + closed modal**: receipt-cards endpoint aborted at the network layer
  (Playwright route-abort) with a batch set — the producer rejects, the shell catches: error toast
  "Could not produce the document for preview", modal never opens, no blob minted, and the
  `previewing` flag resets (no dead button afterwards).
- **401 path**: access token corrupted in localStorage → immediate redirect to `#/auth/login`
  (session restored afterwards).
- **Blob hygiene (box 2)**: `URL.createObjectURL`/`revokeObjectURL` instrumented — 3+ previews across
  both screens end at 4 created / 4 revoked / 0 active blob URLs. No accumulation.

#### Defect found by the smoke and fixed in this pass (recorded)

`[previewProducer]="previewDocument"` delivers an UNBOUND method reference — when the shell invokes
`this.previewProducer()`, `this` inside the producer was the VIEWER, not the host screen. Every
producer read `this.rows` off the wrong instance: follow-up-sheets threw `TypeError … reading
'length'` (silent no-op via the shell's optional chaining), and orphan-data's batch guard read
`this.batchNumber` off the viewer (always undefined — the pickBatch toast would fire even with a
batch picked). 18-41's `[exportRequest]` bindings carried the identical defect (producers returned
null → the engine never ran). Fix: the 12 bound producers converted from methods to arrow-function
PROPERTIES (lexical `this`) across the 8 host screens — files in the File List; re-verified live:
preview and export both work end to end after the fix.
- `Frontend/src/app/modules/reports/charity-payment-tracking/charity-payment-tracking.component.ts` — smoke fix: `previewDocument` method → arrow property (unbound-`this` defect)
- `Frontend/src/app/modules/reports/cheque-statement/cheque-statement.component.ts` — smoke fix: `previewDocument` → arrow property
- `Frontend/src/app/modules/reports/family-orphans-by-date-report/family-orphans-by-date-report.component.ts` — smoke fix: `previewDocument` → arrow property
- `Frontend/src/app/modules/reports/follow-up-sheets/follow-up-sheets.component.ts` — smoke fix: `previewDocument` → arrow property
- `Frontend/src/app/modules/reports/new-beneficiaries-report/new-beneficiaries-report.component.ts` — smoke fix: `previewDocument` → arrow property
- `Frontend/src/app/modules/reports/orphan-data-report/orphan-data-report.component.ts` — smoke fix: `previewReceiptCards` → arrow property

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Consolidated live smoke passed on two screens (preview/print/save/empty/failure/401 + blob hygiene). Producer this-binding defect found and fixed (12 arrow-property conversions, shared with 18-41). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
