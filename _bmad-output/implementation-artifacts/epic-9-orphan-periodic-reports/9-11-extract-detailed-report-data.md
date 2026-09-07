# Story 9-11: Extract detailed report data

| Field | Value |
| --- | --- |
| Story key | `9-11-extract-detailed-report-data` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-11 — تفاصيل التقارير |
| Priority / size | Should · 3 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.U.11 scenario) |
| Route | `#/periodic-orphan-reports/orphan-reports/generate` |
| Endpoint | `POST /api/OrphanReports/generate` (+ the existing `POST /api/OrphanReports/export` blob sibling) |
| Depends on | **9-1** (registered module, scope) |
| Roles | HQ roles → `SuperAdmin`, `Admin`, `Accountant`, `Employee` |

## Status

review

## Story

As a HQ role, I want to be able to extract detailed report data تفاصيل التقارير, so that I can
answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a HQ role on the generate screen, when the actor sets the extract criteria (charity,
   report number, codes-only, date range) and runs it, then no stored data is changed — the
   operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `POST
   /api/OrphanReports/generate` and the result renders without a page reload.
3. Given a charity user, when the function is invoked, then only that charity's reports are
   returned; an HQ role may pass an explicit `charityId` (AC 3/4 pattern of the epic).
4. Given no row matches the criteria, when the extract runs, then the grid renders empty and the
   paging control reports zero pages.
5. Given the actor presses استخراج, when rows are present, then a detailed Excel extract downloads;
   when no rows match, then the actor is told there is nothing to produce rather than receiving an
   empty file.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.11 passes end to end; the extract carries the report dimensions the
detailed DTO provides; the scoping is enforced server-side.

**Batch-parameter deferral (recorded):** §14.U.11's legacy signature includes `batchId` — there is
no Batch entity in the Domain today. Ship the extract without the batch filter and leave
`// TODO EP-10: batch filter` where the predicate would go (17-2 deferred-rule precedent).

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `OrphanReportsController.cs` `POST generate` (:37) → `OrphanReportResultDto`; `POST export` (:69) → blob | Exists |
| Service | `OrphanReportService` (DI restored by 9-1) | `GenerateReport(OrphanReportFilterDto)` exists; **verify the filter actually narrows** (the 15-1 wire-mismatch defect class — check `OrphanReportFilterDto` keys against what the SPA sends) |
| Frontend | `orphan-reports-generate.component.*` + `orphan-report.service.ts generateReport/exportReport` | Exist; unreachable before 9-1 |
| Excel | Service-layer exports are **TODO stubs returning empty arrays** (ClosedXML was never wired; no Excel package is referenced in any csproj) | Client-side ExcelJS is the platform export path (16-1 precedent) |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Filter contract audit needed end to end** — `OrphanReportFilterDto` keys vs the SPA's
   `generateReport` payload; any silent no-op filter is a defect (15-1's entire defect 1).
2. **No caller scope on the generate path** — pin per claims; explicit `charityId` for HQ only.
3. **Export stub** — `POST export` returns an empty blob; the real extraction is ExcelJS
   client-side off the generate result (or wire the server stub properly — see Tasks; pick one,
   record it).
4. **Result shape**: `OrphanReportResultDto` must carry the detailed dimensions (report +
   orphan identity + charity name) the extract needs; extend if it is a thin summary today.

## Tasks / Subtasks

- [x] **Task 1 — Contract + scope** (AC 2, 3, 4): align filter keys (charityId, reportNo, isCodes,
      dateFrom/dateTo — batchId deferred) between SPA, DTO, and service predicate; caller scope;
      empty-result behaviour
      — `OrphanReportFilterDto.ReportNo` added (Contains match); SPA sends `{fromDate, toDate,
      charityId, reportNo}` with `0001-01-01` sentinel for unset dates and `codesOnly` handled
      client-side (it selects the column set, not a server predicate). Caller scope:
      `effectiveCharityId = _currentUser.CharityId ?? filter.CharityId` pins BOTH the legacy orphan
      branch and the new reports branch — forged charity ids ignored. Date window inclusive
      (`< ToDate.Date.AddDays(1)`); `batchId` deferred with `// TODO EP-10: batch filter` marker.
- [x] **Task 2 — Detailed projection** (AC 2): verify/extend `OrphanReportResultDto` to the
      detailed column set (orphan identity, charity, report period, review state, key dimensions);
      one query, includes, no N+1
      — `OrphanReportDetailRow` appended (report identity/period, orphan code+name, charity name,
      review state incl. refuse reason, school/faculty/specialization/grade/degree/level, medical,
      marriage/death); `Reports` + uncapped `ReportsTotalCount` on the result DTO. One translatable
      query (`OrderByDescending(ReportDate).Take(1000).Select(...)` — nav traversals `r.Orphan.Code`,
      `r.Charity.Name` become LEFT JOINs, no N+1); `EducationalLevelName` batch-resolved after the
      query (the report entity has no EducationLevel nav — one dictionary lookup, not per-row).
- [x] **Task 3 — Screen** (AC 1, 4, 5): rebuild `orphan-reports-generate.component` — criteria
      panel (جمعية drop-down HQ-gated, رقم التقرير, أكواد toggle, من/الي تاريخ), result grid,
      paging, empty state, `OnPush`/`trackBy`; استخراج البيانات downloads the ExcelJS-built
      workbook from the loaded rows; empty result → "nothing to produce" message, no file
      — full rewrite of the old orphan-registry mock (dead `charities: any[]`, hardcoded English
      arrays, fake `userRole`). OnPush + cdr; client-side paging over the capped batch (20/page)
      with a capped indicator when `reportsTotalCount > rows.length`; codes-only toggle switches
      the grid + workbook column set (code + reportNo only); ExcelJS export per the 16-1 pattern
      with write-chunk and writeBuffer failure catches; zero rows → info toast, no file.
- [x] **Task 4 — i18n** — labels/messages under `orphanReports.generate.*` in **both** `ar.json`
      and `en.json`
      — 20 keys per locale incl. 21 `columns.*`; status badges reuse `periodicReports.status.*`.
- [ ] **Task 5 — Verification** (AC 1–6): live check — charity pin holds; `reportNo` and date
      window narrow; codes-only toggle changes the column set; export downloads and opens with
      rows; empty criteria match → message not file; unauthenticated → 401; `npm run build` green;
      tests excluded per the standing user decision
      — `dotnet build` Application 0 errors; `npx tsc --noEmit` zero findings for the module;
      `npm run build` pending (running with later stories' edits).

## Dev Notes

### Platform rules that bind this story

- Read-only; raw envelope; camelCase wire; pagination clamps and inclusive end-date bound (9-1
  Task 3 rules apply to this filter too).
- ExcelJS client-side is the sanctioned export path — do NOT add a server Excel package for this
  story (the TODO stubs stay until a platform-level decision reopens them).
- This is the general-purpose pull 9-12/9-13 specialise — build the shared detailed projection
  here; those stories only add the state predicate + entry points.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Accepted-only / refused-only variants + their list-screen commands | 9-12, 9-13 |
| Numbers-in-period branch of the statistics endpoint | 9-15 |
| Non-renewed orphans extract | 9-14 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.11] scenario (legacy signature
  with batchId)
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-11 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanReportsController.cs#L37] generate action
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-1-list-missions.md] the wire-mismatch defect
  class this story audits for

## Dev Agent Record

### Agent Model Used

Claude (GLM-5) via Claude Code — `/bmad-dev-story` pass over EP-09.

### Debug Log References

- `dotnet build Backend/src/IIROSA.Application/IIROSA.Application.csproj` — 0 errors (132 warnings,
  pre-existing nullable/async warnings).
- `npx tsc --noEmit` — zero findings filtered for `periodic-orphan-reports`.
- `Frontend/build-epic9.log` — full `npm run build` (pending; covers 9-5..9-10, one further build
  covers this story's frontend edits).

### Completion Notes List

- **Defect 1 (wire mismatch) audited**: the SPA's old generate screen sent a rich orphan-registry
  payload (sponsorshipStatus, ageFrom/ageTo, gender, include* flags) which the service *does*
  honour for the legacy `Orphans` branch — not a no-op. The §14.U.11 criteria (charity, reportNo,
  codes, dates) are now first-class: `ReportNo` added to the DTO + predicate; codes handled
  client-side.
- **Defect 2 (scope) fixed**: `effectiveCharityId = _currentUser.CharityId ?? filter.CharityId`
  applied to the orphan branch (was unpinned) and the new reports branch. Charity callers cannot
  widen; HQ may pin explicitly.
- **Defect 3 (export stub)**: resolution recorded — client-side ExcelJS off the generate result is
  the export path (16-1 precedent); server `POST export` TODO stubs stay until a platform-level
  decision reopens them.
- **Defect 4 (result shape) fixed**: `OrphanReportResultDto` now carries `Reports` +
  `ReportsTotalCount`; `OrphanReportDetailRow` is the shared detailed projection 9-12/9-13 reuse.
- **Extract cap**: rows capped at 1000 per batch (Take(1000)) with the uncapped count surfaced —
  grid shows a capped indicator; the batch filter itself stays `TODO EP-10` (no Batch entity).
- **Legacy branch preserved**: the orphan-registry pull (`Orphans`/`Summary`/`Metadata`) is intact
  for its own consumers; the reports branch was appended, not substituted.
- **Route**: `orphan-reports/generate` now `PermissionGuard`-gated (`PeriodicReports.View`) with
  live `orphanReports.generate.title` keys (previously AuthGuard-only with keys that resolved to
  the key string).
- **Endpoint roles**: generate stays `SuperAdmin,Admin,Charity` (recorded — the story table lists
  HQ roles incl. Accountant/Employee; the permission guard carries the UX gate, matching the
  epic's other extract screens).

### File List

- `Backend/src/IIROSA.Application/DTOs/OrphanReport/OrphanReportFilterDto.cs` — `ReportNo` filter
- `Backend/src/IIROSA.Application/DTOs/OrphanReport/OrphanReportResultDto.cs` — `Reports`,
  `ReportsTotalCount`, `OrphanReportDetailRow` class
- `Backend/src/IIROSA.Application/Services/OrphanReportService.cs` — ctor deps
  (`IRepository<PeriodicOrphanReport>`, `IRepository<EducationLevel>`, `ICurrentUserService`);
  orphan branch pinned; §14.U.11 reports branch (pin, ReportNo, inclusive dates, count, capped
  projection, level-name batch resolve)
- `Frontend/src/app/modules/periodic-orphan-reports/models/periodic-orphan-report.model.ts` —
  `OrphanReportDetailRow` interface; `reports`/`reportsTotalCount` on result; `reportNo` on filter
- `Frontend/src/app/modules/periodic-orphan-reports/orphan-reports-generate/orphan-reports-generate.component.ts/.html/.scss`
  — full §14.U.11 rewrite (criteria + grid + paging + ExcelJS export)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-orphan-reports-routing.module.ts` —
  generate route: PermissionGuard + live title keys
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` —
  `orphanReports.generate.*` (20 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-11 and module spec §14.U.11; batch filter deferred to EP-10; export-stub defect recorded. |
| 2026-08-24 | Implemented: ReportNo filter + pinned scope + detailed Reports projection (capped 1000, uncapped count); §14.U.11 screen rebuilt (criteria, grid, paging, codes toggle, ExcelJS export); i18n ar+en; route guard + title fix. Backend 0 errors, tsc clean; full build pending. |
| 2026-08-24 | Verification pass: final gates run — `npm run build` GREEN (epic-9 module compiled; NG8107 optional-chain warnings only) and backend 0 errors for epic-9 code (the only 2 solution errors are the parallel epic-18 session’s in-flight untracked `ReportService.cs` — CS0019 ×2, left untouched per convention). Live API wedged (accepts TCP, empty replies) — restart pending; live walkthrough stays batched. Status ready-for-dev → review. |


### Review Findings (epic review 2026-08-24)

> Review Outcome 2026-08-24 — all items patched & verified: D1 resolution applies (see 9-10).

- [x] [Review][Decision] D1 (also applies here) — see 9-10 — **resolved 2026-08-24: endpoint roles widened** (Accountant + Employee added to [Authorize] on POST /api/OrphanReports/generate and /statistics, matching the PERMISSION_ROLES map)
- [x] [Review][Patch] P1b Soft-delete leak: detailed extract includes deleted reports [OrphanReportService.cs:454]
- [x] [Review][Patch] P41 Grid headers misaligned: orphanCode has no th; codesOnly mode 2-header vs 3-cell body [orphan-reports-generate.component.html:85-136]
- [x] [Review][Patch] P30c HQ charity dropdown markForCheck [orphan-reports-generate.component.ts:72]
- [x] [Review][Patch] P42b Pagination clamp [orphan-reports-generate.component.ts:124]
- [x] [Review][Patch] P43b exporting flag never set [orphan-reports-generate.component.ts:44]
- [x] [Review][Patch] P51b Page-number *ngFor no trackBy [orphan-reports-generate.component.html:525]
- [x] [Review][Patch] P52 reportDate cell lacks "—" fallback [orphan-reports-generate.component.html:490]
- [x] [Review][Patch] P58 const filter: any — restore typed DTO [orphan-reports-generate.component.ts:787]
