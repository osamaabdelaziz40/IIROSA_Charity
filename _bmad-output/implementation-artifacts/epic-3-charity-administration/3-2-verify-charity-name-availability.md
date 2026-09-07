# Story 3-2: Verify charity name availability

| Field | Value |
| --- | --- |
| Story key | `3-2-verify-charity-name-availability` |
| Epic | EP-03 — Charity Administration |
| Use case | UC-CHR-02 — التحقق من اسم الجمعية |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/08-UC-CHR-Charity-Administration.md` (§8.S.2 screen, §8.U.2 scenario) |
| Route | `#/charities/create`, `#/charities/:id/edit` |
| Endpoint | `GET /api/Charities/check-name` |

## Status

done

## Story

As a general director, I want to be able to verify charity name availability التحقق من اسم
الجمعية, so that I learn a name is already taken while I am still typing it, rather than losing the
form to a failed save.

## Acceptance Criteria

1. Given a general director with an active session in the module, when the actor opens the screen
   with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Charities/check-name` and the response is rendered on the screen without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §8.S are implemented with their mandatory flags and
lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

### Note on AC 3

**Corrected after code review.** This note originally dismissed AC 3 *and* AC 4 with a single
argument that covers only AC 3. AC 4 is in fact satisfied — it maps onto the `excludeId`
parameter this story implemented, documented under "Design decisions" below.

AC 3 does not apply to this story. **A charity name must be unique across the whole register, not per tenant.**
Scoping the check to the caller's own charity would report a name as available when another
charity already holds it, and the save would then be refused by
`CharityService.CreateCharityAsync`, which validates uniqueness globally. The check therefore runs
globally, exactly matching the rule the save enforces.

The scoping intent behind those criteria is honoured instead by **authorisation**: the endpoint is
restricted to the roles that may create or update a charity, so it cannot be used to enumerate the
register. This is recorded rather than silently ignored.

## Tasks / Subtasks

- [x] **Task 1 — Expose the existing uniqueness check over HTTP** (AC 1, AC 2, AC 5)
  - [x] Add a `CharityNameAvailabilityDto` result type
  - [x] Add `GET /api/Charities/check-name` to `CharitiesController`
  - [x] Restrict it to the roles that may create or update a charity
- [x] **Task 2 — Wire the client** (AC 2)
  - [x] Add `checkNameAvailability` to the Angular `CharityService`
  - [x] Add an async validator on the name field of the charity form
  - [x] Surface the "name already taken" message through `ar.json` / `en.json`
- [~] **Task 3 — Tests** — EXCLUDED FROM SCOPE by user decision. The test layer is deliberately out of scope for epic 3; this is not deferred to a later story.
  - [~] Endpoint and validator tests — EXCLUDED, no test project exists under `Backend/tests`

## Dev Notes

### What already existed before this story

The check itself was fully implemented below the API boundary and is **not** new work:

- `ICharityRepository.IsNameUniqueAsync(string name, Guid? excludeId)` —
  `CharityRepository.cs`
- `ICharityService.IsNameUniqueAsync(string name, Guid? excludeId)` — `CharityService.cs:836`
- Already called by `CreateCharityAsync` (`CharityService.cs:62`) and `UpdateCharityAsync`
  (`CharityService.cs:193`)

Only the HTTP surface and the client wiring were missing, which is why the story was correctly
sitting at `backlog` rather than `in-progress`.

### Design decisions

- **`excludeId` matters on edit.** Without it, editing a charity without renaming it would report
  its own name as taken. The parameter is passed through to the existing service method.
- **Attribute routing handles the literal/parameter overlap.** `check-name` is a literal segment
  and takes precedence over `{id}`, the same way the existing `my-profile` route already does in
  this controller.
- **The response is a small DTO, not a bare boolean**, so the client can render the name it asked
  about without tracking the in-flight request itself.

## Dev Agent Record

### Implementation Plan

1. Add the result DTO next to the other charity DTOs.
2. Add the endpoint, delegating to the service method that already exists — no business logic in
   the controller.
3. Add the client method and an async validator debounced so it does not fire on every keystroke.

### Debug Log

- Pre-existing blocker cleared while completing story 3-1: `AppIdentityDbContext` had **two**
  design-time factories, so every `dotnet ef` command failed with "An item with the same key has
  already been added". The duplicate was removed and the survivor hardened.

- **`npm install` fails on this project as configured.** `ngx-bootstrap@^12.0.0` declares a peer
  dependency on `@angular/animations@^17.0.0`, but the project is on Angular 18, so npm aborts
  with `ERESOLVE`. Dependencies were restored with `--legacy-peer-deps` to allow a typecheck;
  `tsc --noEmit -p tsconfig.app.json` then passed with 0 errors. **This is a real project defect,
  not a story defect** — `ngx-bootstrap` needs upgrading to v18. Until then every developer and CI
  job must pass `--legacy-peer-deps`, and the resolved tree is not the one `package.json`
  describes.

- Frontend verification was limited to a typecheck. No unit tests were run: none exist, and tests
  are deferred by the same user decision recorded on story 3-1.

### Completion Notes

**AC 1 and AC 2 hold. AC 5 now holds** — the interceptor originally handled 401 only, so the
"role is not permitted" half failed on a 403; a 403 branch was added during review follow-up. `GET /api/Charities/check-name` reads only; it delegates to the
`IsNameUniqueAsync` the save path already uses, so the check and the rule can't drift apart. The
`[Authorize]` attribute plus 401 *and* 403 handling in `auth.interceptor` satisfy AC 5.

**AC 3 does not apply**; **AC 4 is satisfied** by `excludeId`. Charity names are unique
register-wide, so scoping the check per tenant would report a taken name as free and push the
failure to the save. Restricting the endpoint to `SuperAdmin,Admin` limits who can probe the
register. (Originally both criteria were dismissed together — corrected after code review.)

**Async validator details.** `debounceTime` via `timer(400)` keeps the check off every keystroke;
`switchMap` cancels superseded requests so a slow earlier reply can't overwrite a newer one;
`first()` completes the stream, without which the control would sit in `pending` forever. A failed
request resolves to valid — a name check is a convenience and a network blip must not block the
save, and the server re-checks uniqueness on write regardless.

**`excludeId`** is passed from `this.charityId`, so editing a charity without renaming it does not
report its own name as taken.

**Verification.** Backend builds with 0 errors and the route is registered. Both i18n files parse.
Frontend typecheck — see Debug Log.

## File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Application/DTOs/Charity/CharityNameAvailabilityDto.cs` | New — result DTO |
| `Backend/src/IIROSA.Api/Controllers/CharitiesController.cs` | New `GET check-name` endpoint |
| `Frontend/src/app/modules/charities/models/charity.model.ts` | New `CharityNameAvailability` |
| `Frontend/src/app/modules/charities/services/charity.service.ts` | New `checkNameAvailability` |
| `Frontend/src/app/modules/charities/charity-form/charity-form.component.ts` | Async validator on `name` |
| `Frontend/src/app/shared/components/text-input/text-input.component.ts` | Renders the `nameTaken` error |
| `Frontend/src/assets/i18n/en.json` | `validation.nameTaken` |
| `Frontend/src/assets/i18n/ar.json` | `validation.nameTaken` |
| `Backend/Framework/Framework.Identity/Data/AppIdentityDbContextFactory.cs` | **Deleted** — duplicate design-time factory |
| `Backend/Framework/Framework.Identity/AppIdentityDbContextDesignTimeFactory.cs` | Resolves the connection string from the API appsettings |
| `Frontend/src/app/core/interceptors/auth.interceptor.ts` | Review fix — 403 handling (AC 5) |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-CHR-02 and module spec §8.S / §8.U. |
| 2026-08-19 | Implemented `GET /api/Charities/check-name` and the client async validator (Tasks 1–2). |
| 2026-08-19 | Removed duplicate `AppIdentityDbContext` design-time factory that blocked all EF migrations. |

### Review Findings

Adversarial code review, 2026-08-19. Three layers: Blind Hunter, Edge Case Hunter, Acceptance
Auditor. Findings below are deduplicated across layers.

- [x] [Review][Decision] **Endpoint does not return `ApiResponse<T>`** — it returns a bare DTO on success and two different anonymous shapes on failure, against the CLAUDE.md rule that every response is `ApiResponse`/`ApiResponse<T>`. Only 1 of 21 controllers currently follows that rule, so fixing this endpoint alone makes it inconsistent with its own neighbours. Fix here, or log as systemic debt? `IIROSA.Api/Controllers/CharitiesController.cs:178,193` — RESOLVED as DEFER: 20 of 21 controllers ignore the rule, so converting this one endpoint would make its own controller internally inconsistent and break any client that reads the other endpoints. Logged as systemic debt; it should be one migration across all controllers, not a per-story fix.
- [x] [Review][Decision] **No unique index on `Charity.Name`** — `builder.HasIndex(x => x.Name)` has no `.IsUnique()`, unlike `Code`. Two concurrent creates of the same name both pass `IsNameUniqueAsync` and both insert. Adding the constraint needs a migration and will fail if duplicates already exist in your data. `IIROSA.Domain/Configurations/CharityConfiguration.cs:145` — RESOLVED as DEFER: the constraint is correct but the migration would fail if duplicates already exist, and I cannot inspect your data. Run the detection query in `deferred-work.md` first, clean up any hits, then add the index.
- [x] [Review][Decision] **`package-lock.json` was rewritten by the `--legacy-peer-deps` install** — RESOLVED: reverted with `git checkout`. `node_modules` is left in place so local builds work, but the committed lockfile still describes the tree `package.json` intends, so the workaround does not become permanent.
- [x] [Review][Patch] **`ex.Message` returned to the client** — the 500 handler echoes raw exception text, which on an EF/SqlException carries table, column and constraint names. `IIROSA.Api/Controllers/CharitiesController.cs:193`
- [x] [Review][Patch] **Trim mismatch lets duplicates through** — the endpoint checks `name.Trim()` but `CreateCharityAsync` calls `IsNameUniqueAsync(dto.Name)` untrimmed, and the form POSTs the untrimmed value. `" Alpha"` is checked as `"Alpha"`, reported free, and stored with the leading space; every later check for `"Alpha"` then reports available. SQL Server ignores trailing but not leading spaces. `IIROSA.Application/Services/CharityService.cs:62,193`
- [x] [Review][Patch] **The staleness guard was designed and never implemented** — `CharityNameAvailabilityDto.Name` exists specifically so a late reply can be recognised as stale, and both the DTO and the model comment say so, but the validator does `map(result => result.isAvailable ? null : { nameTaken: true })` and never compares `result.name` to the submitted name. `charity-form.component.ts:195`
- [x] [Review][Patch] **`catchError(() => of(null))` reports every name as available on any failure** — including the 403 that every non-admin gets from this `[Authorize(Roles = "SuperAdmin,Admin")]` endpoint. The user then loses the completed form to a refused save, which is the exact failure this story set out to prevent. `charity-form.component.ts:198`
- [x] [Review][Patch] **Form can be submitted while the validator is `PENDING`** — `onSubmit` guards only `if (this.charityForm.invalid)`, and a `PENDING` control is not `INVALID`. Typing a duplicate name and clicking Save inside the 400 ms debounce bypasses the check entirely and surfaces the untranslated server error instead. `charity-form.component.ts:612`
- [x] [Review][Patch] **AC 5 is unmet for the "role is not permitted" half** — `auth.interceptor.ts` handles 401 only and contains no 403 branch, but an authenticated-yet-unauthorised caller gets 403, which falls through with no navigation. This affects story 3-1 equally. `core/interceptors/auth.interceptor.ts:49`
- [x] [Review][Patch] **AC 4 was dismissed incorrectly** — the note argues register-wide name uniqueness, which justifies dismissing AC 3 only. AC 4 maps onto the `excludeId` parameter this story actually implemented and documents under "Design decisions". Two criteria were dismissed with one argument that covers one of them.
- [x] [Review][Patch] **No maximum-length guard** — `Charity.Name` is `HasMaxLength(200)` but the form has only `minLength(3)` and the endpoint only checks whitespace. A 201-character name is reported available, then fails the save as a 500.
- [x] [Review][Patch] **A `+` in the name is checked against the wrong string** — Angular's default `HttpParams` codec re-writes `%2B` to a literal `+`, which ASP.NET Core decodes as a space. `"Al Noor + Partners"` is checked as `"Al Noor   Partners"`. `charity.service.ts:131`
- [x] [Review][Patch] **Undeclared rewrite of the user story text** — the benefit clause differs from `epics.md` US-CHR-02 with no Change Log entry.
- [x] [Review][Defer] **Validation and normalisation live in the controller** — `IsNullOrWhiteSpace` and `.Trim()` sit in the action rather than a FluentValidation validator in the service layer. Deferred: the charity validator is stories 3-3/3-5.
- [x] [Review][Defer] **Server-side strings are not translated** — `"A name is required to check availability"` is hard-coded English while the client-side message was translated. Deferred, matches every other controller in the project.
- [x] [Review][Defer] **Controller inherits `ControllerBase`, not `ApiController`** — pre-existing across 20 of 21 controllers.
- [x] [Review][Defer] **`text-input` breaks the 4-file shape** — `.css` instead of `.scss`, no `.spec.ts`; the new `nameTaken` branch ships with no coverage. Pre-existing.
- [x] [Review][Defer] **`catchError(this.handleError)` passed unbound** — `handleError` is a plain private method used this way by ~25 methods in the service. Pre-existing and systemic.
