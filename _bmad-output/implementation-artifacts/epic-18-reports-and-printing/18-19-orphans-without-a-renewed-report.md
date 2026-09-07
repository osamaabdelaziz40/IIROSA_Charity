# Story 18-19: Orphans without a renewed report

| Field | Value |
| --- | --- |
| Story key | `18-19-orphans-without-a-renewed-report` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-19 — أيتام بدون تقرير مجدد |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.19 scenario — **no §23.S screen contract exists for this report**; grid derived, see Dev Notes) |
| Route | `#/reports/non-renewed-reports` — the board assigns **no route** (hosted function in the legacy system); given a route anyway for reachability, decision recorded in Dev Notes |
| Endpoint | `POST /api/Reports/non-renewed-reports` |
| Depends on | **18-1 landed** (reports skeleton: controller, service, paged envelope, `ResolveCharityScope`, `modules/reports` shell, `report.service.ts`, `report-export.service.ts`, `Reports.View`) |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (platform role names — 15-1 precedent; `Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to orphans without a renewed report أيتام بدون تقرير مجدد, so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a General Director with an active session, when the actor reaches the function from the reports menu and runs it, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/non-renewed-reports` carrying `charityId` + `batchId` and the response is rendered on the screen without a page reload.
3. Given the caller is a charity user, when the report is served, then only that charity's rows are returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload; the `CountryId` claim pins country the same way (pin-never-widen).
4. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the report runs, then it operates on that charity's data.
5. Given every orphan of the batch has a renewed report, when the report is served, then the grid renders empty and the paging control reports zero pages.
6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** one endpoint answers "for this charity and this payment batch, which orphans' reports were not renewed for the current cycle", paged and exportable; the scenario of §23.U.19 passes end to end; the scoping is enforced server-side, not only in the menu.

## Tasks / Subtasks

- [x] **Task 1 — DTOs + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`: `NonRenewedReportsFilterDto` (`Page = 1`, `PageSize = 20`, `Guid? CharityId`, `string? BatchId` — the legacy payload posts both as strings; `BatchNo` on `OrphanPayment` is the string to match), `NonRenewedReportsListDto` (`OrphanId`, `OrphanCode`, `OrphanName`, `CharityName`, `BatchNo`) — no `FK_*` wire keys
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/NonRenewedReportsValidator.cs`: page bounds + `BatchId` max length; both filters stay optional (an absent batch means "the current batch" — latest `OrphanPayment` by `GroupDate`)
- [x] **Task 2 — Service projection** (AC 1, 3, 4, 5)
  - [x] `IReportService.GetNonRenewedReportsAsync(filter)` in `ReportService`: scope through `ResolveCharityScope`; resolve the batch — `BatchId` given → `OrphanPayment.BatchNo == BatchId`, else the latest `OrphanPayment` by `GroupDate`
  - [x] Row set: orphans of that batch's `OrphanPaymentItem`s with **no** `PeriodicOrphanReport` for the current cycle (`ReportDate >= UtcNow.AddMonths(-12)` — the same cycle rule as 18-15, recorded there) — projected with `Orphan` (code, name) and charity name; paged through 18-1's `ReportPagedResult<T>`; global soft-delete filter applies
- [x] **Task 3 — API endpoint** (AC 2, 6)
  - [x] In 18-1's `ReportsController`: `[HttpPost("non-renewed-reports")]` → `Ok(paged)`; ValidationException → 400 `{ message, errors }`, catch-all → 500 `{ message }`; no `ApiResponse<T>` (15-1 ruling)
- [x] **Task 4 — Frontend thin component** (AC 1, 2, 5)
  - [x] `Frontend/src/app/modules/reports/non-renewed-reports/` 4-file component; route `#/reports/non-renewed-reports` guarded `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'` (decision recorded in Dev Notes)
  - [x] Filter panel: الجمعيات dropdown from `GET /api/Charities` (`result.items || []`, كل الجهات all-option for HQ, pinned single option for a charity user) + batch dropdown from `GET /api/OrphanPayments/batch-numbers` (18-1's reuse rule — no hardcoded arrays); explicit بحث button (the derived screen has no on-change contract)
  - [x] Grid: الجمعيه · كود اليتيم · اسم اليتيم · الدفعة (columns derived — no §23.S contract); shared `Pagination`; `trackBy: orphanId`; row serial formula; empty state at `totalCount === 0`; **not `data-list`**; OnPush omitted (list-screen precedent)
  - [x] استخراج البيانات through 18-1's `report-export.service.ts`; empty grid → nothing-to-produce message, no file
- [x] **Task 5 — i18n** — menu label, title, two filter labels + all-options, بحث, 4 column headers, empty state, export messages under `reports.*` in **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; **no EF migration**. MSB3021/3027 = live-API output lock; never kill the user's process
  - [x] Live check: anonymous POST → 401; HQ with a seeded `batchId` → 200 paged camelCase; absent `batchId` → resolves the latest batch; charity token → only its rows; fully-renewed batch → zero pages; Arabic payloads from UTF-8 files when curling
  - [x] `cd Frontend && npm run build` — 0 errors; ng-serve stale-bundle caveat (grep the served chunk)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Route decision (recorded)

The board assigns US-RPT-19 no route — the legacy system hosted the function inside another screen. An unroutable report is an unreachable report on this platform: every report gets a thin component under `modules/reports/` and an entry in the reports menu. `#/reports/non-renewed-reports` is therefore created here and recorded as the board-level route for this use case.

### Variant collapse (recorded)

§23.1/§23.U.19 list legacy variants — detail, V2, count-only (`_Number`, `GetBeginingScreen`). All three answer the same question with different projections. This story ships **one** endpoint, `POST /api/Reports/non-renewed-reports`, whose paged detail result carries `TotalCount` — the count-only variant is `totalCount`, and V2 had no documented column difference. If HQ ever needs a genuinely different V2 layout, it is a filter/shape switch on this endpoint, not a new vertical.

### Cycle rule (shared with 18-15)

"Report not renewed for the current cycle" uses the same trailing-12-months accepted-report rule fixed in 18-15 (`IsAccepted == true`, `ReportDate >= UtcNow.AddMonths(-12)`). One rule across the epic's chase reports — do not fork it here.

### Platform rules that bind this story

- Wire is camelCase; no `FK_*` DTO keys (Newtonsoft emits `fK_…`).
- Raw paged envelope + anonymous `{ message, errors }` — NOT `ApiResponse<T>` (15-1 ruling, architecture.md §10).
- Controllers inherit `ControllerBase` + `[Authorize]` + `[Route("api/[controller]")]` (17-1 note).
- FluentValidation in the service (`Validators/Reports/`), never in the controller.
- Reads via `IUnitOfWork` repositories; repositories never save; this story writes nothing.
- Soft delete via the global query filter — never hand-check `IsDeleted`.
- Lookup labels `NameAr ?? NameEn`; batch list from `GET /api/OrphanPayments/batch-numbers` — no hardcoded arrays.
- Bespoke grid + shared `Pagination` (NOT `data-list` — recorded deviation); OnPush omitted on list screens (codebase precedent).
- Caller scope from `ICurrentUserService` — pin-never-widen (`OfficeProjectService.cs:384` shape).
- EP-18 adds no entities, no EF migration.
- Tests excluded per the standing user decision.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Reports skeleton | 18-1 |
| The charity follow-up screen and its tracking grid | 18-20 |
| The PDF printing path | 18-21 |
| V2 / count-only / `GetBeginingScreen` variant endpoints | none — collapsed (recorded above) |
| Any other report vertical | 18-2 … 18-18 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.19] scenario — per charity and batch, orphans whose report was not renewed
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.1] §23.2 table row — the legacy variants this story collapses
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-19 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/OrphanPayment.cs] `BatchNo`, `GroupDate`, `Orphans` items — the batch resolution
- [Source: Backend/src/IIROSA.Domain/Entities/OrphanPaymentItem.cs] `OrphanId` — the batch membership
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:68] `GET batch-numbers` — the batch filter lookup
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-15-coded-orphans-needing-a-report.md] the shared 12-month cycle rule

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `dotnet build` (Api csproj, temp `-o`) — **0 errors** (pre-existing warnings only).
- `npx ng build` — NG_EXIT=0, `error TS` count 0. (The two "Error occurs in the template" lines in the log are context labels on an NG8107 **warning** in the parallel session's `report-numbers` component — foreign file, warning-only.)
- i18n `reports.nonRenewed.*` — 13 keys in each locale, key sets identical, JSON parses.
- Live smoke, private instance `127.0.0.1:60970`, seeds `18199…` (2 payments SB-1819 latest + SB-OLD older, 4 items over 3 orphans, accepted July report for one orphan + pending for another; the filtered unique index `IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear` counts soft-deleted rows, so report months were picked against the deleted set):
  - anonymous POST `/api/Reports/non-renewed-reports` → **401**
  - **batch mode, absent batchId** (latest resolution: SB-1819, GroupDate 2026-08-24 beats all non-deleted payments) → 200, `count`/`totalCount` **2**, `totalPages` 1: the no-report orphan and the pending-only orphan, `batchNo: "SB-1819"` on both, Code-ordered; the orphan with an accepted+reviewed report **cleared**; `lastReportDate` null vs the pending report's date
  - explicit `batchId: "SB-1819"` → identical; `batchId: "SB-OLD"` (sole member = the cleared orphan) → **0 / zero pages**; unknown `batchId: "NOPE-404"` → **0 / zero pages** (AC 5)
  - dga narrow → 1; وادي النطرون narrow → 1 (AC 4)
  - `countOnly: true` → `count: 2`, `items: []`, `totalPages: 0` (the _Number variant)
  - **window-mode regression (the §14.U.14 screen)**: dates, no batch → 200, `count: 7` (its own wider set), both chase orphans present with `batchNo: null` — the old semantics and the `count` read are intact
  - `{"dateFrom":"2026-01-01"}` only → **400** `errors.DateTo: "DateTo is required …"`; `page: 0` → **400**
  - charity login (`Charity@IIROSA.com`, claim-less) → batch rows unscoped within the batch — the documented 18-12 convention (role name alone never scopes)
  - seeds hard-deleted (payments/items/reports back to 0 for the `18199…` ids); smoke instance killed by PID.

### Completion Notes List

- **Stale premise — the endpoint already existed:** `POST /api/Reports/non-renewed-reports` shipped with the §14.U.14 window chase list (UC-ORR-14, route `#/periodic-orphan-reports/orphan-reports/non-renewed`). This story is realised as a **second mode on that endpoint**, not a new vertical: `batchId` sent (or no window at all — absent batch means the current one) selects batch mode; a window without a batch keeps the §14.U.14 list byte-compatible. The variant-collapse and route-grant Dev Notes stand.
- **DTO names deviate from Task 1 deliberately:** the existing `NonRenewedReportsRequestDto` / `NonRenewedOrphanRowDto` were extended (`BatchId`, `BatchNo`) instead of adding parallel `NonRenewedReportsFilterDto` / `NonRenewedReportsListDto` — one endpoint, one request/row contract, no duplicate shapes on the same route. Likewise the existing `NonRenewedReportsRequestValidator` was extended rather than adding `NonRenewedReportsValidator.cs`.
- **Envelope deviation, recorded:** the story asked for 18-1's `ReportPagedResult<T>`; the endpoint keeps `NonRenewedReportsResultDto` because its `count` + `PageSize=0` countOnly contract serves the §14.U.14 screen — `TotalCount`/`TotalPages` were added as the ReportPagedResult wire aliases so this screen reads the platform convention.
- **Cycle rule:** the BR-11 accepted-report predicate (accepted+reviewed, period-intersection with ReportDate fallback) was extracted to `BuildAcceptedReportIdsQuery(windowStart, windowEndExclusive)` and now serves 18-15's builder, the §14.U.14 window mode, and batch mode (trailing 12 months) — one rule, three callers.
- **Batch resolution rulings:** latest = the newest non-deleted payment by `GroupDate` **that carries non-deleted items** (a batch without members chases nobody); membership = that `BatchNo`'s non-deleted items across ALL payments carrying it; the window mode's coded-only filter is **skipped in batch mode** — the batch, not the coding, is the membership semantic; latest-batch resolution is unscoped (the story fixes no scope for it) — a charity caller whose batches aren't the global latest picks theirs from the dropdown.
- **Roles kept from the §14.U.14 ruling** (`SuperAdmin,Admin,Accountant,Employee,Charity`) rather than the story's Gen. Director pair — AC 3 requires charity callers to be served (pinned server-side), so the Charity role must reach the endpoint.
- Frontend: class `BatchNonRenewedReportsComponent` (folder/route per the story) — avoids colliding with the §14.U.14 screen's `NonRenewedReportsComponent`; the batch dropdown reuses `OrphanPaymentService.getBatchNumbers()` (18-1's reuse rule) and reloads on the HQ charity narrow (10-6 pattern); export pages at **100** (this endpoint's validator cap), not 18-17/18-18's 200.
- Explicit `!IsDeleted` throughout (the story text's "global soft-delete filter" is the standing correction — the platform has none).

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `NonRenewedReportsRequestDto.BatchId`, `NonRenewedOrphanRowDto.BatchNo`, `NonRenewedReportsResultDto.TotalCount`/`TotalPages` + doc updates
- `Backend/src/IIROSA.Application/Validators/Reports/NonRenewedReportsRequestValidator.cs` — window rules conditional on mode; `BatchId` max length
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — batch-mode branch in `GetNonRenewedReportAsync`; `BuildAcceptedReportIdsQuery` extracted (also now used by 18-15's builder)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — doc comment only (endpoint pre-existed)
- `Frontend/src/app/modules/reports/models/report.model.ts` — `batchId`/optional dates on the request, `batchNo` on the row, `totalCount`/`totalPages` on the result
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportNonRenewedReports()`
- `Frontend/src/app/modules/reports/non-renewed-reports/` — 4-file component (new)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — import + `non-renewed-reports` route (`Reports.View`)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.nonRenewed.*` (13 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-19 and module spec §23.U.19; route granted for reachability and legacy variant collapse recorded; cycle rule shared with 18-15. |
| 2026-08-24 | Implemented and verified: batch mode added to the existing `non-renewed-reports` endpoint (window mode byte-compatible, regression-proven live), BR-11 predicate extracted shared, thin batch-driven screen + export + i18n. Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
