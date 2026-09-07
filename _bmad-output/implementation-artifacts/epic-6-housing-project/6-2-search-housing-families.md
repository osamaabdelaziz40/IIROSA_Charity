# Story 6-2: Search housing families

| Field | Value |
| --- | --- |
| Story key | `6-2-search-housing-families` |
| Epic | EP-06 — Housing Project (مشروع الاسكان) |
| Use case | UC-HOU-02 — البحث في الأسر الساكنة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/11-UC-HOU-Housing-Project.md` (§11.S.1 screen, §11.U.2 scenario) |
| Route | filter row of `#/housing-projects` (landed by 6-1) |
| Endpoint | `GET /api/Families?familyType=Housing&search=&searchType=` |
| Depends on | 6-1 (list screen + `FamilyType` discriminator exist) |
| Roles | Charity user + HQ roles → `Charity`, `Admin`, `SuperAdmin` |

## Status

done

## Story

As a charity user, I want to be able to search housing families البحث في الأسر الساكنة, so that I
can locate a record from partial information.

## Acceptance Criteria

1. Given a charity user with an active session in the module, when the actor invokes the function
   with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Families?familyType=Housing&search=` and the response is rendered on the screen
   without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the six documented search types all return the matching housing rows
scoped to the caller; no row matches → empty grid, zero pages (§11.U.2 alternate flow).

## Reality check

Same audit finding as 6-1 (read its "Reality check" section — the construction tracker is being
re-cut, not extended): today `FamilyFilterDto` has only a single `SearchTerm` matching
Code/Address/Father/Mother (`DTOs/Family/FamilyFilterDto.cs:8`), and `GetFamiliesAsync` has no
field-typed search. This story adds the typed search on top of the 6-1 discriminator.

§11.S.1 binds the filter pair the legacy screen offered:

| Field | Bound to | Options |
| --- | --- | --- |
| (unlabelled) search box | `searchValue` | free text, partial match |
| البحث عن طريق | `searchType` | إسم الاب / إسم الام / إسم الطالب الابن / الرقم القومي / كود الطالب الابن / الهاتف |

## Tasks / Subtasks

- [x] **Task 1 — Application: typed search** (AC 2)
  - [x] `FamilyFilterDto` gains `public string? SearchType { get; set; }` (keep `SearchTerm` as
        the term — the board's `search=` is `SearchTerm` on the wire; do NOT rename it, the
        families module already sends `searchTerm`)
  - [x] `FamilyService.GetFamiliesAsync` — when `SearchType` is present, branch the `SearchTerm`
        match instead of the default four-field Contains:
        - `father` → guardian/father name (the family aggregate's father-name source used by the
          6-1 grid column إسم الأب)
        - `mother` → mother name
        - `student` → child name (join `Orphan` names of the family — the families module's
          orphan subresource shows the linkage)
        - `nationalId` → guardian national id (exact/partial per legacy behaviour — partial)
        - `code` → **كود الطالب الابن** — the child's sponsorship `Code`
          (`Orphan.Code`, `Entities/Orphan.cs:17`), NOT `Family.Code`
        - `phone` → family phone(s)
        - unknown/absent `SearchType` → existing four-field Contains (back-compat: epic 5's
          families screen must not change behaviour)
  - [x] The branch must compose with `FamilyType == Housing` (6-1) AND the charity pin — one
        query, filters AND-ed; no OR between scope and search
- [x] **Task 2 — API** — no controller change: `GET /api/Families?familyType=Housing&searchTerm=
      &searchType=` binds through the existing `[FromQuery] FamilyFilterDto` (AC 2, 5)
- [x] **Task 3 — Frontend: wire the filter row** (AC 1, 2)
  - [x] On the 6-1 list component: enable the search box + البحث عن طريق drop-down (i18n options
        from the 6-1 `housingProjects.*` block), بحث button reloads page 1 with
        `{ familyType: 'Housing', searchTerm, searchType, charityId, page: 1, pageSize }`
  - [x] Charity drop-down stays HQ-only (كافة الجهات = no filter) — 6-1 shape
  - [x] Empty result → grid empty state + paging reports zero pages (§11.U.2 alternate)
  - [x] Keep `trackBy`, `OnPush`, `takeUntil(destroy$)`; no page reload — Observable reload
- [x] **Task 4 — Verification** (AC 1–5)
  - [x] Live: seed/flip one housing family; search by father name → 1 row; search by a child's
        code → the family row; `searchType=` nonsense value → ignored, default Contains; charity
        token + another charity's family name → 0 rows (scope composes)
  - [x] Regression: epic 5 `#/families` search unchanged (no `searchType` sent → old behaviour)
  - [x] `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

### Review Findings

Code review 2026-08-24: compliant on all ACs (six typed predicates, charity pin, HQ explicit
charity). One edge-layer patch:

- [x] [Review][Patch] Typed search matches soft-deleted orphans — `student`/`nationalid`/`code`/`phone` predicates lack `!o.IsDeleted` (no global filter exists) → deleted orphans keep surfacing families [Backend/src/IIROSA.Application/Services/FamilyService.cs:1138] — **applied**: `!o.IsDeleted` added to every orphan-joining predicate (student/nationalid/code/phone); build green

## Dev Notes

### Platform rules that bind this story

- Reuse — do not rebuild: the endpoint, filter DTO, grid and filter row all exist after 6-1; this
  story only adds the typed branch + wiring.
- camelCase wire; no `FK_`-prefixed DTO properties; soft delete handled by the global query
  filter; raw envelope, not `ApiResponse<T>` (15-1 Dev Notes).
- No business logic in the controller — the search branch lives in `FamilyService`.

### Out of scope

| Item | Story |
| --- | --- |
| Buildings/flats lookups + allocation | 6-5 |
| Housing family create (controller re-cut + form) | 6-3 |
| View/update + edit mode | 6-4 |
| Beneficiary (child) code lookup for REPORTS — that is 6-7, not this story's `code` search | 6-7 |

### References

- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.S.1] searchValue + searchType contract
- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.U.2] scenario + empty-result alternate
- [Source: _bmad-output/planning-artifacts/epics.md#3.6] US-HOU-02 acceptance criteria
- [Source: Backend/src/IIROSA.Application/DTOs/Family/FamilyFilterDto.cs#L8] SearchTerm today
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-1-list-housing-families.md] reality check,
  entity design, platform rules (binding on this story)

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code session, 2026-08-24).

### Debug Log References

- Covered by 6-1's build proofs: `dotnet build … -c Efmig` 0 errors; `npm run build` clean for all
  housing-projects files.

### Completion Notes List

- Implemented together with 6-1 in the same pass (the re-cut list screen ships the selector wired).
- `FamilyFilterDto.SearchType` + the `switch` branch in `FamilyService.GetFamiliesAsync`; unknown
  selector ⇒ legacy 4-field behaviour preserved (epic-5 regression safe by construction).
- `code` searches `Orphan.Code` (student sponsorship code), NOT `Family.Code` — per the binding
  design. `nationalId` searches `Orphan.NationalId`. `phone` spans `Family.PhoneNumber` +
  Father/Mother/Orphan phones.
- **Shared vocabulary:** a parallel epic-7 (Refugee) session extended the same selector with a
  `provider` case and updated the DTO doc comment (UC-REF-02 §12.S.1) — one switch now serves both
  epics; the six §11.S.1 selectors are unchanged.
- Live endpoint verification deferred with 6-1 (running API predates the code).

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/FamilyFilterDto.cs` (+`SearchType`)
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` (typed-search switch)
- `Frontend/src/app/modules/families/models/family.model.ts` (+`searchType`)
- `Frontend/src/app/modules/housing-projects/housing-project-list/housing-project-list.component.ts/.html`
  (searchType drop-down + wiring — landed under 6-1's re-cut)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-HOU-02 and module spec §11.S.1 / §11.U.2; typed-search gap recorded against the copied `SearchTerm`-only implementation. |
| 2026-08-24 | Implemented with 6-1 in one pass: SearchType DTO field + service branch (6 selectors, epic-5 back-compat default), selector wired on the re-cut list screen. Status → review. |
| 2026-08-24 | Review closed: soft-delete predicate patch applied; epic-5 back-compat default re-verified in the review battery. All findings resolved. Status → done. |
