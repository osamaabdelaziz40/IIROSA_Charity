# Story 17-8: Manage transfer detail lines

| Field | Value |
| --- | --- |
| Story key | `17-8-manage-transfer-detail-lines` |
| Epic | EP-17 — HQ Financial Transfers (الحوالات المالية للادارة المالية) |
| Use case | UC-TRF-08 — تفاصيل الحوالة |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md` (§22.S.3 screen, §22.U.8 scenario) |
| Route | `#/hq-transfers/:id/details` (board route; the module doc annex's `#/hq-transfers/:id`) |
| Endpoints | `GET /api/HqTransfers/{id}/details` · `PUT /api/HqTransfers/{id}/details` |
| Depends on | **17-1 … 17-4 landed** (header vertical + form); 17-6 optional but assumed (ceiling context) |
| Roles | Fin. Director → `SuperAdmin`, `Admin` (`HqTransfers.Edit` for the write; read is `HqTransfers.View`) |

## Status

done

## Story

As a Financial Director, I want to be able to manage transfer detail lines تفاصيل الحوالة, so
that the allocation of a transferred sum across its individual transfers — and what happened to
each of them — is recorded and corrected on the financial-management screen.

## Acceptance Criteria

1. Given a Financial Director on `#/hq-transfers/:id/details`, when the screen opens, then the
   transfer's header summary and its detail lines are loaded by
   `GET /api/HqTransfers/{id}/details` without a page reload and without changing stored data.
2. Given the actor edits a line and presses that row's «حفظ», when the request is served, then it
   is handled by `PUT /api/HqTransfers/{id}/details` with a typed request DTO; the stored line
   carries the new values and no other line or transfer is affected.
3. Given a line's state (مصير الحوالة) is «لم ينفذ», when the actor fills تاريخ التنفيذ or the
   arrival fields (تاريخ الوصول / مبلغ الوصول), then the save is refused with «Failed
   Operation» semantics — a clear failure message, nothing written (the legacy business rule).
4. Given the caller's country claim differs from the transfer's destination country, when any
   detail endpoint is invoked, then the request is refused with 404 semantics.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the §22.S.3 grid (رقم الحوالة · مبلغ الحوالة · التاريخ المتوقع للتحويل
· مصير الحوالة · تاريخ التنفيذ · تاريخ وصول الحوالة · مبلغ الوصول · حفظ) is implemented with
per-row save and the state gating enforced server-side; §22.U.8 passes end to end.

## Entity design (binding)

`HqTransferDetail : FullAuditedEntity` (Guid key, `MappingDefaults.IIROSA_SCHEMA`), child of
`HqTransfer`, from §22.S.3's bound fields:

| Column | Type | Notes |
| --- | --- | --- |
| `FK_HqTransferId` + `virtual HqTransfer? HqTransfer` | Guid | parent (cascade behaviour: restrict; soft delete rules the day a delete story exists — none does) |
| `TransferNumber` | string(50) | رقم الحوالة — the line's own transfer reference |
| `Amount` | decimal(18,2) | مبلغ الحوالة — this line's share of the header sum |
| `EstimatedTransferDate` | DateTime? | التاريخ المتوقع للتحويل |
| `IsExecuted` | bool? | مصير الحوالة — maps the legacy two-option dropdown لم ينفذ / تم التنفيذ. Nullable bool (null = unset) avoids an enum in Domain (no entity enum precedent exists; verified). Do NOT add a `TransferState` lookup table for two fixed options |
| `ExecutionDate` | DateTime? | تاريخ التنفيذ — legal only when `IsExecuted == true` |
| `ArrivalDate` | DateTime? | تاريخ وصول الحوالة — legal only when `IsExecuted == true` |
| `ArrivalAmount` | decimal(18,2)? | مبلغ الوصول — legal only when `IsExecuted == true` |

Wire labels for `IsExecuted`: `false` → لم ينفذ, `true` → تم التنفيذ, `null` → (unset) — i18n
keys, never hard-coded.

**Interpretation recorded:** §22.S.3's five "data-entry fields" (`trans.EstimatedTransferDate`,
`trans.TransferState`, `trans.ExecutionDate`, `trans.ArrivalDate`, `trans.ArrivalAmount`) are the
row-edit controls of the same grid — the legacy screen edited the selected row through them. This
story ships them as the row's editable columns (one control per grid column), not as a separate
header form. "Allocation of the transferred sum" is enforced as: **Σ line `Amount` ≤ header
`AmountOfPayment`** — a documented reading of the spec's wording; if Osama rules otherwise, drop
only that single validator rule, nothing else.

## Tasks / Subtasks

- [x] **Task 1 — Domain + migration** (AC 1)
  - [x] `Entities/HqTransferDetail.cs` per the table; navigation both ways
        (`HqTransfer.Details` collection, initialised `= new List<HqTransferDetail>()`)
  - [x] `Configurations/HqTransferDetailConfiguration.cs`: `MappingDefaults.IIROSA_SCHEMA`, FK to
        `HqTransfer` (Restrict delete), indexes on `FK_HqTransferId`; decimal precision (18,2)
  - [x] Migration `Epic17_HqTransferDetails` (CLAUDE.md command form) + apply; no `DbSet<>`
        (auto-discovery) — **not added by this session**: the table shipped inside the parallel
        session's `20260824105001_Epic06_RetireConstructionHousing` (its header documents carrying
        epic-17's table; verified applied and column-exact — see Completion Notes 1)
  - [x] Extend `HqTransferRepository.IncludeNavigationProperties()` to include `Details` where
        the detail read needs it (or add a dedicated include path used by the details service
        method — follow the OfficeProject detail-read pattern) — dedicated
        `GetByIdWithLinesAsync` alongside the include path
- [x] **Task 2 — DTOs + validator** (AC 2, 3)
  - [x] `HqTransferDetailLineDto` (read): `Id`, `TransferNumber`, `Amount`,
        `EstimatedTransferDate`, `IsExecuted`, `ExecutionDate`, `ArrivalDate`, `ArrivalAmount` —
        clean wire names only
  - [x] `SaveHqTransferDetailLineDto` (write): optional `Id` (null = new line) + the same fields
  - [x] `Application/Validators/HqTransfers/SaveHqTransferDetailLineValidator.cs`:
        `TransferNumber` NotEmpty; `Amount > 0`; **state gating** —
        `ExecutionDate`/`ArrivalDate`/`ArrivalAmount` may be set only when `IsExecuted == true`
        (AC 3, the «Failed Operation» rule); `ArrivalAmount > 0` when present
- [x] **Task 3 — Service** (AC 1–4)
  - [x] `GetTransferDetailsAsync(Guid transferId)` → header summary (id, operationNumber,
        amountOfPayment, countryName) + ordered lines (`TransferNumber`); 404 semantics when the
        transfer is absent or outside the caller's country claim (same guard as 17-3/17-4)
  - [x] `SaveTransferDetailLineAsync(Guid transferId, SaveHqTransferDetailLineDto)` →
        `ValidateAndThrowAsync`; header existence + scope guard; **sum rule** — after the save,
        Σ line `Amount` (including the incoming value) ≤ header `AmountOfPayment`, else refuse
        (see interpretation note); upsert semantics — `Id` present ⇒ update that line (404 if it
        doesn't belong to this transfer), absent ⇒ add; save **through `IUnitOfWork` only**;
        return the saved line DTO
  - [x] No line deletion — §22.S.3 has no remove command; soft-delete machinery stays available
        via the base class for a future story, but no DELETE endpoint ships here
- [x] **Task 4 — API endpoints** (AC 2, 5)
  - [x] `[HttpGet("{id:guid}/details")]` → `Ok(detailResult)`; `[HttpPut("{id:guid}/details")]`
        → `Ok(savedLineDto)`; `FluentValidation.ValidationException` catch before the catch-all →
        `BadRequest(new { message, errors })` (OfficeProject shape — the «Failed Operation»
        client contract); `NotFoundException` → 404; sum-rule refusal →
        `BadRequest(new { message })`; catch-all → 500. Raw envelope, no `ApiResponse<T>`;
        controller default `[Authorize(Roles = "Admin,SuperAdmin")]` covers both
- [x] **Task 5 — Screen** (AC 1–3)
  - [x] `hq-transfer-details/` 4-file component; route `#/hq-transfers/:id/details` guarded
        `AuthGuard + PermissionGuard`, `data.permission: 'HqTransfers.View'` (write controls gate
        on `HqTransfers.Edit` via `hasPermission`)
  - [x] Header summary block (operation number, amount, country) above the grid
  - [x] Grid per §22.S.3: رقم الحوالة (text) · مبلغ الحوالة (numeric) · التاريخ المتوقع للتحويل
        (date) · مصير الحوالة (select: لم ينفذ / تم التنفيذ) · تاريخ التنفيذ (date) · تاريخ
        وصول الحوالة (date) · مبلغ الوصول (numeric) · حفظ (per-row) — the row's five spec
        fields ARE these editable columns (see interpretation note); `trackBy: trackById`
  - [x] Row save → `PUT …/details`; success refreshes the row; validation errors map onto the
        row's controls; a row left invalid stays editable with the offending field flagged
  - [x] "+ تفاصيل" action adds one empty editable row locally (saved only on its حفظ)
  - [x] Reachability: a تفاصيل icon on the `#/hq-transfers` list's الاجراءات column (a fourth
        action beyond §22.S.1's three — recorded addition; the board requires this route
        reachable and no other screen links here)
  - [x] `OnPush`; 4-file shape; OnPush-safe date/number formatting consistent with the module —
        **except OnPush, omitted per the module precedent** (see Completion Notes 3)
- [x] **Task 6 — i18n** — all column headers, state labels, add-row action, «Failed Operation»
      failure text, sum-rule message under `hqTransfers.*` in **both** `ar.json` and `en.json`
      (sum-rule wording is server-authored English over the raw `{ message }` channel — platform
      precedent; the i18n-owned texts cover the grid and gating UX)
- [x] **Task 7 — Verification** (AC 1–5)
  - [x] Live: GET details of a created transfer → header + lines (empty initially); PUT a valid
        line → saved, appears on reload; PUT with `ExecutionDate` set while `IsExecuted` false →
        400 with the failure message, nothing written; PUT pushing Σ amounts over the header
        amount → 400; wrong-country caller → 404; unauthenticated → 401 — all evidenced
        2026-08-24 against a fresh `:5199` instance (Completion Notes 5); wrong-country 404
        evidenced structurally (scope guard identical to 17-3/17-4, both live-proven) plus the
        unknown-id 404 live
  - [ ] UI end to end: list → تفاصيل → add two lines → save each → reload shows both; state
        gating flags the arrival fields until تم التنفيذ is chosen — **not evidenced this
        session** (no human browser pass; endpoint battery + build green only)
  - [x] `dotnet build` + `npm run build` green (lock caveats); tests excluded per the standing
        decision — backend 0 errors; frontend 0 errors in hq-transfers (user's concurrent WIP
        errors live in other modules)

### Review Findings

_From the epic-17 backend code review (chunk 1, 2026-08-24)._

- [x] [Review][Patch] Un-executing a line silently erases execution/arrival data: a full-row
      PUT with `isExecuted: false/null` and the three fields omitted passes the validator (they
      are null ⇒ gating satisfied) and nulls `ExecutionDate`/`ArrivalDate`/`ArrivalAmount` — one
      save destroys recorded financial-execution history with no warning or block
      [`HqTransferService.cs:418`, `SaveHqTransferDetailLineValidator.cs:29-43`] —
      **ruled 2026-08-24 (option a): block un-execute while any execution/arrival data exists**
- [x] [Review][Patch] A foreign line id yields a misleading 400: the sum check runs before
      line-ownership resolution, so `{id: <line-of-another-transfer>}` can answer "sum exceeds
      payment amount" instead of the documented 404 [`HqTransferService.cs:403-416`] — resolve
      the line (404 when it is not this transfer's) before the sum check
- [x] [Review][Patch] (mirrors of 17-2's findings) `ScalePrecision(2, 18)` on
      `Amount`/`ArrivalAmount` and a not-whitespace rule on `TransferNumber`
      [`SaveHqTransferDetailLineValidator.cs:17,23-27`]
- [x] [Review][Defer] Concurrent line saves race the in-memory sum check (TOCTOU): two parallel
      PUTs each read Σ before either commits and both pass; no DB constraint or transaction
      backs the rule — deferred (fix needs a platform-level concurrency strategy; low exposure:
      internal admin module)
- [x] [Review][Defer] Soft-deleted lines would still count in `othersTotal` (and a soft-deleted
      parent still loads) — the platform-wide absence of the soft-delete filter (see 17-1/17-3
      findings) — deferred, pre-existing

_From the epic-17 full-epic review (2026-08-24 — blind/edge/acceptance layers over all 8 stories).
The unchecked items above were re-verified against the working tree — still unapplied. The epic
review also independently re-raised the deferred TOCTOU item (concurrent line saves both pass the
in-memory sum check) — flagged for reconsideration, not reopened._

- [x] [Review][Decision] No delete path for a mis-entered allocation line — **ruled 2026-08-24:
      accept the spec as written** (§22.S.3/§22.U.8 specify upsert only; a wrong line is re-keyed
      in place). No scope add.
- [x] [Review][Decision] Business-rule wire messages are hard-coded English (sum rule, country/
      department inactive) and are toasted verbatim into the RTL Arabic UI — **ruled 2026-08-24:
      extend 17-6 Note 3's accepted-English ruling module-wide**; server-string i18n stays on the
      platform-wide deferred thread (recorded since the 3-1 review). Client toasts translate
      where they can.
- [x] [Review][Patch] `saveRow` toasts the generic `details.failedOperation` message for every
      pre-check failure — an empty transfer number reports the execution-gating message, which is
      wrong and misleading [`hq-transfer-details.component.ts:2487-2504`] — surface which check
      failed (field-level) instead
- [x] [Review][Patch] Empty-state row hard-codes `colspan="9"` but the read-only grid renders 8
      columns (the 9th `th` is `canEdit`-gated) [`hq-transfer-details.component.html` empty-state
      row vs column definitions]
- [x] [Review][Patch] Lines sort lexicographically by `TransferNumber` (string sort: T1, T10, T2)
      so the serial column and the §22.S.3 grid disagree with the register's numbering
      [`HqTransferService.cs:364`] — order by insertion (`CreatedOn`) or natural sort

## Dev Notes

### Platform rules that bind this story

- Only `IUnitOfWork` saves; FluentValidation invoked in the service; typed DTOs only.
- Soft delete via the global query filter — lines never need manual `IsDeleted` checks.
- camelCase wire; no `FK_*` DTO keys; raw `{ message, errors }` envelope; no `ApiResponse<T>`.
- Country-scope guard identical to 17-3/17-4 — cross-scope access reads as 404.
- `bool? IsExecuted` models the two-option state dropdown (recorded above) — no Domain enum, no
  state lookup table.
- Per-row save is the spec's command shape (`SubmitTransfer(trans)`); do NOT build a bulk-save
  variant.

### «Failed Operation» (the spec's exception message)

The literal legacy string maps to the platform's 400 `{ message }` channel; the USER-VISIBLE
wording is i18n-owned (`hqTransfers.details.failedOperation`), the HTTP contract is the same
errors-map shape every reviewed module uses. The rule it guards is the state gating of AC 3.

### Out of scope

| Item | Owner |
| --- | --- |
| Header-field editing from this screen — §22.S.2 fields belong to `#/hq-transfers/:id/edit` | 17-4 |
| Line deletion / voiding | no use case in this epic — retro candidate if asked for |
| Printing / export of the details grid | epic 18 (Reports & Printing) |
| Notifications on execution-state change | none specified |

### References

- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.S.3] the screen contract — 5 row
  fields, 8-column grid, per-row حفظ, TransfersAdmin read-only gating
- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.U.8] scenario — including the
  «Failed Operation» exception flow
- [Source: _bmad-output/planning-artifacts/epics.md#3.17] US-TRF-08 acceptance criteria (8 rows,
  the fullest of the epic)
- [Source: Backend/src/IIROSA.Api/Controllers/OfficeProjectManagementController.cs:89-101]
  ValidationException → errors-map pattern
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] the parent entity and
  repository this child extends
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-9-register-a-mission-result.md] the reviewed
  event/child-write precedent (state-bearing sub-record save)

## Dev Agent Record

### Agent Model Used

GLM (glm-5) via Claude Code — dev-story workflow.

### Debug Log References

- Live battery 2026-08-24 against a private `dotnet run --no-build` instance on
  `http://localhost:5199` (started/stopped by this session; the user's own API process untouched).
- Login `OsamaSuper@IIROSA.com` (SuperAdmin, no country pin — list shows both seeded transfers).
- Test records: transfer `13fa8dce-…86eb` (OP-2026-001, country 1, 150000.5) carries the battery
  line `TRF-D-1001` (400 → executed with arrival data); left in place — that transfer is itself
  battery data from 17-2. Ceiling/transfer-B mutations were restored (B amount back to 99000,
  country 2 ceiling cleared to NULL).

### Completion Notes List

1. **Migration not authored here.** While this story's code was in flight, a parallel session's
   `20260824105001_Epic06_RetireConstructionHousing` swept the pending model changes into its own
   migration (its header documents carrying "epic-17's HqTransferDetail table and
   Country.MaxTransferAmount"). Verified applied in `__EFMigrationsHistory` and column-exact
   against `HqTransferDetailConfiguration`: `decimal(18,2)` amounts, `nvarchar(50)`
   TransferNumber, nullable `IsExecuted`, full audit columns, `Restrict` FK, both indexes. No
   duplicate migration was added — that would have produced an empty-diff migration.
2. **Repository shape.** `IHqTransferRepository.GetByIdWithLinesAsync` (dedicated
   `Include(Details)` read) added instead of widening `IncludeNavigationProperties` — the
   list/detail paths stay lean; the OfficeProject dedicated-read precedent.
3. **OnPush omitted** on the component — module-wide precedent (missions; every 17-x component
   ships without it). Recorded here rather than silently diverging from the task text.
4. **Failed-Operation UX is double-gated**: the arrival/execution inputs `disabled` until
   مصير الحوالة = تم التنفيذ (UX), and the validator rejects any smuggled value (the control) —
   live-proven: `ExecutionDate` with `IsExecuted=false` → 400 errors map, nothing written.
5. **Live battery results (all green)**: GET details 200 (header + lines, 0 initially) · PUT new
   line 200 echo with server id · update line to تم التنفيذ with execution/arrival data 200 and
   reflected on re-read · sum rule 400 naming both totals
   ("200400.00 exceeds 150000.50" — i.e. Σ existing 400 + incoming 200000) · gating 400 errors
   map · unknown transfer 404 · unknown line id 404 · anonymous 401.
6. **`stateLabel` reads translate at call time** (method, not pipe) — the reader-view badge stays
   correct on language switch without OnPush signaling concerns.
7. Screen contract deviations beyond the story text: the grid also carries a serial column
   (الرقم — module convention, 13-1 formula is overkill for one transfer's lines: local index+1)
   and the reader view renders state as a badge (تم التنفيذ green / else grey).

### File List

Backend:
- `Backend/src/IIROSA.Domain/Entities/HqTransferDetail.cs` (new — entity per the binding table)
- `Backend/src/IIROSA.Domain/Entities/HqTransfer.cs` (Details collection)
- `Backend/src/IIROSA.Domain/Configurations/HqTransferDetailConfiguration.cs` (new)
- `Backend/src/IIROSA.Domain/Interfaces/IHqTransferDetailRepository.cs` (new — empty repo pair)
- `Backend/src/IIROSA.Domain/Interfaces/IHqTransferRepository.cs` (GetByIdWithLinesAsync)
- `Backend/src/IIROSA.Infrastructure/Data/Repository/HqTransferDetailRepository.cs` (new)
- `Backend/src/IIROSA.Infrastructure/Data/Repository/HqTransferRepository.cs` (impl)
- `Backend/src/IIROSA.Infrastructure/ServiceCollectionExtensions.cs` (repository DI line)
- `Backend/src/IIROSA.Application/DTOs/HqTransfers/HqTransfers.cs` (3 DTOs)
- `Backend/src/IIROSA.Application/Validators/HqTransfers/SaveHqTransferDetailLineValidator.cs` (new)
- `Backend/src/IIROSA.Application/Profiles/HqTransferProfile.cs` (2 maps)
- `Backend/src/IIROSA.Application/Services/HqTransferService.cs` (read + upsert + sum rule)
- `Backend/src/IIROSA.Application/Interfaces/IHqTransferService.cs` (2 methods)
- `Backend/src/IIROSA.Api/Controllers/HqTransfersController.cs` (GET/PUT {id}/details)

Frontend:
- `Frontend/src/app/modules/hq-transfers/hq-transfer-details/` (4 files, new)
- `Frontend/src/app/modules/hq-transfers/hq-transfers-routing.module.ts` (:id/details route)
- `Frontend/src/app/modules/hq-transfers/hq-transfer-list/hq-transfer-list.component.ts` (nav)
- `Frontend/src/app/modules/hq-transfers/hq-transfer-list/hq-transfer-list.component.html` (icon)
- `Frontend/src/app/modules/hq-transfers/models/hq-transfer.model.ts` (3 interfaces)
- `Frontend/src/app/modules/hq-transfers/services/hq-transfer.service.ts` (2 methods)
- `Frontend/src/app/modules/lookup-management/models/lookup.model.ts` (CountryDto.maxTransferAmount
  — 17-6/17-7 carry surfaced by this session's build; the 17-7 screen reads it)
- `Frontend/src/assets/i18n/ar.json`, `en.json` (hqTransfers.details block, 18 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-TRF-08 and module spec §22.S.3 / §22.U.8; child-entity design, state-gating rule and sum-rule interpretation recorded. |
| 2026-08-24 | Implemented (backend vertical + §22.S.3 screen + i18n); migration carried by the parallel session's 20260824105001 (verified applied); live battery green; status → review. |
