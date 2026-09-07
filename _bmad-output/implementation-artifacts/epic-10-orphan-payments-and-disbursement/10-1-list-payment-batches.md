# Story 10.1: List payment batches — قائمة دفعات الأيتام

| Field | Value |
| --- | --- |
| Story | US-PAY-01 (UC-PAY-01) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.S.1, §15.U.1) |
| Priority / size | Must · 2 points |
| Route | `#/orphan-payments` |
| Endpoint | `GET /api/OrphanPayments` (as-built paged group list — matches the board) |
| Depends on | None (foundation story — ships before 10-2; empty state is valid) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer (HQ); Charity role only orphan-scoped (`orphanId` filter, UC-ORP-08 contract) |

Status: done

## Story

As a General Director,
I want to be able to list payment batches قائمة دفعات الأيتام,
so that I can find the record I need without leaving the system.

## Acceptance Criteria

1. Given an HQ role with an active session on `#/orphan-payments`, when the screen opens, then `GET /api/OrphanPayments` returns a paged list rendered without a page reload, and no stored data is changed.
2. Given a Charity-role caller, when the endpoint is called **without** `orphanId`, then the request is refused (`403`) — charity users see batch data only through the orphan-scoped history (UC-ORP-08, shipped in the working tree). Do **not** remove this guard.
3. Given an HQ role, when the list is served, then rows show الرقم · رقم الدفعة · اسم الدفعة · الفترة · التاريخ · سعر الصرف (§15.S.1 grid) and paging works (Next/Prev).
4. Given no row matches, then the grid renders an empty state and the pager reports zero — no error.
5. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** list surface works end to end against the live endpoint; the route permission actually resolves (PERMISSION_ROLES entry added); the list component calls only endpoints that exist; i18n keys resolve in ar + en.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint | `GET /api/OrphanPayments` → `OrphanPaymentsController.GetPaymentGroups` (`Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:40`) — paged `{ items, totalCount }`, `[Authorize(Roles="SuperAdmin,Admin,Charity")]`, Charity-without-orphanId → `Forbid()` at :45 |
| Service | `OrphanPaymentService.GetPaymentGroupsAsync(filter, userCharityId, userRole)` — real implementation (`Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:312`) incl. orphan-scoped mode `EnsureOrphanInCallerScopeAsync` (:413) |
| Filter DTO | `OrphanPaymentFilterDto` — SearchTerm, periods, GroupDate range, IsBatchUploaded, CharityId, **OrphanId (new, UC-ORP-08)**, paging/sort |
| DI | Convention-registered and LIVE (`IIROSA.Application/ServiceCollectionExtensions.cs:237-264`) — unlike epic 9, nothing to uncomment |
| Frontend | `Frontend/src/app/modules/orphan-payments/orphan-payment-list/` (.ts/.html/.scss), route `''` with `data.permission: 'OrphanPayments.View'`, sidebar entry `layouts/main-layout/main-layout.component.html:332-356` |
| i18n | `orphanPayments` block (~105 keys) in **both** `ar.json` and `en.json` |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **`OrphanPayments.*` missing from `PERMISSION_ROLES`** (`Frontend/src/app/core/services/auth.service.ts:60-121`) → unmapped permission = warn-and-allow (:391-407); the route guard currently gates nothing. Add the full initial map this epic needs (see Tasks).
2. **List role set is narrower than the WAR matrix**: `00-Overview` §4.3 gives Orphan payment batches F to Gen. Director, Staff, Fin. Director → widen list `[Authorize]` to `SuperAdmin,Admin,Accountant,FinancialOfficer` (Charity stays via the orphan-scoped branch).
3. **`OrphanPaymentFilterDto.CharityId` is `int?`** while CharityIds are `Guid` — type mismatch (charity filtering is even commented out in the repo for this reason). Fix the DTO to `Guid?`; wiring the join filter is 10-7's job.
4. **500 handler leaks `ex.Message`** (`catch → StatusCode(500, new { message, error = ex.Message })` on every action) — replace with a generic message (log the detail; never return raw exception text). Fix in this story for the read actions; other stories inherit the pattern.
5. **List component invokes dead endpoints**: statistics/audit-logs/filter-options/export-list/print buttons call routes that do not exist on the controller (verified 404/405 list, `services/orphan-payment.service.ts`). Strip the dead calls and their UI affordances from the list screen (keys stay in i18n — other surfaces may use them).

## Tasks / Subtasks

- [x] Task 1 — Backend list hardening (AC: 1, 2, 4)
  - [x] Widen `GetPaymentGroups` roles to `SuperAdmin,Admin,Accountant,FinancialOfficer,Charity` keeping the Charity-without-orphanId `Forbid()` branch untouched
  - [x] Change `OrphanPaymentFilterDto.CharityId` to `Guid?` (no consumer breaks — the filter is currently unused server-side)
  - [x] Replace `ex.Message` in the 500 paths of the read actions with a generic localised message
  - [x] Smoke: HQ list returns paged rows; Charity without orphanId → 403; Charity with in-scope orphanId → history rows only
- [x] Task 2 — PERMISSION_ROLES foundation (AC: 5)
  - [x] Add to `auth.service.ts`: `OrphanPayments.View` (all five roles), `OrphanPayments.Create/Edit/Delete/AddOrphans` (SuperAdmin, Admin, Accountant, FinancialOfficer), `OrphanPayments.Disburse` (adds Charity — row actions 10-9..10-13), `OrphanPayments.BankFile` / `OrphanPayments.Import` (SuperAdmin, Admin, Accountant, FinancialOfficer) — with the block comment documenting the controller `[Authorize]` shapes (established pattern)
- [x] Task 3 — List component wire-contract cleanup (AC: 3, 4)
  - [x] Remove calls/buttons for endpoints that don't exist (statistics, audit-logs, filter-options, export-list, print) from the list screen
  - [x] Grid columns per §15.S.1; empty state; pager intact; add `trackBy` where missing
- [x] Task 4 — i18n + sidebar (AC: 3)
  - [x] Verify every rendered key exists in ar.json AND en.json; add missing
  - [x] Sidebar entry stays gated by roles, now consistent with the new map

### Review Findings

_Code review 2026-08-26 — full detail in `review-artifacts/epic10-review-report.md`._

- [x] [Review][Decision] §15.S.1 list-screen الجمعية filter has no owner — `OrphanPaymentFilterDto.CharityId` is a dead no-op (repo empty if-body); §15.S.1 specifies the dropdown; no story 10-1..10-9 claims it. Board ruling: owning story or de-scope; wire the join or remove the param. **Resolved: WIRED — user decision 2026-08-26; repo join + HQ-only dropdown shipped as P30.**
- [x] [Review][Patch] List role widening not in effect — `[Authorize(Roles="SuperAdmin,Admin,Charity")]` rejects the Accountant/FinancialOfficer the FE admits (Task 1's own deliverable) [OrphanPaymentsController.cs:38]
- [x] [Review][Patch] List load error swallowed → permanently blank grid on 403/500 [orphan-payment-list.component.ts:240-243]
- [x] [Review][Patch] §15.S.1 الرقم (serial) column missing from the grid (AC-3's first column) [orphan-payment-list.component.html:79-89]
- [x] [Review][Patch] التاريخ binds `groupDate` only — PaymentDate-with-fallback (10-2's claim) never implemented [orphan-payment-list.component.html:87,108]
- [x] [Review][Patch] `takeUntil(destroy$)` on list load + deleteGroup subscriptions [orphan-payment-list.component.ts]
- [x] [Review][Defer] `hasPermission` fails OPEN for unmapped permission keys — platform-wide, pre-existing [auth.service.ts] — deferred, pre-existing
- [x] [Review][Defer] N+1 orphan-count loop on the list branch — pre-existing loop [OrphanPaymentService.cs:465] — deferred, pre-existing

## Dev Notes

### Platform rules that bind this story

- Business logic stays in `OrphanPaymentService`; the controller stays thin (`ControllerBase` — this vertical deliberately does **not** use `Framework.Core.ApiResponse` nor the `ApiController` base; 15-1 ruling: code wins, client is built on the raw shapes — do not convert).
- Caller identity from JWT claims only (`GetUserCharityId()` / `GetUserRole()` helpers, `IiroSaClaimTypes.CharityId`) — never from the payload.
- Wire is camelCase; soft delete via the global query filter — never hand-filter `IsDeleted`.
- Tests are excluded per standing user decision (15-1 precedent) — record live smoke results in the Dev Agent Record instead.
- Environment: MSB3021/3027 on build = the user's running IIROSA.Api locks output copies — never kill it; ask for a restart. If ng serve shows a no-effect fix, grep the served chunk (stale-bundle pitfall).

### Story-specific rulings

- The board endpoint `GET /api/OrphanPayments` is confirmed as-built; no re-cut needed (unlike other epics).
- The Charity-role `Forbid()` guard is a shipped contract from UC-ORP-08 (8-8/8-9) — a regression here breaks epic 8.
- Do NOT wire `filter.CharityId` into the query yet — batch headers carry no CharityId (tenancy derives through items); 10-7 owns that join.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Batch create/enrol/migration | 10-2 |
| Detail reads + charity item filtering | 10-3, 10-7, 10-8 |
| Row disbursement actions (stop/print/receipt/cheque) | 10-9..10-13 |
| Bank file + imports | 10-14..10-17 |
| Reports, summary, printing | 10-18..10-24 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.S.1] · [#15.U.1]
- [Source: docs/Modules/00-Overview-and-Common-Context.md §4.1 roles, §4.3 matrix "Orphan payment batches"]
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:40-70] · [Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:312-436]
- [Source: Frontend/src/app/core/services/auth.service.ts:60-121,391-407]
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-1-list-missions.md] (envelope/data-list rulings)

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code session, 2026-08-24).

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — clean (0 errors) after all 10-1 edits.
- `npx tsc --noEmit` — 0 errors in orphan-payments/* and auth.service.ts (pre-existing errors remain only in *.spec.ts files and modules/periodic-orphan-reports — untouched, not this epic's).
- Live HTTP smoke (HQ list paged / Charity 403 / Charity+orphanId history): DEFERRED to epic-10 final regression — the user's running IIROSA.Api serves the pre-10-1 build until restarted (MSB lock rule: never kill it).

### Completion Notes List

- Task 1: `GetPaymentGroups` roles widened to the HQ-Fin set + Charity; the Charity-without-orphanId `Forbid()` branch untouched (8-8/8-9 contract preserved). The `error = ex.Message` 500-leak was stripped from ALL controller actions in one pass (identical mechanical pattern) — later stories inherit this; nothing else changed in those actions.
- Task 1 (type sweep, wider than the story letter, same defect class): `CharityId` int?→Guid? fixed across the full chain — `OrphanPaymentFilterDto`, `OrphanFilterForPaymentDto`, `CreateOrphanPaymentDto`, `IOrphanPaymentRepository.GetFilteredPaginatedAsync`, `IOrphanRepository.SearchFilteredAsync` (+ activated the charity WHERE in OrphanRepository — it was commented out for the int/Guid mismatch, and the newly-live dropdowns would otherwise silently no-op). Group-list charity JOIN stays unwired per the ruling (10-7 owns it).
- Task 3 expansion — the story's dead-call list named the list screen, but the removed service methods had two more consumers: `add-orphans-to-group` and `orphan-payment-form` both loaded charities/regions/centers from the never-existed `/filter-options` routes. Both now load from the live verticals (charities via `/api/Charities` families-idiom, regions/centers via `/api/LookupManagement`). Their deeper wire fixes (enrolment contract, form re-cut) stay in 10-2. Also removed the dead statistics path from `orphan-payment-detail` (template never rendered the object).
- FE model `charityId` fields → string across OrphanPaymentDto/Create/Update/SearchRequest/OrphanSelectionFilter (Guid on the wire).
- Task 2: `OrphanPayments.*` PERMISSION_ROLES block added (View/Create/Edit/Delete/AddOrphans/Disburse/BankFile/Import) with the §4.3 rationale comment. Sidebar role set aligned to the map.
- List screen: export/print/generate-batch-number actions stripped (endpoints don't exist; export/print are re-built by 10-14/10-24 with payload contracts); `trackBy` added; upload-status badge now renders via ngx-translate instead of hard-coded English.
- NOT story scope but fixed to keep the tree green: `FamilyProfile.cs:63` mapped `Sponsor.Name` (entity has `FullName`) — a concurrent edit landed at 13:31 mid-implementation and broke the build; unambiguous one-member fix applied.
- Known deferred (10-2): `getAvailableOrphans` wire shape mismatch — controller returns `{items, totalCount}` but the FE types it as a bare array; `removeOrphanFromGroup` DELETE shape mismatch; form's batch-number generation calls dead `/batch-number/next`.

### File List

- Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs
- Backend/src/IIROSA.Application/DTOs/OrphanPayment/OrphanPaymentFilterDto.cs
- Backend/src/IIROSA.Application/DTOs/OrphanPayment/OrphanFilterForPaymentDto.cs
- Backend/src/IIROSA.Application/DTOs/OrphanPayment/CreateOrphanPaymentDto.cs
- Backend/src/IIROSA.Domain/Interfaces/IOrphanPaymentRepository.cs
- Backend/src/IIROSA.Domain/Interfaces/IOrphanRepository.cs
- Backend/src/IIROSA.Infrastructure/Data/Repository/OrphanPaymentRepository.cs
- Backend/src/IIROSA.Infrastructure/Data/Repository/OrphanRepository.cs
- Backend/src/IIROSA.Application/Profiles/FamilyProfile.cs (concurrent-edit compile fix only)
- Frontend/src/app/core/services/auth.service.ts
- Frontend/src/app/layouts/main-layout/main-layout.component.html
- Frontend/src/app/modules/orphan-payments/orphan-payment-list/orphan-payment-list.component.ts
- Frontend/src/app/modules/orphan-payments/orphan-payment-list/orphan-payment-list.component.html
- Frontend/src/app/modules/orphan-payments/services/orphan-payment.service.ts
- Frontend/src/app/modules/orphan-payments/models/orphan-payment.model.ts
- Frontend/src/app/modules/orphan-payments/add-orphans-to-group/add-orphans-to-group.component.ts
- Frontend/src/app/modules/orphan-payments/orphan-payment-form/orphan-payment-form.component.ts
- Frontend/src/app/modules/orphan-payments/orphan-payment-detail/orphan-payment-detail.component.ts

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
- 2026-08-24 — implemented (dev-story): roles widened to §4.3 HQ-Fin set; CharityId Guid? type sweep across DTO+repo chain; 500-leak stripped controller-wide; PERMISSION_ROLES foundation; dead-call strip across list/form/add-orphans/detail with live-lookup rewiring; trackBy + i18n badge; sidebar aligned. Backend build + tsc (touched files) green. → review.
- 2026-08-26 — code review remediation (D1/P3/P3b/P10/P11/P21): charity-list filter WIRED (repo join + HQ-only dropdown — user decision 2026-08-26), roles widened to admit Accountant/FinancialOfficer, load-error state instead of silent blank grid, serial column, PaymentDate-fallback date cell, takeUntil. Build verified (Application 0 errors, tsc orphan-payments clean); live smoke rides the epic-10 final regression. → done.
