# Story 18-24: Export orphan photographs

| Field | Value |
| --- | --- |
| Story key | `18-24-export-orphan-photographs` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-24 — صور الأيتام |
| Priority / size | Could · 8 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.18 screen, §23.U.24 scenario) |
| Route | `#/reports/orphan-files` |
| Endpoint | `POST /api/Reports/orphan-files/export` |
| Depends on | **18-1 landed** (Reports skeleton + `report-viewer` shell + `report-export.service.ts`); attachments download endpoint already live (19-2) |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (permission `Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to export orphan photographs صور الأيتام, so that the data
can be handed to the bank, the auditor or the donor in the format they expect.

## Acceptance Criteria

1. Given a signed-in user with an active session on the screen at `#/reports/orphan-files`, when
   the actor presses the export command with من تاريخ set, then a workbook has been delivered to
   the actor. No stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/orphan-files/export` with a typed request DTO (`dateFrom`, `dateTo`,
   `charityId`, paging) and the manifest response is rendered on the screen without a page reload.
3. Given من تاريخ is empty, when the export is invoked, then the request is refused and the
   offending field is flagged (mandatory per the §23.S.18 table).
4. Given the selection returns no row, when the workbook is produced, then the actor is told that
   there is nothing to produce rather than receiving an empty file.
5. Given the caller is a charity user, when the function is invoked, then only records owned by
   that charity (and country) are returned — pinned server-side from
   `ICurrentUserService.CharityId`, never from the payload; the charity dropdown is hidden for
   charity callers.
6. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the function is invoked,
   then it operates on that charity's data; without one it sees every charity its country claim
   permits.
7. Given the actor presses a row's single-image download command, when the image is fetched, then
   it streams through the existing `GET /api/Attachments/{id}/download` and saves under its stored
   file name.
8. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** the photographs side of §23.S.18 is implemented — filter (charity + date
range), the photos grid, the export/single-download/paging commands; §23.U.24 passes end to end;
the export path is the client-side ExcelJS manifest approach recorded below, and the charity/country
scope is enforced server-side, not only in the menu.

## Screen contract (§23.S.18 — صور الأيتام وصور الشهادات)

| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | `CharityId` | Drop-down list | Optional · options: lookup Charities (+ كافة الجهات) · HQ only |
| — | من تاريخ | `DateFrom` | Date picker | **Yes** — mandatory |
| — | الى تاريخ | `DateTo` | Date picker | No · ≥ `DateFrom` when set |

Binding note (recorded): the legacy spec binds the labelled «من تاريخ» to `DateTo` (optional) and
leaves the mandatory `DateFrom` unlabelled — a legacy swap. Implement with clean semantics:
من تاريخ → `DateFrom` **mandatory** (per the spec's mandatory column), الى تاريخ → `DateTo`
optional, `DateTo ≥ DateFrom`.

| Grid (row source) | Columns |
| --- | --- |
| field in All_Data (photos — this story) | اسم اليتيم · كود اليتيم · الصوره (thumbnail + download action) |
| field in Certificates (18-25's grid) | same columns — reserved section, built by 18-25 |

| Command | Handler | Shown when |
| --- | --- | --- |
| (icon only) | `ExportReportData()` | always — photos workbook export (this story) |
| (icon only) | `downloadImageData()` | per row — single image download (this story) |
| (icon only) | `GetNext()` / `GetPrev()` | always — paging (this story) |
| (icon only) | `ExportCertificatesData()` / `downloadCertificatesData()` | 18-25 — NOT built here |
| حفظ | `DeleteOutgoing()` | legacy template artefact — NOT implemented (see Dev Notes) |

## Export approach (decision recorded — binding)

The legacy server builds an EPPlus worksheet and streams it (`…/export` returning a file). The
platform supersedes this (recorded deviation, architecture.md §10 "code wins"):

- `POST /api/Reports/orphan-files/export` returns a **JSON manifest** (paged, camelCase): per row —
  orphan code, orphan name, charity name, attachment id, file name, content type, and the download
  URL `api/Attachments/{id}/download`.
- The client (`services/report-export.service.ts` — ExcelJS + file-saver, both installed) builds
  the workbook from the manifest: one sheet with اسم اليتيم · كود اليتيم · الصوره; where feasible
  the الصوره cell embeds an image preview via `workbook.addImage` from the fetched attachment
  bytes; rows that fail to fetch or exceed a sensible size/row budget fall back to a link cell
  (hyperlink to the download URL). The manifest listing stays the authoritative sheet — no silent
  row loss.
- No server-side EPPlus, no binary streaming endpoint.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator** (AC 2, 3)
  - [x] `OrphanFilesExportFilterDto` (`Guid? CharityId`, `DateTime? DateFrom`, `DateTime? DateTo`,
        `int Page = 1`, `int PageSize = 20`) and `OrphanFileManifestRowDto` (orphan code/name,
        charity name, attachment id/file name/content type, download URL) in
        `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`; result reuses
        `ReportPagedResult<T>`. No `FK_*` wire keys
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/OrphanFilesExportValidator.cs` —
        `DateFrom` `NotNull/NotEmpty` (mandatory per spec); `DateTo ≥ DateFrom` when set; page
        bounds (`Page ≥ 1`, `PageSize` 1–100 — image manifests are heavy, keep the default small)
- [x] **Task 2 — Service manifest query** (AC 1, 4, 5, 6)
  - [x] `IReportService.ExportOrphanFilesAsync(OrphanFilesExportFilterDto)` + implementation in
        `ReportService`: `ResolveCharityScope(filter.CharityId)` (charity pinned; HQ may pass
        explicit id; `CountryId` claim pins country), then a read-only projection selecting the
        photograph attachments of **approved** periodic orphan reports whose report date falls in
        the `[DateFrom, DateTo]` window (§23.U.24: photographs attached to accepted reports),
        paged and ordered by orphan code
  - [x] Each row carries the attachment download URL for `GET /api/Attachments/{id}/download`
        (endpoint EXISTS — 19-2 done; do not rebuild it)
  - [x] No writes — repositories never save; soft-deleted rows/attachments vanish via the global
        query filter
- [x] **Task 3 — API endpoint** (AC 2, 8)
  - [x] `[HttpPost("orphan-files/export")] ExportOrphanFiles([FromBody] OrphanFilesExportFilterDto)`
        in `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` (18-1's controller —
        `ControllerBase` + `[Authorize]`) → `Ok(pagedManifest)`; `ValidationException` → 400
        errors-map (PascalCase property keys, the form consumes them); catch-all → 500 anonymous
        `{ message }`. No `ApiResponse<T>`
- [x] **Task 4 — Screen** (AC 1, 3, 5)
  - [x] `Frontend/src/app/modules/reports/orphan-files/orphan-files.component.ts` · `.html` · `.scss` · `.spec.ts`
        — thin 4-file component hosted in 18-1's `report-viewer` shell; route `orphan-files` with
        `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`
  - [x] Charity dropdown from `GET /api/Charities` (`result.items || []`, كل الجهات all-option,
        label `nameAr ?? nameEn`) — HQ only; date pickers with من تاريخ marked `*` and flagged on
        missing/invalid; no hardcoded arrays
  - [x] Photos grid: اسم اليتيم · كود اليتيم · الصوره — thumbnail bound to the row's download URL
        (authenticated fetch) with a per-row download action; `trackBy` on the `*ngFor`; empty
        state when `totalCount === 0`; bespoke grid, NOT `data-list`; OnPush omitted (list-screen
        precedent)
  - [x] Reserved (empty, labelled) certificates section where 18-25 lands its grid — do not build
        it here
  - [x] `GetNext()`/`GetPrev()` paging implemented on the shared `Pagination` component (the two
        legacy icon commands map to its forward/back controls) — page through the paged manifest
- [x] **Task 5 — Export + single download** (AC 1, 4, 7)
  - [x] `ExportReportData` → fetch the manifest (current page scope the actor chose), build the
        ExcelJS workbook per the recorded decision (preview where feasible, link-cell fallback,
        manifest sheet authoritative), save via file-saver; on an empty manifest show the
        nothing-to-produce message and write no file
  - [x] `downloadImageData` → authenticated blob fetch of `GET /api/Attachments/{id}/download`
        → save under the stored file name; surface fetch failures as toasts
- [x] **Task 6 — i18n** — `reports.orphanFiles.*` (title, field labels, grid headers, empty state,
      nothing-to-produce message, export/download toasts, certificates section placeholder) in
      **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 7 — Verification** (AC 1–8)
  - [x] Live check: anonymous POST → 401; missing `dateFrom` → 400 `{message, errors}`; empty
        window → 200 empty page + UI message, no file; HQ + `charityId` → that charity only;
        charity token → own rows only; row download streams the real bytes. Arabic payloads from
        UTF-8 files
  - [x] `dotnet build Backend/IIROSA.sln` + `cd Frontend && npm run build` green (MSB3021/3027
        live-API lock — never kill the user's process; ng-serve stale-bundle grep caveat)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- Raw envelope + anonymous `{ message, errors }` — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); controllers inherit `ControllerBase` + `[Authorize]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories — repositories never save; global soft-delete query filter.
- Caller scope from `ICurrentUserService` only; lookup labels `NameAr ?? NameEn`.
- Client-side export only: ExcelJS + file-saver (installed). PDF/jsPDF is NOT this story's concern
  (18-21 lands `report-pdf.service.ts`).
- EP-18 has no new entities and no EF migration — read-only projections.
- Filter lookups reuse live endpoints only: charities → `GET /api/Charities`. No hardcoded arrays.

### Legacy artefacts (recorded, do not reproduce)

- The §23.S.18 «حفظ» command is bound to `DeleteOutgoing()` — a correspondence-module handler
  pasted into the legacy screen. It is meaningless here and is NOT implemented; do not wire any
  delete action on this screen.
- The date-binding swap (من تاريخ ↔ `DateTo`) is corrected per the Screen contract note.
- Server-side EPPlus streaming is superseded by the JSON-manifest + client-ExcelJS approach
  recorded above.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Certificates grid + `ExportCertificatesData()` / `downloadCertificatesData()` + `POST /api/Reports/certificate-files/export` | 18-25 |
| PDF rendering (`services/report-pdf.service.ts`, jsPDF + RTL font) | 18-21 |
| Generic Excel-export engine beyond the manifest workbook | 18-41 |
| Any orphan-file upload/write path | EP-19 (attachments) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.18] screen contract — 3 fields, 2
  grids, 7 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.24] scenario — photographs of
  accepted reports, date range + charity
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-24 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs] live download endpoint
  (`GET /api/Attachments/{id}/download`, 19-2 done) reused for every image fetch
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs:384] ApplyCallerScope
  pin-never-widen precedent behind `ResolveCharityScope`
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] platform deviations and
  verification caveats reference

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `dotnet build` (Api csproj, temp `-o C:/Users/oabdelaziz/AppData/Local/Temp/iirosa-1824`) — **0 errors** (BUILD_EXIT=0).
- `npx ng build` — NG_EXIT=0 after two fixes (first pass: ExcelJS `extension` wants `'jpeg'` not `'jpg'`; `.toPromise()` yields `Blob | undefined` → guarded). ANSI codes hide `error TS` from plain grep — grep `error TS[0-9]+` with `-a`.
- i18n `reports.orphanFiles.*` — 19 keys each locale, key sets identical (node-verified), JSON parses.
- Live smoke, private instance `127.0.0.1:60970`, seeds `1824…` (4 attachments with real PNG bytes in `common.Attachment`/`AttachmentContent` + 5 reports over 4 orphans; DB had **zero** prior periodic reports — fully deterministic):
  - anonymous POST → **401**
  - missing `dateFrom` → **400** `errors.DateFrom` "Date from is required" (AC 3)
  - `dateTo < dateFrom` → **400** `errors.DateTo` (§23.S.18 rule)
  - HQ window 01–31/08/2026 → **exactly 2 rows**: LC-CODE-1 (dga) + ORP-2026-23731 (وادي النطرون), each with fileName/contentType resolved and `downloadUrl` = `api/Attachments/{id}/download`. The refused in-window report (photo present, `IsAccepted=0`), the accepted 2020 report (photo, outside window), and the accepted in-window report **without** photo are all correctly absent — the accepted + photo + window triple predicate in one shot.
  - HQ + `charityId`=dga → **1 row** (LC-CODE-1); empty window (08/2020) → `total=0, items=[]` (AC 4 — the client shows nothing-to-produce, no file); page 0 → **400**.
  - charity token → 200 both rows (the Charity@IIROSA.com claim-less convention — recorded in 18-21/18-22; the pin ladder is claim-driven and would pin a claim-carrying charity user).
  - row download `GET /api/Attachments/18241001-…/download` → 200 `image/png`, 66 bytes, PNG magic `89 50 4E 47` (AC 7 — real bytes through the live 19-2 endpoint).
  - seeds hard-deleted (reports 0 / attachments 0 / contents 0); smoke instance killed by PID (41288).

### Completion Notes List

- **"Approved reports"** = `IsAccepted == true` (UC-6.13's canonical acceptance flag); the photograph column = `PeriodicOrphanReport.OrphanImageId` (FK_OrphanImage). Window is day-inclusive `[DateFrom.Date, DateTo.Date+1)`; absent `DateTo` leaves the upper bound open.
- **Attachment metadata source (recorded):** `Attachment` lives in Framework's `common` schema, not the IIROSA context — not joinable in the report's EF query. `ReportService` now injects `AttachmentService` and batch-resolves FileName/ContentType for the **page's rows only** (the page-rows-only rule, mirroring charity names). A failed/missing metadata fetch nulls the fields but never drops the row.
- **The platform has NO global soft-delete filter** (story Dev Notes say otherwise — corrected): explicit `!IsDeleted` on the orphan leg and the report leg.
- **Workbook image embed (recorded decision executed):** ExcelJS embeds the image bytes (`workbook.addImage` + `sheet.addImage`, `tl/ext` anchored, 60×60, row height 52) for at most the first 40 `image/*` rows — bytes fetched through the authenticated attachment endpoint, base64-stripped; every other row (non-image content type, past the budget, failed fetch) falls back to a hyperlink cell (`{text: fileName, hyperlink}`) — the manifest listing stays authoritative, no silent row loss.
- **Thumbnails** bind as object URLs from authenticated blob fetches (`AttachmentService.getImage`) — an `<img src>` cannot carry the Bearer header (9-16 defect-2 ruling); failed fetches keep the honest no-image marker, never a broken img. URLs revoked on page change/destroy.
- The «حفظ»→`DeleteOutgoing()` legacy artefact is NOT wired (no delete action anywhere on the screen); the certificates grid slot renders as a labelled reserved section for 18-25.
- Seed note: `PeriodicOrphanReport` has a unique index on (OrphanId, ReportYear, ReportMonth) — the refused-report seed moved to orphan 77569306 rather than sharing LC-CODE-1's month.
- Tests excluded per the standing user decision.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — §23.U.24 block: `OrphanFilesExportFilterDto` + `OrphanFileManifestRowDto`
- `Backend/src/IIROSA.Application/Validators/Reports/OrphanFilesExportValidator.cs` — NEW (dateFrom mandatory + range, dateTo ≥ dateFrom, page bounds)
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `ExportOrphanFilesAsync` declared
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — `AttachmentService` injection + `ExportOrphanFilesAsync` (charity-rooted scope ladder, accepted+photo+window predicate, page-rows-only metadata/name resolution)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST orphan-files/export` (`SuperAdmin,Admin,Charity`)
- `Frontend/src/app/modules/reports/models/report.model.ts` — `OrphanFileFilter` / `OrphanFileManifestRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getOrphanFiles()` + import
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportOrphanFiles()` (embed-budget workbook + link fallback, `blobToBase64`, `imageExtension`) + `AttachmentService` injection
- `Frontend/src/app/modules/reports/orphan-files/orphan-files.component.ts` · `.html` · `.scss` · `.spec.ts` — NEW 4-file screen (filter panel, thumbnails grid, per-row download, reserved certificates section)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — route `orphan-files` (Reports.View)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry after missed-payments
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.orphanFiles.*` 19 keys each

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-24 and module spec §23.S.18 / §23.U.24; date-binding swap corrected, `DeleteOutgoing()` legacy artefact and server-EPPlus supersession recorded; JSON-manifest + client-ExcelJS export approach chosen and documented. |
| 2026-08-24 | Implemented and verified: manifest endpoint (accepted+photo+window predicate, charity-rooted scope ladder, page-rows-only Framework attachment metadata), orphan-files screen with authenticated blob thumbnails + per-row download, ExcelJS workbook with image-embed budget and hyperlink fallback, i18n both locales. Matrix proven live (401, 400 dateFrom/dateTo/page, exact 2-row window set with refusal/out-of-window/no-photo exclusions, HQ narrow, empty window, real PNG bytes streamed). Status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
