# Story 9-17: Print the periodic report form

| Field | Value |
| --- | --- |
| Story key | `9-17-print-the-periodic-report-form` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-17 — طباعة التقرير الدوري |
| Priority / size | Should · 3 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.U.17 scenario; the print commands of §14.S.1/§14.S.2) |
| Route | print commands on `#/periodic-orphan-reports` (row), the form, and the detail screen |
| Endpoint | `POST /api/Reports/orphan-report-form/export/pdf` (on the 9-14 `ReportsController` root) |
| Depends on | **9-14** (api/Reports root exists), **9-4** (detail data), **9-16** (documents present are part of variant selection) |
| Roles | Charity, HQ roles → `Charity`, `SuperAdmin`, `Admin`, `Accountant`, `Employee` |

## Status

review

## Story

As a charity user, I want to be able to print the periodic report form طباعة التقرير الدوري, so
that the paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given a charity user with a report on screen, when the actor invokes print, then a printable
   PDF of the official orphan report form is produced and opened for printing.
2. Given the request is accepted, when it is served, then the flow is handled by `POST
   /api/Reports/orphan-report-form/export/pdf` (data + layout resolution) and rendered without a
   page reload.
3. Given the case facts, when the form is produced, then the **layout variant matches the case**:
   studying / not studying / disabled orphan, and which of the marriage, death, medical and
   certificate documents are attached (the legacy 24-template matrix).
4. Given the selection returns no row, when the document is produced, then the actor is told there
   is nothing to produce rather than receiving an empty file.
5. Given a charity user, when the function is invoked, then only records owned by that charity (and
   country) are printable (9-1 scope on the data fetch).
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.17 passes end to end; the printed form carries the report's sections
and the attached photographs/certificates; the variant selection is testable from the payload.

**Rendering decision (recorded):** the platform's sanctioned PDF tool is **client-side jsPDF**
(architecture.md §7; no server PDF package exists in any csproj — verified). The endpoint at the
spec's route therefore returns the **composed print payload** — the report data + the resolved
variant key + the attachment image references — and the client renders the PDF with jsPDF (Arabic
font embedded) and opens the browser print dialog. This keeps the spec's route and semantics while
using the stack's real PDF path; a server-rendered PDF would need a new library decision that is
not this story's to make. Record the envelope shape in the completion notes. (§14.U.17's own
secondary-actors note already names jsPDF as the rendering path.)

## What exists already

| Layer | File | State |
| --- | --- | --- |
| Controller root | `ReportsController` (api/Reports) | Created by 9-14 — add the action here |
| Data | `GET /api/PeriodicOrphanReports/{id}` (9-4) | The full report + attachment ids |
| Frontend | §14.S.1/§14.S.2 print commands (`GotoPrintAction`, `…V2`, `…Students`) | Inert icons; blob-download helper exists; jsPDF is already a platform dependency |

**Variant collapse (recorded):** the legacy realisation shipped four print routes (`export/pdf`,
`…V2`, `…V3`, `…Students`) — presentation variants of the same form family. Ship **one** endpoint;
the variant matrix (AC 3) replaces the route suffixes. The `Students` command maps to the
`IsOrphanStudent` variant of the same matrix.

## Tasks / Subtasks

- [x] **Task 1 — Variant resolver** (AC 3): server-side resolver over the report: base = disabled?
      (Disability set) : studying? (education flags) : not-studying; document sections included
      when `OrphanMarriageImageId` / `OrphanDeadImageId` / `MedicalReportImageId` /
      `OrphanCertificateImageId` present — emitted as a stable variant key (e.g.
      `studying+cert+medical`); unit-testable pure function in Application
      — `OrphanReportFormVariantResolver` (pure static, no I/O) beside the print DTOs; studying
      = `IsOrphanStudent == true` || `EducationalLevelId` set || `School` set.
- [x] **Task 2 — Endpoint** (AC 2, 4, 5): `POST /api/Reports/orphan-report-form/export/pdf`
      `{ reportId }` on `ReportsController` → loads the report through the 9-1-scoped read,
      returns `{ data, variant, attachments: [{slot, id}] }`; unknown/unauthorised id → 404;
      caller scope; thin controller, service does the work
      — dedicated literal-route action `ExportOrphanReportForm` (a literal beats the
      `{reportKey}` parameter route and keeps this story's role set — SuperAdmin, Admin,
      Accountant, Employee, Charity — off the family sheets' action); `GetPrintFormAsync` on
      `IPeriodicOrphanReportService` reuses the 9-4 scoped read + `MapToDetailDtoAsync`; null →
      404 (nothing to produce); missing `ReportId` → 400. `ReportSheetRequestDto` gained
      `ReportId`.
- [x] **Task 3 — Client renderer** (AC 1, 3): jsPDF document per the variant: the §14.S.2 sections
      in the spec's order, RTL Arabic (embedded font — reuse whatever Arabic font asset the
      platform already ships; if none exists, add one under `assets` and record it), photograph +
      document images fetched as authenticated blobs (9-16 pattern) placed in their sections;
      open in a new tab / trigger `print()`; the three legacy commands (print/V2/Students) all
      route here with the matrix choosing the layout
      — **rendered as `window.print()` on a print-optimised Angular view, NOT jsPDF** (see the
      deviation note in the Dev Agent Record). New `periodic-report-print/` component at
      `:id/print`: the official form in §14.S.2 order, RTL inherited, document images via
      authenticated blob + object URLs (§14.U.16 pattern), `@page A4` print CSS; the print
      dialog auto-opens once every image settles (manual button too). The variant drives
      section emphasis (health for `disabled`, education for `studying`) and which document
      sections print; the variant key prints in the footer (testable from the payload).
      Commands: list-row print (§14.S.1) + detail-header print (§14.S.2) — the legacy
      V2/Students variants are the same target with the matrix choosing the layout.
- [x] **Task 4 — i18n** — the form's static labels (it is an official Arabic form: labels come from
      `ar.json` keys under `periodicReports.print.*`; `en.json` carries the same keys for
      completeness)
      — 7 keys per locale; section/field labels reuse the existing `periodicReports.form.*`
      keys; no font asset needed — the browser renders the Arabic natively, which is exactly
      why jsPDF was not used.
- [ ] **Task 5 — Verification** (AC 1–6): live check — a studying orphan with certificate +
      (static gates passed: `dotnet build` Application + Api 0 errors; `npx tsc --noEmit` zero
      non-spec errors; live checks + `npm run build` are this epic's final gate)
      medical attachments yields the `studying+cert+medical` variant and the PDF shows both images;
      a disabled orphan yields the disabled base; missing id → 404 → "nothing to produce" message
      (AC 4); charity pin holds (foreign id → 404); unauthenticated → 401; the printed output
      opens the browser print dialog; `npm run build` green; tests excluded per the standing user
      decision

## Dev Notes

### Platform rules that bind this story

- The endpoint is a read (compose + resolve); only `IUnitOfWork`-free paths; raw envelope; camelCase
  wire.
- jsPDF is the sanctioned PDF renderer — do not add a server PDF package (iText/QuestPDF/etc.);
  that is a platform decision outside this story.
- Arabic RTL in jsPDF requires an embedded Arabic-capable font and `isInputVisual=false`-style
  shaping care — budget time for shaping verification; if output is disconnected letters, the font
  lacks the shaping tables (record the chosen font in the completion notes).
- AC 6's "flagged as printed" post-condition: the module does not record printing — nothing to
  write; note it and move on (§14.U.17's own "where the module records printing" hedge).

### Out of scope (later stories/epics — do not build)

| Item | Story |
| --- | --- |
| The other 40 EP-18 report/print endpoints | EP-18 |
| Excel variants of the form | EP-18 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.17] scenario (24-template note,
  jsPDF secondary-actor note)
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.1] "Printing" row — variant
  selection rule
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-17 acceptance criteria
- [Source: _bmad-output/planning-artifacts/architecture.md#7] jsPDF as the platform PDF export tool

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5), 2026-08-24.

### Debug Log References

- `dotnet build IIROSA.Application` — first pass CS0118 (the entity name `PeriodicOrphanReport`
  resolves to the DTO namespace from inside it); fixed with a `using ReportEntity = global::…`
  alias; rebuild 0 errors.
- `dotnet build IIROSA.Api` — 0 errors after the print action.
- `npx tsc --noEmit` (Frontend) — zero non-spec errors.

### Completion Notes List

- **Rendering deviation (recorded):** the story's "Rendering decision" block planned client-side
  jsPDF, but the platform's already-recorded client-side print ruling (ReportsController doc
  comment, epics 9/10/18) supersedes it: the legacy `/export/pdf` route returns the composed
  JSON payload, the SPA renders the document, and the **browser's print-to-PDF produces the
  file** — `window.print()` on a print-optimised view (`@page A4` print CSS, chrome hidden on
  paper). jsPDF is not a package dependency and 18-21 owns that decision; a side benefit is
  that Arabic RTL needs no embedded font — the browser shapes it natively (the shaping risk
  the Dev Notes warned about disappears).
- **Envelope shape:** `POST /api/Reports/orphan-report-form/export/pdf` with `{ reportId }` →
  `{ variant, report, attachments: [{ slot, id }] }` — `variant` the collapsed matrix key
  (`disabled|studying|not-studying` + `+marriage|+death|+medical|+cert`), `report` the 9-4
  detail DTO, `attachments` the five slots present with their ids (BR-12 — ids only).
- **Variant collapse honoured:** one endpoint + one print screen; the legacy
  `export/pdf|V2|V3|Students` route family and the 24-template matrix are replaced by the
  resolver key, which the client uses for section emphasis (health for `disabled`, education
  for `studying`) and for the document sections printed.
- **AC 6 "flagged as printed":** the module records no print event (Dev Notes / §14.U.17's own
  hedge) — nothing written; noted.
- AC 4/5 both surface as the honest "nothing to produce" panel (unknown id, foreign id → 404,
  and transport failures all land there); never an empty document.
- The endpoint is a **dedicated literal route** rather than a `{reportKey}` switch case: a
  literal outranks the parameter route in ASP.NET Core matching, and it keeps this story's role
  set (Accountant/Employee included) off the families-epic sheet action unchanged.

### File List

- `Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/OrphanReportFormPrint.cs` — new:
  `OrphanReportFormPrintDto`, `OrphanReportFormAttachmentSlotDto`,
  `OrphanReportFormVariantResolver` (pure).
- `Backend/src/IIROSA.Application/DTOs/Reports/ReportSheetDtos.cs` — `ReportId` added.
- `Backend/src/IIROSA.Application/Interfaces/IPeriodicOrphanReportService.cs` —
  `GetPrintFormAsync`.
- `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` —
  `GetPrintFormAsync` (scoped read + variant + slot list + 9-4 mapping).
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` —
  `ExportOrphanReportForm` action (`orphan-report-form/export/pdf`).
- `Frontend/.../periodic-orphan-reports/models/periodic-orphan-report.model.ts` —
  `OrphanReportFormPrintPayload`, `OrphanReportFormAttachmentSlot`.
- `Frontend/.../periodic-orphan-reports/services/periodic-orphan-report.service.ts` —
  `getPrintForm(id)`.
- `Frontend/.../periodic-orphan-reports/periodic-report-print/` — new component
  (`.ts`/`.html`/`.scss`).
- `Frontend/.../periodic-orphan-reports/periodic-orphan-reports-routing.module.ts` — `:id/print`
  route (guarded).
- `Frontend/.../periodic-reports-list/periodic-reports-list.component.html` — row print command.
- `Frontend/.../periodic-report-detail/periodic-report-detail.component.html` — header print
  command.
- `Frontend/src/assets/i18n/ar.json`, `en.json` — `periodicReports.print.*` keys.

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-17 and module spec §14.U.17 / §14.1; jsPDF client-rendering decision and variant-matrix collapse recorded. |
| 2026-08-24 | Implemented: variant resolver + print payload endpoint (404 nothing-to-produce) + print view (`window.print()` per the standing client-side print ruling — NOT jsPDF; deviation recorded) + row/detail commands + i18n; Application/Api builds + tsc green; npm build batched as the epic's final gate. |
| 2026-08-24 | Verification pass: final gates run — `npm run build` GREEN (epic-9 module compiled; NG8107 optional-chain warnings only) and backend 0 errors for epic-9 code (the only 2 solution errors are the parallel epic-18 session’s in-flight untracked `ReportService.cs` — CS0019 ×2, left untouched per convention). Live API wedged (accepts TCP, empty replies) — restart pending; live walkthrough stays batched. Status ready-for-dev → review. |


### Review Findings (epic review 2026-08-24)

- [x] [Review][Patch] P44 Auto-print never fires if any image fails — decrement pending in error handler too [periodic-report-print.component.ts:144-159]
- [x] [Review][Patch] P45 Auto-print setTimeout never cancelled on destroy — prints destination page [periodic-report-print.component.ts:2316]
- [x] [Review][Patch] P53 snapshot.paramMap — stale report on same-route id change [periodic-report-print.component.ts:2216]
- [x] [Review][Patch] P54 annualFeeForStudy hidden when 0; ragged row when optionals absent [periodic-report-print.component.html:1856]
- [x] [Review][Patch] P56 charityName printed twice in form header [periodic-report-print.component.html:1747,1772]
- [x] [Review][Patch] P59 getPrintForm URL builds from environment.apiUrl — /api doubling risk [periodic-orphan-report.service.ts]
- [x] [Review][Patch] P57c Dead code: RouterLink/auth unused [periodic-report-print.component.ts]
