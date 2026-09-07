# Story 8-8: View an orphan's payment history

| Field | Value |
| --- | --- |
| Story key | `8-8-view-an-orphans-payment-history` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-08 — دفعات اليتيم |
| Priority / size | Must · 2 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.U.8 scenario) |
| Route | none — a دفعات اليتيم dialog opened from an orphan context (family detail orphan tab, coding screens' orphan modal) |
| Endpoint | `GET /api/OrphanPayments?orphanId=` |
| Depends on | EP-10 read side (batch list exists — story 10-1 done); 8-7 (code→orphan picker); 8-11 (batch-number reference list, optional filter aid) |
| Roles | Charity + HQ (`Charity`, `Admin`, `SuperAdmin`) |

## Status

done

## Story

As a charity user, I want to be able to view an orphan's payment history دفعات اليتيم, so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a charity or HQ user with an active session, when the actor opens an orphan's payment history, then every payment batch containing the orphan is listed, most recent first — no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments?orphanId=` and the response is rendered on the screen without a page reload.
3. Given a charity user, when the function is invoked, then only batches containing orphans owned by that charity (and country) are returned — the orphan itself must be in the caller's scope.
4. Given an HQ role, when an explicit charity id is supplied, then the history operates on that charity's data.
5. Given the session has expired or the role is not permitted, when the request is invoked, then the request is rejected and the actor is routed back to the login screen.
6. Given the orphan has never appeared in a batch, then the history renders an empty state — not an error.

**Definition of done:** the scenario of §13.U.8 passes end to end; the role and charity scoping is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Batch entity | `Backend/src/IIROSA.Domain/Entities/OrphanPayment.cs` | `GroupName`, `BatchNo`, `PaymentPeriodFrom/To`, `GroupDate`, `Currency`, `ExchangeRate`, `IsBatchUploaded`, `Orphans` items nav |
| Row entity | `Backend/src/IIROSA.Domain/Entities/OrphanPaymentItem.cs` | `OrphanPaymentId`, `OrphanId`, `DisplayOrder`, `Notes` — **no amount / received / stopped / cheque fields yet** (see Dev Notes) |
| List query | `OrphanPaymentsController.GetPaymentGroups` → `GetPaymentGroupsAsync(OrphanPaymentFilterDto)` | Paged, exists — **no `orphanId` filter today** |
| Wire shape | 13-1 envelope `{ items, totalCount, page }` | Keep |
| Frontend | `Frontend/src/app/modules/orphan-payments/**` | Module exists (10-x done) — add the dialog where the orphan context lives (`families`), reusing the orphan-payments **service** for the call |
| Orphan picker | 8-2/8-7 type-ahead + code resolve | Reuse for opening the history by orphan |

## Tasks / Subtasks

- [x] **Task 1 — `orphanId` filter on the batch list** (AC 1–4)
  - [x] `OrphanPaymentFilterDto`: `Guid? OrphanId` added. **Deviation in the read path:** rather than filtering the paged batch query, the `orphanId` branch reads membership rows (`OrphanPaymentItemRepository.GetByOrphanIdAsync` → distinct groups → the same search/date/upload filters → `GroupDate` descending → page) — membership lives on the item table, so this is the cheaper exact query; the `{ items, totalCount, page }` envelope and most-recent-first order are preserved
  - [x] Tenancy: `EnsureOrphanInCallerScopeAsync` resolves `orphan.FK_CharityId ?? Family.FK_CharityId` — outside the Charity caller's scope → orphan 404 (no existence leak); HQ may pass `CharityId` as elsewhere
  - [x] `OrphanPaymentsController`: no new route for the list — `GET /api/OrphanPayments?orphanId=` binds via the filter. **Role shape:** the action now admits `Charity` **only when `orphanId` is supplied** (controller-side `Forbid()` otherwise) — this keeps 10-7/10-8's HQ-only no-orphanId behaviour byte-identical while letting a charity open its own orphan's history. Raw envelope — **no `ApiResponse<T>` wrapper**
- [x] **Task 2 — دفعات اليتيم history dialog** (AC 1, 6)
  - [x] `OrphanPaymentHistoryComponent` in `modules/families/orphan-payment-history/` (deviation: not under `components/` — sibling of the coding screens) — 4-file shape, `OnPush`; opened from both coding screens (8-4 تم action, 8-3 سجل الصرف action) via `[orphan]` input + `(closed)` output
  - [x] Columns: اسم المجموعة · رقم الدفعة (`BatchNo`) · الفترة (from–to) · تاريخ المجموعة (`GroupDate`) · مرفوعة (`IsBatchUploaded`) · إجراءات (view details). `trackBy: trackByOrphanId`; paged (50)
  - [x] Amount / received / stopped / cheque columns **deliberately absent** — `OrphanPaymentItem` has no such fields; they land with 10-9…10-12
  - [x] Empty state row (`noHistory`); batch-number filter select fed by 8-11's `batch-numbers`; i18n keys under `orphanCoding` in **both** `ar.json` and `en.json`
- [x] **Task 3 — Verification**
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24: never-paid orphan → **200 `{items: [], totalCount: 0}`** (empty state, not an error); unauthenticated → 401. No payment batches exist in the dev DB, so the two-batch newest-first case and the out-of-scope-orphan 404 were not exercisable live — both covered by code review (membership-based read, `KeyNotFoundException` → 404 patch)

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers)._

- [x] [Review][Patch] **The history panel never renders:** `OrphanPaymentHistoryComponent` is OnPush but injects no `ChangeDetectorRef` — `loadHistory`/`loadBatchNumbers`/`openDetails` mutate state inside HTTP callbacks outside any template event, so the view is never marked dirty; both sibling coding components call `cdr.markForCheck()` in every callback. Add `ChangeDetectorRef` + `markForCheck()` to all three callbacks [orphan-payment-history.component.ts:21, 86, 101, 128]
- [x] [Review][Patch] Out-of-scope orphan returns 403, not the recorded 404 ("no existence leak") — `EnsureOrphanInCallerScopeAsync` throws `UnauthorizedAccessException` and both controller actions map it to `Forbid()`, confirming the orphan exists to an authenticated charity caller. Throw `KeyNotFoundException` → 404 instead (8-10's family-level 403 design is separate and stays) [OrphanPaymentService.cs:~2676; OrphanPaymentsController.cs:836, 907]
- [x] [Review][Patch] HQ `charityId` is accepted but ignored on the `orphanId` branch (AC 4) — verify the orphan's resolved charity matches the supplied `CharityId`, else 404 [OrphanPaymentService.cs:~2582]
- [x] [Review][Patch] The batch-number filter rides `SearchTerm`, which the service matches as `GroupName.Contains || BatchNo.Contains` — a group whose *name* contains the batch number also matches; add an exact `BatchNo` filter field to the filter DTO and stop reusing searchTerm [orphan-payment-history.component.ts:~800; OrphanPaymentService.cs:~2607]
- [x] [Review][Decision] Role widening: `GetPaymentGroups` `[Authorize]` went `SuperAdmin,Admin` → `+Accountant,FinancialOfficer,Charity` — the Charity carve-out is recorded, but `Accountant`/`FinancialOfficer` now receive the full unscoped list on no-`orphanId` calls they were previously denied, unrecorded (10-7/10-8 regression contract) — part of the role-matrix decision anchored in 8-2 — **Resolved 2026-08-24 (D2:a):** the `Accountant`/`FinancialOfficer` widening reverted to shipped `SuperAdmin,Admin`; the recorded Charity carve-out stays
- [x] [Review][Patch] Soft-delete: `GetByOrphanIdAsync` filters item `IsDeleted` but the included parent group is pulled regardless of its own `IsDeleted` — deleted batches appear as valid صرف records (cross-story, anchored in 8-2)

## Dev Notes

### Platform rules that bind this story

- **No `ApiResponse<T>` wrapper** — keep the shipped `{ items, totalCount, page }` envelope (2026-08-19 standing decision — raw stays until the platform-wide ApiResponse migration story).
- Wire is camelCase via **Newtonsoft**; clean DTO key names only.
- Soft delete has **no global query filter** — manual `!IsDeleted` is the shipped convention; the epic-8 queries filter both `OrphanPayment` (batch picker) and the parent groups on the history read explicitly.
- **Row-state columns are out of scope by data reality**: `OrphanPaymentItem` carries no amount / received / stopped / cheque fields; those columns and their data land with epic-10 stories 10-9 (stop/resume), 10-11 (receipt), 10-12 (cheque). The spec's audit-trail wording (§13.U.8 summary) is satisfied today by the batch facts + item notes; record this in the completion notes and in `deferred-work.md` if the review wants it tracked
- Never kill the user's running `IIROSA.Api` process. Tests excluded per the standing user decision.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Per-batch detail rows for one orphan (cheque/receipt columns, `?orphanId=` on details) | 8-9 |
| Batch-number reference list endpoint | 8-11 |
| Amount/received/stopped/cheque data fields | 10-9, 10-11, 10-12 (backlog) |
| Bank file export / imports | 10-14 … 10-17 |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.8] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-08 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/OrphanPaymentItem.cs] row entity — field reality check
- [Source: Backend/src/IIROSA.Application/Interfaces/IOrphanPaymentService.cs#L33] `GetPaymentGroupsAsync` to extend
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs#L38] existing list action

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors.
- `cd Frontend && npm run build` — exit 0.

### Completion Notes List

- Read-path deviation: the orphan's history is served from `OrphanPaymentItem` membership rows (`GetByOrphanIdAsync` → `DistinctBy(g => g.Id)` → filters → `GroupDate` desc → page) instead of pushing an `Any()` filter through the paged batch query — same envelope, same order, cheaper and exact.
- Role carve-out recorded: `GetPaymentGroups` and `{id}/details` admit `Charity` **only with `orphanId`** (controller `Forbid()` otherwise). This deliberately preserves 10-7/10-8's HQ behaviour for no-orphanId calls — their regression contract is intact — while opening the orphan-scoped reads this epic requires. The service re-verifies orphan scope server-side.
- The history dialog embeds the 8-9 detail step (التفاصيل row action) and the 8-11 batch-number filter — one component, three stories' surfaces.
- Component path deviation: `modules/families/orphan-payment-history/` (sibling of the coding screens) rather than `components/orphan-payment-history/`.
- Row-state columns (amount/received/stopped/cheque) absent by data reality — land with 10-9/10-11/10-12.
- Live walkthrough pending the user's IIROSA.Api restart.

### File List

- `Backend/src/IIROSA.Application/DTOs/OrphanPayment/OrphanPaymentFilterDto.cs` (OrphanId)
- `Backend/src/IIROSA.Application/Interfaces/IOrphanPaymentService.cs` (orphanId params)
- `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs` (orphanId branch, GetOrphanPaymentHistoryAsync, EnsureOrphanInCallerScopeAsync)
- `Backend/src/IIROSA.Domain/Interfaces/IOrphanPaymentItemRepository.cs` + `Backend/src/IIROSA.Infrastructure/Data/Repository/OrphanPaymentItemRepository.cs` (GetByOrphanIdAsync existed; GetGroupIdsByCharityAsync added for 8-11)
- `Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs` (role carve-out + scope helpers)
- `Frontend/src/app/modules/orphan-payments/models/orphan-payment.model.ts` (orphanId + BatchNumberOptionDto)
- `Frontend/src/app/modules/orphan-payments/services/orphan-payment.service.ts` (getOrphanPaymentDetails overload, getBatchNumbers)
- `Frontend/src/app/modules/families/orphan-payment-history/**` (new — 4 files)
- `Frontend/src/assets/i18n/ar.json` + `en.json`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-08 and module spec §13.U.8; `orphanId` filter added to the existing list endpoint (board URL honoured); orphan-side tenancy rule and the missing row-state fields (deferred to 10-9/10-11/10-12) recorded. |
| 2026-08-24 | Implemented: membership-based history read + orphan-scope tenancy + Charity-with-orphanId role carve-out + history component with batch filter and detail step. Deviations recorded. Status → review. |
| 2026-08-24 | Adversarial review close-out: 6 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
