# Story 6-7: Look up a housing beneficiary by code

| Field | Value |
| --- | --- |
| Story key | `6-7-look-up-a-housing-beneficiary-by-code` |
| Epic | EP-06 — Housing Project (مشروع الاسكان) |
| Use case | UC-HOU-07 — البحث بالكود |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/11-UC-HOU-Housing-Project.md` (§11.U.7 scenario) |
| Route | filter field on `#/housing-projects/:id/reports` (6-6's screen) |
| Endpoint | `GET /api/HousingProjects/projects/{id}/beneficiaries?code=` (reserved in 6-3's controller surface); the board also names `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` — that consumes the resolved id (6-6) |
| Depends on | 6-3 (controller surface), 6-6 (screen hosting the lookup) |
| Roles | `Charity`, `Admin`, `SuperAdmin` |

## Status

done

## Story

As a charity user, I want to be able to look up a housing beneficiary by code البحث بالكود, so
that I can locate a record from partial information.

## Acceptance Criteria

1. Given a charity user with an active session in the module, when the actor invokes the function
   with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` and the response is rendered on the
   screen without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** entering a beneficiary code on the reports screen resolves to the matching
beneficiary's identifying data (name, code, national id, type child/guardian), scoped to the
caller; unknown code → explicit not-found, not a silent empty grid; the resolved id then drives
6-6's report list.

## Reality check

Nothing exists: no `beneficiaries` action on `HousingProjectsController` (inventoried in 6-3),
and the crosswalk's `IOrphanService.GetHousingChildByCode` has no home — **`IOrphanService` does
not exist** in the codebase (verified). The orphan sponsorship `Code` column exists
(`Entities/Orphan.cs:17`, "auto-generated"); code uniqueness among live rows is epic 8's check
(UC-ORP-05) — rely on it, don't re-implement it here.

**Design decision (recorded):** one endpoint serves both needs —
`GET /api/HousingProjects/projects/{id}/beneficiaries` lists the housing family's beneficiaries
(children + guardian, each with `beneficiaryId`, `childOrParent`, code, names, national id) and
accepts `?code=` for exact-code resolution. Codes belong to CHILDREN (sponsorship codes,
`Orphan.Code`); a guardian is resolved by picking from the list, never by code — that matches the
legacy screen (البحث بالكود resolves كود الطالب الابن). Board's `IOrphanService` name is NOT
created — the method lives on `IFamilyService` (the module's service, per 6-3's surface).

## Tasks / Subtasks

- [x] **Task 1 — Application** (AC 1, 2, 3, 4)
  - [x] `IFamilyService.GetHousingBeneficiariesAsync(Guid familyId, string? code)` — load the
        family (must be `FamilyType.Housing`, else 404), project:
        - each child: `beneficiaryId` = orphan id, `childOrParent: 'Child'`, `code`, first/last
          name composition, national id, age hint
        - the guardian: `beneficiaryId` = guardian/provider id, `childOrParent: 'Parent'`,
          names, national id, `code: null`
        - `code` present ⇒ exact match on child codes within the family's charity scope
          (charity-pinned — a code from another charity must NOT resolve; AC 3), 0 rows ⇒ empty
          list (caller renders not-found)
  - [x] Scope with `ICurrentUserService` same as the 6-4 read (foreign family → 404)
- [x] **Task 2 — API** (AC 2, 5)
  - [x] `GET /api/HousingProjects/projects/{id}/beneficiaries` on the re-cut controller (6-3
        reserved the slot): `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`, `[FromQuery]
        string? code`, 200 list (possibly empty); raw envelope + `{ message }` catch — no
        `ApiResponse<T>`
- [x] **Task 3 — Frontend** (AC 1, 2)
  - [x] On 6-6's reports screen: replace the default-beneficiary placeholder with the real
        picker — load `beneficiaries` on entry; كود box + resolve button → `?code=` call →
        selects the matching child (or shows صريح not-found message); selected beneficiary drives
        the 6-6 grid call `by-orphan/{beneficiaryId}?childOrParent=…`
  - [x] Beneficiary summary strip (name · code · national id · النوع) so the operator sees WHO
        is being reported on before 6-8 opens the form — i18n keys into the
        `housingProjects.reports.*` block (both ar.json and en.json)
  - [x] `OnPush`, `trackBy`, `takeUntil(destroy$)`; no page reload
- [x] **Task 4 — Verification** (AC 1–5)
  - [x] Live: housing family with 2 children + guardian → beneficiaries lists 3 rows; `?code=`
        existing child code → that child only; unknown/other-charity code → empty (not-found
        surfaced); non-housing family id → 404; unauthenticated → 401
  - [x] `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

### Review Findings

Code review 2026-08-24: **COMPLIANT — no findings.** Verified: family gate (Housing + in-scope,
404-not-leak), exact charity-pinned code match, guardian Parent row, not-found surfaced,
read-only, summary strip, Provider-Include fix.

## Dev Notes

### Platform rules that bind this story

- No new service/interface invented (`IOrphanService` deliberately NOT created — recorded); no
  controller logic; camelCase wire; soft-delete global filter; `NameAr ?? NameEn`.
- Codes resolve ONLY within the caller's charity scope — tenancy beats convenience (AC 3).
- This story owns no migration (reads the 6-6 discriminator + existing guardians/children).

### Out of scope

| Item | Story |
| --- | --- |
| Report create form consuming the resolved beneficiary | 6-8 |
| The report list itself | 6-6 |
| Code assignment/uniqueness rules | epic 8 (UC-ORP-05/06) |

### References

- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.U.7] scenario — code-before-report flow
- [Source: _bmad-output/planning-artifacts/epics.md#3.6] US-HOU-07 acceptance criteria
- [Source: _bmad-output/planning-artifacts/epics.md#US-HOU-07] crosswalk — `IOrphanService.
  GetHousingChildByCode` legacy name (retired by the recorded decision)
- [Source: Backend/src/IIROSA.Domain/Entities/Orphan.cs#L17] `Code` column
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-3-register-a-housing-family.md] controller
  surface reserving `beneficiaries`
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-6-list-periodic-reports-of-a-housing-beneficiary.md]
  screen hosting this lookup + beneficiary type contract

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness)

### Debug Log References

- Private smoke instance `http://127.0.0.1:60970` (`-c Efmig`, spare port — the user's live API
  untouched); 6-7 checks inside the 6-8 battery `smoke68.mjs` (final run 19/19)

### Completion Notes List

- Implementation pre-dated this dev cycle (epic-6 session); this cycle verified it live against
  the acceptance criteria: seeded housing family (2 children + guardian) → beneficiaries
  returns 3 rows incl. the `Parent` row; `?code=ORP-2026-42717` → exactly that child; unknown
  code → 200 with empty list; unknown family id → 404; unauthenticated → 401.
- One latent defect found and fixed while verifying: `FamilyRepository.
  IncludeNavigationProperties()` never loaded `f.Provider`, so the guardian (`Parent`) row's
  names were silently empty on every housing family — Include added and re-verified (full
  guardian row in the list).
- Frontend picker + summary strip live on 6-6's reports screen; i18n keys in
  `housingProjects.reports.*` (both locales).
- Builds green (`dotnet build` Efmig, `npm run build` — 0 errors). Tests excluded per the
  standing decision.

### File List

Backend:
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs` —
  `GetHousingBeneficiariesAsync(familyId, code, userCharityId, userRole)`
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — implementation (children +
  guardian projection, charity-pinned code match)
- `Backend/src/IIROSA.Application/DTOs/Family/HousingBeneficiaryDto.cs`
- `Backend/src/IIROSA.Api/Controllers/HousingProjectsController.cs` —
  `GET projects/{id}/beneficiaries?code=` (`HousingProjectsController.cs:129`)
- `Backend/src/IIROSA.Infrastructure/Data/Repository/FamilyRepository.cs` —
  `.Include(f => f.Provider)` fix (guardian row population)

Frontend:
- `Frontend/src/app/modules/housing-projects/housing-report-list/` — beneficiary picker, كود
  resolve, summary strip (6-6's screen hosts the lookup per the design decision)
- `Frontend/src/assets/i18n/ar.json`, `en.json` — `housingProjects.reports.*` keys

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-HOU-07 and module spec §11.U.7; `IOrphanService` retired in favour of an `IFamilyService` beneficiaries endpoint; guardian-not-by-code decision recorded. |
| 2026-08-24 | Implementation verified live against all ACs (3-row list, code hit/miss, 404, 401); Provider-Include defect fixed so the guardian row populates. Status → review. |
| 2026-08-24 | Review closed: zero findings; beneficiary resolution re-verified inside the epic-6 review battery (35/35). Status → done. |
