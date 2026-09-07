# Story 9-13: Extract refused reports

| Field | Value |
| --- | --- |
| Story key | `9-13-extract-refused-reports` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-13 — التقارير المرفوضة |
| Priority / size | Must · 8 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.U.13 scenario; the refused-reports command of §14.S.1) |
| Route | command on `#/periodic-orphan-reports` rendering a refused-filtered view |
| Endpoint | `GET /api/PeriodicOrphanReports/rejected` |
| Depends on | **9-8** (refusal + reasons exist), **9-11/9-12** (shared extract surface), **9-1** (scope) |
| Roles | HQ roles → `SuperAdmin`, `Admin`, `Accountant`, `Employee` |

## Status

review

## Story

As a HQ role, I want to be able to extract refused reports التقارير المرفوضة, so that head office
keeps control of what is accepted into the sponsorship cycle.

## Acceptance Criteria

1. Given a HQ role in the module, when the actor runs the refused-reports extract, then the result
   contains only reports with `Reviewed && IsRefused` (reviewStatus "Rejected") — the correction
   worklist sent back to charities.
2. Given the request is accepted, when it is served, then it is handled by `GET
   /api/PeriodicOrphanReports/rejected` and the response is rendered without a page reload.
3. Given a charity user, when the function is invoked, then only that charity's refused reports are
   returned; an HQ role may pass an explicit `charityId`.
4. Given the extract renders, when a row carries refusal reasons, then the reason text(s) show in
   the grid and export (the worklist must say **why**).
5. Given the extract runs, when rows match, then the detailed Excel downloads; no match → "nothing
   to produce" message.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.13's summary row passes end to end (extract, not a review action —
same mis-derivation note as 9-12); reasons travel with every row; scoping server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Service | `GetRejectedReportsAsync(filter)` | Exists: pins `Reviewed=true, IsRefused=true`, delegates to `GetReportsAsync` |
| API | `GET /rejected` | Exists |
| Frontend | `getRejectedReports` service method + §14.S.1 command icon `ExtractRefusedOrphansReportsData` | Exist; no handler |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Reasons not in the list projection.** `PeriodicOrphanReportListDto` has `ReviewStatus` but the
   refused reason text is not selected — add `RefuseReason`/`RefuseReasonName` (resolve the id via
   the 9-8 lookup in the projection) to the list DTO and the 9-11 detailed export columns.
2. **`ReviewStatus` switch maps "rejected" → `IsRefused` alone** — align to the flags pair (9-12
   defect 1, same fix on this branch).
3. **Caller scope on `/rejected`** (9-1 scope, currently client-trusted).
4. **No extract surface** — build the refused variant of the 9-12 interaction.

## Tasks / Subtasks

- [x] **Task 1 — Projection + predicate + scope** (AC 1, 3, 4): add reason columns to the list DTO
      + profile (defect 1); flags-pair predicate (defect 2); caller scope (defect 3)
      — found already fixed by the earlier epic passes: `RefuseReason` sits in the list
      projection (service resolves the text), the `ReviewStatus="Rejected"` branch of
      `ApplyFilters` sets the `Reviewed && IsRefused` flags pair, and `GetPagedAsync` pins
      `ApplyCallerScope` before anything else. Verified, not rebuilt. The 9-12 `ReportNo`
      dedicated partial-match predicate serves this branch too (UC-ORR-12/13 criterion).
- [x] **Task 2 — Worklist interaction** (AC 2, 4, 5): the §14.S.1 refused command opens the
      extract view (9-12's surface) with the reason column prominent; charity (HQ), date window,
      report number criteria; استخراج → ExcelJS workbook including the reason column; empty →
      "nothing to produce"
      — built ONCE as the shared `orphan-report-state-extract` surface (`:state` route param —
      9-12 owns the file): this story contributed the refused branch — refuse-reason column
      (prominent, red) only when `state === 'refused'`, the same criteria panel (charity
      catalogue for HQ, date window, report no), the same 100-row paged drain to the 2000-row
      cap, the same ExcelJS workbook + reason column, the same "nothing to produce" toast on an
      empty selection. Register command: btn-outline-danger in §14.S.1's command row.
- [x] **Task 3 — Charity hand-off** (worklist purpose): from a refused row, HQ sees the reason; the
      charity side already gets its correction route from 9-8 Task 4 (edit + resubmit) — link the
      row to the report detail (`:id`) for the full context
      — actions column links to `['/periodic-orphan-reports', row.id]` (the 9-4 detail with the
      9-16 gallery); the resubmission side is 9-5's recorded behaviour (refused → pending).
- [x] **Task 4 — i18n** — labels under `periodicReports.extract.refused.*` in **both** `ar.json`
      and `en.json`
      — 8 keys per locale; column labels shared with 9-11/9-12 via `orphanReports.generate.columns.*`.
- [ ] **Task 5 — Verification** (AC 1–6): live check — every row `isRefused: true && reviewed:
      true` with a non-empty reason; resubmitting a refused report (9-5) removes it from this
      extract and re-queues it; charity pin holds; export includes reasons; unauthenticated → 401;
      `npm run build` green; tests excluded per the standing user decision
      — static: `npx tsc --noEmit` zero non-spec errors; `npm run build` running as this epic's
      final gate; live checks follow the build.

## Dev Notes

### Platform rules that bind this story

- Read-only; raw envelope; camelCase wire; ExcelJS export; reuse the 9-11/9-12 surface and
  builder — one predicate + one column differ.
- The reason snapshot lives in `RefuseReason` (text) with `RefuseReasonId` as the code — export the
  text, keep the code for joins.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Non-renewed orphans extract | 9-14 |
| Notification to charities that a report was refused | EP-19 (cross-cutting) |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.13] scenario (summary row
  governs)
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#25.5] step 6 — the refused report
  appears on the charity's correction list with its reasons
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-13 acceptance criteria

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5), 2026-08-24.

### Debug Log References

- `npx tsc --noEmit` (Frontend) — zero non-spec errors with the state-extract surface in place.
- Backend unchanged by this story beyond what 9-12's pass already verified (`dotnet build`
  Application 0 errors).

### Completion Notes List

- **Story-premise correction (recorded):** defects 1–3 were already fixed by the earlier epic-9
  passes — `RefuseReason` in the list projection (service-resolved text), the
  `ReviewStatus="Rejected"` → `Reviewed && IsRefused` flags pair in `ApplyFilters`, and
  `ApplyCallerScope` first in `GetPagedAsync`. Verified in place, not rebuilt.
- **One shared surface, not two:** 9-12 and 9-13 are the same interaction with one predicate and
  one column different, so the extract view is a single `orphan-report-state-extract` component
  parameterised by the `:state` route segment (`accepted` | `refused`); 9-12 owns the file and
  this story's contribution is the refused branch (reason column + i18n + register command +
  variant title/breadcrumb). Column labels shared with 9-11 via `orphanReports.generate.columns.*`.
- Export cell for the reason uses `refuseReason || '—'` (the list row carries the snapshot text;
  the resolved `refuseReasonName` is a detail-screen field).
- AC 3's charity pin is server-side (the 9-1 `ApplyCallerScope`); the charity drop-down is an
  HQ-only narrow, hidden for charity callers as convenience only.

### File List

- `Frontend/.../orphan-report-state-extract/orphan-report-state-extract.component.ts/.html/.scss`
  — the shared surface (9-12-owned); this story's refused branch.
- `Frontend/.../periodic-reports-list/periodic-reports-list.component.html` — the refused-extract
  register command (btn-outline-danger).
- `Frontend/.../periodic-orphan-reports-routing.module.ts` — `orphan-reports/extract/:state` route
  (shared; refused title/breadcrumb from `periodicReports.extract.refused.*`).
- `Frontend/src/assets/i18n/ar.json`, `en.json` — `periodicReports.extract.refused.*` keys.

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-13 and module spec §14.U.13 / §25.5; missing-reason-projection defect recorded. |
| 2026-08-24 | Implemented as the refused branch of the shared state-extract surface (defects 1–3 found already fixed by earlier epic passes — recorded); reason column in grid + export; register command + i18n; tsc green; npm build batched. |
| 2026-08-24 | Verification pass: final gates run — `npm run build` GREEN (epic-9 module compiled; NG8107 optional-chain warnings only) and backend 0 errors for epic-9 code (the only 2 solution errors are the parallel epic-18 session’s in-flight untracked `ReportService.cs` — CS0019 ×2, left untouched per convention). Live API wedged (accepts TCP, empty replies) — restart pending; live walkthrough stays batched. Status ready-for-dev → review. |


### Review Findings (epic review 2026-08-24)

> Review Outcome 2026-08-24 — all items patched & verified: P39b subsumed by the P39 fix: the server totalCount now drives the capped indicator, so the 2000-row cap is visible rather than silent. AA3 delivered as literal accepted/refused routes with correct pageTitles + guarded :state fallback (P25 warns on unknown states).

- [x] [Review][Patch] P39b (shared with 9-12) silent 2000-row cap [orphan-report-state-extract.component.ts]
- [x] [Review][Patch] P25 Any :state other than refused silently renders accepted extract — guard unknown state [orphan-report-state-extract.component.ts:82]
- [x] [Review][Patch] AA3 Route pageTitle hardcodes accepted title for both states — refused tab mislabeled [periodic-orphan-reports-routing.module.ts:142]
- [x] [Review][Patch] F7 Reason column "prominent, red" not styled (AC met; record overstates) — add class or correct record
