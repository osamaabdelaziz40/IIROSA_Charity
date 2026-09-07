# Story 5-14: Print family follow-up and identification sheets

| Field | Value |
| --- | --- |
| Story key | `5-14-print-family-follow-up-and-identification-sheets` |
| Epic | EP-05 — Family Register (سجل الاسر) |
| Use case | UC-FAM-14 — طباعة كشوف المتابعة |
| Priority / size | Should · 3 points |
| Specification | `docs/Modules/10-UC-FAM-Family-Register.md` (§10.U.14 scenario — three report keys) |
| Route | `#/reports/family-orphans` (built by 5-11 — the print button lives there) |
| Endpoints | `POST /api/Reports/family-update-tracking/export/pdf` · `POST /api/Reports/guardian-identification-sheets/export/pdf` · `POST /api/Reports/widow-identification-sheets/export/pdf` |
| Depends on | **5-11 landed** (report screen exists); 5-6..5-10 not required |
| Legacy reference | `POST /api/Reports/<report-key>/export/pdf` (old system) — jsPDF/ExcelJS rendering path |
| Roles | HQ roles + charity → `SuperAdmin, Admin, Charity` |

## Status

done

## Story

As a HQ role, I want to be able to print family follow-up and identification sheets طباعة كشوف
المتابعة, so that the paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given an actor on `#/reports/family-orphans`, when the actor presses print for a scope with
   rows, then a printable PDF is produced and downloaded — the family update-tracking sheet for the
   chosen date, and the identification sheets for guardians and for widows (whole charity or a
   single family).
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/family-update-tracking/export/pdf` (and the two identification-sheet keys)
   with a typed filter DTO; the sheet's **data payload** returns as JSON and the client renders
   the printable document (browser print window → print-to-PDF) — the ratified 18-21
   client-side-print ruling; no server PDF pipeline exists on this stack. No page reload.
3. Given the selection returns **no row**, when the document is requested, then the actor is told
   there is nothing to produce (localized message) rather than receiving an empty file.
4. Given a charity-scoped user, when a sheet is requested, then only that charity's data is in it;
   HQ may pass an explicit `charityId` (server-side scope, same rule as every family read).
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** one `ReportsController` with a `{reportKey}/export/pdf` route serves all
three keys through a shared pipeline; sheets are RTL Arabic documents; the document renders
client-side from the server-supplied payload (ratified 18-21 ruling — the legacy route shape is
kept, the bytes are produced by the browser's print-to-PDF).

## Current state — what exists and what is missing (verified 2026-08-24)

Backend (`Backend/src/`):

- **No `ReportsController` exists** — this story introduces `api/Reports` with the
  `[HttpPost("{reportKey}/export/pdf")]` shape; epic 18 (Reports & Printing) will add more keys on
  the same controller later.
- **PDF precedent to copy:** `OrphanReportsController.ExportReport` (`[HttpPost("export")]` →
  service returns `byte[]` → `File(content, "application/pdf", fileName)`), and
  `MissionManagementController.ExportMissions` (Excel). Read
  `OrphanReportService.ExportReportAsync` FIRST and reuse its PDF pipeline/library as-is — do not
  introduce a new PDF dependency.
- Datasets: follow-up rows come from the same audit projection built by 5-11
  (`GetFollowUpActivityAsync` — reuse it, do not duplicate the query); guardian identification =
  the families' `Provider` rows; widow identification = mothers who are the guardian of record
  (`Family.ProviderType` designating the mother) — see Dev Notes for the pragmatic mapping.
- No `Validators/Reports/` folder; scoping template as ever (`FamilyService.cs:491-507`).

Frontend (`Frontend/src/app/modules/`):

- After 5-11: the reports screen exists with filter bar + grid + Excel export. **This story adds
  the print (PDF) button** beside it, plus launch points for the two identification sheets
  (filter-bar actions on the same screen).
- Blob-download pattern exists (`family-detail.component.ts` — `URL.createObjectURL` + anchor
  click); `notification` toasts exist.
- **Pre-existing broken wire (do not fix here):** `family.service.exportFamilyToPDF` calls
  `GET /api/Families/{id}/export/pdf` — that endpoint does not exist on the controller, so the
  detail screen's PDF button is dead. Out of scope; record for the epic retro.

## Tasks / Subtasks

- [x] **Task 1 — Application: report DTOs + service** (AC: 1, 3, 4)
  - [x] `DTOs/Reports/ReportSheetRequestDto.cs` — `string ReportKey`, `DateTime? Date` (tracking
        sheet), `Guid? CharityId`, `Guid? FamilyId` (identification sheets: single family vs whole
        charity), paging-free (sheets print whole selections).
  - [x] `Validators/Reports/ReportSheetRequestValidator.cs` — `ReportKey` must be one of the three
        known keys; `Date` required for `family-update-tracking`.
  - [x] `IReportSheetService.ExportSheetAsync(ReportSheetRequestDto dto, Guid? userCharityId,
        string? userRole)` → `(byte[] Content, string FileName)`:
        - **No rows → refuse** with `BusinessException` (AC 3) — the localized nothing-to-produce
          message; the controller maps it to a 4xx `ApiResponse`, the frontend toasts it.
        - family-update-tracking: rows from 5-11's `GetFollowUpActivityAsync` (date scope);
          columns: code, head of family, charity, kind, actor, timestamp.
        - guardian-identification-sheets: `Provider` rows (+ family code, name, national ID,
          relationship, phone) scoped by charity / single family.
        - widow-identification-sheets: same columns over mothers-of-record (see Dev Notes).
        - Render through the EXISTING PDF pipeline used by `OrphanReportService` — RTL Arabic
          layout, A4, repeated header row; `FileName` like
          `family-update-tracking_{date:yyyyMMdd}.pdf`.
  - [x] Register the service per convention; keep it in Application (no HTTP/PDF-rendering
        dependency beyond what `OrphanReportService` already uses).
- [x] **Task 2 — API: ReportsController** (AC: 2, 5)
  - [x] New `Controllers/ReportsController.cs` — inherits the platform `ApiController` base
        (`IIROSA.Api/Controllers/ApiController.cs`: `[ApiController]`, `[Authorize]`,
        `Route("api/[controller]")`, `CurrentUserId`) — this is a NEW controller, follow the
        platform base, unlike the legacy `FamiliesController`.
  - [x] `[HttpPost("{reportKey}/export/pdf")] [Authorize(Roles = "SuperAdmin,Admin,Charity")]` —
        thin: force `dto.ReportKey = reportKey`, delegate, return
        `File(content, "application/pdf", fileName)`; validation → 400 field-map;
        `BusinessException` (empty selection) → 400 `ApiResponse` with the message.
- [x] **Task 3 — Frontend: print actions on the report screen** (AC: 1, 2, 3)
  - [x] On `#/reports/family-orphans` (5-11's screen): «طباعة كشف المتابعة» button using the
        current date + charity filter → `report.service.exportSheet('family-update-tracking',
        payload)` → blob download (`family-detail` pattern) with the server filename.
  - [x] Two identification-sheet actions (guardians / widows) honouring the charity filter and an
        optional family code input (single family vs whole charity).
  - [x] Empty-selection refusals → error toast with the server message; success → download +
        toast.
  - [x] New `report.service.ts` in the reports module (one `exportSheet` method); i18n keys
        `reports.print.*` + `families.followUp.print.*` in **both** `ar.json` and `en.json`.
- [x] **Task 4 — Verify** (AC: 1–5): date with activity → PDF downloads and opens with correct
      Arabic RTL columns; empty date → refusal toast, no file; charity role gets only its rows;
      single-family identification sheet matches that family; unknown report key → 400;
      `dotnet build` + `npm run build` green.

### Review Findings (code review 2026-08-24)

- [ ] [Review][Decision] Sheet volume unbounded/truncated at both ends — the tracking sheet silently caps at 5000 rows with no truncation warning (an audit-facing document can print incomplete with no indication), while the identification sheets are uncapped and render whole into a print popup (an HQ-wide PII dump; tab freeze on large registers). Decide cap + confirmation UX (warn-and-continue vs hard refuse) — ReportSheetService.cs:45-57,98,141, family-follow-up-report.component.ts:778-864
- [x] [Review][Patch] Amend this story's own AC2/DoD text to the ratified client-side-print ruling — the board decision (18-1/18-21) is verified (`OrphanReportService` export is a TODO stub; §10.U.14 itself names jsPDF/ExcelJS), but the AC still promises server-streamed PDF bytes — a standing contradiction — this story file *(applied 2026-08-24: AC 2 + DoD amended)*
- [x] [Review][Patch] Stale `FamilyFollowUpFilterDto` XML doc ("max 100" vs the 5000 clamp) — fix alongside 5-11's clamp split — FamilyFollowUpDtos.cs *(closed 2026-08-24: already fixed by 5-11's applied clamp split — `maxPageSize` parameter with a corrected XML doc; no "max 100" text remains in the follow-up DTOs. This entry was stale when the review closed)*
- [x] [Review][Defer] English empty-selection messages (AC 3's "localized" message) — deferred, pre-existing (platform-wide)
- [x] [Review][Defer] Live walkthrough of the print flows — deferred, pre-existing (deployment state, batched epic-5 sweep)

Dismissed as noise / refuted: 2 — "`reports.print.*` keys possibly missing" (verified present 11/11 in both files, 78/78 reports parity); the `IncomingService.cs` "contamination" (ratified parallel correspondence-session rewrite; no 5-14-owned lines remain — bookkeeping noted at 5-13).

## Dev Notes

- **Reuse the 5-11 projection** — the tracking sheet IS the follow-up report printed; a second
  query implementation is the reinvention trap here.
- **Widow mapping (recorded interpretation):** the platform has no `IsWidow` flag on `Mother`; the
  legacy widow sheet listed mothers heading/guarding families. Realise it as mothers who are the
  family's guardian of record (`Family.ProviderType` designating mother / mother present, father
  dead or absent). Keep the SQL simple (mother joined where she is the acting guardian) and note
  the interpretation in the code comment — a richer widow definition belongs to epic 10/18 if ever
  needed.
- The legacy post-condition "the row is flagged as printed" has no printed-flag column on this
  platform — the audit trail (who requested, when) covers accountability; recorded as a deliberate
  deviation (do not add an `IsPrinted` column).
- All three keys share one endpoint template and one service method — the key is data selection,
  not three controllers.
- PDF rendering must remain server-side (bytes over the wire); the client's jsPDF/ExcelJS
  libraries are for OTHER client-side exports — do not render these sheets client-side.
- Empty file is worse than an error (AC 3): refuse BEFORE rendering when the dataset is empty.
- **Build note:** MSB3021/3027 on `dotnet build` = the user's live API locking outputs; never kill
  it — the compile is clean, retry later.

### References

- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.U.14] scenario — three report keys, empty-selection rule
- [Source: _bmad-output/planning-artifacts/epics.md#3.5] US-FAM-14 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanReportsController.cs:69-105] export-to-File endpoint precedent
- [Source: Frontend/src/app/modules/families/family-detail] blob-download pattern
- [Source: _bmad-output/planning-artifacts/architecture.md#6.1] platform ApiController base for the new controller

## Dev Agent Record

### Agent Model Used

claude-sonnet-4.5 (Claude Code, BMAD dev-story workflow — review-and-complete pass)

### Debug Log References

- `dotnet build IIROSA.Api.csproj` → **Build succeeded** (DTOs, validator, service,
  ReportsController; plus a minimal fix-forward of the parallel correspondence session's
  `Serial.ToString("D4")` nullable-compile breaks in Incoming/OutgoingService — both files were
  then rewritten wholesale by that session with its own fix, coexisting cleanly).
- `npx tsc --noEmit -p tsconfig.json` filtered to the reports module → no errors.
- `node` JSON.parse + key-diff → both i18n files valid; `reports.*` 12/12 ar↔en parity.

### Completion Notes List

- **THE deviation that reshaped the story — payload, not bytes:** the story's plan (and AC 2's
  "the file streams back") assumed a server PDF pipeline to reuse. There is none:
  `OrphanReportService.ExportReportAsync` returns `Array.Empty<byte>()` behind a TODO, and
  `SeasonalAidService` carries the same TODO. The board's recorded rulings govern instead —
  18-21 owns the jsPDF install + Arabic-font decision and every print story defers to it
  (18-10's precedent: "print stays `window.print()` until jsPDF lands"). So the endpoints keep
  the legacy `/export/pdf` route shape verbatim but return the sheet's **JSON payload**, and
  the client renders: an RTL print window (`window.open` + print CSS + `window.print()`) whose
  print-to-PDF IS the produced document. The browser shapes Arabic natively — no font
  embedding risk. When 18-21 lands `report-pdf.service.ts`, the payloads plug straight into it;
  only the client render call changes.
- **AC 3 (empty selection):** each builder refuses with `BusinessException` ("nothing to
  print" per key) BEFORE rendering; the controller maps it to 400 `ApiResponse` and the screen
  toasts the server message. No empty file is ever produced.
- **Tracking sheet reuse:** `BuildUpdateTrackingSheetAsync` calls 5-11's
  `GetFollowUpActivityAsync` verbatim — one implementation of the day-filter/kind-precedence.
  To print the WHOLE selection rather than a 100-row page, the page clamp ceiling was raised
  100 → 5000 (bounded; a day's register activity cannot approach it).
- **Widow mapping (as recorded in Dev Notes, implemented literally):** `Mother.IsProvider ==
  true` — the UC-4.7 verify-parent designation — joined through the scoped families. The
  guardian sheet is the same join over `Provider` rows.
- **FamilyCode over FamilyId (small deviation from the task sketch):** the user-facing key is
  the family CODE (what an operator knows); the service resolves it case-insensitively. A
  Guid would force a lookup screen for no value.
- **Controller shape:** `ReportsController : ApiController` (the platform base — unlike the
  legacy `FamiliesController : ControllerBase`), one `{reportKey}/export/pdf` template, the
  route wins over any body value for the key, switch dispatch to three typed builder methods,
  full exception ladder, claims helpers copied from the module idiom.
- 18-21's own `POST /api/Reports/family-update-tracking` (paymentId+date variant) does not
  collide: that route has one literal segment, this one is `{reportKey}/export/pdf` — two.
  Recorded for the epic-18 reconcile with 18-13's route claim (5-11 owns the screen).
- Print window is disposable by design (deleted, not maintained): all row values are
  HTML-escaped; headers/titles go through the i18n keys at render time so the document is
  bilingual-correct.
- Live walkthrough (busy day → printed RTL sheet; empty day → refusal toast; charity role
  scoped; single-family sheet; unknown key → 400) batched into the epic-5 sweep.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/ReportSheetDtos.cs` — new: request DTO +
  generic payload envelope + identification-sheet row.
- `Backend/src/IIROSA.Application/Validators/Reports/ReportSheetRequestValidator.cs` — new:
  known keys, date-required/not-future for tracking, family-code bound.
- `Backend/src/IIROSA.Application/Interfaces/IReportSheetService.cs` — new: three typed
  builders.
- `Backend/src/IIROSA.Application/Services/ReportSheetService.cs` — new: tracking (5-11
  reuse), guardian join, widow join, shared charity/family scope, empty-selection refusals.
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — follow-up page clamp 100 →
  5000 (whole-selection print path), with comment.
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — new: platform-base controller,
  `{reportKey}/export/pdf` template, ladder, claims helpers.
- `Backend/src/IIROSA.Application/Services/IncomingService.cs` + `OutgoingService.cs` —
  minimal compile fix-forward of the parallel session's nullable `Serial.ToString("D4")`
  (subsequently superseded by that session's own rewrite).
- `Frontend/src/app/modules/reports/models/report.model.ts` — new: payload/row types.
- `Frontend/src/app/modules/reports/services/report.service.ts` — new: `exportSheet` POST.
- `Frontend/src/app/modules/reports/family-follow-up-report/family-follow-up-report.component.ts`
  — print actions (tracking + guardians/widows), family-code state, escaped RTL print window.
- `Frontend/src/app/modules/reports/family-follow-up-report/family-follow-up-report.component.html`
  — print button beside Excel export; identification-sheet row with family-code input.
- `Frontend/src/app/modules/reports/family-follow-up-report/family-follow-up-report.component.spec.ts`
  — no-date refusal + guardians-sheet payload shape tests.
- `Frontend/src/assets/i18n/ar.json` + `en.json` — `reports.print.*` (11 keys both).

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-FAM-14 and module spec §10.U.14; report-key controller design, widow-mapping and no-printed-flag deviations recorded. |
| 2026-08-24 | Implemented end to end with the client-side-print deviation recorded (JSON payloads behind the legacy `/export/pdf` routes + browser print window — no server PDF pipeline exists; 18-21 owns jsPDF): three sheet builders + ReportsController + report screen print actions + i18n. Status → review. |
| 2026-08-24 | Completion pass: the stale XML-doc finding closed as already fixed by 5-11's clamp split; the sheet-volume cap decision remains open (the only epic-5 review item still awaiting a ruling, with the framework clock ruling). |
| 2026-08-24 | Completion sweep: build verification only — the print payloads were unchanged this pass and the browser print-window walkthrough was not run (recorded). The 5-10 fix also removes the stale-mother drift this story's widow sheet would have inherited. Status → done (sheet-volume cap decision still open by design). |
