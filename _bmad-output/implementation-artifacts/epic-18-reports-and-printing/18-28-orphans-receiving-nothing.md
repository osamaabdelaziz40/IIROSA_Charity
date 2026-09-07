# Story 18-28: Orphans receiving nothing أيتام لم يصرف لهم

| Field | Value |
| --- | --- |
| Story key | `18-28-orphans-receiving-nothing` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-28 — أيتام لم يصرف لهم |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.28 scenario; hosted command — no dedicated §23.S screen) |
| Route | `#/reports/orphans-without-payment` (thin page hosting the result grid; decision recorded) — launched from the §23.S.3 commands |
| Endpoint | `POST /api/Reports/orphans-without-payment` |
| Depends on | **18-1 landed** (Reports skeleton + shell + export service + the §23.S.3 command slots) |
| Roles | HQ roles → `SuperAdmin`, `Admin` (`Reports.View`) |

## Status

done

## Story

As a HQ role, I want to be able to orphans receiving nothing أيتام لم يصرف لهم, so that I can see
the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given an HQ user with an active session, when the actor invokes متابعة الايتام - لم يتم الصرف
   (from `#/reports/orphans`) or opens `#/reports/orphans-without-payment` directly with a batch +
   charity chosen, then the orphans of that batch for whom NO amount was disbursed are listed. No
   stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/orphans-without-payment` with a typed request DTO (`charityId`, `paymentId`)
   and the raw paged envelope is rendered without a page reload.
3. Given the caller is a charity user, when the report runs, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
   Given an HQ caller (`IsHeadOffice`) with an explicit charity id, the report runs on that
   charity's data; a `CountryId` claim additionally pins the charities of that country.
4. Given every orphan in the batch was disbursed something, when the report runs, then the grid
   renders empty and the paging control reports zero pages.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** §23.U.28 passes end to end — the zero-disbursement gaps of a run are
exposed from the orphans screen's command and the thin report page; the disbursement predicate
resolves from the EP-10 payment-item columns (`Amount`/`IsStopped`/`ReceivedOn` — no new column);
the scope is enforced server-side.

## Filter/result contract

| Field | DTO key | Control | Mandatory | Source |
| --- | --- | --- | --- | --- |
| الدفعة | `PaymentId` | Drop-down | **Yes** | `GET /api/OrphanPayments` (batch list — the module's own paged read) or `batch-numbers` where a batch key suffices; resolve the payment id the report needs |
| الجمعية | `CharityId` | Drop-down | No | `GET /api/Charities` (`result.items || []`) + كافة الجهات — HQ only |

Result grid: رقم اليتيم · أسم اليتيم · الجمعيه · المبلغ المستحق (expected) · حالة الصرف
(disbursement state) — the columns §23.U.28's summary implies, tightened to what the payment item
carries.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator + service** (AC 2, 3)
  - [x] `OrphansWithoutPaymentFilterDto` (`Guid PaymentId`, `Guid? CharityId`, `int Page = 1`,
        `int PageSize = 20`) + `OrphansWithoutPaymentListDto` in `DTOs/Reports/Reports.cs`;
        `ReportPagedResult<T>`; no `FK_*` wire keys
  - [x] `Validators/Reports/OrphansWithoutPaymentFilterValidator.cs` — `PaymentId` NotEmpty; page
        bounds; invoked in the service
  - [x] `IReportService.GetOrphansWithoutPaymentAsync(...)` + implementation:
        `ResolveCharityScope(filter.CharityId)`; resolve the batch's orphan items via the
        `IUnitOfWork` orphan-payment repositories; predicate = item rows whose disbursement is
        zero/absent (no amount recorded, or stopped-with-nothing-received — verify the EP-10
        column semantics of `Amount`/`IsStopped`/`ReceivedOn` before writing it; if the epic-10
        migration has not landed its columns yet, the predicate falls back to items whose
        received state is unset — RECORD which branch shipped). Ordered by orphan code
- [x] **Task 2 — API endpoint** (AC 2, 5)
  - [x] `[HttpPost("orphans-without-payment")] [Authorize(Roles = "SuperAdmin,Admin")]` in 18-1's
        `ReportsController` → `Ok(paged)`; standard ValidationException→400-errors-map /
        catch-all→500 `{ message }` ladder; no `ApiResponse<T>`
- [x] **Task 3 — Thin page + command wiring** (AC 1, 4)
  - [x] `Frontend/src/app/modules/reports/orphans-without-payment-report/` thin 4-file component
        in the shell; route `orphans-without-payment`, `AuthGuard + PermissionGuard`,
        `data.permission: 'Reports.View'`; batch + charity filters per the contract; bespoke grid
        + shared `Pagination`; `trackBy`; empty state; OnPush omitted (list-screen precedent)
  - [x] Wire the §23.S.3 commands 18-1 rendered disabled: متابعة الايتام - لم يتم الصرف
        (`GetRecievedPaymentDetails()`) and أيتام لم تصل لهم أي مبالغ
        (`OrphnasDontTakeAnyAmount()`) both navigate here — the first pre-filtered to the current
        charity selection, the second the all-batches variant (same endpoint, `PaymentId`
        optional there → record which shape shipped; keep ONE endpoint)
- [x] **Task 4 — i18n** — `reports.orphansWithoutPayment.*` in **both** `ar.json` and `en.json`
- [x] **Task 5 — Verification** (AC 1–5) — anonymous → 401; missing `paymentId` → 400 errors-map;
      HQ + `charityId` → that charity; fully-disbursed batch → empty grid; command navigation from
      `#/reports/orphans` works; `dotnet build` + `npm run build` green (MSB3021/3027 live-API
      lock caveat; ng-serve stale-bundle grep); tests excluded per the standing user decision

## Dev Notes

### Command-family note (recorded)

§23.S.3 sketches three gap-analysis commands around this theme; the board assigns one story
(18-28) and one endpoint. The تقرير بيانات غير المستلمين (`GetGotItNotPaymentDetails`) variant
(not-received rather than never-disbursed) rides the same screen later as a maintenance item —
out of scope here. Do not build a third endpoint for it.

### Platform rules that bind this story

- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); `ControllerBase` + `[Authorize]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories; global soft-delete query filter.
- Caller scope from `ICurrentUserService` only; lookup labels `NameAr ?? NameEn`; bespoke grid +
  shared `Pagination` (NOT `data-list`); lazy module; EP-18 adds no entities and no migration —
  the disbursement predicate resolves from EP-10's existing columns.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Non-received (غیر مستلم) variant of the gap list | later maintenance story |
| Received/not-received/stopped printed lists | 18-29 (landed scope) |
| Batch orphan list with amounts (the full list, not the gap) | 18-27 |
| Payment summary/cover pages | 18-32 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.28] scenario — zero-disbursement
  gaps of a run
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.3] the orphans screen whose
  gap-analysis commands launch this report
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-28 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] skeleton + shell + the
  disabled command slots this story wires
- [Source: Backend/src/IIROSA.Domain] `OrphanPayment`/`OrphanPaymentItem` — the EP-10 disbursement
  columns the predicate resolves from

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Backend smoke (private instance, temp build `iirosa-1828` on `127.0.0.1:60970`, seeds prefixed
  `1828…`, hard-deleted after): matrix all green — anon 401; `{}` → 400
  `{"errors":{"PaymentId":"Payment batch is required"}}`; HQ full batch → 3 rows (plain gap +
  stopped + other-charity gap), cheque-issued / received / soft-deleted arms all excluded;
  HQ+`charityId=dga` → 2 rows; HQ+`charityId=wadi` → 1 row; charity token → 403 (role gate);
  empty batch → `totalCount 0`. Post-seed count-verified 0 batches / 0 items.
- Frontend smoke (production `dist/iirosa` served statically on :4306, Playwright with
  `/api/**` routed to the private instance — the prod bundle calls `https://localhost:60960`,
  so the route rewrites all three API hosts): real login through the app's own form (storage
  planting is rejected by the app's boot validation — it wipes auth and bounces to a nested
  `returnUrl` loop; the login form sets consistent state). Batch dropdown lists both seeded
  batches; full run → the same 3 rows with وقف الصرف on the stopped row only and لم يتم الصرف
  on the rest; charity narrow (dga) → the same 2 rows as curl; request bodies carry exactly
  `paymentId`/`charityId`/`page`/`pageSize`; استخراج absent (`[showExport]="false"`); sidebar
  entry renders; the §23.S.3 command link on `#/reports/orphans` navigates to the page.
  Note: DropDown's native `<select>` carries lowercase GUID option values — UI automation must
  match casing.
- `dotnet build` (temp `-o`, live API untouched) and `npx ng build` both exit 0, zero
  `error TS` (one first-run TS2339 `result.count` — `ReportPagedResult<T>` has no `count`,
  fixed before the final build). Tests excluded per the standing user decision.

### Completion Notes List

- **Predicate branch that shipped**: EP-10's columns HAVE landed, so the gap predicate is the
  instrument triple `!IsGotIt && ChiqueNum == null && TransferNo == null` over the batch's live
  items. "Nothing disbursed" = no cheque, no transfer, nothing received — a cheque issued but
  not yet received is the غير مستلم variant (out of scope per Dev Notes), and stopped orphans
  REMAIN in the list (a stopped payment is still a gap; the badge marks the subset). `Amount`
  is the expected entitlement → `Amount ?? 0m`.
- **Scope**: the charity-rooted ladder reused verbatim from 18-22/24 (orphan query
  `!IsDeleted && FK_CharityId != null` → charity-claim pin → HQ narrow via `filter.CharityId`
  → country intersect). Endpoint role gate is HQ-only (`SuperAdmin,Admin`); the service pin is
  defence-in-depth — live-tested: charity token → 403.
- **Command wiring shape**: 18-1's orphan-data screen has NO disabled §23.S.3 command slots to
  wire (grep-verified — the story's Task-3 premise was false). Replacement: one slim command
  link-row on that screen (HQ-gated, since the endpoint is HQ-only) navigating here carrying
  the screen's current charity selection as `?charityId=`, honoured by the page on load. ONE
  endpoint, `PaymentId` mandatory — the story's "all-batches variant" collapses to the same
  page (the batch dropdown IS the frame); no `PaymentId`-optional branch shipped.
- **Batch picker source**: `getBatchNumbers` returns batch-number strings only; the endpoint
  needs the group `Guid`, so the dropdown loads `getOrphanPayments({pageNumber:1,pageSize:100})`
  and labels options `{id, name: batchNo || groupName}`.
- Sidebar li + route added after survey-questionnaire; i18n `reports.orphansWithoutPayment.*`
  17 keys per locale, key-parity node-verified.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `OrphansWithoutPaymentFilterDto`,
  `OrphansWithoutPaymentListDto`
- `Backend/src/IIROSA.Application/Validators/Reports/OrphansWithoutPaymentFilterValidator.cs` — new
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetOrphansWithoutPaymentAsync`
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — field/ctor validator param + method
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST orphans-without-payment`
- `Frontend/src/app/modules/reports/models/report.model.ts` — filter + row interfaces
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getOrphansWithoutPayment`
- `Frontend/src/app/modules/reports/orphans-without-payment-report/` — new 4-file component
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — route + import
- `Frontend/src/app/modules/reports/orphan-data-report/orphan-data-report.component.ts` — RouterLink import
- `Frontend/src/app/modules/reports/orphan-data-report/orphan-data-report.component.html` — §23.S.3 command link-row
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` — 17 keys each

## Change Log

- 2026-08-25 — story implemented to review: endpoint + service + validator (backend), thin
  report page + command link + route/sidebar/i18n (frontend); live backend matrix and dist-served
  UI smoke green; seeds `1828…` hard-deleted and verified; tests excluded per standing decision.

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-28 and module spec §23.U.28 / §23.S.3; thin-page route decision and command-family collapse to one endpoint recorded; disbursement-predicate source pinned to EP-10 columns. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
