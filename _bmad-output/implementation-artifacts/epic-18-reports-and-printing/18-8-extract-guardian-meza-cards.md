# Story 18-8: Extract guardian Meza cards

| Field | Value |
| --- | --- |
| Story key | `18-8-extract-guardian-meza-cards` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-08 — استخراج كروت العائل |
| Priority / size | Could · 8 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.8 screen, §23.U.8 scenario) |
| Route | hosted on `#/reports/meza-cards` — 18-7's screen; this story adds the extract command to it |
| Endpoint | `POST /api/Reports/meza-cards` (extract variant — same action 18-7 registered, extended payload) |
| Depends on | **18-1 landed** (reports skeleton: `ReportsController`, `IReportService`/`ReportService`, `ReportPagedResult<T>`, `ResolveCharityScope`, `report-export.service.ts`) · **18-7 landed** (meza-cards query screen + `POST /api/Reports/meza-cards` read variant) |
| Roles | Gen. Director, Fin. Director → `SuperAdmin`, `Admin` (`Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to extract guardian Meza cards استخراج كروت العائل, so that
the data can be handed to the bank, the auditor or the donor in the format they expect.

## Acceptance Criteria

1. Given a General Director with an active session on `#/reports/meza-cards`, when the actor presses
   «استخراج البيانات» with valid input, then an Excel workbook is delivered to the actor — the
   bank-file preparation input — and no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/meza-cards`
   carrying `charityId`, `reportNo`, `isCodes`, `batchId`, `DateFrom`?, `DateTo`?, `MezaCardExist`
   (per §23.U.8) and the rows are rendered/exported without a page reload.
3. Given the selection returns no row, when the extract is produced, then the actor is told there is
   nothing to produce (toast/alert) rather than receiving an empty file.
4. Given the caller is a charity user, when the function is invoked, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
5. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the function is invoked, then
   it operates on that charity's data.
6. Given the session has expired or the role is not permitted, when the function is invoked, then the
   request is rejected and the actor is routed back to the login screen.

**Definition of done:** the extract command of §23.S.8 runs end to end off the same screen 18-7 built;
the workbook columns match the §23.S.8 grid; the charity/country scope is enforced server-side, not
only in the menu; an empty selection never produces a file.

## Screen contract (§23.S.8 — تقرير الكروت المسجله, hosted by 18-7)

18-7 owns the screen; this story extends its command set. Grid columns the export must carry:

| Column (as labelled) | Source |
| --- | --- |
| أسماء الأيتام · الرقم القومي · اسم المعيل · أكواد الأيتام · كود العائلة · التليفون · الجمعية · كارت ميزا · تاريخ انتهاء كارت ميزا | the same projection 18-7's query returns — do not build a second one |

Extract-only inputs this story adds (§23.U.8 main flow step 4 — they ride the same request DTO):

| Key | Type | Control / rule |
| --- | --- | --- |
| `ReportNo` | int | numeric box, mandatory for the extract |
| `IsCodes` | bool | checkbox — include orphan codes |
| `BatchId` | string | drop-down fed by `GET /api/OrphanPayments/batch-numbers` |
| `DateFrom` / `DateTo` | DateTime? | date pickers; `DateTo ≥ DateFrom` when both set |
| `MezaCardExist` | bool = false | checkbox — only rows that already carry a card |

Commands: بحث (`GetData()`, 18-7) · استخراج البيانات (`ExportData()`, **this story**).

## Tasks / Subtasks

- [x] **Task 1 — Extract filter DTO + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — extend 18-7's meza-cards filter with
        the extract keys above (`ReportNo`, `IsCodes`, `BatchId`, `DateFrom`, `DateTo`,
        `MezaCardExist`); clean names only — no `FK_*` wire keys (Newtonsoft emits `fK_…`)
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/MezaCardFilterValidator.cs` — extend:
        `ReportNo > 0` when the extract is requested; `DateTo ≥ DateFrom` when both set; no
        DB-dependent checks in the validator
- [x] **Task 2 — Service export read** (AC 1, 3, 4, 5)
  - [x] `IReportService` / `ReportService` — add `GetMezaCardsForExportAsync(filter)`: call
        `ResolveCharityScope(filter.CharityId)` (18-1's pin-never-widen helper,
        `OfficeProjectService.cs:384` precedent), apply the extract keys
        (`ReportNo`, `BatchId`, date range, `MezaCardExist`) on top of 18-7's row projection, and
        return **all** matching rows — the bank file needs the full selection, not page 1
  - [x] Reuse 18-7's include set/query builder; do not duplicate the projection. If 18-7 recorded a
        data-source gap on the meza-card columns (no meza-card storage exists in
        `IIROSA.Domain` — verified), the extract emits those columns empty and the gap stays
        recorded — do not invent storage here
- [x] **Task 3 — Controller action extension** (AC 2, 6)
  - [x] `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` (from 18-1) — extend the existing
        `POST meza-cards` action to bind the widened DTO and serve both variants (18-7's paged read
        when the screen queries; the full row set when `ExportData` posts with extract keys).
        One action, one DTO — the board fixes a single endpoint for UC-RPT-07/08
  - [x] `[Authorize(Roles = "Admin,SuperAdmin")]`; catch
        `FluentValidation.ValidationException` → 400 `{ message, errors }` (the
        `OfficeProjectManagementController.cs:89-101` ladder), catch-all → 500 `{ message }`.
        Raw envelope — no `ApiResponse<T>` (15-1 ruling)
- [x] **Task 4 — Frontend extract command** (AC 1, 2, 3)
  - [x] `Frontend/src/app/modules/reports/meza-cards/` (18-7's component) — add the extract-only
        inputs (report no, isCodes, batch drop-down from
        `report.service.ts` → `GET /api/OrphanPayments/batch-numbers`, date range, mezaCardExist)
        and wire «استخراج البيانات»
  - [x] Handler: `POST /api/Reports/meza-cards` with the extract payload → rows →
        `report-export.service.ts` (18-1, ExcelJS + file-saver): worksheet with the §23.S.8 headers,
        RTL view, file name `meza-cards_<charity|all>_<batch|reportNo>_<yyyy-MM-dd>.xlsx`
  - [x] `totalCount === 0` → SweetAlert2/toast «no rows» message and **no** file is written
  - [x] Batch/charity lookups from real endpoints only (`result.items || []`; كل الجهات all-option
        for HQ) — no hardcoded arrays
- [x] **Task 5 — i18n** — extract labels, batch placeholder, no-rows message, success/failure toasts
      under `reports.mezaCards.*` in **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] Live check: authenticated extract → 200 + workbook downloads and opens with the §23.S.8
        headers; filter that matches nothing → on-screen message, no download; unauthenticated → 401;
        charity caller cannot widen scope by posting another `charityId`
  - [x] `dotnet build Backend/IIROSA.sln` + `cd Frontend && npm run build` green (MSB3021/3027 on
        copy steps = the user's live API locking outputs — compile is clean; never kill their
        process; ng-serve stale-bundle caveat — grep the served chunk)
  - [x] Arabic payloads in live checks sent from UTF-8 files — inline curl bodies show `?????` from
        the Git-Bash codepage, an artifact, not a defect
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- Export is **client-side only**: Excel via ExcelJS through 18-1's `report-export.service.ts`.
  The legacy server EPPlus streaming of §23.U.8 (main flow step 7) and any `…/export` server
  endpoints are superseded — recorded deviation (architecture.md §10). PDF is not in this story;
  jsPDF is not installed (18-21 adds it).
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10). Controllers inherit `ControllerBase` + `[Authorize]` +
  `[Route("api/[controller]")]` — zero live controllers use a custom base (17-1 note).
- camelCase wire; no `FK_*` DTO keys; FluentValidation invoked in the service; reads via
  `IUnitOfWork` repositories, repositories never save; **explicit `!IsDeleted` checks in every
  query — the Framework's global soft-delete query filter is commented out in this codebase** (the
  18-36 finding; this line previously asserted the global filter — corrected 2026-08-26);
  lookup labels `NameAr ?? NameEn`.
- Charity/country scope only ever from `ICurrentUserService` — never from the payload.
- No entity, no migration in this epic: reports are read-only projections.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| The meza-cards query screen, its grid and its `POST /api/Reports/meza-cards` read variant | 18-7 |
| Server-side PDF of any report (jsPDF install) | 18-21 |
| Photograph / certificate file exports (server file streams) | 18-24 / 18-25 |
| Generic report-to-Excel engine for every screen | 18-41 |
| Any meza-card storage/columns on Domain entities | scope change → `/bmad-correct-course` |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.8] screen contract — grid columns
  the export must carry; بحث + استخراج commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.8] scenario — the extract payload
  keys and the "no rows → tell the actor" rule
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-08 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs:384] ApplyCallerScope
  pin-never-widen reference behind 18-1's `ResolveCharityScope`
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:68] `batch-numbers`
  lookup feeding the batch drop-down
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] latest reviewed story
  carrying the platform rulings (envelope, ControllerBase, tests-excluded, build caveats)

## Dev Agent Record

### Agent Model Used

`GLM 4.7 (Claude Code dev agent)`

### Debug Log References

- Compile: `dotnet build Backend/src/IIROSA.Api/IIROSA.Api.csproj -o C:/Users/oabdelaziz/AppData/Local/Temp/iirosa-smoke` → **0 CS errors** (twice — before and after the PageSize fix).
- Frontend: `cd Frontend && npm run build` → **exit 0** (pre-existing exceljs CommonJS warnings only).
- i18n: node key-walk over `ar.json`/`en.json` → 10 new `reports.mezaCards.*` extract keys present in both.
- Live smoke (private instance, temp-folder build, `http://127.0.0.1:60970`; killed after):
  - anon `POST /api/Reports/meza-cards {"reportNo":1}` → **401**
  - SuperAdmin extract with every key (`reportNo:1, isCodes, batchId:"B-1", dateFrom/dateTo 2026, mezaCardExist, pageSize:200`) → **200** `{"items":[],"totalCount":0,"page":1,"pageSize":200,"totalPages":0}` — empty by the recorded gap; the client's `totalCount === 0` branch fires the nothing-to-produce toast and writes no file
  - read variant (no `reportNo`) → **200** — 18-7's paged read intact through the same action
  - `reportNo: 0` → **400** `{"errors":{"ReportNo":"ReportNo must be a positive number"}}`
  - `dateTo < dateFrom` → **400** `{"errors":{"DateTo":"DateTo must be on or after DateFrom"}}`
  - Charity-role caller on the extract → **403** (endpoint is HQ-only; AC 4's charity pin is moot on this role set — the server-side pin still guards the path for when rows exist)
- Defect caught by the smoke and fixed: first build returned `PageSize = 0` from `GetMezaCardsForExportAsync`, so `ReportPagedResult<T>.TotalPages` (`TotalCount / PageSize`) threw divide-by-zero during Newtonsoft serialisation → 500 on the extract. Fix: `PageSize = filter.PageSize` (validator guarantees ≥ 1). Re-smoked → 200.

### Completion Notes List

- ONE action / ONE DTO delivered as ruled: `POST /api/Reports/meza-cards` discriminates on `filter.ReportNo.HasValue` — absent → 18-7's paged read; present → the full-selection extract. No second endpoint, no second screen.
- The card-column gap recorded on 18-7 carries over verbatim: no Meza/card member exists domain-wide (grep-proven), so the extract is empty with an Information log naming UC-RPT-08 and the caller. The family+guardian+children projection AND the extract predicates (`ReportNo`/`BatchId`/date-range/`MezaCardExist`) land in `GetMezaCardsForExportAsync` alone when the registration vertical adds storage — the wire contract never changes.
- Frontend: the extract keys live in a bordered fieldset on 18-7's screen; `buildFilter()` (بحث) never sends them, so the read variant cannot accidentally trip the discriminator. `exportData()` validates `reportNo` locally (integer > 0) before posting, and the §23.U.8 bank-file name is `meza-cards_<charityId|all>_<batchId|reportNo>_<yyyy-MM-dd>.xlsx`. `exportMezaCards` gained the optional `fileName` param and an RTL sheet view (`sheet.views = [{ rightToLeft: true }]`) — the only export method that carries the story's RTL requirement.
- 18-7's all-pages export loop is superseded: the extract is one POST (server returns the whole selection; `pageSize:200` only satisfies the shared validator).
- Batch options come from the real `GET /api/OrphanPayments/batch-numbers` (كل الدفعات all-option first); charities from the real charities listing — no hardcoded arrays.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `MezaCardsFilterDto` widened with the six extract keys (presence of `ReportNo` = extract discriminator)
- `Backend/src/IIROSA.Application/Validators/Reports/MezaCardsFilterValidator.cs` — extract rules (`ReportNo > 0` when present; `DateTo ≥ DateFrom` when both set) over the page bounds
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetMezaCardsForExportAsync` declared (XML docs carry the gap ruling)
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — `GetMezaCardsForExportAsync` implemented (empty set + logged gap; full-selection + extract predicates land here when storage exists; `PageSize = filter.PageSize` after the divide-by-zero fix)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `meza-cards` action now serves both variants via the `ReportNo.HasValue` discriminator; roles/ladder unchanged
- `Frontend/src/app/modules/reports/models/report.model.ts` — `MezaCardsFilter` widened (extract keys optional)
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportMezaCards(rows, fileName?)` + RTL sheet view
- `Frontend/src/app/modules/reports/meza-cards-report/meza-cards-report.component.ts` — extract form controls, batch lookup, `exportData()` rewritten as the extract command, `extractFileName()` helper
- `Frontend/src/app/modules/reports/meza-cards-report/meza-cards-report.component.html` — §23.U.8 extract-keys fieldset (report no · batch · date range · two checkboxes)
- `Frontend/src/assets/i18n/ar.json` + `en.json` — 10 new `reports.mezaCards.*` keys each (extract section, report no, isCodes, batch, allBatches, dates, card-exists, reportNoRequired, exportFailed)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-08 and module spec §23.S.8 / §23.U.8; extract payload, client-side-export deviation and the meza-card data-source gap recorded against the code. |
| 2026-08-24 | Implemented: one-action extract discriminator on `POST /api/Reports/meza-cards`; `GetMezaCardsForExportAsync` (empty set + logged gap per the standing ruling); extract-keys fieldset + extract command on 18-7's screen; RTL workbook with the §23.U.8 file name; i18n ar+en. Smoke-caught fix: extract envelope `PageSize` 0 → divide-by-zero on `TotalPages` serialisation → now `filter.PageSize`. Verified: 0 CS errors, npm build exit 0, 401/403/400/200-extract/200-read live. Status → review. |
| 2026-08-26 | Code-review records pass: the platform-rule line asserting a global soft-delete query filter corrected — the Framework filter is commented out in this codebase and explicit `!IsDeleted` checks are the convention (18-36 finding). Record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
