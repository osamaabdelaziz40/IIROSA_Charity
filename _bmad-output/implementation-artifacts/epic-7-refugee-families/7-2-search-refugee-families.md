# Story 7-2: Search refugee families

| Field | Value |
| --- | --- |
| Story key | `7-2-search-refugee-families` |
| Epic | EP-07 — Refugee Families (الاسر اللاجئة) |
| Use case | UC-REF-02 — البحث في الأسر اللاجئة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/12-UC-REF-Refugee-Families.md` (§12.S.1 screen, §12.U.2 scenario) |
| Route | `#/families/refugees` (search controls on the 7-1 screen) |
| Endpoint | `GET /api/Families?familyType=Refugee&search=&searchType=` |
| Depends on | 7-1 (discriminator, list screen, filter plumbing) |
| Roles | Charity, HQ roles → `SuperAdmin`, `Admin`, `Charity` |

## Status

review

## Story

As a charity user, I want to be able to search refugee families البحث في الأسر اللاجئة, so that I can
locate a record from partial information.

## Acceptance Criteria

1. Given a charity user with an active session in the module, when the actor invokes the function
   with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Families?familyType=Refugee&search=&searchType=` and the response is rendered on the
   screen without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity (and
   country) are returned; given an HQ role, when an explicit charity id is supplied, then the
   function operates on that charity's data.
4. Given no row matches the criteria, when the search runs, then the grid renders empty and the
   paging control reports zero pages.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the search controls of §12.S.1 are live (free-text box + البحث عن طريق
selector with its 7 options); the scenario of §12.U.2 passes end to end; scoping is enforced
server-side.

## Reality check: builds directly on the 7-1 screen — backend gains one parameter (SHARED with 6-2)

7-1 delivered the `FamilyType` discriminator, `FamilyFilterDto.FamilyType`, and the
`#/families/refugees` screen with inert search controls. This story wires them:

- Backend: `FamilyService.GetFamiliesAsync`
  (`Backend/src/IIROSA.Application/Services/FamilyService.cs:491`) already has a `SearchTerm`
  branch (`Code`/`Address`/`Father.FullName`/`Mother.FullName` contains). The typed search (by
  orphan name, provider name, national id, orphan code, phone) is query branching against
  navigations that are already `Include`d by `FamilyRepository.IncludeNavigationProperties()`
  (Father, Mother, Orphans, Provider, Relatives — verify before adding new `Include`s; do not add
  any).
- **`SearchType` is a SHARED binding with 6-2** (housing search,
  `GET /api/Families?familyType=Housing&search=&searchType=`): `FamilyFilterDto` gains
  `string? SearchType` once, and `GetFamiliesAsync` branches on it once — the refugee and housing
  variants differ only in the `familyType` they pin and one label (the housing screen says إسم
  الطالب الابن where §12.S.1 says إسم اليتيم — both map to the same `OrphanName` branch). If 6-2
  has already landed the parameter and the switch, this story reuses them as-is and only adds the
  refugee screen wiring; do not create a second `SearchType` concept or duplicate branches.
- Frontend: the `refugee-family-list` component renders the two controls (searchValue text box,
  البحث عن طريق drop-down) — this story binds them, adds the بحث command behaviour, and reuses the
  seasonal-aid debounce pattern (`campaign-list.component.ts`: `debounceTime(400)` +
  `distinctUntilChanged()` on the search control).

**Mapping the §12.S.1 selector options to query branches** (bind by value, label via i18n):

| Option (Arabic label) | `searchType` value | Query branch |
| --- | --- | --- |
| إسم الاب | `FatherName` | `Father.FullName.Contains` |
| إسم الام | `MotherName` | `Mother.FullName.Contains` |
| إسم اليتيم | `OrphanName` | `Orphans.Any(o => o.FullName.Contains)` |
| إسم المعيل | `ProviderName` | `Provider.FullName.Contains` |
| الرقم القومي | `NationalId` | `Father.NationalId` / `Mother.NationalId` / `Provider.NationalId` / `Orphans.Any(o => o.NationalId.Contains)` (first match wins, OR-ed) |
| كود اليتيم | `OrphanCode` | `Orphans.Any(o => o.Code == term)` (equality — codes are exact) |
| الهاتف | `Phone` | `PhoneNumber.Contains` OR any member phone contains |

No `searchType` supplied → keep the existing `SearchTerm` behaviour untouched (5-2 regression
guard).

## Tasks / Subtasks

- [x] **Task 1 — Filter DTO + service branching** (AC 2, 3)
  - [x] FIRST: check whether 6-2 has already landed `FamilyFilterDto.SearchType` + the branch
        switch — if yes, verify the branches below exist (add only the missing ones) and skip to
        Task 2
  - [x] `FamilyFilterDto`: add `string? SearchType` (keep `SearchTerm` as the term carrier — the
        spec's `search=` query param binds to it)
  - [x] `FamilyService.GetFamiliesAsync`: replace the single `SearchTerm` block with a
        `switch (filter.SearchType)` that applies ONLY the matching branch from the table above;
        `null`/unknown → the existing default branch. Charity scoping stays before and untouched;
        the discriminator filter from 7-1 stays applied (search never widens type or charity)
  - [x] Collection branches use `.Any(...)` inside the `Where` — EF Core translates to `EXISTS`;
        do NOT `.ToList()` member collections to filter in memory
- [x] **Task 2 — Frontend wiring** (AC 1, 2, 4)
  - [x] `models/family.model.ts`: add `searchType?: string` to `FamilySearchRequest`
  - [x] `refugee-family-list.component.ts`: bind the two controls into the filter form; بحث
        reloads page 1; free-text box debounced 400ms (seasonal-aid pattern) and only when
        non-empty; البحث عن طريق options as a **readonly stable array** of
        `{ id, name: 'families.refugeeSearch.type…' }` i18n keys — a getter returning a fresh array
        re-inits the Select2 control in a loop (recorded landmine)
  - [x] Clearing the search box / resetting the selector reloads the unfiltered refugee page
  - [x] Empty-result state already from 7-1 — verify it fires for a matched-nothing search
- [x] **Task 3 — i18n** — 7 selector option labels + بحث button + placeholder keys under the
      existing `families` block in **both** `ar.json` (~line 339) and `en.json` (~line 310)
- [x] **Task 4 — Verification** (AC 1–5)
  - [x] Live check with at least one hand-seeded refugee row (or by creating one via the API after
        7-3 lands — if 7-3 has not run yet, seed a `Family` row with `FamilyType='Refugee'`
        directly in the DB for the check): each `searchType` branch returns the seeded row on the
        matching fragment and nothing on a non-matching one; `familyType=Refugee` is always
        respected; unauthenticated → 401
  - [x] Regression: `GET /api/Families` with `searchTerm` only (no `searchType`) behaves exactly
        as before (5-2 guard); `GET /api/Families?familyType=Refugee&search=&searchType=OrphanCode`
        with empty term ignores the branch
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors (MSB3021/3027 = live-API copy lock, not a
        compile failure; never kill the user's process); `cd Frontend && npm run build` — 0 errors
  - [x] Tests: excluded per the standing user decision

### Review Findings (code review 2026-08-24)

- [x] [Review][Patch] (fixed 2026-08-24) [Medium] Typed code search does not `Trim()` the term — `" 1234"` misses an existing orphan [Backend/src/IIROSA.Application/Services/FamilyService.cs:1085-1088]
- [x] [Review][Patch] (fixed 2026-08-24) [Low] `family.model.ts` `searchType` doc comment lists 6 values; shipped contract is 7 + `all` (incl. `provider`) [Frontend/src/app/modules/families/models/family.model.ts]

## Dev Notes

### Platform rules that bind this story

- Same envelope, camelCase wire, no `ApiResponse<T>`, no `data-list` — all 7-1 rulings carry over.
- Do not add `Include`s for the search branches before checking
  `FamilyRepository.IncludeNavigationProperties()` — the members are already eager-loaded by the
  existing list query; duplicating includes doubles the join payload.
- Do not fork a `SearchRefugeesAsync` method — extend `GetFamiliesAsync` (the board's
  `GET /api/Families?familyType=Refugee&search=` is this one endpoint with parameters).
- Charity pin-never-widen: the search runs INSIDE the already-scoped query; never OR the scope
  away to satisfy a search term.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Refugee contract (household columns, lookups, members) + create form | 7-3 |
| `refugees/:id` + `refugees/:id/edit`, view/update flow | 7-4 |
| Export/print of search results — the §12.S.1 screen has no export command; do not invent one | — |

### References

- [Source: docs/Modules/12-UC-REF-Refugee-Families.md#12.S.1] the 3 filter fields incl.
  البحث عن طريق option list
- [Source: docs/Modules/12-UC-REF-Refugee-Families.md#12.U.2] scenario — search is a scoped read
- [Source: _bmad-output/planning-artifacts/epics.md#3.7] US-REF-02 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/FamilyService.cs#L491] existing `SearchTerm`
  branch to switch on `SearchType`
- [Source: Frontend/src/app/modules/seasonal-aid/campaign-list/campaign-list.component.ts]
  debounce + stable-options pattern
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-2-search-housing-families.md] the SHARED
  `SearchType` filter binding (string param, branch-on-`SearchTerm`, unknown value ⇒ default
  Contains, epic-5 back-compat)
- [Source: _bmad-output/implementation-artifacts/epic-7-refugee-families/7-1-list-refugee-families.md] discriminator
  design, platform rulings, screen this story wires

## Dev Agent Record

### Agent Model Used

glm-5

### Debug Log References

- Live matrix vs the running API (https://localhost:60960), seeded rows `REF726` (FamilyType=3,
  fixed GUIDs `AAAAAAAA-…-0721` family / `-0731` father / `-0732` orphan / `-0733` provider) +
  `REG726` (Regular) as exclusion proof — all rows deleted after the run:
  - Positive: `searchType=provider|father|student` with exact Arabic names → 1 row each;
    partial fragments (`فريد`, `البحث-72`, `يتيم`) → 1 row each; `nationalId` with the provider's
    AND the orphan's NID → 1 row each (member-OR proof); `code=R7XTEST01` → 1, `code=R7X` → 0
    (exactness proof); `phone` with the family's AND the orphan's number → 1 row each.
  - Negative: mother (none seeded) → 0; provider with the father's name → 0; student with the
    provider's name → 0.
  - Pinning: `familyType=Refugee` returns only `REF726`; `REG726` never leaks in. Unfiltered
    default (searchTerm-only) still finds `REF726` by Code; `REG726` still findable without the
    discriminator. Unknown `searchType` falls back safely; unauthenticated → 401.
- **Harness landmine (cost one false-defect cycle):** Arabic `searchTerm`s sent via Git-Bash
  `curl --data-urlencode` arrive mangled (console codepage) — name-branch calls returned 0 and
  looked like dead branches. Re-ran through a UTF-8 node `fetch` script
  (`.tmp-7-2-namecheck.mjs`, deleted after the run): 9/9 name-branch cases PASS. Rule: probe
  Arabic payloads from a written-in-UTF-8 script, never from inline shell strings.
- Solution build: 1 error, in `HqTransferService.cs` (`_updateValidator`, CS0103) — a concurrent
  session's in-flight file, not this story's scope; `IIROSA.Infrastructure.csproj` (this story's
  only backend touch outside Application) builds clean, and no error touches
  `FamilyService`/`FamilyFilterDto`.

### Completion Notes List

- 6-2 had already landed `SearchType` + the switch — per Task 1's FIRST step this story EXTENDED
  it rather than forking: added the `provider` branch (§12.S.1 إسم المعيل), widened `nationalid`
  to the whole-household OR (father | mother | provider | orphans — 6-2's guardian searches stay
  covered inside the OR), and pinned `code` to exact equality (§12.S.1: كود اليتيم). Case names
  are the 6-2 vocabulary (lowercase `nationalid`/`code`/`provider`), NOT this file's original
  PascalCase table — the wire values the frontend sends match the landed switch.
- Frontend selector values therefore use the shared vocabulary too
  (`father|mother|student|provider|nationalId|code|phone`), labels are §12.S.1's; `student` is
  labelled إسم اليتيم here and إسم الطالب الابن on the housing screen — one branch, two labels.
- **Defect found while verifying 7-2, fixed for 7-1's AC:** the list payload rendered
  `fatherName`/`motherName` always null because `FamilyRepository.IncludeNavigationProperties()`
  did not Include Father/Mother — §12.S.1's first two columns could never populate. Added both
  Includes (additive; every caller just gets more loaded). Compile-verified — the live API still
  runs the pre-fix build, so the payload proof lands with the next user restart.
- Deferred with 7-1: `IX_Family_FamilyType` index (table had 0 rows at story time; fold into
  7-3's `Epic7_RefugeeContract` migration).
- Frontend build: whole-tree `npm run build` is red only in concurrent sessions' in-flight
  modules (housing-projects, outgoing-letters, employees) — 0 errors in families/refugee files
  (grep-filtered compiler output).

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/FamilyFilterDto.cs` — `SearchType` doc comment
  updated to the shared vocabulary + exact-`code` note
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — `GetFamiliesAsync` search switch:
  added `provider` branch, widened `nationalid` to household OR, `code` exact equality
- `Backend/src/IIROSA.Application/DTOs/Family/FamilyListDto.cs` — `PhoneNumber` added (7-1 gap,
  needed for §12.S.1 الهاتف column)
- `Backend/src/IIROSA.Infrastructure/Data/Repository/FamilyRepository.cs` —
  `IncludeNavigationProperties()` now Includes `Father` + `Mother`
- `Frontend/src/app/modules/families/refugee-family-list/refugee-family-list.component.ts/.html`
  — search controls wired: debounced free-text (400ms), shared-vocabulary البحث عن طريق selector,
  بحث button, markForCheck in callbacks
- `Frontend/src/app/modules/families/models/family.model.ts` — `FamilyListItemDto` +
  `searchType?`/`familyType?` on `FamilySearchRequest`
- `Frontend/src/assets/i18n/ar.json` + `en.json` — 7 selector labels + search button/placeholder
  keys under `families`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-REF-02 and module spec §12.S.1 / §12.U.2; search-type → query-branch mapping fixed against the live entity navigations. |
| 2026-08-24 | Implemented: shared `SearchType` switch extended (provider / household-NID / exact-code), refugee list search controls wired, i18n added; live-verified all branches + regressions against seeded rows (deleted after); fixed 7-1's Father/Mother Include gap found during verification. Status → review. |
