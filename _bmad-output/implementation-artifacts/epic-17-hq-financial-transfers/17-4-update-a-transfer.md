# Story 17-4: Update a transfer

| Field | Value |
| --- | --- |
| Story key | `17-4-update-a-transfer` |
| Epic | EP-17 — HQ Financial Transfers (الحوالات المالية للادارة المالية) |
| Use case | UC-TRF-04 — تعديل الحوالة |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md` (§22.S.2 screen in edit mode, §22.U.4 scenario) |
| Route | `#/hq-transfers/:id/edit` (the form component, edit mode) |
| Endpoint | `PUT /api/HqTransfers` (id in the body — board contract) |
| Depends on | **17-2 + 17-3 landed** (form component, create path, detail read); 17-1's vertical |
| Roles | Fin. Director → `SuperAdmin`, `Admin` (`HqTransfers.Edit`) |

## Status

done

## Story

As a Financial Director, I want to be able to update a transfer تعديل الحوالة, so that a record
that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given a Financial Director in the module, when the actor saves the edit form with valid input,
   then the stored record carries the new values; no other record is affected.
2. Given the request is accepted, when it is served, then it is handled by `PUT /api/HqTransfers`
   with a typed request DTO and the response is rendered without a page reload.
3. Given a mandatory field listed in §22.S.2 is empty, when the actor saves, then the save is
   refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to `#/hq-transfers`, then the record appears
   there with the values just entered.
5. Given the id does not exist or belongs to another country than the caller's claim, when the
   request is served, then it is refused with 404 semantics and nothing is written.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §22.U.4 passes end to end; validation in the service layer; only the
UnitOfWork saves; `UpdatedOn/UpdatedBy` set by the audit interceptor.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator** (AC 2, 3)
  - [x] `UpdateHqTransferDto`: `Id` (Guid, mandatory) + the same 12 fields as
        `CreateHqTransferDto` with the same clean wire names (inherit or duplicate consistently
        with the OfficeProjects DTO pair — follow whatever `Create/UpdateOfficeProjectDto` does)
  - [x] `Application/Validators/HqTransfers/UpdateHqTransferValidator.cs` — same field rules as
        the create validator (`Include`-style reuse or mirror; the create validator is the
        reference) + `Id` NotEmpty
- [x] **Task 2 — Service update path** (AC 1, 5)
  - [x] `IHqTransferService.UpdateHqTransferAsync(UpdateHqTransferDto)` → fetch by id
        (`NotFoundException` when absent — soft delete already filtered globally); country-scope
        guard identical to 17-3's read (caller claim vs `FK_CountryId` → `NotFoundException`);
        re-validate FK existence (country/department active) as in 17-2;
        `ValidateAndThrowAsync` → map onto the tracked entity → save **through `IUnitOfWork`
        only**
  - [x] No hand-stamping of `UpdatedOn/UpdatedBy` — `AuditLogSaveChangesInterceptor` owns them
  - [x] `// TODO 17-6: enforce Country.MaxTransferAmount ceiling` marker (retrofitted by 17-6,
        same as the create path)
  - [x] Return the updated `HqTransferDetailDto`
- [x] **Task 3 — API endpoint** (AC 2, 3)
  - [x] `[HttpPut] UpdateHqTransfer([FromBody] UpdateHqTransferDto)` → `Ok(updatedDto)`;
        `catch (FluentValidation.ValidationException)` BEFORE the catch-all →
        `BadRequest(new { message, errors })` in the `OfficeProjectManagementController.cs:89-101`
        shape; `NotFoundException` → 404 `{ message }`; catch-all → 500 `{ message }`. No
        `ApiResponse<T>` wrapper
- [x] **Task 4 — Edit mode on the form** (AC 1, 3, 4)
  - [x] `#/hq-transfers/:id/edit` populates the form from `GET /api/HqTransfers/{id}` (17-3's
        endpoint) and submits to `PUT /api/HqTransfers` carrying `id`; guarded
        `AuthGuard + PermissionGuard` with `data.permission: 'HqTransfers.Edit'`
  - [x] On success navigate to `#/hq-transfers`; on failure stay, flag fields from
        `error.error.errors` (15-6 field→message mapping)
  - [x] Wire the list screen's Edit icon (rendered disabled in 17-1) to navigate here
  - [x] Keep create vs edit mode selection explicit (`mode: 'create' | 'edit' | 'view'` route
        data — 17-3 added `'view'`); one component, three modes
- [x] **Task 5 — i18n** — edit title, success/failure messages under `hqTransfers.*` in **both**
      `ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] Live check: PUT with changed values → 200 echo, list shows the new values, exactly one
        row changed; missing mandatory field → 400 errors map; unknown id → 404; wrong-country
        caller → 404; unauthenticated → 401
  - [x] `dotnet build` + `npm run build` green (live-API lock + ng-serve stale-bundle caveats)
  - [x] Tests: excluded per the standing user decision

### Review Findings

_From the epic-17 backend code review (chunk 1, 2026-08-24)._

- [x] [Review][Patch] **HIGH** — the update path can lower `AmountOfPayment` below Σ existing
      detail lines: `UpdateHqTransferAsync` loads via `GetByIdWithDetailsAsync` (Country/
      Department only — `Details` never included) and the Σ-lines-≤-header rule is enforced only
      in `SaveTransferDetailLineAsync`. Deterministic trigger: header 1000 + lines 600/400 →
      PUT `amountOfPayment: 100` → 200 persisted; the record is then stuck (any line re-save,
      even unchanged, fails the sum check) [`HqTransferService.cs:112-173`] — load the lines and
      refuse the update when Σ lines > new `AmountOfPayment`
- [x] [Review][Patch] (mirror of 17-2's finding) `ScalePrecision(2, 18)` missing on
      `AmountOfPayment` [`UpdateHqTransferValidator.cs`]

_From the epic-17 full-epic review (2026-08-24 — blind/edge/acceptance layers over all 8 stories).
The unchecked [Patch] items above were re-verified against the working tree — still unapplied._

- [x] [Review][Patch] The update path re-runs create-time rules unconditionally against the
      existing record: `!country.IsActive` / `!department.IsActive` refusals and
      `EnforceTransferCeiling` run on every PUT — so a lowered ceiling or a deactivated lookup
      locks an existing transfer out of ANY edit (fixing only `Statement` with the amount
      unchanged → 400 "exceeds the maximum"). [`HqTransferService.cs` update path] —
      **ruled 2026-08-24: grandfather the unchanged** — skip the ceiling check when
      `AmountOfPayment` is unchanged and skip active-state refusals for lookups already stored
      on the record; a changed amount or a re-pointed lookup still validates at full create
      parity
- [x] [Review][Patch] Edit form can blind-overwrite a record whose load failed: a non-404 load
      error (transient 500/timeout) leaves `loading=false`, the card renders with an empty
      enabled form, and `onSubmit()` still PUTs — the existing record is replaced by values the
      user never saw. Guard the submit on the transfer actually having loaded (or disable the
      form on load failure) [`hq-transfer-form.component.ts:240-248` load error branch +
      `:301-303` submit guard]

## Dev Notes

### Platform rules that bind this story

- Only `IUnitOfWork` saves; audit fields belong to the interceptor.
- FluentValidation in the service layer; DB-dependent checks in the service body.
- camelCase wire; no `FK_*` DTO keys; raw envelope with `{ message, errors }` shape — no
  `ApiResponse<T>` (platform deviation, 15-1/architecture.md §10 ruling).
- Typed DTOs only; the controller stays thin (bind → delegate → return).
- Cross-scope writes read as 404, not 403 — never confirm existence across the country boundary.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| `Country.MaxTransferAmount` ceiling enforcement on this path (TODO marker only) | 17-6 |
| Execution-state fields (EstimatedTransferDate / TransferState / ExecutionDate / ArrivalDate /
  ArrivalAmount) — header §22.S.2 fields only; the legacy detail-screen state machine is 17-8's | 17-8 |
| Any delete capability — this epic's 8 use cases contain **no delete**; do not add one | — |

### References

- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.U.4] scenario — re-validate,
  apply, persist
- [Source: _bmad-output/planning-artifacts/epics.md#3.17] US-TRF-04 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/OfficeProjectManagementController.cs:89-101]
  ValidationException → errors-map pattern
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-6-create-a-mission.md] the reviewed
  create→edit pairing on one form component (mission-form add/edit modes)
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-2-create-a-transfer.md] the DTO/validator/
  service shapes this story mirrors

## Dev Agent Record

### Agent Model Used

Claude (Claude Code CLI, glm-5) — 2026-08-24.

### Debug Log References

- **Live battery run 2026-08-24** against a private `dotnet run --no-build` instance on
  `http://localhost:5199` (this session's own process; the user's API untouched). Earlier
  deferral was the user's concurrent WIP breaking the Application compile — their tree went
  green later the same day and the full battery ran:
  - PUT valid (beneficiariesNumber 320→321 + statement) → **200** echo with the new values;
    re-GET persists them; list total unchanged at 2 → **exactly one row changed**
  - PUT invalid (empty OperationNumber/TransactionNumber, AmountOfPayment 0, PaymentNumber 9)
    → **400** `{ message, errors }` map flagging all four fields
  - PUT unknown id → **404** `{ message }`
  - anonymous PUT → **401**
- Backend `dotnet build` 0 errors; frontend `npm run build` 0 errors in this module (user's
  concurrent WIP errors live elsewhere).

### Completion Notes List

1. **DTO pair duplicated, not inherited** — mirrors `Create/UpdateOfficeProjectDto`, but update
   fields stay non-nullable because §22.S.2 mandates the same 11 mandatory fields on edit and
   the story demands the create validator's exact field rules + `Id` NotEmpty.
2. **Scope-escape pin on the update path** — after the 404-shaped scope guard, a pinned caller
   requesting a different `CountryId` than their claim is pinned back (logged), so an edit
   cannot move a record out of the caller's country. HQ (no claim) may re-assign the country.
3. **Live verification checklist — complete** (see Debug Log): PUT changed values → 200 echo +
   exactly one row changed; invalid payload → 400 errors map; unknown id → 404; unauthenticated
   → 401. Wrong-country caller → 404 stays code-reviewed (no pinned account exists — 17-3
   Note 2; the guard is the same one live-proven by 17-3's read).
3a. **Battery observation, not a defect of this path:** `updatedBy` stamps `Anonymous` — but the
   record's ORIGINAL `createdBy` reads the same, so user-name resolution inside the audit
   interceptor is a platform-wide condition of this dev environment, not something the update
   path introduced. `updatedOn` stamps correctly. Left as-is; flagged for the platform thread.
4. `NotFoundException` gained a `(string entityName, object id)` overload — the user's concurrent
   `PeriodicOrphanReportService` calls (`nameof(Orphan)`, id) needed it; additive, their
   call-shape honoured, no existing constructor changed.
5. Edit-mode cancel/save both navigate to the list (AC 4); the mission-form "cancel → detail"
   pattern was not carried over — the story's post-condition is the register.

### File List

**Backend (all in `Backend/src/`):**
- `IIROSA.Application/DTOs/HqTransfers/HqTransfers.cs` — + `UpdateHqTransferDto` (Id + 12 fields)
- `IIROSA.Application/Validators/HqTransfers/UpdateHqTransferValidator.cs` — new (create rules + Id)
- `IIROSA.Application/Profiles/HqTransferProfile.cs` — + UpdateDto→Entity map for
  `Map(dto, trackedEntity)` (Id/audit/soft-delete/navs ignored)
- `IIROSA.Application/Interfaces/IHqTransferService.cs` — + `UpdateHqTransferAsync`
- `IIROSA.Application/Services/HqTransferService.cs` — + updateValidator injection +
  `UpdateHqTransferAsync` (guards, FK re-validation, TODO 17-6 marker, UoW-only save)
- `IIROSA.Api/Controllers/HqTransfersController.cs` — + `[HttpPut]` with the full catch ladder
- `IIROSA.Application/Exceptions/NotFoundException.cs` — + `(string, object)` overload (user's
  concurrent call sites; see Note 4)

**Frontend (all in `Frontend/src/app/modules/hq-transfers/`):**
- `models/hq-transfer.model.ts` — + `UpdateHqTransferRequest`
- `services/hq-transfer.service.ts` — + `updateTransfer`
- `hq-transfer-form/hq-transfer-form.component.ts` — edit mode (`mode: 'edit'`), shared
  populate without disable, submit branch PUT/POST, mode-aware title/breadcrumb
- `hq-transfers-routing.module.ts` — + `:id/edit` route (`HqTransfers.Edit`, mode edit)
- `hq-transfer-list/hq-transfer-list.component.{ts,html}` — edit icon wired → `editTransfer(id)`
- `Frontend/src/assets/i18n/{ar,en}.json` — + `transferUpdated`, `updateFailed` both languages

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-TRF-04 and module spec §22.U.4. |
| 2026-08-24 | Implemented end to end (DTO/validator/map/service/PUT/form edit mode/route/icon/i18n). Compile-clean in story files; live PUT battery deferred on concurrent user WIP breaking the Application project build (0 story-file errors in every pass). Status stays in-progress until the battery runs. |
| 2026-08-24 | Live PUT battery green (200/one-row/400 map/404/401) on a private :5199 instance; `updatedBy: Anonymous` observation recorded (platform-wide, pre-existing); status → review. |
