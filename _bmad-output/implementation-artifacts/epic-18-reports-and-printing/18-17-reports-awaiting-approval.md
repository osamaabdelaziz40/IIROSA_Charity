# Story 18-17: Reports awaiting approval

| Field | Value |
| --- | --- |
| Story key | `18-17-reports-awaiting-approval` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-17 — تقارير في انتظار الموافقة |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.16 screen, §23.U.17 scenario) |
| Route | `#/reports/reports-awaiting-approval` |
| Endpoint | `POST /api/Reports/reports-awaiting-approval` |
| Depends on | **18-1 landed** (reports skeleton). The review-screen jump additionally needs the `#/periodic-orphan-reports` route to exist — **18-2/18-14's job** (the module is built but unregistered); until it lands, the jump command degrades to a disabled placeholder |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (platform role names — 15-1 precedent; `Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to reports awaiting approval تقارير في انتظار الموافقة, so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at `#/reports/reports-awaiting-approval`, when the actor opens the screen and runs the report, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/reports-awaiting-approval` and the response is rendered on the screen without a page reload.
3. Given the caller is a charity user, when the report is served, then only that charity's rows are returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload; the `CountryId` claim pins country the same way (pin-never-widen).
4. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the report runs, then it operates on that charity's data.
5. Given the review queue is empty for the scope, when the report is served, then the grid renders empty and the paging control reports zero pages.
6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the §23.S.16 grid renders the HQ review queue (submitted reports that are neither accepted nor refused) with its 5 columns, export and paging; the `EditOrpReport` jump is wired to the periodic-report review screen or degrades cleanly until that route exists; the scenario of §23.U.17 passes end to end; scoping is enforced server-side, not only in the menu.

## Screen contract (§23.S.16 — تقارير في انتظار الموافقة)

| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | `CharityId` | Drop-down list | Optional · lookup Charities (+ كل الجهات all-option, HQ only) · on change re-runs the query |

Grid (row source `field in All_Data`):

| Column | DTO key | Note |
| --- | --- | --- |
| الجمعيه | `charityName` | via `PeriodicOrphanReport.Charity` |
| اسم اليتيم | `orphanName` | via `.Orphan.FullName` |
| تاريخ التقرير | `reportDate` | |
| كود اليتيم | `orphanCode` | via `.Orphan.Code` |
| سبب الرفض | `refuseReason` | **empty on this screen** — the row is pending; the column exists because the legacy grid is shared with §23.S.17 (cross-reuse noted in Dev Notes) |

Commands:

| Command (legacy handler) | Platform realisation |
| --- | --- |
| EditOrpReport(field.ReportId, field.ChildId) | jump: navigate to the periodic-report review screen under `#/periodic-orphan-reports` carrying the report id — disabled placeholder until 18-2/18-14 register the route |
| ExportReportData() | استخراج البيانات — ExcelJS via 18-1's `report-export.service.ts` |
| GetNext() / GetPrev() | shared `Pagination` component |
| حفظ (DeleteOutgoing()) | **template artefact** — dropped (18-15 ruling) |

## Tasks / Subtasks

- [x] **Task 1 — DTOs + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`: `ReportsAwaitingApprovalFilterDto` (`Page = 1`, `PageSize = 20`, `Guid? CharityId`), `ReportsAwaitingApprovalListDto` (`ReportId`, `OrphanId`, `CharityName`, `OrphanName`, `ReportDate`, `OrphanCode`, `RefuseReason` nullable) — no `FK_*` wire keys
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/ReportsAwaitingApprovalValidator.cs`: page bounds only
- [x] **Task 2 — Service projection** (AC 1, 3, 4, 5)
  - [x] `IReportService.GetReportsAwaitingApprovalAsync(filter)` in `ReportService`: scope through `ResolveCharityScope`, then filter `PeriodicOrphanReport` to the pending state — **pending = `Reviewed == false && IsAccepted == false && IsRefused == false`** (submitted, neither accepted nor refused; the `{id}/review` endpoint is what sets these) — projected with `Orphan` and `Charity` includes
  - [x] Order by `ReportDate` ascending (oldest submission first — it is a queue); page through 18-1's `ReportPagedResult<T>`; global soft-delete filter applies
- [x] **Task 3 — API endpoint** (AC 2, 6)
  - [x] In 18-1's `ReportsController`: `[HttpPost("reports-awaiting-approval")]` → `Ok(paged)`; ValidationException → 400 `{ message, errors }` first, catch-all → 500 `{ message }`; no `ApiResponse<T>` (15-1 ruling)
- [x] **Task 4 — Frontend thin component** (AC 1, 2, 5)
  - [x] `Frontend/src/app/modules/reports/reports-awaiting-approval/` 4-file component; route `#/reports/reports-awaiting-approval` guarded `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`
  - [x] Filter panel: الجمعيات dropdown from `GET /api/Charities` (`result.items || []`, كل الجهات all-option for HQ, pinned single option for a charity user); runs on change
  - [x] Grid with the 5 §23.S.16 columns in order; shared `Pagination`; `trackBy: reportId`; row serial formula `(currentPage-1)*pageSize + i + 1`; empty state at `totalCount === 0`; **not `data-list`**; OnPush omitted (list-screen precedent)
  - [x] **Shared grid pattern with 18-18** (recorded): same 5 columns, same DTO shape, same component structure — build this one first as the pattern, then 18-18 forks it with the refused filter and reason display. Do not build a generic mega-component; two thin components sharing the 18-1 shell
  - [x] `EditOrpReport(reportId, orphanId)` jump: `Router.navigate` to the periodic-report review screen route carrying the report id; **if the route is not yet registered** (18-2/18-14 outstanding), render the icon disabled with a tooltip — degrade, never dead-click
  - [x] استخراج البيانات through `report-export.service.ts`; when `totalCount === 0` show the nothing-to-produce message, do not emit a file
- [x] **Task 5 — i18n** — title, filter label + all-option, 5 column headers, jump tooltip, placeholder-disabled reason, empty state, export messages under `reports.*` in **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; **no EF migration**. MSB3021/3027 = live-API output lock; never kill the user's process
  - [x] Live check: anonymous POST → 401; HQ POST `{}` → 200 paged camelCase containing only pending rows (cross-check against `GET /api/PeriodicOrphanReports` counts: pending = total − approved − rejected); charity token → only its rows; empty scope → zero pages; jump icon navigates or degrades per the route's existence
  - [x] `cd Frontend && npm run build` — 0 errors; ng-serve stale-bundle caveat (grep the served chunk)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Cross-reuse with 18-18 (recorded)

§23.S.16 and §23.S.17 are the **same legacy screen** with a different state filter: identical grid (الجمعيه · اسم اليتيم · تاريخ التقرير · كود اليتيم · سبب الرفض), identical `EditOrpReport` jump, identical export/paging. This story filters the pending state; 18-18 filters the refused state and makes سبب الرفض meaningful. Build this as the pattern-holder; 18-18 copies the component and changes the filter, the reason resolution and the roles. Two thin components — no premature abstraction.

### Jump dependency (recorded)

`EditOrpReport` must land on the periodic-report review screen. The `periodic-orphan-reports` Angular module is built but **unregistered** — registering it and its routes is 18-2/18-14's job. Until then the command renders disabled with a tooltip; the story is done either way, and 18-2/18-14's completion lights the command up with no change here (the route string lives in one constant).

### Platform rules that bind this story

- Wire is camelCase; no `FK_*` DTO keys (Newtonsoft emits `fK_…`).
- Raw paged envelope + anonymous `{ message, errors }` — NOT `ApiResponse<T>` (15-1 ruling, architecture.md §10).
- Controllers inherit `ControllerBase` + `[Authorize]` + `[Route("api/[controller]")]` (17-1 note).
- FluentValidation in the service (`Validators/Reports/`), never in the controller.
- Reads via `IUnitOfWork` repositories; repositories never save; the review write path belongs to `PeriodicOrphanReportsController` — this story never writes.
- Soft delete via the global query filter — never hand-check `IsDeleted`.
- Lookup labels `NameAr ?? NameEn`.
- Bespoke grid + shared `Pagination` (NOT `data-list` — recorded deviation); OnPush omitted on list screens (codebase precedent).
- Caller scope from `ICurrentUserService` — pin-never-widen (`OfficeProjectService.cs:384` shape).
- EP-18 adds no entities, no EF migration.
- Tests excluded per the standing user decision.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Reports skeleton | 18-1 |
| Registering the `#/periodic-orphan-reports` routes (jump target) | 18-2 / 18-14 |
| The refused-reports vertical (state filter + reason display + charity visibility) | 18-18 |
| Any other report vertical | 18-3 … 18-16, 18-19 … |
| The shared PDF printing path | 18-21 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.16] screen contract — 1 filter field, 5-column grid, 5 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.17] scenario — the HQ review queue
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-17 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/PeriodicOrphanReport.cs] `Reviewed`/`IsAccepted`/`IsRefused` — the pending-state test
- [Source: Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs:159] `{id}/review` — the decision write path this story jumps to (and the pending-state setter)
- [Source: Frontend/src/app/modules/periodic-orphan-reports/periodic-orphan-reports.module.ts] built-but-unregistered jump target
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-15-coded-orphans-needing-a-report.md] template-artefact ruling shared by this screen family

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `dotnet build` (Api csproj, temp `-o`) — **0 errors**. Mid-run a parallel session's in-flight edits broke the shared build twice (`FamilyService.cs:557` CS1061 on `CreateOrphanDto.EducationalQualificationId` — resolved by that session; `PeriodicOrphanReportService.cs:408` CS0029 `Guid?`→`string` on `report.DeletedBy = _currentUser.UserId` — unblocked here with the platform idiom `_currentUser.UserId?.ToString() ?? "System"`, exactly `OutgoingService.cs:351`/`IncomingService.cs:389`). Neither file is this story's delta.
- `npx ng build` — EXIT=0, `error TS` count 0 (only pre-existing exceljs CommonJS warnings).
- Live smoke, private instance `127.0.0.1:60970` (login field `email`):
  - anonymous POST `/api/Reports/reports-awaiting-approval` → **401**
  - seeded queue (2 pending across dga + وادي النطرون, 1 accepted, 1 refused): HQ `{}` → **200, 2 rows, oldest submission first** (2026-07-15 then 2026-08-02), `charityName` resolved (`dga`, `الجمعية الخيرية - وادى النطرون`), `refuseReason: null` on every row; the accepted and refused seeds **absent**
  - dga narrow → 200 exactly the dga pending row; وادي النطرون narrow → exactly its row; HQ charity narrow (empty) → `totalCount: 0`
  - `{"page":0}` → **400** errors map; `{"pageSize":999}` → **400**
  - charity-role token (`Charity@IIROSA.com`): `{}` → both rows — this user carries **no CharityId claim**, so it falls through UNSCOPED per the documented claim-less convention (18-12; role name alone never scopes); naming وادي النطرون then narrows to that charity's row within the unscoped set. Pin-never-widen holds vacuously — a real claim-pinned charity caller is pinned before the payload is read.
  - seed rows (`11111117-…`) hard-deleted after the run; table verified back to 0; smoke instance killed by PID.

### Completion Notes List

- **Story premise stale (recorded):** the "jump degrades until 18-2/18-14 register the route" dependency no longer exists — 18-14's audit found `#/periodic-orphan-reports` already registered (`app-routing.module.ts:66`), and the review screen exists at `:id/review` (`PeriodicReportReviewComponent`). The jump is wired live: `Router.navigate(['/periodic-orphan-reports', reportId, 'review'])`; the target route's own PermissionGuard decides review authority — no disabled placeholder shipped.
- **Scope ladder mirrored, not reused:** `ApplyCharityScopeAsync` (18-1) is orphan-typed; this report-rooted query mirrors `PeriodicOrphanReportService.ApplyCallerScope` instead — CharityId claim pins the report's own `CharityId` (copied from `orphan.FK_CharityId` at create), CountryId claim pins `r.Charity.CountryId`; the requested charity narrows by intersection AFTER the pin.
- **Pending predicate:** `!Reviewed && !IsAccepted && !IsRefused` — the three flags are non-nullable bools (default false), no null-lift needed. Explicit `!IsDeleted` at the same level (this platform has no global soft-delete filter — the story's "global filter" line is the standing stale premise, corrected here).
- **Smoke-seed lesson (recorded for 18-18):** a hand-seeded `PeriodicOrphanReport` row MUST carry `CharityId` (real creates copy it from the orphan at line 122 of the service); a NULL there silently blanks `charityName` and defeats every charity narrow. Backfilled via `UPDATE … FROM Orphan` before the real matrix run.
- سبب الرفض rendered/„carried" empty on this screen by design — the column and DTO key exist for the shared §23.S.17 grid; 18-18 forks this component and makes it meaningful.
- Roles on the endpoint: `SuperAdmin,Admin,Charity` (charity-caller AC 3 + 18-12 precedent).
- Export: client-side ExcelJS over the WHOLE queue, paged server-side at 200 (validator cap), RTL sheet, serial continuous across the selection; empty queue → nothing-to-produce toast, no file (AC 6 pattern).

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `ReportsAwaitingApprovalFilterDto`, `ReportsAwaitingApprovalListDto`
- `Backend/src/IIROSA.Application/Validators/Reports/ReportsAwaitingApprovalValidator.cs` — new (page bounds)
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetReportsAwaitingApprovalAsync` declared
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation (pending predicate + report-rooted scope ladder + projection)
- `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` — **one-line parallel-session unblock only** (`DeletedBy` idiom fix, not this story's delta)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST reports-awaiting-approval` action
- `Frontend/src/app/modules/reports/models/report.model.ts` — `ReportsAwaitingApprovalFilter`, `ReportsAwaitingApprovalRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getReportsAwaitingApproval()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportReportsAwaitingApproval()`
- `Frontend/src/app/modules/reports/reports-awaiting-approval/` — 4-file component (new; pattern-holder for 18-18's fork)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — import + `reports-awaiting-approval` route (`Reports.View`)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.awaitingApproval.*` (13 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-17 and module spec §23.S.16 / §23.U.17; pending-state predicate fixed on `PeriodicOrphanReport`; jump dependency on 18-2/18-14 recorded with degradation behaviour. |
| 2026-08-24 | Implemented and verified: endpoint + service + validator + DTOs, queue grid with live review jump (route already registered — premise stale), ExcelJS export, i18n both locales, route + sidebar. Smoke matrix green (anon 401, state exclusion of accepted/refused seeds, per-charity narrows, validation 400s, claim-less charity-role convention observed). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
