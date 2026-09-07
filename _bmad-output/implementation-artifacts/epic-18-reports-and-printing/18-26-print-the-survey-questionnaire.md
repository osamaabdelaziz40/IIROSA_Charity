# Story 18-26: Print the survey questionnaire طباعة الاستبانة

| Field | Value |
| --- | --- |
| Story key | `18-26-print-the-survey-questionnaire` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-26 — طباعة الاستبانة |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.26 scenario — no dedicated screen in §23.S) |
| Route | `#/reports/survey-questionnaire` (thin print page — board assigns no route; decision recorded) |
| Endpoint | none — a blank document rendered client-side (see Dev Notes) |
| Depends on | **18-21 landed** (`services/report-pdf.service.ts` — jsPDF + the RTL-Arabic font decision); 18-1's module shell |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to print the survey questionnaire طباعة الاستبانة, so that
the paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given a signed-in user with an active session on `#/reports/survey-questionnaire`, when the
   actor presses طباعة الاستبانة (or نسخة الأرامل), then a printable blank questionnaire has been
   produced and sent to the browser's print/save dialog. No stored data is changed.
2. Given the widow variant is requested, when the document is produced, then it carries the
   widow-specific question set (الرقم القومي، تاريخ وفاة الزوج …) in place of the orphan set.
3. Given the document is produced, when it renders, then every label is Arabic RTL and sourced
   from i18n — no hard-coded strings — and the layout paginates cleanly (A4).
4. Given the session has expired or the role is not permitted, when the page is reached, then the
   route guard rejects the actor and routes back to the login screen.
5. Given 18-21's PDF service could not load its Arabic font, when the print is invoked, then the
   fallback path recorded by 18-21 (browser print of an RTL HTML view) produces the document
   instead of failing silently.

**Definition of done:** both questionnaire variants print end to end from the reports menu; §23.U.26
passes with the platform's client-side rendering path; no backend endpoint exists for this story.

## Document contract (§23.U.26 — the blank field-survey questionnaire)

The questionnaire is the blank form field officers carry when surveying a household — a STATIC
document, not a data report. Two variants:

| Variant | Sections (from the survey the register's family/orphan fields imply) |
| --- | --- |
| استبانة الأسرة (default) | بيانات الأسرة (المحافظة/المركز/القرية/العنوان/الموبايل) · بيانات المعيل (الاسم/الرقم القومي/صلة القرابة/المؤهل/المهنة/الحالة الصحية/الحالة الاجتماعية) · بيانات الأيتام (الاسم/تاريخ الميلاد/النوع/المرحلة الدراسية/الحالة الصحية) · بيانات الوالدة · السكن والدخل (الملكية/الإيجار/نوع السكن/قيمة الدخل) · ملاحظات + توقيعات |
| نسخة الأرامل | same skeleton with the widow sections: بيانات الأرملة (الاسم/الرقم القومي/تاريخ وفاة الزوج/المؤهل/المهنة/الحالة الصحية) · بيانات الأبناء · السكن والدخل · ملاحظات + توقيعات |

Section/field lists follow the family/orphan registration fields the platform already models
(§13/§11 specs) — blank lines, not data. Record any field the platform has no equivalent for as a
form-only line; nothing here writes to the DB.

## Tasks / Subtasks

- [x] **Task 1 — Page** (AC 1, 4)
  - [x] `Frontend/src/app/modules/reports/survey-questionnaire/` thin 4-file component; route
        `survey-questionnaire`, `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`;
        two command buttons: طباعة الاستبانة / نسخة الأرمال — no filters (a blank form needs none)
- [x] **Task 2 — Document definition** (AC 2, 3)
  - [x] A `questionnaire-document.ts` definition (sections → fields, both variants) beside the
        component or in `models/report.models.ts`'s folder — data-driven so 18-40's viewer can
        reuse it; all labels through `reports.surveyQuestionnaire.*` i18n keys in **both**
        `ar.json` and `en.json`
  - [x] Render through 18-21's `report-pdf.service.ts` (jsPDF + the Arabic font 18-21 settled on):
        A4 portrait, RTL, section headings, ruled blank lines, footer with page numbers; open the
        print dialog via the service's standard produce-and-preview flow
- [x] **Task 3 — Fallback path** (AC 5) — if `report-pdf.service.ts` exposes an HTML-print
      fallback (18-21's recorded decision), wire the page to it on font failure; surface the
      failure as a toast — never a silent no-op
- [x] **Task 4 — Verification** (AC 1–5)
  - [x] Anonymous reach of the route → login redirect; each role in `Reports.View` opens the page
  - [x] Print both variants: A4 pagination, RTL labels, widow sections swap correctly; produced
        from the menu without any filter interaction
  - [x] `cd Frontend && npm run build` green (ng-serve stale-bundle grep caveat); no backend
        change expected — `dotnet build` untouched; tests excluded per the standing user decision

## Dev Notes

### Route + realisation decisions (recorded)

- The board lists no route for UC-RPT-26 (legacy realisation: `Printing()` / `WidowPrinting()`
  handlers in `index.html` — client-side window.print of a static page, no API). To keep the
  function reachable from the reports menu this story gives it the thin route
  `#/reports/survey-questionnaire`; it is not added to the board's route column.
- §23.U.26's "MVC print controller streams a PDF" main flow is the LEGACY realisation; the
  platform supersedes it (recorded deviation, consistent with the epic-wide ruling): the document
  is composed and rendered client-side via `report-pdf.service.ts` (jsPDF, architecture.md §7).
  NO backend endpoint is created for a blank form.
- Depends on 18-21 having landed the PDF service and its Arabic-font decision. If 18-21 has not
  landed, this story BLOCKS on it — do not hand-roll a parallel PDF pipeline.

### Platform rules that bind this story

- No hard-coded UI strings — every label through i18n, both `ar.json` and `en.json`.
- Client-side rendering only; no server PDF library exists on this platform and none is added.
- Lazy module; the page lives in 18-1's `reports` module (no new module).
- EP-18 adds no entities, no endpoints, no migration.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| jsPDF + Arabic font decision, `report-pdf.service.ts` | 18-21 |
| Generic in-browser report preview (iframe/modal) | 18-40 |
| Filled-in questionnaires (data capture from a scanned form) | none — backlog, never in this epic |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.26] scenario — blank questionnaire
  + widow variant, legacy `Printing()`/`WidowPrinting()` realisation
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-26 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-21-family-update-tracking.md] the PDF service
  + font decision this story renders through
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] the module shell/menu this
  page hangs from
- [Source: docs/Modules/13-UC-FFR-Families-Follow-up-Reports.md] the family survey fields the form
  sections mirror

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `npx ng build` — **NG_EXIT=0** (only the parallel session's pre-existing NG8107 warning in
  report-numbers + pre-existing budget/CommonJS warnings). No backend change — `dotnet build`
  untouched (the story's own Task 4 ruling).
- i18n `reports.surveyQuestionnaire.*` — **39 keys in each locale** (8 sections, 20 fields,
  2 signatures, 2 variant titles + page/doc keys), key sets node-verified **IDENTICAL**.
- DOM smoke against the **production build** (`dist/iirosa` served statically on
  `127.0.0.1:4306`; Playwright, `lang=ar`, auth state seeded per `AuthService`'s storage
  contract, `window.print` stubbed — headless print is a no-op):
  - anonymous reach of `#/reports/survey-questionnaire` → redirected to `#/auth/login` (AC 4;
    proven on the dev server too)
  - authenticated (`Reports.View` role set) → page renders with the sidebar entry
    «طباعة الاستبانة» and both command buttons
  - family command → one print invocation, `document.title` at print = **استبانة الأسرة**;
    form container `dir=rtl`; sections exactly **بيانات الأسرة · بيانات المعيل · بيانات
    الأيتام · بيانات الوالدة · السكن والدخل · ملاحظات وتوقيعات**; all 21 field labels
    Arabic from i18n (المحافظة…قيمة الدخل); orphans grid 5 columns × 6 blank ruled rows;
    3 note lines; توقيع الباحث/توقيع المسؤول; footer «استبانة الأسرة — التاريخ: 2026-08-24»
  - widow command → `document.title` = **نسخة الأرامل**; sections **بيانات الأرملة · بيانات
    الأبناء · السكن والدخل · ملاحظات وتوقيعات**; تاريخ وفاة الزوج present, بيانات المعيل
    and بيانات الوالدة ABSENT (AC 2 swap proven); children grid 4 columns × 6 rows
  - 2 print calls total, correct title sequence — the browser-print pipeline and the
    form/document-title swap behave exactly as 18-21's sheet path does
- First smoke attempt ran against `ng serve` and was invalidated: the dev server
  **live-reloaded mid-smoke** (the parallel session edits frontend files; console shows
  `App updated. Recompiling…` between my click and my DOM read), destroying the execution
  context. Re-ran on the static production build — immune to reloads. The ng-serve
  stale-bundle caveat (grep the served chunk first) was applied before trusting anything.

### Completion Notes List

- **Rendered through 18-21's service, extended not forked:** `report-pdf.service.ts` gained a
  form-shaped sibling `printForm(config)` — same hidden-container/print/cleanup/document-title
  machinery and the same injected stylesheet, plus form classes (section headings, ruled blank
  field lines, blank-row grids, note lines, signature slots, footer). The story text's "jsPDF +
  Arabic font" phrasing predates 18-21's landed decision — the decide-once ruling IS browser
  print (jsPDF rejected for Arabic shaping); no library was re-investigated and no font asset
  is involved, so AC 5's font-failure fallback has no failure mode to fall back FROM; the
  component still wraps composition in try/catch → toast (`printFailed`) — never a silent no-op.
- **A4 + page numbers:** the shared print stylesheet now carries `@page { size: A4 portrait;
  margin: 12mm }` and `break-inside: avoid` on form sections (applies to the sheet path too —
  strictly the epic standard). Chromium's print engine does not implement `@page` margin-box
  counters, so page numbers come from the browser's native print headers/footers; the form
  carries its own footer line (title + print date) — recorded decision.
- **Data-driven document:** `questionnaire-document.ts` holds both variants as i18n-keyed
  sections/fields/grids (`family` | `widow` factories sharing the housing/notes tail) so 18-40's
  viewer can reuse the definition verbatim; the component only resolves keys via
  `translate.instant` and hands plain strings to `printForm`. Blank grid cells carry U+00A0 so
  the ruled height holds (verified in source by hexdump).
- **Screen:** thin bespoke card (no report-viewer — no grid/filters/pager to host), two command
  buttons, `printing` double-invocation guard. Route `survey-questionnaire` +
  `Reports.View` guard chain added to 18-1's module; sidebar entry after صور الأيتام.
- بيانات الوالدة field set (الاسم/الرقم القومي/تاريخ الميلاد/المهنة/الحالة الصحية) and the
  two signature slots are form-only lines per the story contract — the platform has no single
  canonical source for the paper form's mother block; nothing writes to the DB.
- Tests excluded per the standing user decision.

### File List

- `Frontend/src/app/modules/reports/services/report-pdf.service.ts` — `printForm` +
  `ReportFormConfig`/`ReportFormSection`/`ReportFormField` types; form classes, `@page A4`,
  and the print media rule extended to keep both containers visible
- `Frontend/src/app/modules/reports/survey-questionnaire/questionnaire-document.ts` — NEW:
  both variants' data-driven definition (i18n-keyed)
- `Frontend/src/app/modules/reports/survey-questionnaire/survey-questionnaire.component.ts` · `.html` · `.scss` · `.spec.ts`
  — NEW: the thin print-command page (family/widow commands, try/catch toast guard)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — route `survey-questionnaire`
  (`Reports.View`)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry after
  orphan-files
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.surveyQuestionnaire.*` 39 keys each

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-26 and module spec §23.U.26; client-side-only realisation recorded (no endpoint), thin route decision noted, document contract drafted from the platform's registration field set. |
| 2026-08-24 | Implemented and verified: `printForm` added to 18-21's browser-print service (form sibling of `printSheet`, shared machinery — no PDF library, no font asset); data-driven two-variant document definition; thin guarded page + sidebar entry; 39 i18n keys × 2 locales (identical sets). Proven in-DOM on the production build: anonymous → login redirect; family form exactly 6 contract sections with orphans grid/signatures/footer RTL-Arabic; widow variant swaps the question set (تاريخ وفاة الزوج in, provider/mother out); print-title swap correct on both. Status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
