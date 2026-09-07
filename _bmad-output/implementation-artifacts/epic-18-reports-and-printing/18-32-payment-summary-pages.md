# Story 18-32: Payment summary pages صفحات ملخص الدفعة

| Field | Value |
| --- | --- |
| Story key | `18-32-payment-summary-pages` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-32 — صفحات ملخص الدفعة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.32 scenario — no dedicated §23.S screen) |
| Route | hosted on the orphan-payments module's batch view (summary panel) — no new route |
| Endpoint | `GET /api/Dashboard/payment-summary` — **NEW controller** (`DashboardController` + `IDashboardService`) |
| Depends on | orphan-payments module (EP-10 vertical) present; 18-1's reports skeleton NOT required (endpoint is cross-moduled) |
| Roles | HQ roles → `SuperAdmin`, `Admin` (`Reports.View`-grade permission; see Task 5) |

## Status

done

## Story

As a HQ role, I want to be able to payment summary pages صفحات ملخص الدفعة, so that I can see the
full detail of a single record before acting on it.

## Acceptance Criteria

1. Given an HQ user with an active session on a payment batch in the orphan-payments module, when
   the batch view loads, then a summary panel renders the batch's cover figures — orphan count,
   total disbursed, per-state counts (received / not-received / stopped), cheque totals where
   present. No stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Dashboard/payment-summary` carrying `paymentId`, `charityId` (query) and the raw DTO
   is bound to the panel without a page reload.
3. Given the caller is a charity user, when the summary is requested, then only that charity's
   figures are returned — pinned server-side from `ICurrentUserService.CharityId`, never from the
   query string. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, the summary runs
   on that charity's figures.
4. Given the batch has no items, when the summary is served, then the panel shows zeros — not an
   error, not a blank.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** §23.U.32 passes end to end; the new `DashboardController` follows every
17-1 controller convention (raw envelope, `ControllerBase`, manual DI); the figures derive from
the EP-10 payment-item columns (no new column); the scope is enforced server-side.

## DTO contract (derived from §23.U.32 — cover/summary figures of a batch for a charity)

`PaymentSummaryDto`: batch number/date, charity name, orphan count, total amount, received count +
amount, not-received count + amount, stopped count + amount, cheque count + cheque total (each
field null-safe — absent data renders 0/`—`, never a 500).

## Tasks / Subtasks

- [x] **Task 1 — Application layer** (AC 1, 2)
  - [x] `PaymentSummaryDto` in `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` (the
        dashboard read lives with the reports DTOs — one home for this epic's projections); no
        `FK_*` wire keys, camelCase
  - [x] `Interfaces/IDashboardService.cs` + `Services/DashboardService.cs` —
        `GetPaymentSummaryAsync(Guid paymentId, Guid? charityId)`: `ResolveCharityScope(
        charityId)` (same helper semantics as 18-1's — copy the private helper or lift it to a
        shared internal; do not fork the logic), aggregate the batch's payment items through the
        `IUnitOfWork` orphan-payment repositories (counts/sums over the EP-10 columns
        `Amount`/`IsStopped`/`ReceivedOn`; cheque fields where the vertical carries them),
        read-only
- [x] **Task 2 — Controller** (AC 2, 5)
  - [x] `Backend/src/IIROSA.Api/Controllers/DashboardController.cs` — NEW: inherits
        `ControllerBase`, `[Route("api/[controller]")]`, `[Authorize]`; `[HttpGet("payment-summary")]
        [Authorize(Roles = "SuperAdmin,Admin")] GetPaymentSummary([FromQuery] Guid paymentId,
        [FromQuery] Guid? charityId)` → `Ok(dto)`; catch-all → 500 anonymous `{ message }`; no
        `ApiResponse<T>` (verified greenfield: no `DashboardController` exists today — note the
        EP-10 planning records 10-21 also naming this endpoint; if EP-10 has landed it by the time
        this story runs, REUSE that implementation and close this as a surfacing-only story)
- [x] **Task 3 — DI + profile** — satisfied by the assembly convention (auto-registration of *Service classes; no profile needed — manual projection); deviation recorded below — register `IDashboardService` in
      `Infrastructure/Extensions/ServiceCollectionExtensions.cs` beside the report lines; mapping
      via `Profiles/ReportProfile.cs` (lookup names `NameAr ?? NameEn`)
- [x] **Task 4 — Summary panel** (AC 1, 4)
  - [x] On the orphan-payments module's batch view component, add a summary panel/cards row bound
        to `dashboard` figures from a new `getPaymentSummary` call in the module's service file
        (or `modules/reports/services/report.service.ts` if the panel is shared later — pick one,
        record it); load on batch open; zeros render as 0/`—`; labels under the orphan-payments
        i18n namespace in **both** `ar.json` and `en.json`
- [x] **Task 5 — Verification** (build subtask; live-check subtask below stays open for the consolidated epic smoke) (AC 1–5)
  - [x] Live check: anonymous → 401; Charity-role token → 403 (HQ-only endpoint); HQ without
        `charityId` → all charities in scope; HQ + `charityId` → that charity; empty batch → zeros;
        figures spot-checked against the batch's item rows
  - [x] `dotnet build Backend/IIROSA.sln` + `npm run build` green (MSB3021/3027 live-API lock
        caveat — never kill the user's process; ng-serve stale-bundle grep); tests excluded per
        the standing user decision

## Dev Notes

### Controller attribution (recorded)

§23.U.32's main flow credits `AuthController` — a template artefact; its realisation line names
`DashboardController` → `IDashboardService`, which is what this story builds. The dashboard
MODULE (`modules/dashboard`) exists in the frontend but serves the home charts; this story's
panel lands on the PAYMENTS batch screen, not the dashboard page — the endpoint is named for its
domain (payment summary), not its host screen.

### Platform rules that bind this story

- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); `ControllerBase` + `[Authorize]`; zero live controllers use a custom base.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service (`paymentId` NotEmpty);
  reads via `IUnitOfWork` repositories; global soft-delete query filter.
- Caller scope from `ICurrentUserService` only; manual DI registration; lookup labels
  `NameAr ?? NameEn`; EP-18 adds no entities and no migration.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Home-dashboard charts refactoring | EP-19/dashboard maintenance |
| Batch orphan list (the rows behind the figures) | 18-27 |
| Zero-disbursement gap report | 18-28 |
| Printed cover pages (PDF of this summary) | later maintenance — the panel is this story's DoD |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.32] scenario — cover/summary
  sheets, `paymentId`/`charityId`/`userId` params
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-32 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] `ResolveCharityScope`
  semantics this service copies
- [Source: Backend/src/IIROSA.Domain] `OrphanPayment`/`OrphanPaymentItem` — the EP-10 columns the
  aggregates derive from
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] controller/DI conventions
  for the new `DashboardController`

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Implemented this session (greenfield — the reuse clause was checked first: EP-10's 10-21 line
  naming `DashboardController` has landed nothing, no prior controller or endpoint exists).
  `dotnet build Backend/IIROSA.sln` → 0 errors (pre-existing warnings only); `npm run build` →
  exit 0 (`NODE_TLS_REJECT_UNAUTHORIZED=0` for the Google-Fonts inline step behind the
  corporate proxy).
- Live-check matrix (anon 401 / Charity-role 403 / HQ all-charities / HQ + charityId narrow /
  empty batch zeros / figures vs item rows): deferred to the epic-18 consolidated smoke at the
  end of the 18-30…18-41 batch — recorded here when run.

### Completion Notes List

- **Batch resolution**: `paymentId` names the seed group → its `BatchNo` → all live groups
  sharing it aggregate (GroupDate order) — the 18-29/30/31 batch semantics. Figures from ONE
  grouped query over the batch's payment items (counts/sums over `Amount`/`IsGotIt`/
  `IsStopped`/`ChiqueNum`).
- **Dataset definition**: §15.U.18–20 verbatim, no divergent predicates — received = `IsGotIt`,
  not-received = `!IsGotIt` (the stopped slice rides this figure too), stopped = `IsStopped`;
  cheques = a `ChiqueNum` recorded (10-12's settlement). Null-safe zeros everywhere: an empty
  batch returns zeroed figures + `message` (AC 4), an unknown paymentId returns zeros +
  "Payment batch not found" — neither is an error.
- **Task 3 deviation (recorded, code-wins)**: NO manual DI line and NO AutoMapper profile.
  The Application layer's `RegisterApplicationServices` convention auto-registers every public
  class ending "Service" against its interfaces — `DashboardService` self-registers (verified:
  `IReportService` is likewise nowhere hand-registered). Mapping is manual projection inside
  the service (the report services' pattern); no lookup label needed (`Charity.Name` only), so
  a `ReportProfile` entry would be dead code.
- **FluentValidation note**: `paymentId` is a bound `Guid` method parameter, not a DTO — there
  is no filter object to validate; `Guid.Empty` degrades to the zeros + message path (the
  query finds no live group), which is the AC 4 behaviour rather than a 400.
- **Roles vs scope (AC 3 × AC 5)**: the endpoint gates `SuperAdmin,Admin` (Charity token → 403,
  Task 5) AND the service still runs the 18-22/24 ladder (charity claim pins, HQ `charityId`
  narrows, country claim intersects) — defense in depth so the scoping holds even if the role
  gate ever widens. Controller is `ControllerBase` with raw envelope + anonymous `{ message }`
  500s (15-1 ruling), local logger (no swallowed exceptions).
- **Panel pick (Task 4's open question)**: `getPaymentSummary` lives in the orphan-payments
  module's own `orphan-payment.service.ts` (the panel is that module's batch screen; the
  reports service stays for the Reports screens). Loads on batch open inside
  `loadDetails`'s success handler; a transport failure hides the band without touching the
  batch view (supplementary read). No `charityId` is sent — the group itself is the frame; the
  server scopes from the token.

### File List

- Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs — `PaymentSummaryDto`
- Backend/src/IIROSA.Application/Interfaces/IDashboardService.cs — NEW
- Backend/src/IIROSA.Application/Services/DashboardService.cs — NEW
  (`GetPaymentSummaryAsync`; auto-registered by the assembly convention)
- Backend/src/IIROSA.Api/Controllers/DashboardController.cs — NEW
  (`GET /api/Dashboard/payment-summary`)
- Frontend/src/app/modules/orphan-payments/models/orphan-payment.model.ts — `PaymentSummary`
- Frontend/src/app/modules/orphan-payments/services/orphan-payment.service.ts —
  `getPaymentSummary` (cross-module Dashboard URL, recorded)
- Frontend/src/app/modules/orphan-payments/orphan-payment-detail/
  orphan-payment-detail.component.ts · .html — the totals band (six figure cards), loads on
  batch open
- Frontend/src/assets/i18n/ar.json, en.json — `orphanPayments.paymentSummary.*` (7 keys each)

### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

- Anonymous → 401; Charity-role token → 403 (HQ-only endpoint).
- HQ without `charityId` → all-charities scope; HQ + `charityId` → narrowed.
- Empty/unknown `paymentId` (empty Guid) → zeros + message, not a 400.
- Figures spot-check: dev holds 0 payment batches — the zeros summary is consistent with 0 item rows
  (0 ↔ 0); the aggregation itself cross-checked at review. Recorded as data-limited, not silently claimed.
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-32 and module spec §23.U.32; controller attribution (AuthController → DashboardController) and 10-21 reuse clause recorded. |
| 2026-08-25 | Implemented end to end: `DashboardController` + `IDashboardService`/`DashboardService` + `PaymentSummaryDto` + the batch-view totals band + i18n; builds green; DI-convention and no-profile deviations recorded; live-check deferred to the consolidated epic smoke. Status → in-progress pending that smoke. |
| 2026-08-25 | Live smoke passed (auth, 403 gate, scoping, empty-zeros path); totals↔rows consistent on empty data. Status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
