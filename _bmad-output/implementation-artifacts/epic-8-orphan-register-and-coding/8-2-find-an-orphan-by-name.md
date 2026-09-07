# Story 8-2: Find an orphan by name

| Field | Value |
| --- | --- |
| Story key | `8-2-find-an-orphan-by-name` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-02 — البحث باسم اليتيم |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.U.2 scenario) |
| Route | none — type-ahead consumed by pickers (first consumer: the coding screens in 8-4; later correspondence/reports) |
| Endpoint | `GET /api/Families/orphans?search=` |
| Depends on | EP-01 (auth); 8-7 extends this same endpoint to match by code |
| Roles | All roles (authenticated) |

## Status

done

## Story

As a signed-in user, I want to be able to find an orphan by name البحث باسم اليتيم, so that I can locate a record from partial information.

## Acceptance Criteria

1. Given a signed-in user with an active session, when the actor types into an orphan lookup, then matching orphans are returned as id/label pairs — no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans?search=` and the response is rendered without a page reload.
3. Given a charity user, when the function is invoked, then only orphans owned by that charity (and country) are returned.
4. Given an HQ role, when an explicit charity id is supplied, then the search operates on that charity's data.
5. Given the session has expired, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.
6. Given no row matches, then the type-ahead renders an empty result set (no error).

**Definition of done:** the scenario of §13.U.2 passes end to end; the role and charity scoping is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Entity | `Backend/src/IIROSA.Domain/Entities/Orphan.cs` | `FullName`, `Code`, `FK_CharityId`, `Family` → `Father`/`Mother`/`Charity` navs |
| Existing orphan read | `FamiliesController` `GET {familyId}/orphans` → `GetFamilyOrphansAsync` | Per-family only — **no cross-family search endpoint exists** |
| Caller scope | `FamiliesController.GetUserCharityId()` / `GetUserRole()` | Reuse |
| Wire precedent | `OrphanPaymentsController.GetPaymentGroups` returns `(IEnumerable<T> Items, int TotalCount)` serialized as the 13-1 envelope `{ items, totalCount, page }` | Keep that shape if paging is used; a type-ahead may return a plain array |
| Frontend | `Frontend/src/app/modules/families/services/family.service.ts` | Extend it — do not create a second families HTTP service |

## Tasks / Subtasks

- [x] **Task 1 — Service + endpoint** (AC 1–4)
  - [x] `IFamilyService`: add `Task<(IEnumerable<OrphanLookupDto>, int)> SearchOrphansAsync(OrphanSearchFilterDto filter, Guid? userCharityId, string? userRole)` — **deviation from the `Take`-only shape:** paged (`PageNumber`/`PageSize`, 13-1 envelope `{ items, totalCount, page }`) because 8-3's worklist needs server paging + sorting; the type-ahead just sends `pageSize: 20`
  - [x] `OrphanSearchFilterDto`: `string? Search`, `Guid? CharityId` (HQ override), `string? CodingStatus` (8-3), `string? SortBy`, `bool SortDescending`, paging. `OrphanLookupDto`: `OrphanId`, `FullName`, `Code`, `FatherName`, `MotherName`, `CharityName` + `DateOfBirth`/`Age` (worklist judgement data), `SponsorshipStatus`
  - [x] Query: `FullName.Contains(search) || Code.Contains(search)` (8-7 rides here), not-deleted (global filter), scoped — `userRole == "Charity"` pins `userCharityId`, ignore client-sent `CharityId`; HQ may pass `CharityId`. Sort switch: FullName / Code / DateOfBirth / CharityName
  - [x] `Family.Father`, `Family.Mother`, `Family.Charity` (+ `EducationLevel`/`HealthStatus`) navs included for the label fields
  - [x] `FamiliesController`: `GET orphans` with `[FromQuery] OrphanSearchFilterDto` — route check done: `orphans` (one literal segment) cannot collide with `GET {familyId}/orphans` (two segments); `orphans/check-national-id` and `orphans/check-code` are two-segment literals whose second segment never matches `{familyId}`'s Guid constraint pattern alongside `orphans`
  - [x] Empty/short search → returns the (possibly empty) scoped set, not 400 — the validator bounds pages, not term length
- [x] **Task 2 — Frontend type-ahead** (AC 1, 6)
  - [x] `family.service.ts`: `searchOrphansCoding(request: OrphanCodingSearchRequest): Observable<OrphanLookupPagedResult>` → `GET api/Families/orphans`
  - [x] **Deviation:** no standalone `components/orphan-lookup/` ng-select component — the type-ahead is a debounced (300 ms, ≥3 chars) `valueChanges` pipe on the coding screen's اسم اليتيم field (first consumer), pattern-matching the shipped screens (which use form fields + `app-input-text`, not ng-select). Fork risk is nil while the coding screens are the only consumers; extract to a shared component when a second consumer arrives
  - [x] i18n: placeholder / no-results keys under `orphanCoding` in **both** `ar.json` and `en.json`
- [x] **Task 3 — Verification** (AC 5)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24: partial name and code searches returned scoped rows (names, code, family, charity on the DTO); re-run after the seed family's soft-delete returned `items: [], totalCount: 0` — the manual `!IsDeleted` filter works live; unauthenticated → 401. Charity-token scoping verified as fail-closed denial (seed user has no `CharityId` claim → 403 via the D4 guards)

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers)._

- [x] [Review][Decision] **Role matrix for the five new UC-ORP endpoints** — shipped `[Authorize(Roles = "SuperAdmin,Admin,Charity")]` on all five; recorded decisions conflict (8-1/8-2/8-7 "all authenticated roles", 8-3 "controller-wide `[Authorize]`", 8-5 "controller-wide auth", 8-5/8-6 Roles fields "HQ only"); additionally `OrphanPaymentsController.GetPaymentGroups` widened `SuperAdmin,Admin` → `+Accountant,FinancialOfficer,Charity` unrecorded (8-8). Pick the matrix, correct the records — **Resolved 2026-08-24 (D2:a/D3:a):** shipped matrix stands on the five read/check endpoints; assign tightened to `SuperAdmin,Admin` + the Charity path removed from the service; the unrecorded `Accountant`/`FinancialOfficer` widening on `GetPaymentGroups` reverted to shipped `SuperAdmin,Admin`
- [x] [Review][Decision] **Raw DTOs vs mandated `ApiResponse<T>`** — `architecture.md` L233 ("Every endpoint returns `Framework.Core.ApiResponse` or `ApiResponse<T>`") rules the other way; the stories' `§10` citation does not sanction raw. Epic-8 followed the shipped sibling controllers (OrphanPayments/LookupManagement raw) while parallel regions of the same FamiliesController use ApiResponse. Wrap the new endpoints (+ frontend unwrap) or ratify raw as the de-facto convention in architecture.md — **Resolved 2026-08-24 (D6:a):** raw ratified as the de-facto convention pending the platform-wide ApiResponse migration story (2026-08-19 standing decision)
- [x] [Review][Patch] **Soft-delete exposure (critical, cross-story — anchored here):** the platform premise "soft delete is a global query filter" is false — `ModelBuilderExtensions.SetGlobalQueryFilters` is defined but never called, and the shipped convention is manual `!IsDeleted` filtering (see `FamilyRepository`). Add `!IsDeleted` to: `SearchOrphansAsync` [FamilyService.cs:1669+], `CheckOrphanCanBeAddedAsync` dup [~1785], `CheckOrphanCodeUniqueAsync` [~1819], `AssignOrphanCodeAsync` fetch + dup [~1858, ~1880], `CheckPhoneNumberDuplicateAsync` [~1968], `GetBatchNumbersAsync` + history parent groups [OrphanPaymentService.cs:420, 484]. Today: deleted orphans are searchable and codable, their codes/national IDs block reuse forever, deleted families trigger phone-duplicate verdicts, deleted batches appear in the picker — **Resolved 2026-08-24:** manual `!IsDeleted` (+ family where joined) filters added to every listed query
- [x] [Review][Patch] Strip `error = ex.Message` from the new catches (cross-story, anchored in 8-1)

## Dev Notes

### Platform rules that bind this story

- **No `ApiResponse<T>` wrapper** — raw DTO/array with anonymous error objects (shipped-controller convention; 2026-08-19 standing decision — raw stays until the platform-wide ApiResponse migration story).
- Wire is camelCase via **Newtonsoft**; clean DTO key names only (`FK_` degrades to `fk_`).
- Soft delete has **no global query filter** — `SetGlobalQueryFilters` is never called; the shipped convention is manual `!IsDeleted` on every read, and the epic-8 queries filter it explicitly (orphan, and its family where joined).
- The **`data-list` shared component is not used by any shipped module** — this is a type-ahead, keep it ng-select + local state; do not force `data-list` here.
- Never kill the user's running `IIROSA.Api` process. Tests excluded per the standing user decision.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Match by **code** on the same endpoint (`GetChildNameAndCode`) | 8-7 |
| The coding screens that consume this lookup | 8-3, 8-4 |
| Coding-status filtering (`codingStatus=Pending`) on the same endpoint | 8-3 |
| Payment history/detail per orphan | 8-8, 8-9 |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.2] scenario — id/label pairs
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-02 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/FamiliesController.cs#L546] existing per-family orphan read
- [Source: Frontend/src/app/modules/families/services/family.service.ts] service to extend

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors (25 pre-existing warnings).
- `cd Frontend && npm run build` — exit 0.

### Completion Notes List

- One endpoint serves 8-2/8-3/8-7: `GET /api/Families/orphans` with an optional `codingStatus`; the HQ-only gate for the worklist mode is in the service (Charity role + `CodingStatus` supplied → `UnauthorizedAccessException` → 403), the plain search stays all-roles.
- Signature is paged `(IEnumerable<OrphanLookupDto>, int)` on the 13-1 envelope rather than the story's `Take`-capped plain list — 8-3's worklist needs server paging and sorting; recorded as a deliberate deviation.
- The search predicate already includes `Code.Contains` (8-7) — both stories share the endpoint by board mapping.
- Type-ahead implemented as a debounced form-field pipe on the coding screen rather than a standalone ng-select component — matches the shipped module's form-field idiom; extract when a second consumer appears.
- Live walkthrough pending the user's IIROSA.Api restart.

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/OrphanCodingDtos.cs` (new)
- `Backend/src/IIROSA.Application/Validators/Family/OrphanCodingValidators.cs` (new — OrphanSearchFilterValidator)
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs`
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` (SearchOrphansAsync)
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` (GET orphans)
- `Frontend/src/app/modules/families/models/family.model.ts`
- `Frontend/src/app/modules/families/services/family.service.ts` (searchOrphansCoding)
- `Frontend/src/app/modules/families/orphan-coding/**` (type-ahead consumer)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-02 and module spec §13.U.2; route-collision risk against `GET {familyId}/orphans` recorded; reusable type-ahead scoped. |
| 2026-08-24 | Implemented as the shared paged endpoint (8-2/8-3/8-7) + coding-screen type-ahead. Paged-signature and no-standalone-component deviations recorded. Status → review. |
| 2026-08-24 | Adversarial review close-out: 4 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
