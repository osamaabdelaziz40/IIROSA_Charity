# Story 3-1: List all charities

| Field | Value |
| --- | --- |
| Story key | `3-1-list-all-charities` |
| Epic | EP-03 — Charity Administration |
| Use case | UC-CHR-01 — قائمة الجمعيات |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/08-UC-CHR-Charity-Administration.md` (§8.S.1 screen, §8.U.1 scenario) |
| Route | `#/charities` |
| Endpoint | `GET /api/Charities` |
| Depends on | EP-01 — story `1-10-issue-a-jwt-access-token-on-login` (charity/country claims) |

## Status

done

## Story

As a general director, I want to be able to list all charities قائمة الجمعيات, so that I can
find the record I need without leaving the system.

## Acceptance Criteria

1. Given a general director with an active session on the screen at `#/charities`, when the actor
   opens the screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/Charities` and
   the response is rendered on the screen without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §8.S are implemented with their mandatory flags and
lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Tasks / Subtasks

- [x] **Task 1 — Carry the caller's tenancy in the identity model** (AC 3, AC 4)
  - [x] Add `CharityId` (`Guid?`) and `CountryId` (`int?`) to `ApplicationUser`
  - [x] Surface the same two fields on `UserDto`
  - [x] Add an EF Core migration for the two new columns (`20260819073139_AddCharityAndCountryToApplicationUser`)
- [x] **Task 2 — Put tenancy in the access token** (AC 3, AC 4)
  - [x] Define claim-type constants so the names are not stringly-typed
  - [x] Emit `charityId` and `countryId` claims from all three token-generation paths
- [x] **Task 3 — Expose the caller's tenancy to the application layer** (AC 3, AC 4)
  - [x] Add `ICurrentUserService` to `IIROSA.Application/Interfaces`
  - [x] Implement it over `IHttpContextAccessor` and register it in DI
- [x] **Task 4 — Enforce scope server-side on the list** (AC 3, AC 4, DoD)
  - [x] Add `CharityId` to `CharityFilterDto` so an HQ role can target one charity
  - [x] Apply the caller's charity/country scope inside `CharityService.GetCharitiesAsync`
  - [x] Let the `Charity` role reach `GET /api/Charities`, scoped to its own record
- [~] **Task 5 — Tests** — EXCLUDED FROM SCOPE by user decision. The test layer is deliberately out of scope for epic 3; this is not deferred to a later story.
  - [~] `CharityService` scoping unit tests — EXCLUDED, no test project exists under `Backend/tests`
  - [~] `charity-list.component.spec.ts` — EXCLUDED

## Dev Notes

### Architecture constraints (from `CLAUDE.md` and `architecture.md`)

- Clean Architecture: `Api → Application → Domain`; `Infrastructure → Domain`. The Application
  layer must not reference `HttpContext`, hence `ICurrentUserService` is declared in Application
  and implemented in the API layer.
- Tenancy is enforced **server-side**, never by hiding menu items.
- Only `IUnitOfWork` saves; repositories never call `SaveChanges`.

### Findings from the review that produced these tasks

- `CharitiesController.GetCharities` passed the client-supplied `CharityFilterDto` straight to the
  service. No scope was derived from the caller, so any authenticated HQ user could enumerate every
  country's charities by omitting `CountryId`.
- `ApplicationUser` had no link to a charity or country. `AgancyId` (`int?`) is a legacy field and
  is not the `Charity` key, which is a `Guid`.
- `TokenService` emitted no tenancy claims, so scope could not be recovered from the token.
- `HasAccessToCharity` in `CharitiesController` is a stub that returns `true` unconditionally. It
  guards `GET /api/Charities/{id}` (story 3-4), not this story, and is left for 3-4.

### Deliberately out of scope

- Backbone for stories 3-3/3-5 (FluentValidation) and 3-7/3-8/3-9 (enforcing the lock and rights
  flags) — separate stories, tracked in `sprint-status.yaml`.
- Populating `CharityId`/`CountryId` on existing user rows is a data task, not a code task.

## Dev Agent Record

### Implementation Plan

1. Extend the identity model and the DTO that feeds token generation.
2. Emit the two tenancy claims from all THREE token paths: `GenerateTokenAsync(UserDto)`,
   `GenerateTokenInternalAsync(ApplicationUser)`, and `ImpersonationService` — the third was
   missed on the first pass and found by code review.
3. Introduce `ICurrentUserService` so the Application layer can read the caller without taking a
   dependency on ASP.NET Core.
4. Apply the scope inside the service, not the controller, so every caller of
   `GetCharitiesAsync` inherits it.

### Debug Log

- `Backend/tests/` is empty — there is no test project in the solution, so the red-green-refactor
  cycle in the workflow could not be executed. Recorded as a deferred task by user decision.

### Completion Notes

**AC 3 and AC 4 are enforced server-side** (corrected after code review — the original wording claimed more than the code delivered; see Review Findings). `CharityService.ApplyCallerScope` rewrites the incoming
filter before it reaches the repository:

- a charity-bound caller has `CharityId` overwritten with their own, so asking for another
  charity returns their own record and the attempt is logged as a warning;
- a head-office caller keeps any `CharityId` they chose (AC 4) but is pinned to their own
  `CountryId` when the token carries one, which is what stops an HQ user in one country from
  enumerating another's by simply omitting the country filter.

The scope lives in the service rather than the controller, so every caller of
`GetCharitiesAsync` inherits it.

**Design note.** `ICurrentUserService.IsHeadOffice` deliberately returns false for any caller
carrying a charity claim, whatever roles they also hold. Role alone never widens scope.

**Not addressed here (other stories):** the `HasAccessToCharity` stub on `GET /api/Charities/{id}`
belongs to story 3-4; FluentValidation belongs to 3-3/3-5; enforcing the lock and rights flags
belongs to 3-7/3-8/3-9.

**Tests deferred by user decision**, so this story cannot pass the workflow's definition of done
and remains `in-progress` rather than moving to `review`.

## File List

| File | Change |
| --- | --- |
| `Backend/Framework/Framework.Identity/Data/Entities/ApplicationUser.cs` | Added `CharityId`, `CountryId` |
| `Backend/Framework/Framework.Identity/Data/Dtos/UserDto.cs` | Added `CharityId`, `CountryId` |
| `Backend/src/IIROSA.Application/Interfaces/IiroSaClaimTypes.cs` | New — claim-type constants |
| `Backend/src/IIROSA.Application/Interfaces/ICurrentUserService.cs` | New — caller abstraction |
| `Backend/src/IIROSA.Api/Services/CurrentUserService.cs` | New — claims-based implementation |
| `Backend/src/IIROSA.Application/Services/TokenService.cs` | Emits tenancy claims on both token paths |
| `Backend/src/IIROSA.Application/Services/CharityService.cs` | Injected `ICurrentUserService`; added `ApplyCallerScope` |
| `Backend/src/IIROSA.Application/DTOs/Charity/CharityFilterDto.cs` | Added `CharityId` |
| `Backend/src/IIROSA.Domain/Interfaces/ICharityRepository.cs` | Added `charityId` parameter |
| `Backend/src/IIROSA.Infrastructure/Data/Repository/CharityRepository.cs` | Added `charityId` filter clause |
| `Backend/src/IIROSA.Api/Controllers/CharitiesController.cs` | List endpoint now admits the `Charity` role |
| `Backend/src/IIROSA.Api/Program.cs` | Registered `IHttpContextAccessor` and `ICurrentUserService` |
| `Backend/Framework/Framework.Identity/Data/migrations/20260819073139_AddCharityAndCountryToApplicationUser.cs` | New — migration (+ index on `CharityId`) |
| `Backend/Framework/Framework.Identity/Data/migrations/20260819073139_...Designer.cs` | New — generated |
| `Backend/Framework/Framework.Identity/Migrations/AppIdentityDbContextModelSnapshot.cs` | Updated by the migration |
| `Backend/Framework/Framework.Identity/Data/Services/ImpersonationService.cs` | Review fix — emits tenancy claims |
| `Backend/Framework/Framework.Identity/Data/Services/UserAppService.cs` | Review fix — persists `CharityId`/`CountryId` |
| `Backend/Framework/Framework.Identity/Data/Dtos/UserCreateOrUpdateDtoBase.cs` | Review fix — carries tenancy at creation |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-CHR-01 and module spec §8.S.1 / §8.U.1. |
| 2026-08-19 | Implemented server-side charity/country scoping for `GET /api/Charities` (Tasks 1–4). Backend builds with 0 errors. |

### Review Findings

Adversarial code review, 2026-08-19. Three layers: Blind Hunter, Edge Case Hunter, Acceptance
Auditor. Findings below are deduplicated across layers.

- [x] [Review][Decision] **No backfill for existing user rows** — RESOLVED: backfilled in the migration by joining `identity.Users.Id` to `IIROSA.Charity.UserId`, which is the authoritative link. Guarded on the table existing and only touching NULL rows, so it is safe on a fresh database and re-runnable.
- [x] [Review][Decision] **Design-time factory fallback masks a mis-invoked `database update`** — RESOLVED: the fallback was removed entirely. The factory now locates `Backend/src/IIROSA.Api/appsettings.json` by walking up from the working directory, honours `ASPNETCORE_ENVIRONMENT` for the overlay file, and throws a clear message if no `DefaultConnection` is found. There is no phantom database to silently target, and `dotnet ef` now works from any directory in the tree.
- [x] [Review][Patch] **`ApplyCallerScope` fails open** — a caller with neither claim (unauthenticated, pre-deploy token still inside its 30-minute life, null `CharityId`, or an unparseable claim silently swallowed by `Guid.TryParse`) receives the entire unscoped register. A tenancy filter must fail closed. `IIROSA.Application/Services/CharityService.cs:542-566`
- [x] [Review][Patch] **Nothing ever populates `ApplicationUser.CharityId`** — `CreateUserAccountForCharity` builds `UserCreateDto { UserName, Email, FullName, Password, IsActive, RoleNames = ["Charity"] }` with no charity id, and `UserCreateDto` has no such property. Combined with widening the endpoint to the `Charity` role this is a net data-exposure regression versus the previous `SuperAdmin,Admin`. `IIROSA.Application/Services/CharityService.cs:934-942`
- [x] [Review][Patch] **A third token-generation path was missed** — `ImpersonationService` builds its own claim list (`NameIdentifier`, `Name`, `Email`, `FullName`, `Jti`, roles, impersonation claims) and was not updated, so impersonation tokens carry no tenancy. Task 2 of this story claims both paths were done; there are three. `Framework.Identity/Data/Services/ImpersonationService.cs:470-478`
- [x] [Review][Patch] **`HasAccessToCharity` stub is now exploitable** — it returns `true` unconditionally and guards `GET /api/Charities/{id}` and `{id}/profile`. This story scoped the list but the list now hands a `Charity`-role user the ids of every other charity to feed straight into those unguarded reads. `ICurrentUserService.CharityId` is the primitive that closes it. `IIROSA.Api/Controllers/CharitiesController.cs:636-648`
- [x] [Review][Patch] **AC 4 breaks for an HQ caller with a country claim** — `ApplyCallerScope` sets `filter.CountryId` without clearing a caller-supplied `filter.CharityId`, and the repository ANDs both. Asking for a charity in another country returns an empty page with no diagnostic. `IIROSA.Application/Services/CharityService.cs:561-566`
- [x] [Review][Patch] **`IsHeadOffice` and `IsInRole` are dead code** — `ApplyCallerScope` never consults roles, so the behaviour the Completion Notes describe is not the behaviour the code implements. Resolved by the fail-closed rewrite. `IIROSA.Api/Services/CurrentUserService.cs`
- [x] [Review][Patch] **Board and story disagree** — `sprint-status.yaml` carries `3-1-list-all-charities: done` while this file says `in-progress` and states it cannot pass DoD. `_bmad-output/implementation-artifacts/sprint-status.yaml:126`
- [x] [Review][Patch] **`ApplyCallerScope` mutates the model-bound filter in place** — the `filter = ApplyCallerScope(filter)` reassignment implies a pure function while the body mutates its argument. `IIROSA.Application/Services/CharityService.cs:545`
- [x] [Review][Patch] **New parameter inserted mid-signature** — `charityId` was added before `countryId` in `GetFilteredPaginatedAsync`, where every parameter is optional. A future positional caller passing literal `null` shifts silently. Append it instead. `IIROSA.Domain/Interfaces/ICharityRepository.cs:29`
- [x] [Review][Patch] **File List is incomplete** — the migration `.Designer.cs` and the updated `AppIdentityDbContextModelSnapshot.cs` are both changed on disk but absent from the File List above.
- [x] [Review][Patch] **No index on `Users.CharityId`** — a column that exists to answer "which users belong to this charity" gets a table scan.
- [x] [Review][Patch] **Completion Notes overstate scope** — they claim AC 3 and AC 4 "now hold" end to end, but this story changed no frontend file and no client ever sends `CharityId`, so the AC 4 path is unexercised through the screen the AC is written against.
- [x] [Review][Defer] **Tenancy claims have no revocation path** — moving a user between charities has no effect until their token expires (30 min). Inherent to stateless JWT; deferred, pre-existing design.
- [x] [Review][Defer] **No tests accompany a security-boundary change** — four distinct branches in `ApplyCallerScope`, zero tests. Deferred by explicit user decision.

### Verification round (2026-08-19)

A second adversarial pass verified every fix from the first round and hunted regressions the fixes
themselves caused. It found three fixes incomplete and two new defects. All are now closed:

- [x] `ImpersonationService` had **two** token methods; only one was patched. The other mints the
      token handed back when impersonation *ends*, so a country-pinned admin silently widened to
      every country on exit.
- [x] `HasAccessToCharity` was correct but all three call sites still gated on
      `User.IsInRole("Charity") &&`, so a charity-bound user holding `Accountant` or
      `FinancialOfficer` skipped the check and could read any charity.
- [x] The backfill could bind a head-office account to a charity (stale `Charity.UserId`), and
      `UPDATE … FROM` would pick arbitrarily where a user was reachable from two charity rows.
      Both are now excluded.
- [x] **Startup migrated only `ApplicationDbContext`.** `UseIdentityDBMigration()` had zero callers
      repo-wide, so on any existing database the first login would have thrown
      `Invalid column name 'CharityId'`. Migration now runs for both contexts, outside the
      seed-data gate (it was also skipped entirely in Production), and rethrows rather than
      swallowing.
- [x] Design-time factory defaulted to `Development` while ASP.NET Core defaults to `Production`,
      so `database update` could migrate `IIROSA_Db_Dev` while the API ran against `IIROSA_Db`.
- [x] `FamiliesController.GetUserCharityId()` read a literal `"CharityId"` claim that no token
      carried. Because .NET matches claim types case-insensitively, the new `charityId` claim
      activated ~17 dormant call sites as a side effect. Now bound to `IiroSaClaimTypes.CharityId`
      so the coupling is explicit.
- [x] Admin-created users got no tenancy — only charity creation set it. `CreateUserDto` and
      `UserAppServiceExtended` now carry it.

## Accepted exceptions to the definition of done

Marked `done` on 2026-08-19 by explicit user decision, with these gaps open:

1. **No tests.** `Backend/tests/` has no test project and no `.spec.ts` was added. The workflow's
   DoD requires them. This is security-critical code — `ApplyCallerScope` has four branches, and
   the fail-open bug that shipped in the first pass lived in the branch a happy-path manual test
   never reaches.
2. **`Family.FK_CharityId` vs `ApplicationUser.CharityId` are unverified against each other.**
   They are populated by separate mechanisms. If they disagree for an existing tenant, that
   tenant's family register reads as empty. Verify before production.
3. **The seeded `Charity@IIROSA.com` account has no `CharityId`** and no seeded charity links to
   it, so it now correctly sees an empty register. Update the seed data or link it to a charity.

## Change log

- 2026-09-10 — **Statistics band added to the list screen** (re-platform extension requested by
  the user; no WAR.IIROSA precedent). `GET /api/Charities/statistics`
  (`[Authorize(Roles = "SuperAdmin,Admin,Charity")]`, literal route above `{id}`) →
  `ICharityService.GetStatisticsAsync()` → `ICharityRepository.GetRegisterStatisticsAsync()`:
  one grouped aggregate pass over the scoped live rows (total / active / inactive / locked /
  receiving-donations / added-this-month) plus a by-country FK grouping with lookup-resolved
  NameAr/NameEn. Tenancy rides `ApplyCallerScope` with a blank filter verbatim — charity caller
  sees own-record counts, country-pinned HQ their country, unscopeable callers zeros, so the band
  can never claim a row the grid under it would hide. Frontend: six cards + per-country pills on
  `charity-list`, register-scoped (NOT filter-reactive), refreshed after activate/deactivate,
  lock/unlock and delete; load failure keeps the band hidden without blocking the grid. The dead
  `charities.statistics` string in ar/en i18n (no consumer) became the key namespace with its old
  label preserved as `.title`. No schema change — no migration. Verified: solution build 0
  errors, `npm run build` green. Docs: §8.S.1 screen spec + §8.U.1 scenario updated.
