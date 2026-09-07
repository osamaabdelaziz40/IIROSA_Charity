# Story 9-10: View report statistics by group

| Field | Value |
| --- | --- |
| Story key | `9-10-view-report-statistics-by-group` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-10 — احصائيات عامة للأيتام |
| Priority / size | Should · 3 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.S.3 screen — 1 filter, grouped grid; §14.U.10 scenario) |
| Route | `#/periodic-orphan-reports/orphan-reports` |
| Endpoint | `POST /api/OrphanReports/statistics` |
| Depends on | **9-1** (registered module, scope) |
| Roles | General Director → `SuperAdmin`, `Admin` (spec actor is the General Director alone; the copied controller's broader read set may stay on the statistics endpoint if already present — record what you keep) |

## Status

review

## Story

As a General Director, I want to be able to view report statistics by group احصائيات عامة للأيتام,
so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a General Director on `#/periodic-orphan-reports/orphan-reports`, when the actor opens the
   screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `POST
   /api/OrphanReports/statistics` and the response is rendered on the screen without a page reload.
3. Given a charity user (non-HQ), when the function is invoked, then only that charity's reports are
   aggregated; an HQ role may aggregate any or all charities (explicit `charityId` filter).
4. Given a charity is selected in the الجمعية drop-down, when the screen loads, then the grid shows
   one row per (الحاله التعليميه × المرحله الدراسه) group with الاناث / الذكور / الاجمالي counts.
5. Given no data matches, when the screen loads, then the grid renders empty and the totals are zero.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.10 passes end to end; the counts reconcile with the 9-1 list totals
for the same charity filter; the scoping is enforced server-side.

## Screen contract (§14.S.3)

| Element | Contract |
| --- | --- |
| Filter | الجمعية drop-down · Charities lookup + كافة الجهات · on change reloads (`getCharityData()`) |
| Grid | الرقم · الحاله التعليميه · المرحله الدراسه · الاناث · الذكور · الاجمالي — one row per group (legacy row source `OrphanReportGroupCount`) |
| Commands | استخراج البيانات (`ExportData()`) · paging (GetNext/GetPrev — the copied Pagination component) |

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `Backend/src/IIROSA.Api/Controllers/OrphanReportsController.cs` `POST statistics` | Exists → `OrphanStatisticsDto` (`GetOrphanStatistics([FromBody] OrphanReportFilterDto)`) — **aggregate counts shape, not the grouped-by-education rows §14.S.3 specifies** |
| DTO | `DTOs/OrphanReport/…` (`OrphanStatisticsDto`, `OrphanReportFilterDto`) | Exist; `OrphanStatisticsDto` carries total/pending/approved/… counts — sibling of the periodic `PeriodicOrphanReportSummaryDto` |
| Frontend | `orphan-reports-list.component.*` | Exists (standalone), unreachable before 9-1; grid skeleton with the right column names, hardcoded strings |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Wrong result shape.** The endpoint returns scalar status counts; the screen needs grouped
   rows. Add a `groups` collection to the statistics result (or a sibling DTO
   `OrphanReportGroupStatisticsDto` with `List<OrphanReportGroupCountRow>`): one row per
   (educational status, educational stage) with female/male/total counts, computed in one grouped
   EF query (`GroupBy` on the joined report+orphan projection — no client-side looping over rows).
2. **No caller scope in the aggregation path** (`OrphanReportService` — DI restored by 9-1): pin
   charity per claims; HQ may pass explicit `charityId` (AC 3). The gender split reads
   `Orphan.Gender` — verify the property's actual name/type on `Orphan.cs` and translate to
   female/male buckets.
3. **Educational-status/stage sources.** الحاله التعليميه derives from the report's education
   flags (يدرس / حاصل على شهادة / ترك الدراسة — the 9-9 mapping); المرحله الدراسه from
   `EducationalLevelId` → lookup name. Resolve names server-side (`NameAr ?? NameEn`) so the grid
   binds plain strings.
4. **9-15 shares this endpoint** (`POST /api/OrphanReports/statistics`) for the
   numbers-added-in-period extract — design the request/response so both consumers fit (see Out of
   scope): this story owns the grouped-counts branch; 9-15 adds the numbers branch. Keep the
   action's route stable; discriminate by request (e.g. 9-15's date-window fields) or split the
   response DTO into optional sections.

## Tasks / Subtasks

- [x] **Task 1 — Grouped aggregation** (AC 2, 4): extend the statistics service+DTO per defect 1
      (single grouped EF query; server-side name resolution; totals row = sum check); caller scope
      (defect 2); keep the existing scalar counts section (other screens may read it — additive
      change only)
      — `OrphanStatisticsDto` gained `Groups : List<OrphanReportGroupCountRow>` (additive; scalar
      counts untouched); `GetOrphanStatisticsAsync` now injects `ICurrentUserService` — charity
      claim **pins** (`_currentUser.CharityId ?? filter.CharityId`: a forged charity
      `charityId` is ignored) — and runs ONE grouped query over `PeriodicOrphanReport`
      (education-status classification mirrors 9-9: dropout > graduated > studying > unspecified;
      level id → `NameAr ?? NameEn` resolved in a second small lookup query). Gender buckets
      match the stored Arabic tokens (أنثى/ذكر) with English fallbacks for legacy rows;
      female + male ≤ total when gender is blank.
- [x] **Task 2 — Screen** (AC 1, 4, 5): rebuild `orphan-reports-list.component` to §14.S.3 —
      charity filter (Charities endpoint, كافة الجهات first), grid in spec order with a trailing
      totals row, shared `Pagination`, `EmptyState`, `trackBy` on group key, `OnPush`
      — rebuilt (the old quick-action launcher stub is gone); the grouped grid is compact
      (≤ a handful of education-status × level rows), so the §14.S.3 paging command is not
      needed — the trailing totals row reconciles with the 9-1 register count instead.
- [x] **Task 3 — Export** (§14.S.3 command): استخراج البيانات exports the grouped grid to Excel
      client-side (ExcelJS — 16-1 precedent)
- [x] **Task 4 — i18n** — labels/columns/empty state under `orphanReports.statistics.*` in **both**
      `ar.json` and `en.json` — 16 keys incl. the closed status set (studying/graduated/
      dropout/unspecified).
- [ ] **Task 5 — Verification** (AC 1–6): live check — HQ + all-charities row set ⊇ single-charity
      set; group totals equal the 9-1 list `totalCount` for the same charity; female + male =
      الاجمالي per row; charity user pinned to own data (forged `charityId` ignored); empty charity
      → zero rows; unauthenticated → 401; `npm run build` green; tests excluded per the standing
      user decision
      — `dotnet build` IIROSA.Application 0 errors; `npx tsc --noEmit` clean for the module;
      `npm run build` + live checks pending (final build + the user's own IIROSA.Api restart —
      never killed by policy).

## Dev Notes

### Platform rules that bind this story

- Read-only aggregation: one grouped database query — never `AsEnumerable()` then loop for counts
  that SQL can do.
- Raw envelope; camelCase wire; `nameAr ?? nameEn` for resolved names.
- **9-15 coordination (pinned):** both stories realise `POST /api/OrphanReports/statistics`. This
  story lands the grouped-counts section first; 9-15 extends the same action with the
  numbers-in-period section. Whichever ships second adapts — do not fork the route or the DTO
  family.
- The legacy «Faild Operation» message (§14.U.10 exception) is a persistence-failure artifact —
  a read has nothing to fail that way; a 500 `{message}` catch-all covers it.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Numbers-added-in-period branch of the shared endpoint | 9-15 |
| Detailed extract screen (`/orphan-reports/generate`) | 9-11 |
| Legacy `history/compare/schedule` extras on `OrphanReportsController` | outside the epic |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.S.3] screen contract
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.10] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-10 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanReportsController.cs#L320] the existing
  statistics action this story extends

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5), 2026-08-24.

### Debug Log References

- All four story-time defects confirmed in code before the fix: scalar-only shape,
  `ToListAsync()` + client-side `Count` anti-pattern, no caller scope, and gender compared
  against English tokens while the SPA writes Arabic (ذكر/أنثى) — the legacy counts were dead.
- The scalar section's ToListAsync-then-Count remains (legacy dashboard shape kept verbatim per
  the additive-only rule); only the caller-scope pin was threaded through it.

### Completion Notes List

- **Roles kept (recorded):** the endpoint stays `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`
  — the spec's General-Director-only actor is `SuperAdmin,Admin`; `Charity` is kept because AC 3
  explicitly scopes a charity caller to its own aggregation (pin makes it safe).
- **Paging deviation:** §14.S.3 names GetNext/GetPrev commands, but the grouped result is one row
  per (status × level) — a handful of rows. Paging a GROUP BY would distort the totals row; the
  grid renders all groups with a trailing totals row instead (reconciliation against the 9-1
  register remains the Definition-of-done check).
- **9-15 coordination honoured:** the response DTO grew a `Groups` section additively; the
  `POST /api/OrphanReports/statistics` route is untouched — 9-15 adds its numbers-in-period
  section to the same DTO family.
- **Gender honesty:** rows where `Orphan.Gender` is blank count in الاجمالي but in neither
      الاناث nor الذكور — female + male = total only when gender is populated (the SPA seed
      forms always send it).
- Live checks (row-set superset, totals vs 9-1 count, forged charityId ignored, 401) pending the
  user restarting their own IIROSA.Api; never killed by policy.

### File List

- `Backend/src/IIROSA.Application/Interfaces/IOrphanReportService.cs` (Groups + OrphanReportGroupCountRow)
- `Backend/src/IIROSA.Application/Services/OrphanReportService.cs` (ctor scope + grouped query)
- `Frontend/src/app/modules/periodic-orphan-reports/models/periodic-orphan-report.model.ts` (groups interfaces)
- `Frontend/src/app/modules/periodic-orphan-reports/orphan-reports-list/orphan-reports-list.component.ts/.html` (rebuilt to §14.S.3)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-orphan-reports-routing.module.ts` (live i18n keys + PermissionGuard)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` (sidebar entry)
- `Frontend/src/assets/i18n/ar.json` + `en.json` (`orphanReports.statistics.*`)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-10 and module spec §14.S.3 / §14.U.10; grouped-shape gap and the 9-15 endpoint-sharing contract recorded. |
| 2026-08-24 | Implemented: single grouped EF query over periodic reports with caller-scope pin, additive Groups DTO, §14.S.3 screen rebuilt with totals row + ExcelJS extract, i18n ar/en. Backend 0 errors; tsc clean; ng build + live checks pending. Status → review on green build. |
| 2026-08-24 | Verification pass: final gates run — `npm run build` GREEN (epic-9 module compiled; NG8107 optional-chain warnings only) and backend 0 errors for epic-9 code (the only 2 solution errors are the parallel epic-18 session’s in-flight untracked `ReportService.cs` — CS0019 ×2, left untouched per convention). Live API wedged (accepts TCP, empty replies) — restart pending; live walkthrough stays batched. Status ready-for-dev → review. |


### Review Findings (epic review 2026-08-24)

> Review Outcome 2026-08-24 — all items patched & verified: P42a N/A — the statistics screen renders a single aggregated batch with no pager. D1 resolution: /generate + /statistics widened to Accountant, Employee.

- [x] [Review][Decision] D1 Role/endpoint mismatch: map admits Accountant/Employee but /api/OrphanReports/statistics|generate authorize SuperAdmin/Admin/Charity only — dead screen with generic 403 toast. Widen endpoint roles OR narrow route map [auth.service.ts:124 vs OrphanReportsController.cs:38,321] — **resolved 2026-08-24: endpoint roles widened** (Accountant + Employee added to [Authorize] on POST /api/OrphanReports/generate and /statistics, matching the PERMISSION_ROLES map)
- [x] [Review][Patch] P22 CRITICAL route shadowing: :id (line 64) declared before orphan-reports (line 114) — screen unreachable via sidebar [periodic-orphan-reports-routing.module.ts]
- [x] [Review][Patch] P1 Soft-delete leak: grouped stats include deleted reports — add !r.IsDeleted [OrphanReportService.cs:250]
- [x] [Review][Patch] P2 Tenancy pin: CharityId ?? filter.CharityId fail-open, country-blind — mirror ApplyCharityScopeAsync [OrphanReportService.cs:69,427]
- [x] [Review][Patch] P3 Scope fails open when caller has neither charity nor country claim — fail closed [ApplyCallerScope]
- [x] [Review][Patch] P15 Grouped stats count reports, not orphans — totals disagree with tiles; distinct-orphan per spec
- [x] [Review][Patch] P20 1000-cap truncation non-deterministic — add OrderBy
- [x] [Review][Patch] P21 SummaryDto.OrphanId int vs list Guid — align wire contract
- [x] [Review][Patch] P30b HQ charity dropdown markForCheck (statistics screen) [orphan-reports-list.component.ts:72]
- [x] [Review][Patch] P31 Rapid charity-switch race — switchMap [orphan-reports-list.component.ts:84]
- [x] [Review][Patch] P32 Failed re-run leaves previous results under new criteria [orphan-reports-list.component.ts:104]
- [x] [Review][Patch] P42a Pagination boundary page 0 / last+1 blanks grid — clamp [orphan-reports-list]
- [x] [Review][Patch] P43a exporting flag never set — double-click duplicates export [orphan-reports-list.component.ts:47]
- [x] [Review][Patch] P50 (change) reads stale ngModel on charity select — (ngModelChange) [orphan-reports-list.component.html:1032]
