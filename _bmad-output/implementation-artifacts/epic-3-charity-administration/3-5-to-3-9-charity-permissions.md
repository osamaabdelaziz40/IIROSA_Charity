# Stories 3-5, 3-7, 3-8, 3-9 — Charity update and write permissions

| Field | Value |
| --- | --- |
| Epic | EP-03 — Charity Administration |
| Use cases | UC-CHR-05, UC-CHR-07, UC-CHR-08, UC-CHR-09 |
| Specification | `docs/Modules/08-UC-CHR-Charity-Administration.md` |

Grouped into one file because 3-7, 3-8 and 3-9 share a single root cause and a single fix.
Story 3-6 was already `done` and was re-verified, not changed.

## Status

done

## Review findings

- **3-5 had no server-side validation**, so every rule story 3-3 added to the create path was
  bypassable: create a minimal record, then amend it. Conflicts were also raised as
  `InvalidOperationException`, giving a message with no field identity.
- **3-7, 3-8 and 3-9 were toggles that did nothing.** `Charity.IsLocked`, `IsAddEnabled` and
  `IsUpdateEnabled` were persisted, filtered on and rendered as working controls in the UI — and a
  repo-wide grep found **no service anywhere that read them**. Head office could lock a charity or
  withdraw its add rights and the charity carried on working. This defeated the epic's stated goal,
  "control what each of them may add or change".

## What was implemented

- **`UpdateCharityValidator`** mirroring the create validator, invoked in `UpdateCharityAsync`,
  with conflicts raised as field errors and a `ValidationException` catch on the update endpoint.
  It also adds the `BankId` `GreaterThan(0)` rule the create validator was missing — `BankId` is a
  `Restrict` FK, so `0` reached SQL and surfaced as a 500 leaking constraint detail.
- **`ICharityWriteGuard` / `CharityWriteGuard`** — the single place the three flags are read.
  `EnsureCanAddAsync` / `EnsureCanUpdateAsync` resolve the caller's charity from the tenancy claim
  and refuse when it is locked or the relevant permission is off. Locked outranks the individual
  flags. Head-office callers are exempt, because the flags exist for head office to restrain a
  charity. A claim naming a charity that no longer exists is refused rather than allowed through.
- **`CharityWriteForbiddenException` → HTTP 403** in `ExceptionMiddleware`. A dedicated type keeps
  it away from the `InvalidOperationException` case, which maps to 404.
- **Wired into `FamilyService.CreateFamilyAsync` and `UpdateFamilyAsync`** — the family register is
  the charity-facing write these permissions exist to govern.
- **Update-path server errors now flag form fields**, matching the create path.

## Scope boundary — important

The guard is wired into **`FamilyService` only**. `HousingProjectService`, `OrphanPaymentService`,
`OrphanReportService` and `OfficeProjectService` also perform charity-scoped writes and are **not**
yet guarded, so a locked charity can still write through those paths. Each belongs to its own epic
(5, 6, 9, 10, 13) and wiring them blind, without reading their use cases, is how the accidental
`FamiliesController` activation happened in the 3-1 review round. The mechanism is in place and
each call site is one line.

## Accepted exceptions to the definition of done

1. **No tests** — consistent with 3-1 through 3-4. `CharityWriteGuard` is security-relevant.
2. **`OrphanReportService` is unguarded** — it has no create/update method of the expected shape.
   Worth a look when epic 9 is picked up.

## File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Application/Interfaces/ICharityWriteGuard.cs` | New — guard contract + exception |
| `Backend/src/IIROSA.Application/Services/CharityWriteGuard.cs` | New — reads the three flags |
| `Backend/src/IIROSA.Application/Validators/Charity/UpdateCharityValidator.cs` | New — update rules |
| `Backend/src/IIROSA.Application/Services/CharityService.cs` | Update validator + field-level conflicts |
| `Backend/src/IIROSA.Application/Services/FamilyService.cs` | Guard on create and update |
| `Backend/src/IIROSA.Api/Controllers/CharitiesController.cs` | `ValidationException` catch on update |
| `Backend/src/IIROSA.Api/Middleware/ExceptionMiddleware.cs` | `CharityWriteForbiddenException` → 403 |
| `Backend/src/IIROSA.Api/Program.cs` | Registered `ICharityWriteGuard` |
| `Frontend/src/app/modules/charities/charity-form/charity-form.component.ts` | Update-path error mapping |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Added update validation (3-5) and charity write-permission enforcement (3-7/3-8/3-9). |
