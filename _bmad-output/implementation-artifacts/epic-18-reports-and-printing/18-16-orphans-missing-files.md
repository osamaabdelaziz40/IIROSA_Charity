# Story 18-16: Orphans missing files

| Field | Value |
| --- | --- |
| Story key | `18-16-orphans-missing-files` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-16 — أيتام مطلوب لهم ملفات |
| Priority / size | Should · 8 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.15 screen, §23.U.16 scenario) |
| Route | `#/reports/orphans-missing-files` |
| Endpoint | `POST /api/Reports/orphans-missing-files` |
| Depends on | **18-1 landed** (reports skeleton — controller, service, paged envelope, `ResolveCharityScope`, `modules/reports` shell, `report.service.ts`, `report-export.service.ts`, `Reports.View`) |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (platform role names — 15-1 precedent; `Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to orphans missing files أيتام مطلوب لهم ملفات, so that data produced outside the system is carried in without manual re-keying.

## Ruling: the epic's «Import a file» semantics are a template artefact (recorded — read this before the ACs)

§23.U.16 and US-RPT-16 describe an **import** (upload a workbook, match rows to records, apply values, report unmatched rows). §23.S.15 — the actual screen contract — describes a **query**: a charity dropdown, a 5-column read-only grid (الجمعيه · رقم اليتيم · اسم اليتيم · العنوان · القريه), an export command (`ExportReportData`) and paging. There is no file field on the screen. The import text is the generic-epics template talking; the screen is authoritative (architecture.md §10 code-wins). **This story implements the query + export.** The import acceptance criteria (file-layout mismatch, unmatched-row reporting) are dropped; if an import is genuinely required later it is a separate backlog item with its own screen and write path — do not smuggle a multipart upload into this story.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at `#/reports/orphans-missing-files`, when the actor runs the report, then no stored data is changed — the operation is a read (see the ruling above for why US-RPT-16's import post-conditions do not apply).
2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/orphans-missing-files` and the response is rendered on the screen without a page reload.
3. Given the caller is a charity user, when the report is served, then only that charity's rows are returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload; the `CountryId` claim pins country the same way (pin-never-widen).
4. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the report runs, then it operates on that charity's data.
5. Given no orphan in scope is missing files, when the report is served, then the grid renders empty and the paging control reports zero pages.
6. Given the actor presses استخراج while the grid is empty, when the export runs, then the actor is told there is nothing to produce rather than receiving an empty file.
7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the query grid of §23.S.15 renders its 5 columns for the selected charity with paging and the Excel export; the scenario of §23.U.15's platform-real reading (a charity's missing-files worklist) passes end to end; the role and charity scoping is enforced server-side, not only in the menu.

## Screen contract (§23.S.15 — أيتام مطلوب لهم ملفات)

| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | `CharityId` | Drop-down list | Optional · lookup Charities (+ كل الجهات all-option, HQ only) · on change re-runs the query |

Grid (row source `field in All_Orphans`):

| Column | DTO key | Source |
| --- | --- | --- |
| الجمعيه | `charityName` | `Charity.NameAr ?? NameEn` via `Orphan.FK_CharityId` |
| رقم اليتيم | `orphanCode` | `Orphan.Code` |
| اسم اليتيم | `orphanName` | `Orphan.FullName` |
| العنوان | `address` | `Orphan.Family.Address` |
| القريه | `village` | `Orphan.Family.CityVillage` |

Commands:

| Command (legacy handler) | Platform realisation |
| --- | --- |
| ExportReportData() | استخراج البيانات — ExcelJS workbook via 18-1's `report-export.service.ts` |
| GetNext() / GetPrev() | shared `Pagination` component |
| حفظ (DeleteOutgoing()) | **template artefact** — dropped, same ruling as 18-15 |

## Tasks / Subtasks

- [x] **Task 1 — DTOs + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`: `OrphansMissingFilesFilterDto` (`Page = 1`, `PageSize = 20`, `Guid? CharityId`), `OrphansMissingFilesListDto` — the 5 wire keys above **plus** `MissingDocumentsName` (nullable; carried for the export detail column and the drill-down future, not rendered in the on-screen 5-column grid) and `OrphanId`/`FamilyId` for row identity — no `FK_*` names (Newtonsoft emits `fK_…`)
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/OrphansMissingFilesValidator.cs`: page bounds only
- [x] **Task 2 — Service projection** (AC 1, 3, 4, 5)
  - [x] `IReportService.GetOrphansMissingFilesAsync(filter)` in `ReportService`: scope through 18-1's `ResolveCharityScope`, then compose rows over `Orphan` projected with `Family` (`Address`, `CityVillage`) and `Charity` (name) — include paths through the repository, never ad-hoc `Include` in the service
  - [x] Missing-files determination (recorded): an orphan is **in** the report when its latest `PeriodicOrphanReport` (highest `ReportDate` per `OrphanId`) has `MissingDocuments == true`; `MissingDocumentsName` carries what is missing. Supplementary hard-null signals (`Orphan.PhotoAttachmentId == null`, report image ids null) are noted in Dev Notes but NOT unioned in v1 — one clear rule beats three overlapping ones
  - [x] `Backend/src/IIROSA.Application/Profiles/ReportProfile.cs` (18-1's): add the `Orphan` → `OrphansMissingFilesListDto` map with explicit `ForMember` bridges (`Code → OrphanCode`, `FullName → OrphanName`, `Family.Address → Address`, `Family.CityVillage → Village`, `Charity.NameAr/NameEn → CharityName` via the `NameAr ?? NameEn` convention) — no `FK_*` source name leaks onto the wire
  - [x] Null-safety of the projection (decided): an orphan with no `Family` row renders empty `address`/`village` strings, never a thrown NRE — project through `Family?.Address ?? string.Empty` in the map, and cover it in the live check
  - [x] Order by `orphanCode` ascending; page through 18-1's `ReportPagedResult<T>`; global soft-delete filter does the rest
- [x] **Task 3 — API endpoint** (AC 2, 7)
  - [x] In 18-1's `ReportsController`: `[HttpPost("orphans-missing-files")] GetOrphansMissingFiles([FromBody] OrphansMissingFilesFilterDto)` → `Ok(paged)`; `catch (ValidationException)` first → 400 `{ message, errors }` (the `OfficeProjectManagementController.cs:89-101` ladder), catch-all → 500 `{ message }`; no `ApiResponse<T>`
- [x] **Task 4 — Frontend thin component** (AC 1, 2, 5, 6)
  - [x] `Frontend/src/app/modules/reports/orphans-missing-files/` 4-file component; route `#/reports/orphans-missing-files` guarded `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`
  - [x] Filter panel per §23.S.15: الجمعيات dropdown from `GET /api/Charities` (`result.items || []`; كل الجهات all-option for HQ; pinned single option for a charity user); runs on change
  - [x] Grid with the 5 spec columns in order; shared `Pagination`; `trackBy: orphanId`; row serial `(currentPage-1)*pageSize + i + 1` (13-1 formula); empty state when `totalCount === 0`; **not `data-list`**; OnPush omitted (list-screen precedent)
  - [x] استخراج البيانات (`ExportReportData`): export ALL matching rows (re-fetch with a large page or a dedicated `pageSize = totalCount` call — never just the visible page) through `report-export.service.ts`; RTL sheet (right-to-left columns); when `totalCount === 0` show the nothing-to-produce toast and do not emit a file
- [x] **Task 5 — i18n** — title, filter label + all-option, 5 column headers, empty state, export success/no-rows/failure messages under `reports.*` in **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–7)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; **no EF migration**. MSB3021/3027 = live-API output lock; never kill the user's process
  - [x] Live check: anonymous POST → 401; HQ POST `{}` → 200 paged camelCase with `charityName`/`address`/`village` null-safe (orphans without a family render empty strings, not errors); charity token → only its rows even when the payload names another charity; page bounds violation → 400 errors map; empty scope → `totalCount: 0`, zero pages; export on empty → message
  - [x] `cd Frontend && npm run build` — 0 errors; ng-serve stale-bundle caveat (grep the served chunk); Arabic payloads from UTF-8 files when curling
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Why this is 8 points as a query

The size carries the join topology (`Orphan` → `Family` → `Charity` + latest-report-per-orphan determination), the full-scope Excel export, and the recorded deviation analysis that kills the import semantics. None of it is UI chrome.

### Missing-files rule (decided here, one rule)

Primary signal: latest `PeriodicOrphanReport.MissingDocuments == true` (the entity's own flag, paired with `MissingDocumentsName`). The alternative — union of null attachment ids (`PhotoAttachmentId`, `OrphanImageId`, `OrphanCertificateImageId`, `MedicalReportImageId`, …) — mixes "never captured" with "flagged missing" and produces noise on legacy rows. If HQ later wants the union, it is a filter switch on this same endpoint, not a new story vertical.

### Platform rules that bind this story

- Wire is camelCase; no `FK_*` DTO keys (Newtonsoft emits `fK_…`).
- Raw paged envelope + anonymous `{ message, errors }` — NOT `ApiResponse<T>` (15-1 ruling, architecture.md §10).
- Controllers inherit `ControllerBase` + `[Authorize]` + `[Route("api/[controller]")]` (17-1 note: zero live controllers use a custom base).
- FluentValidation in the service (`Validators/Reports/`); DB-dependent rules stay in the service body.
- Reads via `IUnitOfWork` repositories; repositories never save; this story has no write path.
- Soft delete via the global query filter — never hand-check `IsDeleted`.
- Lookup labels `NameAr ?? NameEn`; orphan names are data, not lookups — render as stored.
- Bespoke grid + shared `Pagination` (NOT `data-list` — recorded deviation); OnPush omitted on list screens (codebase precedent).
- Caller scope from `ICurrentUserService` — pin-never-widen (`OfficeProjectService.cs:384` shape).
- EP-18 adds no entities, no EF migration.
- Tests excluded per the standing user decision.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Reports skeleton | 18-1 |
| Any other report vertical | 18-2 … 18-15, 18-17 … |
| File/photograph bulk exports (images, not rows) | 18-24 / 18-25 |
| The shared PDF printing path | 18-21 |
| An actual import/upload flow for missing files | none — separate backlog item if ever required |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.15] screen contract — 1 filter field, 5-column grid, export + paging commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.16] scenario — source of the import artefact this story records against
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-16 acceptance criteria (import clauses superseded by this story's ruling)
- [Source: Backend/src/IIROSA.Domain/Entities/Orphan.cs] `Code`, `FullName`, `FK_CharityId`, `PhotoAttachmentId`, `Family` nav
- [Source: Backend/src/IIROSA.Domain/Entities/Family.cs] `Address`, `CityVillage` — the العنوان / القريه columns
- [Source: Backend/src/IIROSA.Domain/Entities/PeriodicOrphanReport.cs] `MissingDocuments`, `MissingDocumentsName`, `ReportDate` — the missing-files signal
- [Source: Backend/src/IIROSA.Api/Controllers/OfficeProjectManagementController.cs:89-101] ValidationException → errors-map ladder
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-15-coded-orphans-needing-a-report.md] sibling story that established the template-artefact ruling

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — 0 errors, 194 warnings (pre-existing), output locked-copy clean (temp `-o` build used for smoke; user's live API untouched).
- `npx ng build` — EXIT=0, `error TS` count 0 (log `ng-build-18-16.log`; only the pre-existing `exceljs` CommonJS warnings).
- Live smoke, private instance `127.0.0.1:60970` (login field is `email`, e.g. `Admin@IIROSA.com` — `userName` returns 400 "email" null):
  - anonymous POST `/api/Reports/orphans-missing-files` → **401**
  - HQ `{}` → **200** 1 row: `ORP-2026-42717` / كريم متولي السقا / `address` شارع الترع - عمارة الدخان / `village` حي الدخيلة / `charityName` dga / `missingDocumentsName` "Birth certificate" — full camelCase envelope, family join live
  - HQ narrow dga → 200 same 1 row; narrow وادي النطرون → 200 `totalCount: 0` (its coded orphans have no reports at all — no report ≠ missing files); narrow HQ charity (empty) → 200 `totalCount: 0`
  - `{"page":0}` → **400** `{"message":"One or more fields are invalid","errors":{"Page":"Page must be at least 1"}}`; `{"pageSize":500}` → **400**
  - charity token (`Charity@IIROSA.com`, pinned dga): `{}` → own 1 row; payload naming وادي النطرون → still the dga row only (claim pins, request never widens)
  - **anti-semi-join negative path**: seeded orphan `ORP-2026-43223` with a flagged report (2026-06-01) superseded by a later clean report (2026-08-01) — absent from every response, as designed
  - seed rows (`11111116-…-0001…0004`) hard-deleted after the run; table verified back to 0 rows; smoke instance killed by PID

### Completion Notes List

- **Deviations from story text (all sibling-precedent):**
  - Task 2 names `Profiles/ReportProfile.cs` — no such profile exists; every 18-x sibling projects with direct LINQ `Select` in `ReportService`. Followed the codebase (deviation recorded).
  - Screen-contract table says `Charity.NameAr ?? NameEn` — `Charity` has a plain `Name` column; resolved `c.Name` (18-12/18-15 convention).
  - Story text says "global soft-delete filter — never hand-check `IsDeleted`" — this platform has **no** global filter; explicit `!IsDeleted` at every level (standing correction across EP-18).
- **Latest-report rule as anti-semi-join**: "highest `ReportDate` per orphan is flagged" is expressed as *a flagged report with no later live report for the same orphan* — EF translates `GroupBy+First` poorly, the anti-semi-join cleanly; proven live by the negative-path seed above.
- `MissingDocumentsName` attached in-memory per page (18-12 attach pattern) — it lives on the report row, the page is orphan-shaped.
- Roles on the endpoint: `SuperAdmin,Admin,Charity` — charity-caller AC 3 requires the Charity role to reach the endpoint (18-12 precedent); the header's "Gen. Director only" would lock charity users out of their own worklist.
- Export: client-side ExcelJS over ALL matching rows, paged server-side at 200 (validator cap), RTL sheet; empty selection → nothing-to-produce toast, no file (AC 6).
- 5 spec columns on screen + serial (13-1 formula); `missingDocumentsName` carried on the wire for the export column, not rendered in the on-screen grid.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `OrphansMissingFilesFilterDto`, `OrphansMissingFilesListDto`
- `Backend/src/IIROSA.Application/Validators/Reports/OrphansMissingFilesValidator.cs` — new (page bounds)
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetOrphansMissingFilesAsync` declared
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation (anti-semi-join + scope + attach + charity-name resolve)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST orphans-missing-files` action
- `Frontend/src/app/modules/reports/models/report.model.ts` — `OrphansMissingFilesFilter`, `OrphansMissingFilesRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getOrphansMissingFiles()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportOrphansMissingFiles()`
- `Frontend/src/app/modules/reports/orphans-missing-files/` — 4-file component (new)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — import + `orphans-missing-files` route (`Reports.View`)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.orphansMissingFiles.*` (13 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-16 and module spec §23.S.15 / §23.U.16; import semantics recorded as template artefact against the §23.S.15 query screen; missing-files rule decided on `PeriodicOrphanReport.MissingDocuments`. |
| 2026-08-24 | Implemented and verified: endpoint + service + validator + DTOs, query screen with 5-column grid, ExcelJS export, i18n both locales, route + sidebar. Smoke matrix green (anon 401, HQ/charity scoping, superseded-flag exclusion, validation 400s, seeded positive path cleaned back to 0 rows). Status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
