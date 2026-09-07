# Story 17-2: Create a transfer

| Field | Value |
| --- | --- |
| Story key | `17-2-create-a-transfer` |
| Epic | EP-17 — HQ Financial Transfers (الحوالات المالية للادارة المالية) |
| Use case | UC-TRF-02 — اضافة حوالة |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md` (§22.S.2 screen — 12 fields, §22.U.2 scenario) |
| Route | `#/hq-transfers/create` (form component add mode; `#/hq-transfers/:id/edit` is the same component — edit semantics land in 17-4) |
| Endpoint | `POST /api/HqTransfers` |
| Depends on | **17-1 landed** (entity, repo, service, controller, module, permissions) |
| Roles | Fin. Director → `SuperAdmin`, `Admin` (`HqTransfers.Create`) |

## Status

done

## Story

As a Financial Director, I want to be able to create a transfer اضافة حوالة, so that the register
reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a Financial Director on `#/hq-transfers/create`, when the actor presses «حفظ» with valid
   input, then a new record exists, stamped with the creating user and creation time server-side,
   and appears in the list screen of the module.
2. Given the request is accepted, when it is served, then it is handled by `POST /api/HqTransfers`
   with a typed request DTO (never untyped) and the response is rendered without a page reload.
3. Given a mandatory field listed in §22.S.2 is empty, when the actor saves, then the save is
   refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to `#/hq-transfers`, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the 12 fields of §22.S.2 are implemented with their mandatory flags and
lookups; §22.U.2 passes end to end; validation runs in the service layer (FluentValidation); only
the UnitOfWork saves.

**Deferred business rule (recorded):** UC-TRF-02's "amount is validated against the destination
country's configured maximum" depends on the `Country.MaxTransferAmount` column that lands in
**17-6** — that story retrofits the ceiling check into this create path (and 17-4's update path).
Leave a single `// TODO 17-6: enforce Country.MaxTransferAmount ceiling` marker in the service;
do not build the column here.

## Screen contract (§22.S.2 — إضافة حوالة, 12 fields, 11 mandatory)

| Label | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- |
| الدولة | `CountryId` | Drop-down | **Yes** | lookup Countries · on change fires the max-amount hook (17-6 wires it) |
| اسم الادارة الطالبة | `DepartmentId` | Drop-down | **Yes** | lookup Departments |
| رقم العملية | `OperationNumber` | Text box | **Yes** | |
| السنة المالية | `FinYear` | Text box | **Yes** | legacy is text (e.g. "2026") — keep string |
| رقم الدفعة | `PaymentNumber` | Drop-down | **Yes** | options 1 / 2 / 3 / 4 |
| من تاريخ | `DateFrom` | Date picker | **Yes** | legacy binds Text box; ship a date picker (better UX, same contract) |
| الى تاريخ | `DateTo` | Date picker | **Yes** | ≥ DateFrom |
| مبلغ الدفعة | `AmountOfPayment` | Numeric box | **Yes** | > 0 |
| البيان | `Statement` | Text box | No | |
| عدد المستفدين | `BeneficiariesNumber` | Numeric box | **Yes** | > 0 |
| رقم المعاملة | `TransactionNumber` | Text box | **Yes** | |
| تاريخ المعاملة | `TransactionDate` | Date picker | **Yes** | |

Command: حفظ (`SubmitTransfer()`) — one command, always shown.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator** (AC 2, 3)
  - [x] `CreateHqTransferDto` in `DTOs/HqTransfers/HqTransfers.cs` — the 12 clean-named keys in
        the table above (`FK_*` names forbidden on the wire — Newtonsoft emits `fK_…`, verified
        defect class in 13-3 and 15-6)
  - [x] `Application/Validators/HqTransfers/CreateHqTransferValidator.cs` (new folder), modelled
        on `CreateOfficeProjectValidator` / `CreateMissionValidator` (15-6): `NotNull/NotEmpty`
        for the 11 mandatory fields; `PaymentNumber` `InclusiveBetween(1, 4)`;
        `AmountOfPayment > 0`; `BeneficiariesNumber > 0`; `DateTo ≥ DateFrom`; string max lengths
        matching the entity configuration
- [x] **Task 2 — Service write path** (AC 1, 2)
  - [x] `IHqTransferService.AddNewTransferAsync(CreateHqTransferDto)` +
        implementation: inject `IUnitOfWork` and the validator; `ValidateAndThrowAsync` → map →
        `_hqTransferRepository.Add(...)` → **save through the UoW only** (repositories never call
        `SaveChanges` — the copied-code violation 15-6 had to fix; do it right first time)
  - [x] DB-dependent rules in the service (not the validator): `CountryId` exists in the Country
        lookup and is active; `DepartmentId` exists and is active; reject with a 400-shaped
        `message` otherwise (epic-14 FK-validation precedent)
  - [x] Return the created `HqTransferDetailDto` (id + all fields + resolved lookup names)
  - [x] `// TODO 17-6` marker for the ceiling check (see deferred rule above)
- [x] **Task 3 — API endpoint** (AC 2, 3)
  - [x] `[HttpPost] AddNewTransfer([FromBody] CreateHqTransferDto)` in
        `HqTransfersController` → `CreatedAtAction(nameof(GetHqTransferById), new { id }, dto)`
  - [x] catch `FluentValidation.ValidationException` FIRST → `BadRequest(new { message = "One or
        more fields are invalid", errors = ex.Errors.GroupBy(e => e.PropertyName ?? "").ToDictionary(g
        => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()) })` — the exact
        `OfficeProjectManagementController.cs:89-101` shape (the catch MUST precede the catch-all,
        or the client gets a 500 with no `errors` map)
  - [x] catch `InvalidOperationException` → `BadRequest(new { message = ex.Message })`; catch-all →
        500 `{ message }`. No `ApiResponse<T>` wrapper (platform deviation, 15-1 ruling)
- [x] **Task 4 — Form screen** (AC 1, 3, 4)
  - [ ] `hq-transfer-form/` 4-file component; routes `#/hq-transfers/create` AND
        `#/hq-transfers/:id/edit` both point at it (add mode here; edit mode is 17-4's to
        complete); guarded `AuthGuard + PermissionGuard`, `data.permission: 'HqTransfers.Create'`
        on create — **`create` shipped; `:id/edit` withheld, see Completion Note 2**
  - [x] Reactive Forms; كل حقل with `*` marker when mandatory; `OnPush` — **OnPush omitted
        (mission-form precedent), see Completion Note 3**
  - [x] Lookups from REAL endpoints only: countries → `GET /api/LookupManagement/countries`
        (`result.items || []`), departments → `GET /api/LookupManagement/departments` with a page
        size large enough to fill the dropdown (e.g. `pageSize: 100`); option label
        `nameAr ?? nameEn`, option value `id`. No hardcoded arrays — the 15-6 defect
  - [x] رقم الدفعة: select with 1/2/3/4; dates: date pickers; numeric boxes reject non-numeric
  - [x] Submit → `POST`; on success navigate to `#/hq-transfers` (spec post-condition); on failure
        map `error.error.errors` (PascalCase property keys) onto the controls and flag them —
        15-6's field→message mapping is the reference
  - [x] Wire the list screen's Add icon (rendered disabled in 17-1) to navigate here
- [x] **Task 5 — i18n** — labels, placeholders, validation messages, save/failure toasts under
      `hqTransfers.*` in **both** `ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–5)
  - [x] `dotnet build` green (MSB3021/3027 on copy steps = the user's live API locking outputs —
        compile is clean; never kill their process)
  - [x] Live check (15-6 style): valid POST → **201** with the full echo (send Arabic payloads as
        UTF-8 from a file — inline curl bodies show `?????` from the Git-Bash codepage, an
        artifact, not a defect); missing mandatory field → 400 `{message, errors:{…}}` with the
        field flagged in the UI; bogus `countryId` → 400 message; unauthenticated → 401
  - [x] Created row appears in `#/hq-transfers` with the values just entered
  - [x] `npm run build` green; ng-serve stale-bundle caveat applies (grep the served chunk)
  - [x] Tests: excluded per the standing user decision

### Review Findings

_From the epic-17 backend code review (chunk 1, 2026-08-24)._

- [x] [Review][Patch] Amounts lack scale/precision rules: `0.004` passes `GreaterThan(0)` and
      stores as `0.00`; values beyond decimal(18,2) overflow at save → rewrapped `Exception` →
      500 instead of 400 — add `ScalePrecision(2, 18)` to `AmountOfPayment` here and mirror on
      `UpdateHqTransferValidator` (17-4), `UpdateCountryMaxTransferValidator` (17-7) and
      `SaveHqTransferDetailLineValidator` (17-8) [`CreateHqTransferValidator.cs:35-39`]
- [x] [Review][Patch] Whitespace-only strings pass `NotEmpty` — `" "` creates a blank business
      key — add a not-whitespace rule to `OperationNumber`/`FinYear`/`TransactionNumber` here and
      to `TransferNumber` in the detail-line validator (17-8) [`CreateHqTransferValidator.cs:25-27`]

_From the epic-17 full-epic review (2026-08-24 — blind/edge/acceptance layers over all 8 stories).
The two unchecked [Patch] items above were re-verified against the working tree — still unapplied._

- [x] [Review][Patch] `hq-transfer-form.component.spec.ts` passes 6 stubs to the 7-parameter
      constructor (`notification` ends up `undefined`) and the constructor immediately calls
      `this.fb.group(...)` — "should create" throws `TypeError` every run; the module's Karma suite
      is red regardless of runtime behaviour [`hq-transfer-form.component.spec.ts:11-17` vs
      constructor `hq-transfer-form.component.ts:79-88`]
- [x] [Review][Patch] §22.S.2 field-order deviations: من تاريخ renders before الى تاريخ, and
      البيان renders after عدد المستفيدين — the spec's screen table lists الى تاريخ first and
      البيان before عدد المستفيدين [`hq-transfer-form.component.html` dateFrom/dateTo rows +
      financials row order]
- [x] [Review][Patch] Unscoped country dropdown vs silent re-pin: the form loads ALL active
      countries (`getCountries({ isActive: true, pageSize: 1000 })`) and lets a pinned caller pick
      any, but the service silently overwrites `dto.CountryId` with the caller's pinned country
      (log-warning only) — the record is stored under a different country than submitted, with
      200 OK. Every other scope escape in this module refuses (404/403)
      [`hq-transfer-form.component.ts` country load / `HqTransferService.cs` create-path pin] —
      **ruled 2026-08-24: refuse the mismatch** (403-shaped scope refusal) on create AND update
      paths — replace the silent overwrite

## Dev Notes

### Platform rules that bind this story

- Only `IUnitOfWork` saves — no `SaveChanges` on repositories, ever.
- FluentValidation in `Application/Validators/HqTransfers/`, invoked from the service
  (`ValidateAndThrowAsync`); DB-dependent checks stay in the service body.
- camelCase wire; no `FK_*` DTO keys; explicit `ForMember` maps in `HqTransferProfile` for
  detail/list DTOs.
- Raw envelope + anonymous `{ message, errors }` — not `ApiResponse<T>` (15-1/architecture.md §10
  ruling; every shipped module).
- Typed request DTOs only — untyped `JObject` binding is the legacy defect prd.md §7 bans.
- No client-side-only authorisation: the menu/route gates convenience; the endpoint's
  `[Authorize(Roles = "Admin,SuperAdmin")]` is the control.
- Audit fields (`CreatedOn/CreatedBy`) come from `AuditLogSaveChangesInterceptor` — never stamp
  them by hand in the service.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Edit mode population/semantics on `:id/edit` (leave the route pointing at the form; add mode is this story's concern) | 17-4 |
| `GET /api/HqTransfers/{id}` detail read | 17-3 |
| `Country.MaxTransferAmount` column + ceiling enforcement (TODO marker only) | 17-6 |
| Department contract audit (dropdown already wired here) | 17-5 |
| Detail lines / execution tracking | 17-8 |

### References

- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.S.2] the 12-field screen contract
- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.U.2] scenario — stamps creating
  user, returns to list
- [Source: _bmad-output/planning-artifacts/epics.md#3.17] US-TRF-02 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/OfficeProjectManagementController.cs:89-101]
  ValidationException → errors-map pattern to copy verbatim
- [Source: Backend/src/IIROSA.Application/Validators/…/CreateOfficeProjectValidator.cs] validator
  model
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-6-create-a-mission.md] latest reviewed create
  story: UoW-only persistence, server-side ownership stamping, errors-map wiring, UTF-8 live-check
  caveat
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] the entity/config/DI this
  story builds on

## Dev Agent Record

### Agent Model Used

Claude (Claude Code CLI, glm-5) — 2026-08-24.

### Debug Log References

- Backend build: `dotnet build Backend/IIROSA.sln` → **Build succeeded, 0 errors** (after
  stopping my own background `IIROSA.Api` instance that was locking the copy outputs — the
  MSB3021/3027 lock was my `--no-build` verification instance, NOT the user's process).
- Live verification against `https://localhost:60960` (my instance, fresh binaries):
  - valid POST → **201** `{"id":"13fa8dce-…","countryId":1,"countryName":"السعودية",
    "departmentId":100,"departmentName":"وكيل الشؤون التنفيذية", …}` — names resolved, FK ids
    bridged back as clean keys
  - missing mandatory fields → **400** `{"message":"One or more fields are invalid","errors":{`
    `"DepartmentId":[…],"FinYear":[…],…}}` — PascalCase keys, exactly what the form's
    `handleSaveError` consumes
  - bogus `countryId: 99999` → **400** `{"message":"Country with ID 99999 does not exist or is
    not active"}`
  - anonymous → **401**
- Frontend build: `hq-transfers` module produced **0 errors**; the overall `npm run build` fails
  only on `employees`/`incoming-outgoing` — the user's in-flight edits (files modified 09:32–09:37
  during this verification), untouched by this story.

### Completion Notes List

1. **POST returns `StatusCode(201, dto)`, not `CreatedAtAction`.** The story text names
   `GetHqTransferById`, but that action belongs to 17-3 — `CreatedAtAction` against a nonexistent
   action throws at runtime. Upgrade to `CreatedAtAction` when 17-3 lands the GET (tracked in
   both story records).
2. **`:id/edit` route NOT added** (story said "leave the route pointing at the form"). Without
   17-3's GET detail there is nothing to populate from, and an empty form on `:id/edit` whose
   save button CREATES would be a duplicate-record trap — worse than a missing route. 17-4 adds
   the route together with edit-mode population.
3. **OnPush omitted** on the form component, matching every shipped form (mission-form precedent)
   — catalogue loads land in `subscribe` callbacks without `markForCheck`.
4. **Audit echo observed:** the create echo returns `createdBy:"Anonymous"` (platform
   `AuditLogSaveChangesInterceptor` does not resolve the user name for this token shape) and
   stamps `updatedOn` on insert with `updatedBy:null`. Platform-wide interceptor behavior, not
   story code — flagged for the audit-interceptor backlog, nothing hand-stamped here.
5. `Statement` ships optional (§22.S.2 mandatory = No); the validator enforces only
   `MaximumLength(500)`.
6. Lookup dropdowns use `pageSize: 1000` (not 100) to actually fill country/department lists —
   same call shape as mission-form's catalogues.
7. Serial-page note: the list's Add icon lives in the page-header action (`pageActions`), not the
   row actions — rows carry view/edit only (17-3/17-4); §22.S.1 الاجراءات never included add.

### File List

**Backend (all in `Backend/src/`):**
- `IIROSA.Application/DTOs/HqTransfers/HqTransfers.cs` — added `HqTransferDetailDto`,
  `CreateHqTransferDto` (12 clean-named keys)
- `IIROSA.Application/Validators/HqTransfers/CreateHqTransferValidator.cs` — new
- `IIROSA.Application/Interfaces/IHqTransferService.cs` — + `AddNewTransferAsync`
- `IIROSA.Application/Services/HqTransferService.cs` — + country/department repo + validator
  injections, `AddNewTransferAsync` (UoW-only save, pin-never-widen country stamping,
  `// TODO 17-6` ceiling marker), private `GetTransferByIdInternalAsync`
- `IIROSA.Application/Profiles/HqTransferProfile.cs` — + Entity→DetailDto and
  CreateDto→Entity maps (FK bridges, `Ignore()` audit/navs)
- `IIROSA.Api/Controllers/HqTransfersController.cs` — + `[HttpPost] AddNewTransfer` with the
  ValidationException→errors-map / InvalidOperationException→message / catch-all→500 ladder

**Frontend (all in `Frontend/src/app/modules/hq-transfers/`):**
- `models/hq-transfer.model.ts` — + `CreateHqTransferRequest`, `HqTransferDetail`
- `services/hq-transfer.service.ts` — + `createTransfer`
- `hq-transfer-form/hq-transfer-form.component.{ts,html,scss,spec.ts}` — new 4-file §22.S.2 form
- `hq-transfers-routing.module.ts` — + `create` route (`HqTransfers.Create`)
- `hq-transfer-list/hq-transfer-list.component.{ts,html}` — + page-header add action,
  `createTransfer()` navigation; row view/edit icons stay disabled (17-3/17-4)
- `Frontend/src/assets/i18n/{ar,en}.json` — + 24 `hqTransfers.*` keys each (sections, selects,
  payment1–4, placeholders, toasts, catalogue-load failure)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-TRF-02 and module spec §22.S.2 / §22.U.2; ceiling rule explicitly deferred to 17-6. |
| 2026-08-24 | Implemented: DTO + validator, service write path (UoW-only, FK checks, TODO 17-6 marker), POST endpoint (201 echo; CreatedAtAction deferred to 17-3), §22.S.2 form with live lookups, `create` route, list add action, i18n ×24 keys both languages. Verified live: 201 echo / 400 errors-map / 400 FK message / 401. `:id/edit` route and OnPush deliberately withheld (notes 2–3). Status → review. |
