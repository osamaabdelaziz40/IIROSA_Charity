# Epic 3 — Charity Administration: retrospective

| Field | Value |
| --- | --- |
| Epic | EP-03 — Charity Administration |
| Stories | 9 of 9 `done` |
| Points | 36 |
| Date | 2026-08-19 |
| Outcome | Delivered. First epic completed on this board. |

## What the epic actually turned out to be

It was scoped as charity CRUD. It became the epic that built the platform's **multi-tenancy
enforcement**, because every story kept failing on the same definition-of-done line — "the role and
charity scoping is enforced server-side, not only in the menu" — and nothing existed to enforce it
with.

Delivered along the way, none of it in the original story scope:

- Tenancy on the identity model (`CharityId`/`CountryId`), carried into the JWT from all three
  token paths, read through a new `ICurrentUserService`.
- `ApplyCallerScope` — fail-closed query scoping.
- `ICharityWriteGuard` — the first code anywhere to read `IsLocked`/`IsAddEnabled`/`IsUpdateEnabled`,
  now enforced at 8 write entry points across 4 services.
- Route-level authorisation that actually decides (`hasPermission` was a stub returning
  `isAuthenticated()`).
- The first working FluentValidation in the solution.

## What went well

- **The adversarial review layers earned their cost repeatedly.** Every serious defect in this epic
  was found by review, not by writing code. Three separate rounds each found something that would
  have reached production.
- **Fail-closed as a default paid off immediately.** Once `ApplyCallerScope` denied what it could
  not scope, the follow-on bugs (missing backfill, unpopulated `CharityId`) degraded to "sees
  nothing" instead of "sees everything".
- **Blocking on decisions was right.** Backfill strategy, unique-index-on-name, and the
  `ApiResponse` question all needed data or judgement that wasn't mine to supply.

## What went badly — and the pattern behind it

Three times, work was reported complete when it did not function:

1. **Story 3-1** shipped a tenancy control that protected nobody — nothing populated `CharityId`.
2. **Story 3-3** shipped validation that never executed — the controller's `catch (Exception)`
   swallowed `ValidationException` into a 500, and separately `[Required]` on an unsent `City`
   field meant *every* create was rejected before any of it ran.
3. **Two fixes introduced new defects** — the 403 interceptor ejected users mid-form; the PENDING
   submit guard allowed duplicate submits.

The common cause: **"it compiles and the code is present" was treated as "it works."** Every one of
these passed `dotnet build` and `tsc --noEmit`. None had been executed. Build success is evidence
of syntax, not behaviour, and it was repeatedly used as though it were both.

A second, related pattern: **claims in story files outran the code.** Task 2 of 3-1 claimed both
token paths were updated; there were three. Story 3-3 claimed it fixed a 404 that was actually a
working 400, and committed that wrong reasoning as an in-code comment. Story 3-2 dismissed two
acceptance criteria with an argument covering one.

## Lessons for the next epic

1. **Run it before calling it done.** No amount of static verification substitutes for one
   execution of the happy path. The `City` bug would have surfaced on the first create attempt.
2. **When a fix is applied, review the fix.** Two of this epic's defects were introduced by
   remediation. The verification round that caught them should be standard after any large patch
   set, not an extra.
3. **Grep for the mechanism before assuming it runs.** FluentValidation was registered and dead;
   `PermissionGuard` was wired and inert; `UseIdentityDBMigration()` had zero callers. Registration
   is not invocation.
4. **A claim in a story file is an assertion that needs evidence.** "Both token paths" should have
   been a grep, not a recollection.
5. **Watch for changes that activate dormant code.** Adding a `charityId` claim silently switched on
   ~17 tenancy call sites in `FamiliesController` that had never run, because .NET matches claim
   types case-insensitively. Nothing in the diff touched that file.

## Debt this epic leaves behind

Recorded in `epic-3-closeout.md` and `deferred-work.md`:

- **No test layer** — excluded by decision. The uncovered code is security-relevant.
- **Three identity migrations pending**, two predating this epic, all applying on next startup.
- Family tenancy for pre-existing rows unverified (query provided).
- `OrphanReportService` unguarded; epic 5 should review the tenancy logic this epic switched on.
- `ngx-bootstrap` incompatible with Angular 18; 20 of 21 controllers bypass `ApiResponse<T>`;
  `Charity.Name` has no unique index.

## Assessment

Scope delivered in full, and the platform gained tenancy enforcement it did not have. But the epic
took three review rounds to reach a state where the code did what its story files already claimed —
and without those rounds it would have shipped a cross-tenant data exposure and a create screen that
could not create.
