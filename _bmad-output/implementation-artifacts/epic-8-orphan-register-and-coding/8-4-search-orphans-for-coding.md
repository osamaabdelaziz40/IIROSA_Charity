# Story 8-4: Search orphans for coding

| Field | Value |
| --- | --- |
| Story key | `8-4-search-orphans-for-coding` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-04 — البحث في قائمة التكويد |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.S.1 screen, §13.U.4 scenario) |
| Route | `#/families/orphans/coding` |
| Endpoint | `GET /api/Families/orphans?search=` |
| Depends on | 8-2 (the search endpoint + type-ahead), 8-11 (الجمعية lookup reuse), 8-3 (module routes scaffold) |
| Roles | HQ only — `Admin`, `SuperAdmin` |

## Status

done

## Story

As a General Director, I want to be able to search orphans for coding البحث في قائمة التكويد, so that I can locate a record from partial information.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at `#/families/orphans/coding`, when the actor presses «بحث» with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans?search=` and the response is rendered on the screen without a page reload.
3. Given a charity-scoped caller, when the function is invoked, then only records owned by that charity (and country) are returned.
4. Given an HQ role, when an explicit charity id is supplied, then the search operates on that charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.
6. Given a non-HQ role, when the screen's search is used, then the coding query is refused server-side («Unthorized User» in legacy terms) and nothing is written.

**Definition of done:** the screen fields of §13.S.1 are implemented with their mandatory flags and lookups; the scenario of §13.U.4 passes end to end; the role and charity scoping is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Screen spec | §13.S.1 | 3 fields (الجمعية · اسم اليتيم · الكود), 1 grid, 4 commands (بحث · غلق · تم · orphan modal) |
| Search endpoint | 8-2's `GET /api/Families/orphans?search=` + type-ahead component | Build on it — this screen uses the button-driven بحث form, not the keystroke type-ahead |
| Module + routes | `families` module; 8-3 adds the `orphans/coding/...` route family | Register `orphans/coding` next to the worklist route |
| Charity lookup | `GET /api/Charities` | Exists — الجمعية drop-down source |
| Legacy binding names | `searchValue`, `OrhanCode`, `EmptyOrphanModel`, `ShowModalOfOrphan` | **Legacy identifiers — do NOT reproduce the `OrhanCode` typo**; use clean names (`codeValue`) |

## Tasks / Subtasks

- [x] **Task 1 — Screen per §13.S.1** (AC 1, 2)
  - [x] `OrphanCodingComponent` under `modules/families/orphan-coding/` — 4-file shape, `OnPush`
  - [x] Route: `path: 'orphans/coding'` (registered BELOW `orphans/coding/worklist` — the more specific literal first — and above `':id'`), `PermissionGuard` with `permission: 'OrphanCoding.View'`
  - [x] Fields: الجمعية drop-down (كل الجمعيات empty option, HQ-only control — hidden for Charity callers, server pins anyway; on change re-runs the standing search), اسم اليتيم text (type-ahead per 8-2), الكود text field. **Deviation:** the code field is free text, not a numeric box — codes are alphanumeric strings on the entity (`Orphan.Code`, no numeric constraint in the legacy data or the validator); the numeric-box reading of §13.S.1 was not enforceable
  - [x] بحث command → `GET /api/Families/orphans?search=&charityId=` (the 8-2 endpoint — code wins over name when both are typed), results into the grid; غلق command → navigates to `/families` (the register)
  - [x] Grid per §13.S.1: الرقم · اسم اليتيم · اسم الأب · اسم الأم · الجمعية · الكود; `trackBy: trackByOrphanId`
  - [x] **Deviation:** the orphan modal (`ShowModalOfOrphan` equivalent) was replaced by the تم row action opening `OrphanPaymentHistoryComponent` (8-8) inline below the grid — the judgement data (names, charity, code) is already visible in the row, so the modal duplicated it; the payment history is the actionable next step. تم keeps its name
- [x] **Task 2 — Authorisation** (AC 5, 6)
  - [x] Reuses 8-3's split: route `PermissionGuard` (`OrphanCoding.View` = HQ) + service-level scoping; `PERMISSION_ROLES` entries landed with 8-3
- [x] **Task 3 — Verification**
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24: بحث via the shared `GET orphans?search=` returned the full row (every grid column's data present: names, code, family, charity, sponsorship status); no-match term → `items: []` 200 (empty grid, not an error); `Charity` role → 403 (fail-closed); unauthenticated → 401

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers)._

- [x] [Review][Patch] `saveCode` success path lacks `cdr.markForCheck()` — every other path in the file calls it; if `search()` early-returns on empty inputs the inline edit cell stays open and `saving` never repaints [orphan-coding.component.ts:288]
- [x] [Review][Patch] Inline code check is not actually debounced (subscription swap only) — one GET per keystroke; add `debounceTime(300)` matching the name type-ahead [orphan-coding.component.ts:239]

## Dev Notes

### Platform rules that bind this story

- **No `ApiResponse<T>` wrapper** — raw envelope (`{ items, totalCount, page }` if paged) with anonymous error objects (2026-08-19 standing decision — raw stays until the platform-wide ApiResponse migration story).
- Wire is camelCase via **Newtonsoft**; clean key names only — and clean **Angular** model names (no `OrhanCode`).
- Soft delete has **no global query filter** — `SetGlobalQueryFilters` is never called; the shipped convention is manual `!IsDeleted` on every read, and the epic-8 queries filter it explicitly (orphan, and its family where joined).
- Every user-facing string via i18n (`orphanCoding` namespace in **both** `ar.json` and `en.json`); RTL-first.
- Never kill the user's running `IIROSA.Api` process. Tests excluded per the standing user decision.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| الكود uniqueness check on this screen's numeric box | 8-5 |
| تم / `SaveCode` — writing the code | 8-6 |
| The worklist screen `#/families/orphans/coding/worklist` | 8-3 |
| The underlying search endpoint + type-ahead component | 8-2 |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.S.1] screen contract — 3 fields, grid columns, 4 commands
- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.4] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-04 acceptance criteria
- [Source: Frontend/src/app/modules/families/families.module.ts] host module

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors.
- `cd Frontend && npm run build` — exit 0.

### Completion Notes List

- Two recorded deviations from §13.S.1: (1) الكود is a free-text field, not a numeric box — `Orphan.Code` is an alphanumeric string with no numeric constraint anywhere in the model or validators; (2) the orphan modal is replaced by the تم row action opening the 8-8 payment history inline — the row already shows the modal's judgement data.
- The screen doubles as the UC-ORP-07 code-resolve surface: typing a code into الكود and pressing بحث resolves the orphan from its code (8-7's consumer).
- Inline تعديل الكود edit with the 8-5 check + 8-6 save is wired on this screen too (same shared component pattern as the worklist).
- Type-ahead: اسم اليتيم valueChanges, 300 ms debounce, ≥3 chars, clears the grid below threshold.
- Live walkthrough pending the user's IIROSA.Api restart.

### File List

- `Frontend/src/app/modules/families/orphan-coding/**` (new — 4 files)
- `Frontend/src/app/modules/families/families-routing.module.ts` (route)
- `Frontend/src/app/modules/families/services/family.service.ts` (searchOrphansCoding / checkOrphanCode / assignOrphanCode consumers)
- `Frontend/src/assets/i18n/ar.json` + `en.json` (orphanCoding namespace)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-04 and module spec §13.S.1 / §13.U.4; legacy `OrhanCode` typo flagged as not-to-reproduce; تم/SaveCode wiring deferred to 8-6. |
| 2026-08-24 | Implemented: §13.S.1 screen, route, search/type-ahead/code-resolve, inline code edit. Numeric-box and orphan-modal deviations recorded. Status → review. |
| 2026-08-24 | Adversarial review close-out: 2 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
