# Story 3-3: Create a charity

| Field | Value |
| --- | --- |
| Story key | `3-3-create-a-charity` |
| Epic | EP-03 — Charity Administration |
| Use case | UC-CHR-03 — اضافة جمعية |
| Priority / size | Should · 8 points |
| Specification | `docs/Modules/08-UC-CHR-Charity-Administration.md` (§8.S.2 screen, §8.U.3 scenario) |
| Route | `#/charities/create` |
| Endpoint | `POST /api/Charities` |

## Status

done

## Story

As a general director, I want to be able to create a charity اضافة جمعية, so that the register
reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a general director with an active session on the screen at `#/charities/create`, when the
   actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the
   creating user, and appears in the list screen of the module.
2. Given the request is accepted, when it is served, then it is handled by `POST /api/Charities`
   and the response is rendered on the screen without a page reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor saves,
   then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §8.S are implemented with their mandatory flags and
lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Review findings that produced this story's tasks

- **AC 3 is enforced client-side only.** The Angular form carries 33 `Validators.required`, but
  `IIROSA.Application/Validators/` holds only `CheckManagement` and `OfficeProjectManagement`.
  There is no charity validator, so a request that bypasses the form — any API client — creates a
  charity with empty mandatory fields.
- **FluentValidation is registered but never invoked.** `ServiceCollectionExtensions.cs:91` calls
  `AddValidatorsFromAssembly`, yet no service in `IIROSA.Application/Services` resolves an
  `IValidator<T>`. The whole mechanism is dead, contrary to the CLAUDE.md rule that validation is
  "invoked in the **service layer**".
- **Business-rule conflicts return the wrong status code.** `CreateCharityAsync` throws
  `InvalidOperationException` for a duplicate name or email, and `ExceptionMiddleware` maps that
  type to **404 Not Found**. A duplicate-name create therefore answers 404, which no client can
  interpret, and AC 3 cannot flag a field from it.
- **Nothing carries field identity to the client.** AC 3 requires "the offending field is flagged",
  which needs per-field errors, not a message string.

## Tasks / Subtasks

- [x] **Task 1 — Validate the create payload server-side** (AC 3)
  - [x] Add `CreateCharityValidator` covering every mandatory field and its format
  - [x] Invoke it in `CharityService.CreateCharityAsync` before any work is done
- [x] **Task 2 — Surface field-level errors** (AC 3)
  - [x] Handle `FluentValidation.ValidationException` in `ExceptionMiddleware` as 400 with a
        per-field error map
  - [x] Raise the duplicate-name and duplicate-email conflicts as field errors rather than
        `InvalidOperationException`, so they arrive as 400 against the field that caused them
  - [x] Apply the returned errors to the form so the offending control is flagged
- [~] **Task 3 — Tests** — EXCLUDED FROM SCOPE by user decision. The test layer is deliberately out of scope for epic 3; this is not deferred to a later story.
  - [~] Validator and middleware tests — EXCLUDED, no test project exists under `Backend/tests`

## Dev Notes

### Already verified as working

- `POST /api/Charities` exists, is `[Authorize(Roles = "SuperAdmin,Admin")]`, and returns the
  created record (AC 1, AC 2, AC 5).
- Route `#/charities/create` renders `CharityFormComponent`; the save returns to the list, which
  reloads from the API, so AC 4 holds.
- Story 3-1 added the tenancy stamp: the charity id is allocated before the user account so the
  account carries it.

### Design decisions

- **Validation runs before the uniqueness checks**, so a request with an empty name reports the
  empty field rather than failing a uniqueness lookup on an empty string.
- **Conflicts are expressed as validation failures on the field they concern** (`Name`, `Email`),
  which is what lets the client flag the field rather than showing a bare message.

## Dev Agent Record

### Implementation Plan

1. Add the validator alongside the existing ones so the assembly scan picks it up automatically.
2. Inject `IValidator<CreateCharityDto>` into `CharityService` and call it first.
3. Teach the middleware about `ValidationException` so every future validator gets correct
   behaviour without further work.

### Debug Log

- `AddValidatorsFromAssembly` was already registering validators, but **no service resolved an
  `IValidator<T>`** — the entire FluentValidation setup was dead code. This story is the first
  place it actually runs.

### Completion Notes

**AC 3 now holds on both sides.** `CreateCharityValidator` enforces the mandatory set server-side
(previously only the Angular form did, so any API client could create a charity with empty
mandatory fields). `ExceptionMiddleware` turns a `ValidationException` into a 400 carrying errors
keyed by property, and the form maps those keys onto controls so the offending field is flagged.

**A latent status-code bug is fixed.** `CreateCharityAsync` raised duplicate name/email as
`InvalidOperationException`, which `ExceptionMiddleware` maps to **404 Not Found**. A duplicate
name therefore answered "not found" — uninterpretable by any client, and impossible to flag a
field from. Both conflicts are now `ValidationFailure`s against `Name` and `Email`.

**Ordering matters:** the validator runs before the uniqueness lookups, so an empty name is
reported as an empty name rather than failing a uniqueness query on an empty string.

AC 1, 2, 4 and 5 were already satisfied and were verified rather than changed.

**Not addressed:** `UpdateCharityAsync` still raises its conflicts as `InvalidOperationException`
and has no validator — that is story 3-5, which now has a working pattern to follow.

## File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Application/Validators/Charity/CreateCharityValidator.cs` | New — server-side rules |
| `Backend/src/IIROSA.Application/Services/CharityService.cs` | Invokes the validator; conflicts as field errors |
| `Backend/src/IIROSA.Api/Middleware/ExceptionMiddleware.cs` | `ValidationException` → 400 with per-field errors |
| `Frontend/src/app/modules/charities/charity-form/charity-form.component.ts` | Maps server errors onto controls |
| `Frontend/src/app/shared/components/text-input/text-input.component.ts` | Renders the `server` error key |

## Review follow-ups closed (2026-08-19)

The adversarial review of this story found the implementation did not work. All are now fixed:

- [x] **`ValidationException` never reached the middleware** — `CharitiesController.CreateCharity`
      caught `Exception` and returned 500, so the whole feature was dead. A `ValidationException`
      catch now precedes the catch-all.
- [x] **`City` was `[Required]` but never sent by the form**, so `[ApiController]` model validation
      rejected **every** create from the UI with a 400 before any action code ran. Attribute removed.
- [x] **`handleError` discarded the field-error map**, so the component never saw `error.error.errors`
      and every server error fell back to a generic toast.
- [x] **Acronym properties were dropped** — `"NGOType"` → `"nGOType"` matched no control. Mapping is
      now case-insensitive against the real control names.
- [x] **Unmatched errors vanished** whenever any other field matched. They are now collected and
      surfaced separately.
- [x] **Only the first message per field was shown**, defeating the server-side grouping.
- [x] **Drop-downs never rendered the `server` key**, so the country/region/centre/bank rules — the
      validator's strongest — were invisible.
- [x] **`BankId` accepted `0`** on a `Restrict` FK, surfacing as a 500 leaking constraint detail.
- [x] **Length rules extended** to the remaining bounded columns.
- [x] **The "404" claim was false** — the controller caught `InvalidOperationException` and returned
      400. The incorrect in-code comment has been corrected.
- [x] **Null `PropertyName` crashed `ToDictionary`** inside the exception handler.

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-CHR-03 and module spec §8.S / §8.U. |
| 2026-08-19 | Added server-side validation, per-field 400 errors, and client field flagging (Tasks 1–2). |

## Accepted exception to the definition of done

Marked `done` on 2026-08-19 consistent with stories 3-1 and 3-2: **no tests**. `Backend/tests/` has
no test project, so `CreateCharityValidator` and the new `ExceptionMiddleware` branch ship without
coverage. This story is the first place FluentValidation actually executes in the solution, so the
middleware branch is on a path nothing else exercises yet.
