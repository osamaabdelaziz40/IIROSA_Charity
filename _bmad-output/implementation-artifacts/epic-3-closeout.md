# Epic 3 — Charity Administration: closeout

All 9 stories `done` as of 2026-08-19. Backend builds with 0 errors; frontend typechecks with
0 errors. Nothing committed.

## Test layer — excluded, not deferred

By explicit user decision the test layer is **out of scope for this epic**. `Backend/tests/` holds
no test project and no `.spec.ts` files were added. Every story file records this as an accepted
exception rather than an open task, so nothing is left dangling.

The code this leaves uncovered is security-relevant, and worth naming plainly:

- `CharityService.ApplyCallerScope` — four branches; the fail-open bug that shipped in the first
  pass lived in the branch a happy-path manual test never reaches.
- `CharityWriteGuard` — decides whether a locked or restricted charity may write.
- `AuthService.PERMISSION_ROLES` — must be kept in step with the controllers' `[Authorize]`
  attributes by hand.
- `CreateCharityValidator` / `UpdateCharityValidator`.

## Two things to do before running this against a real database

### 1. Three identity migrations are pending, not one

`dotnet ef migrations list --context AppIdentityDbContext` reports:

```
20260513174128_AddRefreshToken            (Pending)
20260513194130_AddRefreshTokenTable       (Pending)
20260819073139_AddCharityAndCountryToApplicationUser  (Pending)
```

The two refresh-token migrations predate this epic and were never applied — the identity database
is three migrations behind, not one. `Program.cs` now migrates `AppIdentityDbContext` at startup,
so **all three will apply the first time the API runs**. That is correct but worth knowing in
advance, because the two older ones were not reviewed as part of this work.

They were deliberately **not** applied here: applying migrations somebody else wrote, to a real
database, is a decision for whoever owns that database.

### 2. Family tenancy for pre-existing rows

New families are consistent by construction: `FamiliesController.CreateFamily` stamps
`dto.CharityId` from the caller's tenancy claim, the same claim `ApplyCallerScope` and
`CharityWriteGuard` read.

The open question is historical rows, whose `FK_CharityId` was set by other means. If a charity's
`Family.FK_CharityId` values disagree with the `ApplicationUser.CharityId` the migration backfilled,
that charity's family register will read as empty. To find any mismatch:

```sql
SELECT c.Id AS CharityId, c.Name, u.Id AS UserId,
       COUNT(f.Id) AS FamiliesUnderCharity
FROM [IIROSA].[Charity] c
LEFT JOIN [identity].[Users] u ON u.CharityId = c.Id
LEFT JOIN [IIROSA].[Family] f ON f.FK_CharityId = c.Id
WHERE c.IsDeleted = 0
GROUP BY c.Id, c.Name, u.Id
HAVING COUNT(f.Id) = 0 AND u.Id IS NOT NULL;
```

Rows returned are charities whose login is now scoped to a charity with no families — either
genuinely empty, or a mismatch to investigate.

## Carried forward to other epics

- `OrphanReportService` has no create/update entry point of the guarded shape; unguarded.
- `FamiliesController.GetUserCharityId()` now resolves, activating ~17 previously dormant tenancy
  call sites in epic 5. Bound explicitly to `IiroSaClaimTypes.CharityId` so it cannot flip again
  silently, but epic 5 should review the behaviour it switched on.
- Seeded `Charity@IIROSA.com` has no `CharityId` and no seeded charity links to it, so it now
  correctly sees an empty register. Update the seed data or link it.
- `ngx-bootstrap@^12` is incompatible with Angular 18; `npm install` needs `--legacy-peer-deps`.
- 20 of 21 controllers do not use `ApiResponse<T>`; `Charity.Name` has no unique index.
  Both recorded in `deferred-work.md`.
