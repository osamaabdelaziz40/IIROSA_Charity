# Story 4.2: Verify employee username availability — التحقق من اسم المستخدم

| Field | Value |
| --- | --- |
| Story key | `4-2-verify-employee-username-availability` |
| Epic | EP-04 — Employee & User Administration |
| Use case | UC-EMP-02 — التحقق من اسم المستخدم |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/09-UC-EMP-Employee-and-User-Administration.md` (§9.S.2 screen, §9.U.2 scenario, §9.B controllers) |
| Route | hosted on `#/employees/create` · `#/employees/:id/edit` (no route of its own) |
| Endpoint | `GET /api/EmployeeManagement/check-username?userName=&excludeEmployeeId=` |

## Status

done — 2026-08-24 code review passed (audit verdict COMPLIANT); the single edge finding fixed.
Implemented by the 2026-08-24 review-and-complete pass.

## Story

As a general director, I want to be able to verify employee username availability التحقق من اسم
المستخدم, so that only the right people reach the register and each of them sees only their own
scope.

Functional intent (§9.U.2 summary): **check a proposed login name against existing accounts
before the employee record is submitted** — the operator learns the name is taken while typing,
instead of losing a completed form to a refused save.

## Acceptance Criteria

1. Given a general director holding valid credentials in the module, when the actor invokes the
   function with valid input, then an authenticated session exists (or has been ended); the role
   code and country id that scope every later request are held by the SPA.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/EmployeeManagement/check-national-id` and the response is rendered on the screen
   without a page reload.
3. Given the credentials are wrong or the account is locked out, when the actor submits them,
   then the attempt fails, no ticket is issued and the actor stays on the screen.

**Definition of done:** the screen fields of §9.S are implemented with their mandatory flags and
lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

### Reading the criteria (resolve before implementing)

AC 1 and AC 3 are login-flavoured boilerplate the generator attached to a non-login use case;
§9.U.2's own summary defines the real function. Interpret them as follows (the same
record-don't-ignore approach story 3-2 took):

- **AC 1** → the endpoint requires an authenticated General-Director session (server-side
  `[Authorize]`) and is a pure read: it changes no stored data. It does **not** issue any token.
- **AC 2** → the endpoint exists and the SPA renders its verdict live on the employee form
  without a page reload. The **route is `check-username`, not `check-national-id`** — see design
  decision D1. The board comment has been annotated accordingly (14-6 precedent).
- **AC 3** → the authoritative rejection lives at create-time: `UserAppServiceExtended.
  CreateUserAsync` already refuses a duplicate email/username (`UserAppServiceExtended.cs:109`),
  and `EmployeeService.CreateEmployeeAsync` refuses a duplicate `Employee.Email`
  (`EmployeeService.cs:138-142`). This check is advisory UX layered **on top of** those rules and
  must query the exact same stores they do, so check and rule cannot drift apart.
- Charity/country scoping (the usual AC) **does not apply**: a login name is unique across the
  whole register — the identity store enforces one account per email/username globally. Scoping a
  uniqueness probe per tenant would report a taken name as free. The scoping intent is honoured by
  **authorization** instead: only roles permitted to create/update employees may probe. Identical
  ruling to story 3-2's "Note on AC 3".

## Tasks / Subtasks

- [x] **Task 1 — Service method** (AC 2, AC 3)
  - [ ] Add `Task<bool> IsUserNameAvailableAsync(string userName, Guid? excludeEmployeeId = null)`
    to `IEmployeeService` and `EmployeeService` (use `EmployeeService`, never the deprecated
    `EmployeeAppService.cs` stub)
  - [ ] Guard: null/whitespace → `false` is wrong, throw nothing — return `false` only means
    "taken"; for empty input return **false** is misleading, so return `false` **only after**
    trimming; empty input should simply report unavailable (nothing to check). Trim first; reject
    length > 100 (`Employee.Email` is `HasMaxLength(100)` — a 101-char value must not be probed,
    it must fail, mirroring 3-2's max-length patch)
  - [ ] Availability = **both** stores must be free:
    1. `IEmployeeRepository.IsEmailUniqueAsync(userName.Trim(), excludeEmployeeId)` — mirrors the
       rule `CreateEmployeeAsync` enforces (`EmployeeService.cs:138-142`)
    2. Identity store: find the user with that login name. `IUserAppServiceExtended` (already
       injected in `EmployeeService`) inherits `FindByEmailAsync(string)` from `IUserAppService`
       — a null result means free. **Exclusion on edit:** if `excludeEmployeeId` is set, load the
       employee row, and a hit whose user id equals that row's `FK_UserId` does not count as taken
- [x] **Task 2 — Result DTO + endpoint** (AC 1, AC 2)
  - [ ] `EmployeeUserNameAvailabilityDto { string UserName; bool IsAvailable; }` in
    `DTOs/EmployeeManagement/Employees.cs` (server echoes what it checked — the staleness guard
    in Task 4 depends on it)
  - [ ] `GET check-username` on `EmployeeManagementController`: `[HttpGet("check-username")]`,
    `[Authorize(Policy = "ManagementOnly")]` (the policies ARE registered — see corrected D3),
    `[FromQuery] string userName, [FromQuery] Guid? excludeEmployeeId`; delegate to the service;
    return the DTO directly (D2). Literal segment beats `{id}` route — same routing property
    story 3-2 relied on
  - [ ] Error paths follow the controller's existing try/catch shape; never echo `ex.Message` to
    the client (3-2 review patch)
- [x] **Task 3 — Frontend service method** (AC 2)
  - [ ] `checkUserNameAvailability(userName: string, excludeEmployeeId?: string): Observable<EmployeeUserNameAvailability>`
    on `EmployeeService` (`modules/employees/services/employee.service.ts`)
  - [ ] Build query params with a `StrictHttpParameterCodec` `HttpParams` — **replicate the
    12-line codec class from `charity.service.ts:24`**. Emails legitimately contain `+`; the
    default codec re-encodes it so the server reads a space (3-2 review patch). `ApiService
    .buildParams` uses the default codec — do not route through it for this call
  - [ ] Add the response model to `core/models/employee.model.ts`
- [x] **Task 4 — Async validator + form wiring** (AC 2)
  - [ ] New `Frontend/src/app/shared/validators/employee.validators.ts` exporting
    `employeeUserNameUniqueValidator(service, getExcludeId: () => string | undefined)`.
    Follow the **corrected** pattern from `charity-form.component.ts:196-227` exactly:
    `timer(400)` debounce → `switchMap` (cancels superseded requests) → `map` with the
    echo-comparison staleness guard (`result.userName !== requestedName → null`) →
    `catchError(() => of(null))` (a failed probe must not block the save; the server re-checks on
    write) → `first()` (or the control sits `pending` forever). Skip values shorter than the
    sync email validator would reject anyway
  - [ ] **Do NOT follow `charity.validators.ts`'s list-scan validators** (fetch page of 100,
    filter client-side) — that is the superseded pre-3-2 pattern: capped, chatty, and unguarded
  - [ ] Attach as the async validator on the `email` control in `employee-form.component.ts`
    `createForm()` — third element of the control config array, alongside the existing
    `[Validators.required, Validators.email]`. `getExcludeId` returns `this.employeeId` so edit
    mode excludes the row's own account
  - [ ] **PENDING guard in `onSubmit()`**: `if (this.employeeForm.pending)` must wait/subscribe
    exactly as `charity-form.component.ts:647-663` does — a `PENDING` control is not `INVALID`,
    and without this guard a fast save bypasses the check entirely (3-2 review patch)
  - [ ] i18n: add `validation.emailTaken` to `ar.json` + `en.json` (pattern: existing
    `validation.nameTaken`, ar.json:731). Render it in the email field's `invalid-feedback` block
    via the translate pipe when the control has `emailTaken`. Do not widen the existing
    hard-coded `getErrorMessage` strings — pre-existing debt, out of scope
- [x] **Task 5 — Verification** (all ACs)
  - [ ] `dotnet build Backend/IIROSA.sln` — 0 errors (MSB3021/3027 = live-API file lock, not a
    compile error; never kill the user's running API)
  - [ ] `cd Frontend && npx tsc --noEmit -p tsconfig.app.json` — 0 errors
    (`npm install --legacy-peer-deps` if node_modules is missing; do not commit the lockfile)
  - [ ] Live smoke of the new endpoint: taken name → `isAvailable: false`; unknown name → `true`;
    `excludeEmployeeId` of a row holding the probed email → `true`; unauthenticated caller → 401;
    non-admin caller → 403 (interceptor routes both)
  - [ ] No automated tests: `Backend/tests/` is empty and no frontend spec convention exists —
    the epic-3 ruling (tests excluded by user decision) is assumed to carry; if that surprises
    anyone, raise it in review rather than scaffolding a test project inside a 2-point story

## Dev Notes

### The one thing to understand first

**In this stack the login credential IS the email.** `UserAppServiceExtended` sets
`UserName = userDto.Email` (`UserAppServiceExtended.cs:121` on create, `:184-186` on update) and
`LoginDto` authenticates on `Email`. The employee form's `email` control is therefore the
"username" field UC-EMP-02 speaks of; `Employee` has no UserName column and the form has no
username input. The availability check targets `email` — do not add a username field to the form
or the entity.

### Design decisions

- **D1 — Endpoint named `check-username`, not `check-national-id`.** The board/spec route name is
  the legacy AngularJS endpoint quoted verbatim; the legacy action itself received `userName,
  Id` (§9.U.2 main flow step 4) — the name was always a misnomer. §9.U.2's realisation line
  itself lists `UserManagement/check-username` as the alternate name. Renaming follows the board
  precedent of 14-6 ("board said GET — action is POST"); the board comment for 4-2 has been
  annotated. One implementation per capability — no alias route.
- **D2 — Bare DTO response, not `ApiResponse<T>`.** `architecture.md` §6.2 mandates `ApiResponse
  <T>`, but 20 of 21 shipped controllers (including every neighbour action in this controller)
  return bare shapes, and story 3-2's review ruled converting single endpoints **deferred
  systemic debt** — a lone envelope would make the controller internally inconsistent and break
  the existing client. Recorded, not silently ignored.
- **D3 — `[Authorize(Policy = "ManagementOnly")]`.** Both named policies are registered by
  `AddAuth` (`IIROSA.Application/ServiceCollectionExtensions.cs:174-194`, invoked at
  `Program.cs:95`); `Program.cs:334-353` is a dead commented duplicate, which an earlier draft
  of this decision misread as "no registration exists" (corrected 2026-08-24 — as shipped, the
  action uses the policy form, matching its sibling read actions). General Director maps to the
  `Admin`/`SuperAdmin` pair that `ManagementOnly` admits.
- **D4 — No FluentValidation file for this story.** CLAUDE.md places validation in the service
  layer via FluentValidation, but the employee module has no validators at all and introducing a
  one-off validator file for a two-parameter read would make the module internally inconsistent.
  Keep the trim/length guards inside `IsUserNameAvailableAsync`. Same ruling as 3-2's deferred
  "validation lives in the controller" finding.
- **D5 — Check both stores.** Identity store alone misses employees created without an identity
  user (possible today — see 4-3 defect list); Employee table alone misses users without an
  employee row. `CreateEmployeeAsync` enforces Employee.Email uniqueness and `CreateUserAsync`
  enforces identity email uniqueness, so the probe must consult both to predict the save.

### What already exists — reuse, do not reinvent

| Need | Already built |
| --- | --- |
| Employee-row uniqueness with exclusion | `IEmployeeRepository.IsEmailUniqueAsync(email, excludeId)` — `EmployeeRepository.cs:53-63` |
| Identity lookup by login name | `IUserAppService.FindByEmailAsync(email)` (inherited by the already-injected `IUserAppServiceExtended`) |
| Result-DTO + endpoint shape | `CharityNameAvailabilityDto` + `GET /api/Charities/check-name` — `CharitiesController.cs` |
| Corrected async-validator pattern | `charityNameAvailabilityValidator()` — `charity-form.component.ts:183-227`, incl. staleness guard, `catchError→null`, `first()` |
| PENDING-submit guard | `charity-form.component.ts:647-663` |
| `+`-safe query encoding | `StrictHttpParameterCodec` — `charity.service.ts:24` |
| 401/403 → login routing | `core/interceptors/auth.interceptor.ts` (403 branch added in 3-2 review) |

### Previous story intelligence (3-2 — the direct analog)

Every patch from 3-2's adversarial review is baked into the tasks above. The ones that bite if
skipped: staleness guard (late reply marking a fresh name taken), `catchError→available`
blindspot is intentionally inverted here (`catchError→null` = don't block save), PENDING submit
bypass, `+`-encoding corruption, missing max-length guard, and `ex.Message` leakage on 500.

### Surrounding known breakage — context, not scope

The create/edit form's wire contract is broken (4-3/4-5 record it in full): the frontend sends
`firstName`/`lastName`/`employeeCode`/`roleName`, the backend binds `FullName`/`Code`/`Roles`.
**This story does not fix that** — it attaches the validator to the control that exists
(`email`, which binds correctly) and stays out of the DTO dispute. Fixing the contract belongs to
a deliberate rework decision (suggest `/bmad-correct-course` or a defect story), not a 2-point
availability check.

### Project Structure Notes

- New files: `shared/validators/employee.validators.ts` (4-file shape not applicable — validators
  are single-file in this folder; `charity.validators.ts` sets the precedent, add to its
  `index.ts` barrel).
- Touched: `IEmployeeService.cs`, `EmployeeService.cs`, `Employees.cs` (DTO),
  `EmployeeManagementController.cs`, `employee.service.ts`, `employee.model.ts`,
  `employee-form.component.ts` (+ its `.html` for the error line), `ar.json`, `en.json`.
- No migration, no entity change, no DI registration change (service already registered).
- Keep the controller thin: bind → delegate → return. No business logic in the action.

### Environment warnings

- `dotnet build` MSB3021/MSB3027 = output-copy lock from the user's **running API** — compile is
  clean; never kill the process.
- `ng serve` may serve a stale bundle after its watcher dies — grep the served chunk for
  `checkUserNameAvailability` before trusting a "no effect" result.
- `npm install --legacy-peer-deps`; never commit the rewritten `package-lock.json`.

## Dev Agent Record

### Agent Model Used

Claude (GLM-5) — 2026-08-24 review-and-complete pass (first implementation of this story).

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — compile clean; MSB3021/3027 are the live-API output-copy
  lock only. The running process predates the endpoint — **live smoke pending an API restart**.
- `npx tsc --noEmit -p tsconfig.app.json` — zero errors in the touched files.

### Completion Notes List

- Endpoint shipped as designed: `GET check-username`, `[Authorize(Policy = "ManagementOnly")]`
  (D3 as corrected — the policies are registered in `ServiceCollectionExtensions.AddAuth`;
  the earlier D3 draft's role-form instruction was a misread of the dead Program.cs block).
- `IsUserNameAvailableAsync` consults both stores; edit mode excludes the row's own `FK_UserId`;
  empty or >100-char input reports unavailable without throwing.
- Frontend: `checkUserNameAvailability` uses `HttpClient` + replicated `StrictHttpParameterCodec`
  (not `ApiService.buildParams` — default codec turns `+` into a space).
- `employeeUserNameUniqueValidator` follows the corrected charity pattern (400 ms debounce,
  switchMap, echo staleness guard, error→valid, `first()`); attached to `email` via
  `{ validators, asyncValidators }`; PENDING guard in `onSubmit()` waits for the probe then
  re-enters, with `awaitingValidation` + subscription teardown preventing double-submits.

### File List

- `Backend/src/IIROSA.Application/Interfaces/IEmployeeService.cs` — method declaration
- `Backend/src/IIROSA.Application/Services/EmployeeService.cs` — implementation
- `Backend/src/IIROSA.Application/DTOs/EmployeeManagement/Employees.cs` — result DTO
- `Backend/src/IIROSA.Api/Controllers/EmployeeManagementController.cs` — endpoint
- `Frontend/src/app/modules/employees/services/employee.service.ts` — client call
- `Frontend/src/app/shared/validators/employee.validators.ts` — new; + barrel export
- `Frontend/src/app/modules/employees/employee-form/employee-form.component.ts` — wiring
- `Frontend/src/assets/i18n/ar.json`, `en.json` — `validation.emailTaken`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-EMP-02, module spec §9.S/§9.U, and a full code audit of the employee stack. Endpoint renamed `check-national-id` → `check-username` (decision D1); board comment annotated. |
| 2026-08-24 | Implemented (Tasks 1–5). D3 corrected before implementation: policies ARE registered (`AddAuth`), so the action uses `[Authorize(Policy = "ManagementOnly")]`. |
| 2026-08-24 | Code review fix: null-Id identity hit now reports taken. |

## Review Findings (code review 2026-08-24)

Both items resolved in the same-day review-fix pass.

- [x] [Review][Patch] Identity hit with null `Id` reported available [EmployeeService.cs:149-152]
  — fixed: a located account with no readable id is now reported taken (return false)
- Dismissed as by-design (recorded so future reviews don't re-raise): the validator fails open
  on transport errors (`catchError → null`) — deliberate, the server re-checks at save time
  (story D4/3-2 pattern); check-then-act between probe and save is advisory UX, not a control;
  the raw `HttpClient` instead of `ApiService` is deliberate (strict `+` encoding, 3-2 precedent)
