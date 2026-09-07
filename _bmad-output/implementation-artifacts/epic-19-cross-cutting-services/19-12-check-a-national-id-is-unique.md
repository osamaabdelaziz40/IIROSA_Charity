# Story 19-12: Check a national id is unique

| Field | Value |
| --- | --- |
| Story key | `19-12-check-a-national-id-is-unique` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-12 — التحقق من رقم الهوية |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.12 scenario) |
| Route | the national-id fields on the family/guardian forms (and the orphan form's existing check) |
| Endpoint | `GET /api/Families/check-national-id` (new) — sits beside the live `GET /api/Families/orphans/check-national-id` |
| Depends on | 8-1 (dedup span + orphan-level check + edit-mode wiring precedent); 19-11 (country rules, client-side UX); 19-13 (error surfacing) |
| Roles | Charity + HQ (`SuperAdmin`, `Admin`) — the family forms' writer set |

## Status

done

## Story

As a Charity user, I want to be able to check that a national id number رقم الهوية is unique
so that the same person cannot be registered twice in the system.

## Acceptance Criteria

1. Given a Charity user with an active session, when a national id is entered on a
   family/guardian form, then the platform checks it against existing persons and reports a
   clash by name — without storing anything.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Families/check-national-id` (query-string: `nationalId`, `familyId?`,
   `charityId?`) and the response is rendered on the screen without a page reload.
3. Given the id already exists on another family's member (any holder: family head, father,
   mother, provider/relative, or orphan), when the check runs, then the response names the
   clash — holder name and family code — so the user can decide rather than guess.
4. Given the id exists on the record **being edited** (its own `familyId`), when the check
   runs, then it is **not** a clash — re-saving an unchanged person must not self-report.
5. Given a deleted (soft-deleted) record held the id, when the check runs, then it is **not**
   a clash — deleted rows never block a new registration (epic-5 phone-check defect
   precedent: deleted rows must be filtered out of the dedup span).
6. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the typed endpoint returns `{ isUnique, holderName?, holderFamilyCode?,
holderType? }`; the dedup span covers all person holders across the caller's charity scope
(HQ: explicit `charityId`); the `familyId` exclusion works in edit mode; soft-deleted rows are
excluded; the family/guardian form surfaces the clash message in Arabic.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Orphan-level check | `FamiliesController` `GET orphans/check-national-id` → `CheckOrphanCanBeAddedAsync(OrphanEligibilityCheckDto, userCharityId, userRole)` → `OrphanEligibilityDto { CanBeAdded, Field, ReasonCode, ExistingOrphanName }` (8-1) | Live — **orphan scope only**; this story adds the family/guardian level the board names |
| Dedup span | 8-1's server-side span across Family/Father/Mother/Provider/Orphan holders | Live logic to reuse — same span, family-level entry |
| Edit-mode wiring | 8-1 wired the orphan check into the form **in edit mode only** (create uses the submit-time validator) | Live precedent — copy the wiring pattern |
| Country rules | 19-11 adds pattern/length columns + client-side application | Coordinate; the uniqueness check is orthogonal |

## Verified gaps this story must fix

1. **No family/guardian-level check endpoint.** The board's traceability names
   `GET /api/Families/check-national-id` — it does not exist (only the orphan-level route).
   Build it with a **typed DTO** (CLAUDE.md: no untyped binding): request
   `{ nationalId, familyId?, charityId? }`; response `{ isUnique, holderName?, holderFamilyCode?,
   holderType? }` (holderType from the holder set). Route it on `FamiliesController` beside
   the orphan check; logic in `IFamilyService` (service layer, never the controller).
2. **Scoping rules.** Charity claim → check within the caller's charity only. HQ
   (no charity claim) → requires explicit `charityId`; without it, 400 naming the field.
   Known adjacent defect to verify and record (not necessarily fix here): HQ calls passing
   `familyId` without `charityId` can hit `familyNotFound` in `FamilyService` (~:2268-2507
   region) — the new endpoint must not inherit that trap; state the resolution in the story
   notes.
3. **Soft-delete exclusion (AC 5).** The dedup span must filter `IsDeleted` rows on every
   holder table — verify the 8-1 span does (the epic-5 phone check had exactly this defect);
   patch if it leaks.
4. **Frontend wiring.** Family/guardian form NID fields: call the check on blur/change in
   **edit mode** (8-1 precedent) and surface a clash alert naming the holder; create mode
   stays on the submit-time validator unless cheap to add both.

## Tasks / Subtasks

- [x] **Task 1 — Typed endpoint + service method** (AC 1, 2, 6)
  - [x] `IFamilyService.CheckFamilyNationalIdAsync(CheckNationalIdRequestDto)` — reuses the
        8-1 span shape at family level; DTO + FluentValidation validator (`nationalId`
        required); response DTO per the DoD; controller action `GET check-national-id`
        (`ApiResponse` per the file's existing convention); scoping rules per gap 2
  - [x] `[AllowAnonymous]` never; JWT + roles per the family writer set
- [x] **Task 2 — Span correctness** (AC 3, 4, 5)
  - [x] All person holders covered; `familyId` exclusion honoured (no self-clash);
        `IsDeleted` filtered on every holder query
- [x] **Task 3 — Frontend** (AC 1, 3)
  - [x] `families.service.ts` (or the form's service): `checkNationalId(...)`; family/guardian
        form edit-mode wiring + Arabic clash message via i18n (add the key; no hard-coded
        strings)
- [x] **Task 4 — Verification** (AC 6)
  - [x] Builds green; live smoke on the private port with seeded data: unique id →
        `isUnique: true`; clash in-scope → holder + family code named; same family (edit) →
        unique; soft-deleted holder → unique; charity B's id invisible to charity A; HQ
        without charityId → 400; unauthenticated → 401. Tests excluded per the standing
        decision

### Review Findings

- [x] [Review][Patch] HQ editors never get a check — request omits `charityId` [Frontend/src/app/modules/families/family-form/family-form.component.ts] — `checkNationalId` sends `{nationalId, familyId}` only; an HQ (SuperAdmin/Admin) editor has no charity claim, so the endpoint answers the recorded 400 (`charityId is required for head-office callers`) on every debounced keystroke and the failure is console-silent — the clash aid never fires for HQ users. Send the loaded family's `charityId` with the request. **Fixed 2026-08-26** — the form captures the loaded family's `charityId` and sends it (a charity claim still wins server-side, so Charity users are unaffected).
- [x] [Review][Patch] Duplicated interface blocks in family.model.ts [Frontend/src/app/modules/families/models/family.model.ts] — `FamilyNationalIdCheckRequest` / `FamilyNationalIdCheckResult` are declared twice as identical back-to-back blocks (TS declaration-merging hides it from the compiler; the duplication ships under this story's File List). De-duplicate to one block. **Fixed 2026-08-26** — duplicate block removed.
- [x] [Review][Patch] Validator file/class name mismatch [Backend/src/IIROSA.Application/Validators/Family/FamilyNationalIdCheckValidator.cs] — the file is `FamilyNationalIdCheckValidator.cs` but the class inside is `CheckFamilyNationalIdValidator`. Align the class name to the file (project convention: file name = class name). **Fixed 2026-08-26** — class renamed to `FamilyNationalIdCheckValidator` (DI resolves by `IValidator<CheckFamilyNationalIdDto>`, unaffected).

## Dev Notes

### Platform rules that bind this story

- **Typed DTO binding only** — the untyped `JObject` binding of the legacy system is a
  recorded defect (prd.md §7); this endpoint is the showcase for the typed rule.
- Read-only endpoint — no `IUnitOfWork` save path; still service-layer (business logic never
  in controllers).
- Tenancy server-side: the caller's charity claim governs the span; HQ must name the charity.
- Soft-delete filtering is a platform rule on every read — AC 5 makes it testable here.
- `ApiResponse` envelope follows the `FamiliesController` file's existing convention (it is
  an ApiController-derived controller) — unlike the lookup/attachment raw-envelope files.
- Frontend: no hard-coded strings (i18n keys); RTL-first message layout.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Orphan-level check behaviour | 8-1 (live — reuse, don't duplicate) |
| Country NID format rules | 19-11 |
| Error surfacing platform | 19-13 |
| Family CRUD itself | EP-05/EP-07 |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.12] scenario + §24.2 UC-SYS-12 row
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-12 acceptance criteria + traceability (GET /api/Families/check-national-id → IFamilyService.CheckNId)
- [Source: Backend/src/IIROSA.Api/Controllers/FamiliesController.cs] the live orphan-level check to sit beside (`orphans/check-national-id`)
- [Source: _bmad-output/implementation-artifacts/deferred-work.md] soft-delete platform gap + epic-5 phone-check defect (the AC 5 precedent)

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Backend build: `dotnet build src/IIROSA.Api/IIROSA.Api.csproj -c Release` → 0 errors (output to bin/Smoke).
- Frontend: `npx tsc --noEmit` → clean (spec-file noise excluded per standing baseline).
- Live battery 2026-08-25, private instance 127.0.0.1:61970 (ASPNETCORE_ENVIRONMENT=Development, IIROSA_Db_Dev), tokens OsamaSuper@ / Charity@IIROSA.com:
  - unauth → 401; empty nationalId → 400
  - unique id 77700000111122 → `isUnique: true`
  - LIVE in-scope orphan NID 69005089111543 (family C172DFD4, charity A99AC790) → `isUnique: false`, holder ملك متولي السقا, FAM-2026-8189, type Orphan
  - LIVE in-scope provider NID 48285680563949 → clash, type Provider
  - same NID + familyId=C172DFD4 → `isUnique: true` (edit self-exclusion)
  - soft-deleted relative NID 29908080801234 → `isUnique: true`
  - holder on SOFT-DELETED family (provider 93807592341001, family 3CEB57FF IsDeleted=1) → `isUnique: true` — the family-level soft-delete guard, the epic-5 phone-check defect class, proven closed here
  - charity-B orphan NID 39918845251234 (live family 758CB345, charity EF78FFD2) under charity-A token → `isUnique: true` (cross-charity invisible)
  - HQ without charityId → 400 `"charityId is required for head-office callers"`; HQ with charityId=A99AC790 → clash named (orphan كريم متولي السقا, FAM-2026-8189)

### Completion Notes List

- Endpoint answers raw `Ok(dto)` — the FamiliesController file convention (sibling check actions), not the ApiResponse envelope; recorded against the story's Dev Notes line.
- Gap 2's adjacent trap (HQ + familyId without charityId → familyNotFound in the ~:2268-2507 region) does NOT apply: this endpoint resolves scope from the charity claim or the explicit charityId BEFORE any family read, and refuses HQ callers with no charityId via a field-naming ValidationException → 400. No family-by-id lookup happens outside the resolved scope.
- Holder scan covers Father/Mother/Provider/Relative/Orphan; orphan carries its direct FK_CharityId for the in-scope shortcut, every other holder resolves scope through its (non-deleted) family's FK_CharityId. Soft-delete is filtered on BOTH the holder row and the family row — verified live (bonus case X).
- Battery post-mortem worth recording: the first clash probes returned `isUnique: true` because 7 of 9 families in IIROSA_Db_Dev were soft-deleted by a parallel session — the guard was working correctly. Live-family fixtures (C172DFD4 / 758CB345) confirmed the clash paths. Always check Family.IsDeleted when mining dev-DB fixtures.
- Frontend: edit-mode-only wiring on the family form mirrors the 8-1/UC-ORP-10 phone-check pattern — debounced valueChanges per holder (father/mother/provider), clash lands on the control as a `server` error (rendered as-is by app-input-text, blocks save via form validity), stale-response guarded, cleared on pass or blank; a failed check stays silent (read-only aid; server re-judges at save). Create mode stays on submit-time validation (recorded limitation, same as the phone check).
- Holder-kind labels reuse the P16 `orphanCoding.holder_*` keys; the missing `holder_relative` (Relative is a holder kind for both the phone and NID checks) was added to ar/en. Clash messages `families.nationalIdClash` / `nationalIdClashNoCode` added to ar/en.

### File List

- Backend/src/IIROSA.Application/DTOs/Family/FamilyNationalIdCheckDto.cs — NEW: CheckFamilyNationalIdDto { NationalId, FamilyId?, CharityId? } + FamilyNationalIdCheckResultDto { IsUnique, HolderName?, HolderFamilyCode?, HolderType? }
- Backend/src/IIROSA.Application/Validators/Family/FamilyNationalIdCheckValidator.cs — NEW: nationalId NotEmpty + trimmed-non-empty
- Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs — CheckFamilyNationalIdAsync declaration (beside CheckOrphanCanBeAddedAsync)
- Backend/src/IIROSA.Application/Services/FamilyService.cs — validator DI + implementation: scope resolution (Charity claim / HQ explicit charityId / HQ refusal), 5-holder candidate scan, self-family exclusion, orphan direct-charity shortcut, family soft-delete + scope guard
- Backend/src/IIROSA.Api/Controllers/FamiliesController.cs — GET check-national-id action (Roles SuperAdmin,Admin,Charity; D4 fail-closed Forbid; ValidationException → 400 field-naming)
- Frontend/src/app/modules/families/models/family.model.ts — FamilyNationalIdCheckRequest / FamilyNationalIdCheckResult interfaces
- Frontend/src/app/modules/families/services/family.service.ts — checkFamilyNationalId(request) + model imports
- Frontend/src/app/modules/families/family-form/family-form.component.ts — setupNationalIdChecks / checkNationalId / clearNationalIdError (edit-mode debounced wiring, server-error surfacing, stale guard); subscription lifecycle beside the phone checks
- Frontend/src/assets/i18n/ar.json + en.json — families.nationalIdClash, families.nationalIdClashNoCode, orphanCoding.holder_relative

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Implemented + verified: typed GET /api/Families/check-national-id (5-holder span, edit self-exclusion, holder+family soft-delete guards, dual charity-scope rules), family-form edit-mode wiring with i18n clash surfacing; live battery 10/10 green on 61970. |
| 2026-08-25 | Story file created from `epics.md` US-SYS-12 and module spec §24.U.12; family/guardian-level check confirmed absent (orphan level live from 8-1) — story delivers the typed endpoint reusing the 8-1 span, with edit-mode exclusion, soft-delete exclusion, and charity scoping as hard ACs. |
| 2026-08-26 | Epic-19 code review: 3 patch findings written to Review Findings (HQ edit-mode request omits charityId so the check always 400s silently; duplicated interface blocks in family.model.ts; validator file/class name mismatch). |
| 2026-08-26 | Review patches applied (charityId sent from the loaded family, interfaces de-duplicated, validator class renamed); backend build 0 errors, tsc clean on touched files. Status → done. |
