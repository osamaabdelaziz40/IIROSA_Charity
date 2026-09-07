# Story 9-15: Extract report numbers added in a period

| Field | Value |
| --- | --- |
| Story key | `9-15-extract-report-numbers-added-in-a-period` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-15 — أرقام التقارير المضافة |
| Priority / size | Must · 5 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.U.15 scenario; the report-numbers command of §14.S.1) |
| Route | command on `#/periodic-orphan-reports` rendering the numbers-in-period view |
| Endpoint | `POST /api/OrphanReports/statistics` (the 9-10 shared action — this story owns its numbers branch) |
| Depends on | **9-10** (the shared statistics action + its DTO family), **9-1** (scope) |
| Roles | HQ roles → `SuperAdmin`, `Admin`, `Accountant`, `Employee` |

## Status

review

## Story

As a HQ role, I want to be able to extract report numbers added in a period أرقام التقارير
المضافة, so that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a HQ role in the module, when the actor requests the report numbers registered for a
   charity between two dates, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `POST
   /api/OrphanReports/statistics` (numbers branch) and the response renders without a page reload.
3. Given the window, when the extract runs, then the result lists the `ReportNo` values created in
   it (with orphan code/name and report date), scoped to the caller's charity; HQ may pass an
   explicit `charityId`.
4. Given no report was added in the window, when the extract runs, then the view says so — no
   empty file, no zero-row workbook.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.15 passes end to end (the summary's reconcile-submissions purpose —
an extract/read, despite the epic's "Create a record" typing, which is a derivation artifact); the
numbers reconcile with `ReportNo` generation from 9-3.

**Endpoint-sharing contract (pinned with 9-10):** both stories realise `POST
/api/OrphanReports/statistics`. 9-10 owns the grouped-counts branch; this story adds the
numbers-in-period branch to the same action/DTO family (discriminated by the date-window +
`includeReportNumbers` request shape 9-10 designed for). Do not fork the route, do not displace the
grouped branch. If 9-10 has not landed yet, land both branches consistently yourself and note it.

**Payment-batch deferral (recorded):** the legacy signature's `PaymentId` restricts to a payment
batch — no batch model exists yet; accept-and-ignore with `// TODO EP-10: payment-batch filter`
(17-2 deferred-rule precedent).

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `OrphanReportsController.cs` `POST statistics` (:320) | Exists — extended by 9-10 with the grouped branch + shared DTO |
| Data | `PeriodicOrphanReport.ReportNo` (`POR-YYYY-NNNN`, generated on create by 9-3's path) + `CreatedOn` | The extract's raw material |
| Frontend | §14.S.1 command icon `ExtractReportNumbersThatAdded` | Inert; no handler |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **No numbers branch exists** — the statistics action returns aggregate/grouped counts only; add
      the report-numbers listing (projection: `ReportNo`, orphan code/name, `ReportDate`,
      `CreatedOn`, review state) to the shared response or a discriminated sibling DTO.
2. **Caller scope on the numbers branch** — 9-1 scope (charity pin; HQ explicit filter), same as
      every read in this epic.
3. **No extract surface** — build the numbers view + export off the §14.S.1 command.

## Tasks / Subtasks

- [x] **Task 1 — Numbers branch** (AC 2, 3): extend the shared statistics service/DTO (defect 1)
      with the in-window `ReportNo` listing — ordered by `CreatedOn`; single query; caller scope
      (defect 2); `PaymentId` TODO marker
      — `OrphanReportFilterDto.IncludeReportNumbers` (bool) discriminates the branch;
      `OrphanStatisticsDto` += `ReportNumbers` (List<OrphanReportNumberRow>, newest-first, capped
      1000) + uncapped `ReportNumbersCount` + `UnnumberedReportsCount` (null-ReportNo legacy
      bucket). Branch fills only when the flag is set AND both window ends are non-default.
      Window bounds `CreatedOn` (registered-when, inclusive end date); scope reuses the same
      `effectiveCharityId` pin as the grouped branch; single translatable query with orphan-code
      projection; `// TODO EP-10: payment-batch filter` marker. The 9-10 grouped branch untouched
      (additive only — the pinned contract held).
- [x] **Task 2 — View + export** (AC 1, 4): the §14.S.1 command opens the numbers view — charity
      (HQ-gated) + من/الي تاريخ criteria, count tile, grid (رقم التقرير · كود اليتيم · اسم اليتيم ·
      تاريخ التقرير · حالة الاعتماد), `OnPush`/`trackBy`; استخراج → ExcelJS (9-11 builder); empty
      window → "nothing to produce" message
      — new `ReportNumbersComponent` at `orphan-reports/report-numbers` (route + PermissionGuard),
      reached by the §14.S.1 register command; calls `getOrphanStatistics` with
      `includeReportNumbers: true`; count tile + unnumbered tile (shown only when > 0); grid with
      null-ReportNo rows rendered under —; client-side paging; capped indicator when
      `reportNumbersCount > rows.length`; ExcelJS export; zero rows → info toast, no file.
- [x] **Task 3 — i18n** — labels under `periodicReports.reportNumbers.*` in **both** `ar.json` and
      `en.json`
      — 14 keys per locale incl. 3 `columns.*`; reuses `orphanReports.generate.*` for shared labels.
- [ ] **Task 4 — Verification** (AC 1–5): live check — create reports dated inside/outside the
      window (9-3); extract lists exactly the inside ones with their numbers; charity pin holds;
      count tile equals row count; empty window → message; unauthenticated → 401; `npm run build`
      green; tests excluded per the standing user decision
      — `dotnet build` Application 0 errors; `npx tsc --noEmit` zero findings for the module;
      `npm run build` pending (batched with the later stories' edits).

## Dev Notes

### Platform rules that bind this story

- Read-only; raw envelope; camelCase wire; inclusive end-date bound (9-1 rule) — a report created
  any time on the end date counts.
- Do not touch the grouped branch's shape (9-10's contract) — additive only.
- The numbers exist because 9-3 generates `ReportNo`; if legacy rows carry null `ReportNo`, list
  them under a — bucket and note the count.

### Out of scope (later stories/epics — do not build)

| Item | Story |
| --- | --- |
| Payment-batch restriction | EP-10 (TODO marker) |
| Print of the numbers list | EP-18 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.15] scenario (legacy signature
  with PaymentId)
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.S.1] the numbers command icon
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-15 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-9-orphan-periodic-reports/9-10-view-report-statistics-by-group.md] the
  shared-endpoint contract this story extends

## Dev Agent Record

### Agent Model Used

Claude (GLM-5) via Claude Code — `/bmad-dev-story` pass over EP-09.

### Debug Log References

- `dotnet build Backend/src/IIROSA.Application/IIROSA.Application.csproj` — Build succeeded.
- `npx tsc --noEmit` — zero findings filtered for `periodic-orphan-reports`.
- Full `npm run build` pending — batched with the later stories' frontend edits.

### Completion Notes List

- **Shared-endpoint contract held**: the numbers branch is additive to
  `POST /api/OrphanReports/statistics` — 9-10's grouped shape untouched; the branch is
  discriminated by `includeReportNumbers` + the date window (both required, else the fields stay
  empty for the 9-10 consumers).
- **Window semantics**: "added in a period" bounds `CreatedOn` (when the register entry was
  made), not ReportDate — reconcile-submissions purpose; inclusive end date per the 9-1 rule.
  ReportDate still shows in the row.
- **Legacy nulls**: rows with null ReportNo stay in the extract under a — bucket with their own
  count tile (shown only when non-zero) — the "list them and note the count" ruling.
- **Cap**: 1000 rows newest-first with uncapped `reportNumbersCount` + capped indicator.
- **PaymentId**: accepted nowhere yet — `// TODO EP-10: payment-batch filter` marker at the
  branch head.
- **Endpoint roles**: statistics keeps `SuperAdmin,Admin,Charity` (9-10's contract; the story
  table's HQ role list rides the permission guard in the UX).

### File List

- `Backend/src/IIROSA.Application/DTOs/OrphanReport/OrphanReportFilterDto.cs` —
  `IncludeReportNumbers` flag
- `Backend/src/IIROSA.Application/Interfaces/IOrphanReportService.cs` — `ReportNumbers`,
  `ReportNumbersCount`, `UnnumberedReportsCount` on OrphanStatisticsDto; `OrphanReportNumberRow`
- `Backend/src/IIROSA.Application/Services/OrphanReportService.cs` — numbers branch (pin, window,
  counts, capped projection)
- `Frontend/src/app/modules/periodic-orphan-reports/models/periodic-orphan-report.model.ts` —
  `OrphanReportNumberRow` interface; numbers fields on OrphanStatisticsDto; filter flag
- `Frontend/src/app/modules/periodic-orphan-reports/report-numbers/report-numbers.component.ts/.html/.scss`
  — new numbers view (criteria, count tiles, grid, ExcelJS export)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-orphan-reports-routing.module.ts` —
  `orphan-reports/report-numbers` route + PermissionGuard
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-reports-list/periodic-reports-list.component.html`
  — §14.S.1 numbers command button
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` —
  `periodicReports.reportNumbers.*` (14 keys per locale)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-15 and module spec §14.U.15; shared-statistics branch contract with 9-10 pinned; payment-batch deferral recorded. |
| 2026-08-24 | Implemented: includeReportNumbers branch on the shared statistics action (CreatedOn window, pin, 1000 cap, unnumbered bucket); numbers view with count tiles + ExcelJS export; i18n ar+en. Backend 0 errors, tsc clean; full build pending. |
| 2026-08-24 | Verification pass: final gates run — `npm run build` GREEN (epic-9 module compiled; NG8107 optional-chain warnings only) and backend 0 errors for epic-9 code (the only 2 solution errors are the parallel epic-18 session’s in-flight untracked `ReportService.cs` — CS0019 ×2, left untouched per convention). Live API wedged (accepts TCP, empty replies) — restart pending; live walkthrough stays batched. Status ready-for-dev → review. |


### Review Findings (epic review 2026-08-24)

> Review Outcome 2026-08-24 — all items patched & verified: D1 resolution applies (see 9-10).

- [x] [Review][Decision] D1 (also applies here) — see 9-10 — **resolved 2026-08-24: endpoint roles widened** (Accountant + Employee added to [Authorize] on POST /api/OrphanReports/generate and /statistics, matching the PERMISSION_ROLES map)
- [x] [Review][Patch] P1c Soft-delete leak: numbers/counts include deleted reports [OrphanReportService.cs:510]
- [x] [Review][Patch] P16 IncludeReportNumbers window semantics — silent no-op without window; partial indistinguishable from empty
- [x] [Review][Patch] P42e Pagination clamp [report-numbers.component.ts:127]
- [x] [Review][Patch] P30f HQ charity dropdown markForCheck [report-numbers.component.ts:71]
