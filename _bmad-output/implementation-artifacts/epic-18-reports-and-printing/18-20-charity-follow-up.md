# Story 18-20: Charity follow-up

| Field | Value |
| --- | --- |
| Story key | `18-20-charity-follow-up` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-20 — متابعة الجمعيات |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.12 screen, §23.U.20 scenario) |
| Route | `#/reports/charity-payment-tracking` |
| Endpoint | `POST /api/Reports/charity-payment-tracking` |
| Depends on | **18-1 landed** (reports skeleton). Hosts 18-21's print commands — that story wires them; this story renders them disabled |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (platform role names — 15-1 precedent; `Reports.View`). **HQ-only module**: the tracking grid is cross-charity by design |

## Status

done

## Story

As a General Director, I want to be able to charity follow-up متابعة الجمعيات, so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at `#/reports/charity-payment-tracking`, when the actor runs متابعة تسليمات الجميعات, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/charity-payment-tracking` and the response is rendered on the screen without a page reload.
3. Given the caller is not an HQ role, when the function is invoked, then the request is rejected server-side (the endpoint authorises `SuperAdmin`/`Admin`; the menu entry is convenience only); the `CountryId` claim still pins country — pin-never-widen.
4. Given an HQ caller with the كل الجهات all-option, when the report runs, then one tracking row per charity appears in the batch scope; with an explicit charity, only that charity's row.
5. Given the batch has no activity in scope, when the report is served, then the grid renders empty and the paging control reports zero pages.
6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the §23.S.12 filter panel (charity + batch + update-start date) drives a cross-charity tracking grid over the selected payment batch; the scenario of §23.U.20 passes end to end; the two family-update commands render and are wired by 18-21; the endpoint authorises HQ roles only.

## Screen contract (§23.S.12 — متابعة الجمعيات)

| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | `CharityId` | Drop-down list | Optional · lookup Charities (+ كل الجهات all-option) · on change re-runs the tracking |
| — | الدفعة المالية المنصرقة للايتام | `BatchId` | Drop-down list | Optional · lookup Batches (`GET /api/OrphanPayments/batch-numbers`) |
| تحديثات الجمعيات | من فضلك ادخل تاريخ بدا التحديث | `DateOfStartingUpdate` | Date picker | Optional · consumed by 18-21's print (families updated from this date) |

Commands:

| Command (legacy handler) | Platform realisation |
| --- | --- |
| متابعة تسليمات الجميعات (printCharityPaymentTracking()) | runs the tracking grid — `POST /api/Reports/charity-payment-tracking` (this story; the legacy name says "print" but the screen rendered data — see Dev Notes) |
| متابعة تحديثات الجميعات (FamilyUpdateTracking()) | opens the family-update flow — **wired by 18-21**; renders disabled here |
| تم (printFamilyUpdateTracking()) | confirms the date and prints the family-update sheet — **wired by 18-21**; renders disabled here |
| غلق (CloseFamilyUpdateTrackingModal()) | resets the panel/filters — legacy modal handler flattened to an inline reset (recorded in Dev Notes) |

Grid (§23.S.12 formally records 0 grids — deviation recorded in Dev Notes; columns from the §23.2 summary):

| Column | DTO key |
| --- | --- |
| الجمعيه | `charityName` |
| عدد الايتام بالدفعة | `orphansInBatch` |
| التقارير المدخلة | `reportsEntered` |
| تم رفع الدفعة | `batchUploaded` (from `OrphanPayment.IsBatchUploaded`) |
| تاريخ الرفع | `uploadDate` |

## Tasks / Subtasks

- [x] **Task 1 — DTOs + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`: `CharityPaymentTrackingFilterDto` (`Page = 1`, `PageSize = 20`, `Guid? CharityId`, `string? BatchId`, `DateTime? DateOfStartingUpdate` — the date rides along so 18-21 reuses the same model), `CharityPaymentTrackingRowDto` (`CharityId`, `CharityName`, `OrphansInBatch`, `ReportsEntered`, `BatchUploaded`, `UploadDate` nullable) — no `FK_*` wire keys
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/CharityPaymentTrackingValidator.cs`: page bounds, `BatchId` max length, `DateOfStartingUpdate` sane-range only
- [x] **Task 2 — Service projection** (AC 1, 4, 5)
  - [x] `IReportService.GetCharityPaymentTrackingAsync(filter)` in `ReportService`: resolve the batch (`BatchNo == BatchId`, else the latest `OrphanPayment` by `GroupDate`), then compose **one row per charity** present in the batch: `orphansInBatch` = count of `OrphanPaymentItem`s grouped by the orphan's `FK_CharityId`; `reportsEntered` = count of `PeriodicOrphanReport`s for those orphans; `batchUploaded`/`uploadDate` from the batch header (`IsBatchUploaded`, `UploadDate`)
  - [x] Scope: `IsHeadOffice` may filter by explicit `CharityId` or see كل الجهات; the `CountryId` claim pins country through `ResolveCharityScope`. **Real data only** — where a tracked state has no persisted flag today (printed / confirmed per charity), the column is omitted rather than fabricated (recorded in Dev Notes)
  - [x] Paged through 18-1's `ReportPagedResult<T>`; global soft-delete filter applies
- [x] **Task 3 — API endpoint** (AC 2, 3, 6)
  - [x] In 18-1's `ReportsController`: `[HttpPost("charity-payment-tracking")]` with `[Authorize(Roles = "SuperAdmin,Admin")]` (the ReportsController-wide attribute already covers it — assert, don't re-decorate if inherited) → `Ok(paged)`; ValidationException → 400 `{ message, errors }`, catch-all → 500 `{ message }`; no `ApiResponse<T>` (15-1 ruling)
- [x] **Task 4 — Frontend thin component** (AC 1, 2, 5)
  - [x] `Frontend/src/app/modules/reports/charity-payment-tracking/` 4-file component; route `#/reports/charity-payment-tracking` guarded `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`
  - [x] Filter panel per §23.S.12: الجمعيات from `GET /api/Charities` (+ كل الجهات all-option — HQ-only screen), batches from `GET /api/OrphanPayments/batch-numbers`, date picker for من فضلك ادخل تاريخ بدا التحديث — no hardcoded arrays
  - [x] متابعة تسليمات الجميعات button runs the query; grid renders the 5 columns above; shared `Pagination`; `trackBy: charityId`; empty state at `totalCount === 0`; **not `data-list`**; OnPush omitted (list-screen precedent)
  - [x] متابعة تحديثات الجميعات + تم render **disabled** with a tooltip (18-21 wires them to the family-update print); غلق resets the filter panel to defaults
  - [x] استخراج البيانات through 18-1's `report-export.service.ts`; empty grid → nothing-to-produce message, no file
- [x] **Task 5 — i18n** — menu label, title, 3 field labels + all-options, 3 command labels + disabled tooltips, 5 column headers, empty state, export messages under `reports.*` in **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; **no EF migration**. MSB3021/3027 = live-API output lock; never kill the user's process
  - [x] Live check: anonymous POST → 401; charity-role token → 403 (HQ-only, server-side); HQ POST with a seeded `batchId` → 200 paged camelCase, one row per charity; explicit charity → one row; empty batch → zero pages
  - [x] `cd Frontend && npm run build` — 0 errors; ng-serve stale-bundle caveat (grep the served chunk)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Grid deviation (recorded)

§23.S.12 formally records «Grids on the screen: 0» — the legacy screen printed the tracking sheet rather than displaying it (its handler is `printCharityPaymentTracking`). The epic context asks for on-screen grid data ("entered/printed/disbursed/confirmed per charity"), and a supervisory dashboard you cannot see is not a dashboard. This story displays the grid; the print path it replaces lands with the shared PDF service (18-21). Column set: only states the platform actually persists today (`OrphanPaymentItem` counts, `PeriodicOrphanReport` counts, `IsBatchUploaded`/`UploadDate`). Per-charity "printed"/"confirmed" flags do not exist as data — those columns are **omitted, not faked**; if cheque/disbursement state is later exposed per charity, adding columns here is a small change, not a new story.

### Modal flattening (recorded)

غلق/تم are modal-close/confirm handlers from the legacy family-update modal. On this platform the "modal" is the filter panel section تحديثات الجمعيات: غلق resets it; تم is 18-21's print trigger once that story lands. No `ng-bootstrap` modal is built for a two-field confirmation.

### Platform rules that bind this story

- Wire is camelCase; no `FK_*` DTO keys (Newtonsoft emits `fK_…`).
- Raw paged envelope + anonymous `{ message, errors }` — NOT `ApiResponse<T>` (15-1 ruling, architecture.md §10).
- Controllers inherit `ControllerBase` + `[Authorize]` + `[Route("api/[controller]")]` (17-1 note); HQ-only authorisation is server-side — the menu gate is convenience.
- FluentValidation in the service (`Validators/Reports/`), never in the controller.
- Reads via `IUnitOfWork` repositories; repositories never save; this story writes nothing.
- Soft delete via the global query filter — never hand-check `IsDeleted`.
- Lookup labels `NameAr ?? NameEn`; charities from `GET /api/Charities`, batches from `GET /api/OrphanPayments/batch-numbers` — no hardcoded arrays.
- Bespoke grid + shared `Pagination` (NOT `data-list` — recorded deviation); OnPush omitted on list screens (codebase precedent).
- Caller scope from `ICurrentUserService` — pin-never-widen (`OfficeProjectService.cs:384` shape); the `CountryId` claim pins even for HQ.
- EP-18 adds no entities, no EF migration.
- Tests excluded per the standing user decision.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Reports skeleton | 18-1 |
| متابعة تحديثات الجميعات / تم print wiring, the shared PDF service, jsPDF dependency | 18-21 |
| Per-charity printed/confirmed tracking columns (no persisted state today) | backlog, only when the payments/cheque modules expose it |
| Any other report vertical | 18-2 … 18-19 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.12] screen contract — 3 fields, 4 commands, the 0-grid record this story deviates from
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.20] scenario — cross-charity batch tracking
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.2] §23.2 table row — "entered, printed, disbursed and confirmed" column intent
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-20 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/OrphanPayment.cs] `BatchNo`, `GroupDate`, `IsBatchUploaded`, `UploadDate` — batch header state
- [Source: Backend/src/IIROSA.Domain/Entities/OrphanPaymentItem.cs] `OrphanId` — per-charity membership counts
- [Source: Backend/src/IIROSA.Domain/Entities/Orphan.cs] `FK_CharityId` — the grouping key
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:68] `GET batch-numbers` — the batch lookup

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `dotnet build` (Api csproj, temp `-o`) — **0 errors**.
- `npx ng build` — NG_EXIT=0, `error TS` count 0 (the log's one "Error occurs" line is the NG8107 warning context on the parallel session's `report-numbers` file).
- i18n `reports.charityTracking.*` — 22 keys in each locale, key sets identical, JSON parses.
- Live smoke, private instance `127.0.0.1:60970`, seeds `18200…` (batch SB-1820 latest, GroupDate 2026-08-24T14:00, `IsBatchUploaded=1` + `UploadDate`; 3 members — 2 dga + 1 وادي النطرون; 1 accepted + 1 pending report, both dga):
  - anonymous POST → **401**; charity-role token (`Charity@IIROSA.com`) → **403** (AC 3/6 — HQ-only, server-side)
  - HQ `batchId: "SB-1820"` → 200, `totalCount` 2, name-ordered rows: **dga (orphans 2 · reports 2 · uploaded true · 2026-08-24T15:00Z)** and **وادي النطرون (orphans 1 · reports 0 · uploaded true · same date)** — members grouped per charity, entered reports counted per member set, header state from the batch payments
  - HQ absent batchId (latest resolution → SB-1820) with `dateOfStartingUpdate: "2026-08-01"` riding → 200 `totalCount` 2 (the date does not affect the query)
  - explicit dga narrow → 1 row `dga (2/2)` (AC 4); unknown `batchId: "NOPE"` → **0 rows / zero pages** (AC 5)
  - `page: 0` → **400**; `dateOfStartingUpdate: "1990-01-01"` → **400** `errors.DateOfStartingUpdate: "DateOfStartingUpdate is outside the sane range"`
  - seeds hard-deleted (0/0/0); smoke instance killed by PID.

### Completion Notes List

- **Scope ladder is charity-rooted** (the orphan-typed `ApplyCharityScopeAsync` mirrored on the charity set): claim pin → HQ narrow (`IsHeadOffice` + explicit `CharityId`) → `CountryId` claim intersect. The endpoint is HQ-only so the charity pin is defensive; the country pin is live for HQ callers carrying the claim.
- **Batch-header state is per charity via its own items' payments** (`any(IsBatchUploaded)` / latest `UploadDate`), not a single shared header value — honest when one `BatchNo` spans several payment groups; identical to the story's "batch header" when it is one payment.
- **`reportsEntered` counts entered (any-state, non-deleted) reports of the charity's batch members** — entered is the legacy column's meaning (التقارير المدخلة), not accepted. Member-id `Contains` runs chunked at 1000 (a batch can exceed the 2100-parameter SQL Server IN limit).
- **Grouping is in memory** after one flattened membership fetch — one row per charity without a grouped-subquery EF translation; paging then applies over the per-charity rows (a handful by construction).
- Batch resolution reuses 18-19's ruling (latest = newest non-deleted payment by `GroupDate` carrying items; membership = that `BatchNo`'s items across all its payments).
- Per-charity printed/confirmed columns stay **omitted, not faked** (the story's real-data-only ruling); `DateOfStartingUpdate` is range-checked and carried on the wire for 18-21, never consumed here.
- غلق flattens to `resetFilters()` (modal handler → inline reset, per Dev Notes); متابعة تحديثات الجميعات/تم render disabled with a `title` tooltip — 18-21 wires them. The run command doubles as the viewer shell's بحث (both wired to `runTracking()`).
- Explicit `!IsDeleted` throughout (standing correction — no global soft-delete filter on this platform); export pages at **100** (this endpoint's validator cap).

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — §23.S.12 block: `CharityPaymentTrackingFilterDto`, `CharityPaymentTrackingRowDto`
- `Backend/src/IIROSA.Application/Validators/Reports/CharityPaymentTrackingValidator.cs` — new
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetCharityPaymentTrackingAsync` declared
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation + `BuildCharityTrackingPageAsync` + private `CharityTrackingGroup` record + validator ctor wiring
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST charity-payment-tracking` (`SuperAdmin,Admin`)
- `Frontend/src/app/modules/reports/models/report.model.ts` — `CharityPaymentTrackingFilter`, `CharityPaymentTrackingRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getCharityPaymentTracking()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportCharityPaymentTracking()`
- `Frontend/src/app/modules/reports/charity-payment-tracking/` — 4-file component (new)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — import + `charity-payment-tracking` route (`Reports.View`)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.charityTracking.*` (22 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-20 and module spec §23.S.12 / §23.U.20; 0-grid record superseded by an on-screen tracking grid, 18-21 print commands rendered disabled, HQ-only authorisation fixed server-side. |
| 2026-08-24 | Implemented and verified: HQ-only endpoint + service + validator + DTOs with the per-charity batch grouping, thin §23.S.12 screen (charity/batch/date panel, run + disabled 18-21 commands + reset), ExcelJS export, i18n both locales. Full matrix proven live (401/403, per-charity rows with counts and upload state, narrow, zero pages, 400s). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
