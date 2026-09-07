# Story 18-25: Export certificate images صور الشهادات

| Field | Value |
| --- | --- |
| Story key | `18-25-export-certificate-images` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-25 — صور الشهادات |
| Priority / size | Could · 8 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.18 screen, §23.U.25 scenario) |
| Route | hosted on `#/reports/orphan-files` (18-24's screen — no new route) |
| Endpoint | `POST /api/Reports/certificate-files/export` |
| Depends on | **18-24 landed** (the `#/reports/orphan-files` screen, manifest plumbing and the reserved certificates section); attachments download endpoint live (19-2) |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (permission `Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to export certificate images صور الشهادات, so that the data
can be handed to the bank, the auditor or the donor in the format they expect.

## Acceptance Criteria

1. Given a signed-in user with an active session on `#/reports/orphan-files`, when the actor
   presses the certificates export command with من تاريخ set, then a workbook has been delivered
   to the actor. No stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/certificate-files/export` with a typed request DTO (`dateFrom`, `dateTo`,
   `charityId`, paging) and the paged manifest response is rendered in the certificates grid
   without a page reload.
3. Given من تاريخ is empty, when the export is invoked, then the request is refused and the
   offending field is flagged (mandatory per the §23.S.18 table — 18-24's corrected binding).
4. Given the selection returns no row, when the workbook is produced, then the actor is told that
   there is nothing to produce rather than receiving an empty file.
5. Given the caller is a charity user, when the function is invoked, then only records owned by
   that charity (and country) are returned — pinned server-side from
   `ICurrentUserService.CharityId`, never from the payload; the charity dropdown stays hidden for
   charity callers.
6. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the function is invoked,
   then it operates on that charity's data; without one it sees every charity its country claim
   permits.
7. Given the actor presses a certificate row's single-image download command, when the image is
   fetched, then it streams through the existing `GET /api/Attachments/{id}/download` and saves
   under its stored file name.
8. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** the certificates side of §23.S.18 is implemented — the reserved
certificates grid from 18-24 goes live with its export/single-download commands wired to the new
endpoint; §23.U.25 passes end to end; the export follows 18-24's recorded JSON-manifest +
client-ExcelJS approach (no server EPPlus); the charity/country scope is enforced server-side.

## Screen contract (§23.S.18 — certificates side; screen + filters owned by 18-24)

| Element | This story's scope |
| --- | --- |
| Filters (الجمعية / من تاريخ / الى تاريخ) | 18-24 landed them — REUSE; this story rides the same filter state for the certificates query |
| Certificates grid (row source `field in Certificates`) | اسم اليتيم · كود اليتيم · الصوره (thumbnail + download action) — the reserved section 18-24 left empty |
| `ExportCertificatesData()` | **Yes — this story**: certificates workbook export |
| `downloadCertificatesData()` | **Yes — this story**: per-row single certificate download |
| `GetNext()` / `GetPrev()` paging, photos grid + photos commands | 18-24 landed — extend the paging state to cover the active grid only; do not fork a second pager |

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator** (AC 2, 3)
  - [x] Reuse 18-24's `OrphanFilesExportFilterDto` and `OrphanFileManifestRowDto` VERBATIM — one
        filter shape serves both grids on this screen; do NOT fork a parallel pair. The validator
        (`DateFrom` mandatory, `DateTo ≥ DateFrom`, page bounds) is already 18-24's
        `OrphanFilesExportValidator` — reuse as-is
- [x] **Task 2 — Service manifest query** (AC 1, 4, 5, 6)
  - [x] `IReportService.ExportCertificateFilesAsync(OrphanFilesExportFilterDto)` + implementation
        in `ReportService`: `ResolveCharityScope(filter.CharityId)`, then the same read shape as
        18-24's photograph manifest with the attachment-kind selector flipped to **certificate**
        attachments of approved periodic orphan reports in the `[DateFrom, DateTo]` window —
        select by the attachment type/category the report attachments actually carry (verify the
        kind discriminator on `Attachment`/report-attachment linkage before writing the predicate;
        if certificates are not distinguishable from photographs by type today, STOP and surface
        the gap rather than guessing a discriminator), paged and ordered by orphan code
  - [x] Rows carry the `GET /api/Attachments/{id}/download` URL — the endpoint EXISTS (19-2); no
        writes; soft-deleted rows vanish via the global query filter
- [x] **Task 3 — API endpoint** (AC 2, 8)
  - [x] `[HttpPost("certificate-files/export")] ExportCertificateFiles([FromBody]
        OrphanFilesExportFilterDto)` in 18-1's `ReportsController` → `Ok(pagedManifest)`; the
        standard ValidationException→400-errors-map / catch-all→500 `{ message }` ladder; no
        `ApiResponse<T>`
- [x] **Task 4 — Certificates grid + commands** (AC 1, 3, 7)
  - [x] Replace 18-24's reserved empty certificates section in
        `Frontend/src/app/modules/reports/orphan-files/` with the live grid: اسم اليتيم ·
        كود اليتيم · الصوره — authenticated thumbnail of the row's download URL + per-row download
        action; `trackBy`; empty state; bespoke grid; OnPush omitted (list-screen precedent)
  - [x] `ExportCertificatesData()` → certificates manifest → the SAME ExcelJS manifest-workbook
        builder 18-24 added to `report-export.service.ts` (preview where feasible, link-cell
        fallback, manifest sheet authoritative) parameterised by grid title/column set; empty
        manifest → nothing-to-produce message, no file
  - [x] `downloadCertificatesData()` → authenticated blob fetch → save under the stored file
        name; failures surface as toasts
  - [x] The shared `GetNext()`/`GetPrev()` paging pages the ACTIVE grid (photos vs certificates
        tab/section state) against its own endpoint — one pager component, two result sets
- [x] **Task 5 — i18n** — extend `reports.orphanFiles.*` with the certificates section keys
      (section title, export/download toasts, empty state) in **both** `ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–8)
  - [x] Live check: anonymous POST → 401; missing `dateFrom` → 400 `{message, errors}`; empty
        window → 200 empty page + UI message, no file; HQ + `charityId` → that charity only;
        charity token → own rows only; certificates grid shows certificate rows while the photos
        grid still shows photo rows (no cross-contamination from the kind selector); row download
        streams real bytes. Arabic payloads from UTF-8 files
  - [x] Regression: 18-24's photos grid/export/single-download still behave (AC of 18-24 unbroken)
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 live-API lock — never kill the
        user's process; ng-serve stale-bundle grep); tests excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- Raw envelope + anonymous `{ message, errors }` — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); `ControllerBase` + `[Authorize]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories — repositories never save; global soft-delete query filter.
- Caller scope from `ICurrentUserService` only; lookup labels `NameAr ?? NameEn`.
- Client-side export only: ExcelJS + file-saver (installed); server EPPlus streaming superseded
  (18-24's recorded deviation); PDF/jsPDF is 18-21's, not this story's.
- EP-18 adds no entities and no EF migration — read-only projections.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Photos grid/export + the screen's filters and route | 18-24 (landed — reuse, do not rebuild) |
| PDF rendering (`services/report-pdf.service.ts`, jsPDF + RTL font) | 18-21 |
| Generic Excel-export engine hardening beyond the manifest builder | 18-41 |
| Any certificate upload/write path | EP-19 (attachments) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.18] screen contract — the
  certificates grid + `ExportCertificatesData()` / `downloadCertificatesData()` commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.25] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-25 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-24-export-orphan-photographs.md] the screen,
  filter DTO/validator, manifest-workbook builder and reserved section this story completes
- [Source: Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs] live download endpoint
  reused for every image fetch

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `dotnet build` (Api csproj, temp `-o C:/Users/oabdelaziz/AppData/Local/Temp/iirosa-1825`) — **0 errors**. First pass failed CS0115 inside the parallel session's mid-write `Epic09_ReportSlotSoftDeleteIndex` Designer — my build raced their file save; the settled file compiles (retried clean; never their code's fault, never touched it).
- `npx ng build` — NG_EXIT=0, no TS errors.
- i18n `reports.orphanFiles.*` grew 19 → **22 keys** each locale (+tabPhotos/tabCertificates/certificatesExportFailed/certificatesDownloadFailed, −certificatesReserved placeholder), key sets identical (node-verified).
- Live smoke, private instance `127.0.0.1:60970`, seeds `1825…` (3 certificate attachments + 1 photo attachment; 4 reports — 2 accepted-with-certificate in-window, 1 refused-with-certificate in-window, 1 accepted-with-PHOTO-only in-window as the cross-contamination probe):
  - anonymous POST → **401**
  - missing `dateFrom` → **400** `errors.DateFrom` (18-24's shared validator, reused verbatim)
  - HQ certificates window → **exactly 2 rows** (LC-CODE-1 + ORP-2026-23731/وادي النطرون, fileName `cert-*.png`, downloadUrl correct); the refused certificate and the **photo-only** row are absent — kind selector proven
  - HQ photos window — **REGRESSION GREEN**: exactly 1 row (ORP-2026-27454, `photo-only.png`); certificate-only rows absent — the shared-helper refactor did not change 18-24's behavior
  - HQ + `charityId`=dga on certificates → **1 row**; empty window (08/2020) → `total=0, items=[]`; page 0 → 400
  - row download `GET /api/Attachments/18251001-…/download` → 200 `image/png` 66 bytes, PNG magic (AC 7)
  - seeds hard-deleted (reports 0 / attachments 0 / contents 0); smoke instance killed by PID (36912).

### Completion Notes List

- **Kind-discriminator verification (the story's STOP gate — PASSED):** certificates and photographs are distinguished by the REPORT's own column pair — `PeriodicOrphanReport.OrphanCertificateImageId` (FK_OrphanCertificateImg) vs `OrphanImageId` (FK_OrphanImage) — exactly the two slots the 14.U.16 gallery renders. NOT `common.Attachment.AttachmentTypeId`. No gap; no guessed discriminator.
- **One code path:** 18-24's `ExportOrphanFilesAsync` body was refactored into a private `GetReportImageManifestAsync(filter, certificateKind, ct)`; both public methods are one-line wrappers. Filter, validator, scope ladder, ordering, metadata resolution and envelope are literally shared — certificates cannot drift from photographs. Verified by the live regression (photos still exactly right).
- **One pager, two result sets:** the component keeps `rows/totalCount/hasRun` (photos) and `certRows/certTotalCount/certHasRun` (certificates) with a single `currentPage/pageSize`; tab switch resets page 1 and re-runs against the active grid's endpoint; the viewer's shared `Pagination` (GetNext/GetPrev) pages whichever grid is active. Thumbnails live in per-grid object-URL maps, released per grid on reload/destroy.
- The workbook builder `exportOrphanFiles(rows, fileName?, sheetKey?)` gained only the sheet-title parameter — same columns (اسم اليتيم · كود اليتيم · الصوره), same embed-budget/hyperlink-fallback decision; certificates export as `certificate-images_<scope>_<from>.xlsx` titled صور الشهادات.
- The story Dev Notes' "global soft-delete query filter" claim is corrected as ever — explicit `!IsDeleted` on both legs.
- Seed note: the parallel session's Epic-09 table repair made `PeriodicOrphanReport.Reviewed` NOT NULL between 18-24's and 18-25's seeds — later seeds carry it.
- Tests excluded per the standing user decision.

### File List

- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `ExportCertificateFilesAsync` declared (same DTO pair)
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — 18-24's method refactored into shared `GetReportImageManifestAsync` + the certificates wrapper (kind = `OrphanCertificateImageId`)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST certificate-files/export` (`SuperAdmin,Admin,Charity`)
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getCertificateFiles()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportOrphanFiles` parameterised by sheet title (UC-RPT-24/25 shared builder)
- `Frontend/src/app/modules/reports/orphan-files/orphan-files.component.ts` · `.html` · `.scss` — twin-grid rework: tab switch, per-grid state/thumbs, active-grid export/download, reserved section replaced by the live certificates grid
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.orphanFiles.*` +3/−1 keys each

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-25 and module spec §23.S.18 / §23.U.25; rides 18-24's screen/DTO/export plumbing with the certificate-kind selector, single pager over two grids, and the attachment-kind verification task recorded. |
| 2026-08-24 | Implemented and verified: certificate kind = the report's `OrphanCertificateImageId` column (STOP gate passed — no AttachmentType guess); 18-24's read refactored into one shared helper behind both endpoints; twin-grid screen with one pager over two result sets; workbook builder parameterised by sheet title. Matrix proven live (401, 400 dateFrom, exact 2-row certificate set with refused + photo-only exclusions, photos regression green, HQ narrow, empty window, real PNG bytes). Status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
