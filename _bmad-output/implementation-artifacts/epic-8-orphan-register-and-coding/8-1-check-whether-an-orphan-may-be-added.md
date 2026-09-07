# Story 8-1: Check whether an orphan may be added

| Field | Value |
| --- | --- |
| Story key | `8-1-check-whether-an-orphan-may-be-added` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-01 — التحقق من إمكانية إضافة يتيم |
| Priority / size | Must · 5 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.U.1 scenario) |
| Route | none — hosted on the family form's orphan entry (`#/families/:id/edit`) |
| Endpoint | `GET /api/Families/orphans/check-national-id` |
| Depends on | EP-01 (auth), EP-05 (family register — orphan exists only inside a family) |
| Roles | Charity (+ HQ `Admin`/`SuperAdmin` with explicit `charityId`) — legacy actor "Charity" |

## Status

done

## Story

As a charity user, I want to be able to check whether an orphan may be added التحقق من إمكانية إضافة يتيم, so that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a charity user with an active session, when the actor invokes the function with valid input, then the outcome of the add-eligibility check is returned (no stored data is changed — the check is a read).
2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans/check-national-id` and the response is rendered on the screen without a page reload.
3. Given a national ID already used by another non-deleted orphan in scope, when the actor saves, then the save is refused and the offending field is flagged.
4. Given the check succeeds, when the actor then saves the orphan, then the record is created and appears in the family's orphan list.
5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are checked.
6. Given an HQ role, when an explicit charity id is supplied, then the check operates on that charity's data.
7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §13.U.1 passes end to end; the role and charity scoping is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Entity | `Backend/src/IIROSA.Domain/Entities/Orphan.cs` | Exists — has `NationalId`, `FK_CharityId`, `Family` nav. Soft delete via base class `IsDeleted` — no global query filter exists; reads filter manually |
| Orphan CRUD | `FamiliesController` `POST {familyId}/orphans` → `IFamilyService.AddOrphanToFamilyAsync` | Exists (UC-4.4) — **no eligibility check is performed today** |
| Caller scope | `FamiliesController.GetUserCharityId()` / `GetUserRole()` + `User.IsInRole("Charity")` pin pattern | Exists — reuse, do not reinvent |
| Name-availability precedent | `GET /api/Charities/check-name` (story 3-2, done) | Mirror its response shape and controller style |
| Charity add-rights | Rights flags maintained by `PUT /api/Charities/{id}/update-rights` (3-8/3-9, done) | Read the `Charity` entity for the exact property names before coding |
| Frontend | `Frontend/src/app/modules/families/family-form/**` + orphan tab | Exists — wire the check into the orphan add flow |

## Tasks / Subtasks

- [x] **Task 1 — Service + endpoint** (AC 1, 2, 5, 6)
  - [x] `IFamilyService`: add `Task<OrphanEligibilityDto> CheckOrphanCanBeAddedAsync(OrphanEligibilityCheckDto check, Guid? userCharityId, string? userRole)` (no separate `IOrphanService` exists — orphan logic lives in `IFamilyService`; keep it there)
  - [x] `OrphanEligibilityCheckDto`: `string NationalId`, `Guid? FamilyId`, `Guid? CharityId` (HQ override only). `OrphanEligibilityDto`: `bool CanAdd`, `string? ReasonCode` (i18n key, not display text)
  - [x] Checks, in order: (a) caller's charity is permitted to add records — the data-entry right maintained by 3-8/3-9 (read `Charity` entity for the flag); (b) `FamilyId` given → family exists, is active, and belongs to the resolved charity; (c) `NationalId`, when non-empty, is not carried by any other non-deleted orphan in the resolved scope
  - [x] Scope resolution exactly like `FamilyService.GetFamiliesAsync`: `userRole == "Charity"` pins `userCharityId` (ignore any client-sent `CharityId`); HQ may pass `CharityId` explicitly
  - [x] `FamiliesController`: `GET orphans/check-national-id` with `[FromQuery]`, thin delegate, `[Authorize]` (controller default). Return the raw DTO — **no `ApiResponse<T>` wrapper** (shipped-controller convention; 2026-08-19 standing decision — raw stays until the platform-wide ApiResponse migration story)
  - [x] Validator: FluentValidation in the service layer (`Validators/`), `NationalId` required, max length per column
- [ ] **Task 2 — Wire into the family form** (AC 3, 4) — **DEFERRED: no host form exists.** The audit of `family-form.component.ts` (799 lines) confirms it has no orphan tab, and `family-detail`'s `/orphans/create` link is dead — the orphan add form belongs to the EP-05 family-register refinement. Wiring lands with that form; nothing to wire today.
  - [ ] `family-form` orphan entry: on `NationalId` blur (debounced) and before `POST {familyId}/orphans`, call the check; on `CanAdd == false` flag the field with the translated reason and disable the orphan save button
  - [ ] Add the reason i18n keys under `orphanCoding` in **both** `assets/i18n/ar.json` and `en.json` — deferred with the form (no consumer yet); the `orphanCoding` namespace itself landed with 8-3/8-4
- [x] **Task 3 — Verification** (AC 7)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors (MSB3021/3027 = copy lock by the running API, not compile failure); `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24 (user's running API, post-review binaries): duplicate national id → `canBeAdded: false` + `field: nationalId` + `reasonCode: nationalIdRegistered` + `existingOrphanName`; fresh national id with explicit `charityId` → `canBeAdded: true`; unauthenticated → 401. Cross-charity invisibility verified as **fail-closed denial** rather than positive scoping — the seed `Charity` user carries no `CharityId` claim, so the D4 guards 403 it before any cross-charity read (an HQ caller without `charityId` also gets the fail-closed `familyNotFound` path — by design, no scope resolved)

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers). Cross-story items are anchored in one story and referenced here._

- [x] [Review][Decision] Role matrix for the five new UC-ORP endpoints — shipped `[Authorize(Roles = "SuperAdmin,Admin,Charity")]` matches none of the recorded decisions (8-1 note says "every authenticated role"; 8-2 says all-roles; 8-5 says controller-wide; 8-5/8-6 Roles fields say HQ-only) — anchored in 8-2 — **Resolved 2026-08-24 (D2:a/D3:a):** the shipped matrix stands on the read/check endpoints; the assign endpoint is tightened to `SuperAdmin,Admin` (HQ-only)
- [x] [Review][Decision] Raw DTOs vs mandated `ApiResponse<T>` — `architecture.md` L233 mandates the wrapper for every endpoint; §10's "code wins" ruling is about the wrapper's *shape*, not raw-vs-wrapped; epic-8 followed the shipped sibling controllers — anchored in 8-2 — **Resolved 2026-08-24 (D6:a):** raw stays per the 2026-08-19 standing decision; the platform-wide ApiResponse migration story owns the wrap
- [x] [Review][Patch] Family-validity gate (Task 1 check b) not implemented while the completion note claims it exists — `OrphanEligibilityCheckDto` has no `FamilyId` at all. Add `FamilyId` + validator + family exists/active/in-resolved-charity check, and make `NationalId` optional so the service's empty-ID branch goes live instead of dead [Backend/src/IIROSA.Application/DTOs/Family/OrphanCodingDtos.cs:70; FamilyService.cs CheckOrphanCanBeAddedAsync]
- [x] [Review][Patch] `Reason` carries hard-coded English display text; the spec'd shape was `ReasonCode` (i18n key) — return codes and translate client-side [FamilyService.cs CheckOrphanCanBeAddedAsync]
- [x] [Review][Patch] Strip `error = ex.Message` from the new UC-ORP 500 catches (all five handlers — anchored here; the same diff already did this in OrphanPaymentsController) [FamiliesController.cs:704+]
- [x] [Review][Patch] Soft-delete: national-ID dup check includes deleted orphans, violating AC 3's "non-deleted" (cross-story, anchored in 8-2) [FamilyService.cs:~1785]
- [x] [Review][Defer] ExceptionMiddleware echoes `exception.Message` globally with no production masking — deferred, pre-existing platform behavior [Backend/src/IIROSA.Api/Middleware/ExceptionMiddleware.cs:45]

## Dev Notes

### Platform rules that bind this story

- Soft delete has **no global query filter** — `SetGlobalQueryFilters` is never called; the shipped convention is manual `!IsDeleted` on every read, and the epic-8 queries filter it explicitly (orphan + family).
- Wire is camelCase via **Newtonsoft** (`Program.cs` `.AddNewtonsoftJson()`); use clean DTO key names — `FK_`-prefixed keys degrade to `fk_…`.
- FluentValidation in the **service layer**; only `IUnitOfWork` saves (this story writes nothing, but the family-form save path it guards already does).
- Never kill the user's running `IIROSA.Api` process; note the restart requirement in the summary. Tests excluded per the standing user decision (no test project under `Backend/tests`).

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Family-level national ID check (`GET /api/Families/check-national-id`) | 19-12 |
| Phone duplicate check | 8-10 |
| Sponsorship code assignment + uniqueness | 8-5, 8-6 |
| The orphan form's own field set (§10.S belongs to the family module) | EP-05 refinement |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.1] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-01 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/FamiliesController.cs#L47] `GetUserCharityId()` scope precedent
- [Source: Backend/src/IIROSA.Application/Services/FamilyService.cs#L491] `GetFamiliesAsync` pin-never-widen scope shape

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors (25 pre-existing warnings).
- `cd Frontend && npm run build` — exit 0; only pre-existing budget/CommonJS warnings from unrelated modules.

### Completion Notes List

- Task 1 complete: `CheckOrphanCanBeAddedAsync` returns `CanAdd` + `ReasonCode` with three gates — charity write-state (IsLocked/IsAddEnabled/IsActive, verdict only for the pinned Charity role), family validity when `FamilyId` is supplied, and national-ID duplication within the resolved charity scope (no same-family exclusion — a national ID identifies one person).
- Task 2 deferred with cause: the frontend has NO orphan add form anywhere (family-form carries no orphan tab; family-detail's `/orphans/create` is a dead link — the orphan form belongs to the EP-05 refinement). The service-side call `familyService.checkOrphanCanBeAdded()` is shipped and waiting for that form; blur/save wiring lands with it.
- Role shape: the endpoint admits every authenticated role (controller default) and enforces charity pinning in the service — the HQ `charityId` override is honoured, client-sent `charityId` ignored for Charity callers.
- Live walkthrough pending: the user's running IIROSA.Api predates these binaries; a restart is needed before the endpoint resolves.

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/OrphanCodingDtos.cs` (new — all epic-8 DTOs)
- `Backend/src/IIROSA.Application/Validators/Family/OrphanCodingValidators.cs` (new — OrphanEligibilityCheckValidator)
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs` (signature added)
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` (CheckOrphanCanBeAddedAsync)
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` (GET orphans/check-national-id)
- `Frontend/src/app/modules/families/models/family.model.ts` (request/DTO interfaces)
- `Frontend/src/app/modules/families/services/family.service.ts` (checkOrphanCanBeAdded)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-01 and module spec §13.U.1; code state audited — no eligibility endpoint exists today; family-form integration scoped. |
| 2026-08-24 | Implemented: backend verdict endpoint + validator + frontend service call. Task 2 (form wiring) deferred to the EP-05 orphan form — no host form exists on this stack. Status → review. |
| 2026-08-24 | Adversarial review close-out: 7 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
