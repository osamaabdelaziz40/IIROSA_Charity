# Story 8-9: View an orphan's payment details

| Field | Value |
| --- | --- |
| Story key | `8-9-view-an-orphans-payment-details` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-09 — تفاصيل دفعة اليتيم |
| Priority / size | Must · 2 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.U.9 scenario) |
| Route | none — a تفاصيل دفعة اليتيم view opened from a history row (8-8) |
| Endpoint | `GET /api/OrphanPayments` (the batch details read — existing `GET /api/OrphanPayments/{id}/details`, extended with `?orphanId=`) |
| Depends on | 8-8 (the history list that feeds the batch picker), 8-11 (batch-number reference list) |
| Roles | Charity + HQ (`Charity`, `Admin`, `SuperAdmin`) |

## Status

done

## Story

As a charity user, I want to be able to view an orphan's payment details تفاصيل دفعة اليتيم, so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a charity or HQ user with an active session, when the actor opens one of an orphan's batches, then the payment rows attached to that orphan in that batch are retrieved — no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments/{id}/details?orphanId=` and the response is rendered on the screen without a page reload.
3. Given a charity user, when the function is invoked, then only that orphan's rows within the caller's charity (and country) scope are returned.
4. Given an HQ role, when an explicit charity id is supplied, then the read operates on that charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.
6. Given the business rule behind the request is broken (batch or orphan out of scope, «Faliure» in legacy terms), then the read is refused and nothing is rendered.

**Definition of done:** the scenario of §13.U.9 passes end to end; the role and charity scoping is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Details read | `OrphanPaymentsController` `GET {id}/details` → `GetPaymentGroupDetailsAsync(Guid id)` | Exists (served 10-7/10-8) — returns `OrphanPaymentDto` including `Orphans` items. **No `orphanId` filter today** |
| DTO | `OrphanPaymentDto.Orphans: List<OrphanPaymentItemDto>` | The rows this story filters |
| History | 8-8's `GET /api/OrphanPayments?orphanId=` | The batch picker's data source |
| Batch numbers | 8-11's `GET /api/OrphanPayments/batch-numbers` | Optional alternate entry: query by رقم الحصة directly |
| Frontend | `Frontend/src/app/modules/orphan-payments/**` + 8-8's history dialog | Reuse both — this story adds the detail step |

## Tasks / Subtasks

- [x] **Task 1 — `orphanId` filter on the details read** (AC 1–4, 6)
  - [x] `GetPaymentGroupDetailsAsync(Guid id, Guid? orphanId = null, Guid? userCharityId = null, string? userRole = null)`: when `orphanId` is set, the same `OrphanPaymentDto` returns with `Orphans` filtered to that orphan; zero rows for that orphan → `KeyNotFoundException` (404). Batch facts stay complete — the header is the batch, the rows are the orphan's
  - [x] Controller `GET {id}/details`: binds `[FromQuery] Guid? orphanId`; wire shape unchanged — the orphan-payments module (10-x) already renders it
  - [x] Tenancy: Charity-with-`orphanId` path re-verifies the orphan's scope server-side (same `EnsureOrphanInCallerScopeAsync` as the history read); out-of-scope orphan → 404, no existence leak; HQ narrows as elsewhere
  - [x] **Backward compatibility:** the no-`orphanId` path is untouched — 10-7/10-8 behaviour preserved (the Charity role still needs `orphanId` on this action per the 8-8 carve-out)
- [x] **Task 2 — تفاصيل دفعة اليتيم view** (AC 1)
  - [x] Detail step in the 8-8 history dialog: the row's التفاصيل action loads `{id}/details?orphanId=` and renders the batch header (GroupName, BatchNo, period, GroupDate, Currency, ExchangeRate, IsBatchUploaded) + the orphan's rows (`DisplayOrder`, `Notes`); عودة للسجل returns to the history list
  - [x] Cheque number / receipt state columns appear **when the fields exist** (10-11/10-12) — nothing rendered now, no faked values
  - [x] `trackBy` on the rows; i18n keys under `orphanCoding` in **both** `ar.json` and `en.json`; OnPush
- [x] **Task 3 — Verification**
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24: unknown batch id on `{id}/details` → **404** with a clean "Payment group with ID … not found" message (no SQL/500 leak); unauthenticated → 401. No payment batches exist in the dev DB, so the orphan-filtered row rendering and the no-`orphanId` 10-x regression case were not exercisable live — both verified in code review

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers)._

- [x] [Review][Patch] Detail header omits الفترة (`PaymentPeriodFrom/To`) and مرفوعة (`IsBatchUploaded`) — both spec'd in Task 2's header list and already on the DTO; only GroupName/GroupDate/Currency/ExchangeRate render [orphan-payment-history.component.html detail step]
- [x] [Review][Patch] Out-of-scope orphan 403 → 404 (shared code path with 8-8; anchored there)

## Dev Notes

### Platform rules that bind this story

- **No `ApiResponse<T>` wrapper** — the shipped `OrphanPaymentDto` shape is the contract (2026-08-19 standing decision — raw stays until the platform-wide ApiResponse migration story).
- Wire is camelCase via **Newtonsoft**; clean DTO key names only.
- Soft delete has **no global query filter** — `SetGlobalQueryFilters` is never called; the shipped convention is manual `!IsDeleted` on every read, and the epic-8 queries filter it explicitly (orphan, and its family where joined).
- Read-side regression risk is the main danger here: 10-7/10-8 are `done` — the optional-parameter change must be additive only. Verify the orphan-payments list/detail screens still render after the change
- Never kill the user's running `IIROSA.Api` process. Tests excluded per the standing user decision.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| The history list + dialog that hosts this step | 8-8 |
| Batch-number reference endpoint | 8-11 |
| Cheque number / receipt state data + columns | 10-11, 10-12 (backlog) |
| Reports on received/not-received/stopped | 10-18 … 10-20, EP-18 |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.9] scenario — rows for one orphan in a chosen batch
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-09 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs#L82] existing details action
- [Source: Backend/src/IIROSA.Application/Interfaces/IOrphanPaymentService.cs#L36] `GetPaymentGroupDetailsAsync` to extend

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors.
- `cd Frontend && npm run build` — exit 0.

### Completion Notes List

- Additive change only: the optional `orphanId` parameter defaults to the previous behaviour; 10-7/10-8's no-orphanId calls are contractually unchanged (their Charity Forbid on this action also stands — Charity reaches this read only through the orphan-scoped history flow).
- Row filtering happens in-memory on the already-materialised `OrphanPaymentDto.Orphans` (one batch's items) — no extra query, exactness guaranteed by the 404-when-empty guard.
- The detail step lives inside `OrphanPaymentHistoryComponent` (header row + orphan rows table); cheque/receipt columns render nothing until 10-11/10-12 add the fields.
- Live walkthrough pending the user's IIROSA.Api restart.

### File List

- `Backend/src/IIROSA.Application/Interfaces/IOrphanPaymentService.cs` (GetPaymentGroupDetailsAsync params)
- `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs` (orphan filter + 404 guard)
- `Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs` (orphanId binding + role carve-out)
- `Frontend/src/app/modules/orphan-payments/services/orphan-payment.service.ts` (getOrphanPaymentDetails(id, orphanId?))
- `Frontend/src/app/modules/families/orphan-payment-history/**` (detail step)
- `Frontend/src/assets/i18n/ar.json` + `en.json`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-09 and module spec §13.U.9; additive `?orphanId=` on the existing details read with an explicit 10-7/10-8 regression guard; missing cheque/receipt columns mapped to 10-11/10-12. |
| 2026-08-24 | Implemented: orphan-scoped details read + 404 guard + history-dialog detail step. 10-7/10-8 regression contract honoured. Status → review. |
| 2026-08-24 | Adversarial review close-out: 2 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
