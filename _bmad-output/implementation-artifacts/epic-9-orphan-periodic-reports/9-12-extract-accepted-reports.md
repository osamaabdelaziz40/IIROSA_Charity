# Story 9-12: Extract accepted reports

| Field | Value |
| --- | --- |
| Story key | `9-12-extract-accepted-reports` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-12 — التقارير المعتمدة |
| Priority / size | Must · 8 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.U.12 scenario; the accepted-reports command of §14.S.1) |
| Route | command on `#/periodic-orphan-reports` rendering an accepted-filtered view (modal/screen from the list) |
| Endpoint | `GET /api/PeriodicOrphanReports/approved` |
| Depends on | **9-1** (list screen, scope) · **9-7** (accepted state exists) · benefits from **9-11** (shared detailed projection) |
| Roles | HQ roles → `SuperAdmin`, `Admin`, `Accountant`, `Employee` |

## Status

review

## Story

As a HQ role, I want to be able to extract accepted reports التقارير المعتمدة, so that head office
keeps control of what is accepted into the sponsorship cycle.

## Acceptance Criteria

1. Given a HQ role in the module, when the actor runs the accepted-reports extract, then the result
   contains only reports with `Reviewed && IsAccepted` (reviewStatus "Approved") — the orphans
   cleared for disbursement.
2. Given the request is accepted, when it is served, then it is handled by `GET
   /api/PeriodicOrphanReports/approved` and the response is rendered without a page reload.
3. Given a charity user, when the function is invoked, then only that charity's accepted reports
   are returned; an HQ role may pass an explicit `charityId`.
4. Given the extract runs over a date window / report number, when rows match, then the detailed
   Excel extract downloads (9-11's detailed projection + export); no match → "nothing to produce".
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.12 passes end to end; the accepted predicate is server-side; the
extract reconciles with the 9-1 list filtered to Accepted.

**Spec note:** §14.U.12's scenario table is a mis-derived copy of the review flow (it describes
deciding, not extracting) — the use case's own summary row governs: "the same detailed extract
restricted to accepted reports". This story is an extract, not a review action.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Service | `GetApprovedReportsAsync(filter)` | Exists: pins `Reviewed=true, IsAccepted=true`, delegates to `GetReportsAsync` |
| API | `GET /approved` | Exists; same read role set |
| Frontend | `periodic-orphan-report.service.getApprovedReports` | Exists (unreachable before 9-1); the §14.S.1 command icon `ExtractAcceptedOrphansReportsData` has no handler |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **`ReviewStatus` filtering vs flags.** `GetReportsAsync`'s status switch maps "approved" →
   `IsAccepted` alone (not `Reviewed && IsAccepted`). With the 9-7 stamping discipline the two are
   equivalent — but align the predicate to the flags pair so a legacy row with a stray flag cannot
   leak in.
2. **Caller scope on `/approved`** — apply the 9-1 scope (it currently trusts the client filter).
3. **No extract surface** — the command icon on the list screen does nothing; build the
   accepted-extract interaction (criteria + grid + export).

## Tasks / Subtasks

- [x] **Task 1 — Predicate + scope** (AC 1, 3): flags-pair predicate (defect 1); caller scope
      (defect 2); 9-1 clamps/envelope already shape the response
      — verified already in place rather than rebuilt: `GetApprovedReportsAsync` pins
      `Reviewed=true, IsAccepted=true, IsRefused=false` then delegates to `GetPagedAsync`, which
      applies `ApplyCallerScope` FIRST (a charity claim pins; request data cannot widen — 9-1's
      fix, inherited here). The ApplyFilters status switch reads the flags pair
      `r.Reviewed && r.IsAccepted` (defect 1's alignment already landed with the 9-7/9-9 work).
      Added: dedicated `ReportNo` filter on `PeriodicOrphanReportFilterDto` + partial-match
      predicate in ApplyFilters (SearchTerm also matched it, but the extract criterion names it
      alone).
- [x] **Task 2 — Extract interaction** (AC 2, 4): the §14.S.1 accepted command opens an
      extract view (inline panel or modal — match whatever 9-11 shipped for the general extract)
      with charity (HQ), date window, report number criteria; grid reuses the 9-11 detailed
      columns; استخراج downloads the ExcelJS workbook; empty → "nothing to produce" message
      — new shared component `OrphanReportStateExtractComponent` at
      `orphan-reports/extract/:state` (route + `PermissionGuard PeriodicReports.View`). 9-11
      shipped a full screen, so the extract is a screen too, reached by the §14.S.1 register
      command (btn on the list header, beside the 9-9 search entry). Loads pages of 100 via
      `getApprovedReports` until drained (cap 2000 — 9-9 export precedent); client-side paging;
      grid reuses the 9-11 column headers (`orphanReports.generate.columns.*`); ExcelJS workbook
      off the loaded rows; zero rows → info toast, no file. **Built as the shared state-extract
      surface**: the `:state` param + component branches exist for `refused` (9-13 wires its
      entry command, title keys and refuse-reason column — those stay 9-13 scope).
- [x] **Task 3 — i18n** — labels under `periodicReports.extract.accepted.*` in **both** `ar.json`
      and `en.json`
      — 8 keys per locale under `.accepted.*` + shared `.criteria`; screen/column labels reuse
      9-11's `orphanReports.generate.*`.
- [ ] **Task 4 — Verification** (AC 1–5): live check — every returned row has `isAccepted: true &&
      reviewed: true`; refusing a report (9-8) removes it from the extract; charity pin holds;
      export opens with rows; unauthenticated → 401; `npm run build` green; tests excluded per the
      standing user decision
      — `dotnet build` Application 0 errors; `npx tsc --noEmit` zero findings for the module;
      `npm run build` pending (batched with the later stories' edits).

## Dev Notes

### Platform rules that bind this story

- Read-only; raw envelope `{items, totalCount, page}`; camelCase wire; ExcelJS client-side export.
- Reuse 9-11's projection/export — do not fork a second detailed DTO or a second Excel builder
  (the accepted/refused extracts differ by one predicate).
- BR-11 read side: EP-10 payments will consume the same accepted state — nothing here should
  write.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Refused variant + correction worklist | 9-13 |
| Non-renewed extract | 9-14 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.12] scenario (summary row
  governs; table is a mis-derivation)
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.S.1] the accepted-extract command
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-12 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs]
  `GetApprovedReportsAsync` — the pin this story corrects

## Dev Agent Record

### Agent Model Used

Claude (GLM-5) via Claude Code — `/bmad-dev-story` pass over EP-09.

### Debug Log References

- `dotnet build Backend/src/IIROSA.Application/IIROSA.Application.csproj` — Build succeeded, 0 errors.
- `npx tsc --noEmit` — zero findings filtered for `periodic-orphan-reports`.
- Full `npm run build` pending — batched with the later stories' frontend edits.

### Completion Notes List

- **Defect 1 (flags pair) and defect 2 (scope) were already fixed** by the 9-1/9-7/9-9 passes:
  `GetApprovedReportsAsync` pins `Reviewed && IsAccepted && !IsRefused`, and `GetPagedAsync`
  applies `ApplyCallerScope` before request filters. This story verified both and added the
  missing dedicated `ReportNo` criterion (DTO + predicate).
- **No DTO/Excel fork**: the extract reuses the paged ListDto (which carries the §14.S.4 wide
  columns) and 9-11's `orphanReports.generate.columns.*` header keys; one builder in the shared
  state-extract component serves accepted and (with 9-13) refused.
- **Row cap honesty**: pages of 100 drained to a 2000-row cap; the header shows the capped
  indicator when the matched set exceeds the loaded batch.
- **Endpoint roles**: `GET /approved` keeps `SuperAdmin,Admin,Accountant,Employee,Charity` —
  Charity included per AC 3 (scoped server-side by the caller pin).
- **9-13 readiness**: the `:state` route param, `getRejectedReports` branch and refuse-reason
  column conditionals are already in the component; 9-13 only adds the register command, its
  `.refused.*` i18n keys and verification.

### File List

- `Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/PeriodicOrphanReportFilterDto.cs` —
  `ReportNo` filter
- `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` — dedicated ReportNo
  predicate in ApplyFilters
- `Frontend/src/app/modules/periodic-orphan-reports/models/periodic-orphan-report.model.ts` —
  `reportNo` on the filter interface
- `Frontend/src/app/modules/periodic-orphan-reports/orphan-report-state-extract/orphan-report-state-extract.component.ts/.html/.scss`
  — new shared state-extract screen (criteria, paged drain, grid, ExcelJS export)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-orphan-reports-routing.module.ts` —
  `orphan-reports/extract/:state` route + PermissionGuard
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-reports-list/periodic-reports-list.component.html`
  — §14.S.1 accepted-extract command button
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` —
  `periodicReports.extract.*` (criteria + 8 accepted keys per locale)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-12 and module spec §14.U.12; scenario mis-derivation noted (extract, not review); predicate alignment recorded. |
| 2026-08-24 | Implemented: dedicated ReportNo criterion (DTO + predicate); verified flags-pair + caller scope already server-side; shared state-extract screen with ExcelJS export + §14.S.1 register command; i18n ar+en. Backend 0 errors, tsc clean; full build pending. |
| 2026-08-24 | Verification pass: final gates run — `npm run build` GREEN (epic-9 module compiled; NG8107 optional-chain warnings only) and backend 0 errors for epic-9 code (the only 2 solution errors are the parallel epic-18 session’s in-flight untracked `ReportService.cs` — CS0019 ×2, left untouched per convention). Live API wedged (accepts TCP, empty replies) — restart pending; live walkthrough stays batched. Status ready-for-dev → review. |


### Review Findings (epic review 2026-08-24)

- [x] [Review][Patch] P39 Silent 2000-row cap: totalCount computed from collected rows, server totalCount discarded — capped indicator can never show; use server total [orphan-report-state-extract.component.ts:133,148]
- [x] [Review][Patch] P42c Pagination clamp [orphan-report-state-extract.component.ts:163]
- [x] [Review][Patch] P30d HQ charity dropdown markForCheck [orphan-report-state-extract.component.ts:88]
- [x] [Review][Patch] P62 Story "capped indicator" claim FALSE — correct Completion Notes after P39 fix
