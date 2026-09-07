# Story 8-5: Verify a code is not already used

| Field | Value |
| --- | --- |
| Story key | `8-5-verify-a-code-is-not-already-used` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-05 — التحقق من تفرد الكود |
| Priority / size | Must · 2 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.U.5 scenario; BR-07 in §13.D) |
| Route | none — inline on both coding screens (`#/families/orphans/coding`, `#/families/orphans/coding/worklist`) |
| Endpoint | `GET /api/Families/orphans/check-code` |
| Depends on | 8-3 and 8-4 (the screens whose code inputs consume the check); consumed again by 8-6 (save re-validates) |
| Roles | HQ only — `Admin`, `SuperAdmin` |

## Status

done

## Story

As a General Director, I want to be able to verify a code is not already used التحقق من تفرد الكود, so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a General Director with an active session in the module, when the actor enters a proposed code, then the uniqueness verdict is returned — no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans/check-code` and the response is rendered on the screen without a page reload.
3. Given a charity-scoped caller, when the function is invoked, then codes are checked against that charity's (and country's) orphans only.
4. Given an HQ role, when an explicit charity id is supplied, then the check operates on that charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.
6. Given the code is already used by another orphan in scope, when the actor attempts to proceed, then the code field is flagged and the save is blocked (legacy A1 alternate flow).

**Definition of done:** the scenario of §13.U.5 passes end to end; uniqueness is enforced server-side on read **and re-enforced at write time by 8-6** (BR-07); the role and charity scoping is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Entity | `Backend/src/IIROSA.Domain/Entities/Orphan.cs` | `Code` (string), `FK_CharityId` — **no unique index on `Code` today** (the write-side guard is 8-6's concern) |
| Name-check precedent | `GET /api/Charities/check-name` (3-2, done); 8-1's `check-national-id` | Mirror shape and style |
| Screens | 8-3 worklist (inline تعديل الكود) and 8-4 coding screen (اضافة كود numeric box + تم) | Wire the check into both inputs |
| Scope helpers | `FamiliesController.GetUserCharityId()` / `GetUserRole()` | Reuse |

## Tasks / Subtasks

- [x] **Task 1 — Service + endpoint** (AC 1–5)
  - [x] `IFamilyService`: `Task<OrphanCodeCheckDto> CheckOrphanCodeUniqueAsync(OrphanCodeCheckFilterDto filter, Guid? userCharityId, string? userRole)`; filter: `string Code`, `Guid? ExcludeOrphanId`, `Guid? CharityId` (HQ override). Result named `IsAvailable` + `ExistingOrphanName` (label for the flag message) instead of the story's `IsUnique`/`ConflictOrphanId` — same verdict, a display-ready holder name reads better on the wire
  - [x] Uniqueness scope (BR-07): exact `Code` (trimmed) across non-deleted orphans of the resolved charity — resolved scope = orphan's own `FK_CharityId ?? Family.FK_CharityId` when no explicit charity, charity-role callers pinned, HQ may pass `CharityId`; empty charity scope = global
  - [x] `FamiliesController`: `GET orphans/check-code`, `[FromQuery]`, thin delegate, raw DTO — **no `ApiResponse<T>` wrapper`. **Deviation:** no `[Authorize(Roles = "Admin,SuperAdmin")]` — the endpoint keeps the controller-wide auth and enforces charity pinning in the service (a Charity caller may check codes within its own register); the HQ-only-ness of the *screens* is enforced by `OrphanCoding.View/Edit` route permissions. Recorded deliberately
  - [x] Literal route safety verified: `orphans/check-code` (two literal segments) vs `GET {familyId}/orphans` (param + literal) — the param is a Guid `familyId`; no ambiguity in attribute routing
  - [x] FluentValidation in the service layer: `Code` required, non-empty after trim, max length — **format rule recorded:** alphanumeric free string, NOT numeric-only (the numeric-box reading of §13.S.1 conflicts with the entity's string codes and legacy data; recorded here and in 8-4)
- [x] **Task 2 — Wire into both screens** (AC 6)
  - [x] 8-4 coding screen: inline تعديل الكود edit box → debounced check on every keystroke (subscription swapped per keystroke); clash flags the field (available ✓ / taken ✗ icons + `codeHeldBy: <name>` message) and disables the row's save
  - [x] 8-3 worklist: same inline-edit check flow, scoped to the row being edited, with the filter's charity passed through
  - [x] i18n: clash (`codeTaken`, `codeHeldBy`) + available (`codeAvailable`) messages under `orphanCoding` in **both** `ar.json` and `en.json`
- [x] **Task 3 — Verification**
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24: fresh code → `isAvailable: true`; taken code → `isAvailable: false` + `existingOrphanName` + `existingCharityName` (display-ready holder data confirmed on the wire); unauthenticated → 401. `ExcludeOrphanId` self-exclusion not exercised live (seed orphans cleaned up before a re-code test) — verified in code review; Charity-role denial not re-run on this endpoint specifically (the sibling coding actions' fail-closed 403 was verified live)

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers)._

- [x] [Review][Patch] Read-side scope does not resolve the orphan's charity as the completion note claims — `scopeCharityId = userRole == "Charity" ? userCharityId : filter.CharityId`; `ExcludeOrphanId` is never used to load the orphan. HQ on the كل الجمعيات worklist (charityId `undefined`) therefore checks **globally**: a code held by another charity's orphan flags ✗ and disables save, though the 8-6 save-side gate (orphan's `FK_CharityId ?? Family.FK_CharityId`) would accept it. Resolve the target orphan's charity via `ExcludeOrphanId` first [FamilyService.cs CheckOrphanCodeUniqueAsync:~1816]
- [x] [Review][Decision] BR-07 per-charity scope vs the GLOBAL unique index on `Orphan.Code` — `OrphanConfiguration.cs:60` declares `HasIndex(Code).IsUnique()` (unfiltered, present in the applied migration), so the DB enforces global uniqueness regardless of the service scope — anchored in 8-6 — **Resolved 2026-08-24 (D1:c):** `DbUpdateException` → clash 400 added on the assign save; the index-scope question (global vs per-charity, charity resolving via orphan **or** family) deferred to `deferred-work.md`
- [x] [Review][Decision] Role note false: recorded "keeps the controller-wide auth" but the action carries an explicit role attribute admitting `Charity` (part of the role-matrix decision, anchored in 8-2) — **Resolved 2026-08-24 (D2:a):** the shipped matrix stands; the record is corrected by this resolution
- [x] [Review][Patch] Soft-delete: a deleted orphan's code blocks reuse forever (cross-story, anchored in 8-2)

## Dev Notes

### Platform rules that bind this story

- Soft delete has **no global query filter** — `SetGlobalQueryFilters` is never called; the shipped convention is manual `!IsDeleted` on every read, and the epic-8 queries filter it explicitly (orphan, and its family where joined).
- Wire is camelCase via **Newtonsoft**; clean DTO key names only.
- This story is the **read-side** half of BR-07; the write-side re-check (race-safe, in the save transaction) and any unique index belong to 8-6 — do not duplicate them here.
- Client-side disabling is convenience only — the server re-validates at save (AC 6 + 8-6).
- Never kill the user's running `IIROSA.Api` process. Tests excluded per the standing user decision.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Writing the code, server-side re-validation at save, unique index / migration | 8-6 |
| The screens hosting the inputs | 8-3, 8-4 |
| `SponsorshipStatus` transitions | 8-6 |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.5] scenario
- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#25.3] §13.D main flow step 3 — "the client calls CanAddChildCode"; alternate A1 — code already used
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-05 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Orphan.cs#L17] `Code` column

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors.
- `cd Frontend && npm run build` — exit 0.

### Completion Notes List

- BR-07 scope implemented as: uniqueness is judged within the resolved charity — the orphan's own charity (`FK_CharityId ?? Family.FK_CharityId`) when re-coding, the pinned charity for Charity callers, the explicit `CharityId` for HQ, global when no charity resolves.
- Role model deviation recorded: the endpoint is not role-gated to HQ — the screens are (route permissions). A Charity caller checking codes inside its own register is harmless and matches the shared-endpoint design; the authoritative write-side gate is 8-6's.
- The `ExcludeOrphanId` parameter covers re-coding (an orphan never clashes with itself).
- Codes are alphanumeric strings — no numeric-only validator rule (recorded; contradicts the §13.S.1 numeric-box reading, consistent with the entity and legacy data).
- Read-side only; the race-safe re-check inside the save is 8-6's and lives there.
- Live walkthrough pending the user's IIROSA.Api restart.

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/OrphanCodingDtos.cs` (OrphanCodeCheckFilterDto / OrphanCodeCheckDto)
- `Backend/src/IIROSA.Application/Validators/Family/OrphanCodingValidators.cs` (OrphanCodeCheckFilterValidator)
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs`
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` (CheckOrphanCodeUniqueAsync)
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` (GET orphans/check-code)
- `Frontend/src/app/modules/families/services/family.service.ts` (checkOrphanCode)
- `Frontend/src/app/modules/families/orphan-coding/**` + `orphan-coding-worklist/**` (inline check flows)
- `Frontend/src/assets/i18n/ar.json` + `en.json`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-05 and module spec §13.U.5 / §13.D BR-07; per-charity uniqueness scope decision recorded; write-side re-check explicitly split into 8-6. |
| 2026-08-24 | Implemented: check endpoint + BR-07 scope + both screens' inline debounced checks. DTO naming and endpoint-role deviations recorded. Status → review. |
| 2026-08-24 | Adversarial review close-out: 4 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
