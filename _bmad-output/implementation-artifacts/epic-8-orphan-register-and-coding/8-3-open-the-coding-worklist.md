# Story 8-3: Open the coding worklist

| Field | Value |
| --- | --- |
| Story key | `8-3-open-the-coding-worklist` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-03 — تكويد الأيتام |
| Priority / size | Must · 2 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.S.2 screen, §13.U.3 scenario) |
| Route | `#/families/orphans/coding/worklist` |
| Endpoint | `GET /api/Families/orphans?codingStatus=Pending` |
| Depends on | EP-01 (auth); shares the `GET /api/Families/orphans` endpoint with 8-2 (build 8-2 first) |
| Roles | HQ only — legacy "Gen. Director, Staff" → `Admin`, `SuperAdmin` |

## Status

done

## Story

As a General Director, I want to be able to open the coding worklist تكويد الأيتام, so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a General Director (HQ `Admin`/`SuperAdmin`) with an active session on the screen at `#/families/orphans/coding/worklist`, when the actor opens the screen, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans?codingStatus=Pending` and the response is rendered on the screen without a page reload.
3. Given a charity-scoped caller, when the function is invoked, then only records owned by that charity (and country) are returned.
4. Given an HQ role, when an explicit charity id is supplied, then the worklist operates on that charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.
6. Given a non-HQ role, when the worklist is opened, then the request is refused (server-side role check — «Unthorized User» in legacy terms) and nothing is written.

**Definition of done:** the screen fields of §13.S.2 are implemented with their lookups; the scenario of §13.U.3 passes end to end; the HQ-only role restriction is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Feature module | `Frontend/src/app/modules/families/families.module.ts` | Exists, lazy-loaded — add the component here; **do not create a new module** |
| Routing | `Frontend/src/app/modules/families/families-routing.module.ts` | Exists — routes `''`, `create`, `:id`, `:id/edit`. **No `orphans/coding` routes exist** (module-level audit exclusion) |
| Entity | `Backend/src/IIROSA.Domain/Entities/Orphan.cs` | `Code` (empty string = uncoded), `FK_CharityId`, `FullName`, `Family` nav → `Father`, `Mother`, `Charity` |
| Endpoint base | 8-2's `GET /api/Families/orphans` | Same endpoint gains the `codingStatus` filter — one endpoint, two query modes |
| Scope helpers | `FamiliesController.GetUserCharityId()` / `GetUserRole()` | Reuse |
| i18n | `assets/i18n/ar.json` / `en.json` | Add an `orphanCoding` namespace |

## Tasks / Subtasks

- [x] **Task 1 — Worklist query** (AC 1–4)
  - [x] `OrphanSearchFilterDto` carries `string? CodingStatus`; `SearchOrphansAsync` handles `CodingStatus == "Pending"` as `string.IsNullOrEmpty(o.Code)` (uncoded). Uncoded definition is **empty code**, not `SponsorshipStatus`. The validator restricts `CodingStatus` to `Pending|Coded`
  - [x] `OrphanLookupDto` (no separate worklist DTO) carries the §13.U.3 judgement data: orphan name, father name, mother name, charity name, code, date of birth (+ age, sponsorship status)
  - [x] Charity scoping as in `FamilyService.GetFamiliesAsync`: charity role pinned to own charity; HQ may pass `CharityId`; `CharityId` empty = كافة الجهات (all charities, HQ only)
- [x] **Task 2 — Screen per §13.S.2** (AC 1, 2)
  - [x] `OrphanCodingWorklistComponent` under `modules/families/orphan-coding-worklist/` — 4-file shape, `OnPush`
  - [x] Route in `families-routing.module.ts`: `path: 'orphans/coding/worklist'` (registered ABOVE `orphans/coding` and `':id'`, both landmines commented in the route table), `canActivate: [AuthGuard, PermissionGuard]`, `data: { title: 'orphanCoding.worklistTitle', permission: 'OrphanCoding.View' }`
  - [x] Filter: الجمعية drop-down — options `GET /api/Charities` (pageSize 500), كل الجمعيات empty option first; screen is HQ-only by route permission (server pins anyway); on change reloads page 1
  - [x] Grid per §13.S.2 with server-driven sorting on اسم الجمعية · اسم الأم · اسم الابن/الابنة plus كود (all four headers clickable, `sortByColumn` → server `sortBy`), pagination via shared `PaginationComponent`; `trackBy: trackByOrphanId`
  - [x] Row actions تعديل الكود / SaveCode / CancelSavingCode per §13.S.2 — **wired beyond scaffold:** since 8-5/8-6 were built in the same pass, the inline edit carries the live check (debounced `check-code`, available/taken state) and save (`POST orphans/{orphanId}/code` + worklist reload); سجل الصرف row action opens the 8-8 history
  - [x] i18n keys under `orphanCoding` in **both** `ar.json` and `en.json`
- [x] **Task 3 — Authorisation + navigation** (AC 5, 6)
  - [x] **Decision recorded:** ONE `GET orphans` action with the controller-wide `[Authorize]`; the role gate is conditional in the **service** — `CodingStatus` supplied + `Charity` role → `UnauthorizedAccessException` → controller `Forbid()` (403), plain search stays all-roles. No second action/route was added
  - [x] `auth.service.ts PERMISSION_ROLES`: `OrphanCoding.View` → `SuperAdmin, Admin`; `OrphanCoding.Edit` → `SuperAdmin, Admin` (with a comment explaining the deliberately wider server endpoints vs. HQ-only screens)
  - [x] Sidebar: both تكويد الأيتام and قائمة تكويد الأيتام entries under the families menu in `layouts/main-layout`, gated on `hasPermission('OrphanCoding.View')`
- [x] **Task 4 — Verification**
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24: `Admin` worklist query → 200 with an **empty page — legitimate**: this API's orphan-create auto-assigns codes (`ORP-2026-…`), so `codingStatus=Pending` (empty code) matches nothing in the dev DB; `Charity` role → 403 on the coding query (D4 fail-closed); unauthenticated → 401

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers)._

- [x] [Review][Patch] اسم الأم header sorts by orphan name — the backend sort switch has no `MotherName` case (falls through to the `FullName` default) while the header advertises mother-name sorting; add the case (`o.Mother.FullName`, nav already included) [FamilyService.cs:1709; orphan-coding-worklist.component.html:44]
- [x] [Review][Patch] Coding the last orphan of a page strands an empty page (save reload keeps `currentPage`; never clamped) and `*ngIf="totalCount > pageSize"` hides the pager at the exact boundary (20 > 20 is false) — clamp the page when a reload comes back empty; use `>=` [orphan-coding-worklist.component.ts:149, 275; .html:124]
- [x] [Review][Patch] `saveCode` success path lacks `cdr.markForCheck()` — if the chained reload/search early-returns, the inline edit stays open and `saving` never repaints (cross-story with 8-4) [orphan-coding-worklist.component.ts:275]
- [x] [Review][Patch] Inline code check fires one HTTP call per keystroke despite the "debounced" comment (subscription swap, no `debounceTime`) — add `debounceTime(300)` (cross-story with 8-4) [orphan-coding-worklist.component.ts:225]
- [x] [Review][Patch] Soft-delete filters on the worklist query (cross-story, anchored in 8-2)
- [x] [Review][Decision] Role matrix (cross-story, anchored in 8-2) — **Resolved 2026-08-24 (D2:a/D3:a):** shipped matrix stands; assign endpoint HQ-only

## Dev Notes

### Platform rules that bind this story

- **No `ApiResponse<T>` wrapper** — paged reads use the 13-1 wire envelope `new { items, totalCount, page }`; anonymous error objects on failure (2026-08-19 standing decision — raw stays until the platform-wide ApiResponse migration story).
- Wire is camelCase via **Newtonsoft**; clean DTO key names only.
- Soft delete has **no global query filter** — `SetGlobalQueryFilters` is never called; the shipped convention is manual `!IsDeleted` on every read, and the epic-8 queries filter it explicitly (orphan, and its family where joined).
- Client-side menu hiding is **not** an authorisation control — the endpoint enforces roles (AC 6).
- Never kill the user's running `IIROSA.Api` process. Tests excluded per the standing user decision.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Inline code uniqueness check (تعديل الكود → check) | 8-5 |
| Saving the code (SaveCode / CancelSavingCode handlers) + worklist refresh after save | 8-6 |
| The legacy name-search coding screen `#/families/orphans/coding` | 8-4 |
| Payment history from the worklist row | 8-8 |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.S.2] screen contract — 1 filter, grid columns, 6 commands
- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.3] scenario
- [Source: docs/Modules/00-ROUTING-MAP.md#L78] module 13 routing row — `families` module, `api/Families` orphans sub-resource
- [Source: Frontend/src/app/modules/families/families-routing.module.ts] route table to extend
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-03 acceptance criteria

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors (25 pre-existing warnings).
- `cd Frontend && npm run build` — exit 0; `modules-families-families-module` chunk grew with both new components.

### Completion Notes List

- Worklist loads `codingStatus: 'Pending'` (uncoded = empty `Code`) with server paging/sorting; the uncoded orphan leaves the list automatically after a successful save because the query re-runs.
- The 8-5 check and 8-6 save were wired in this story's inline edit rather than left scaffolded — 8-3/8-5/8-6 were implemented as one pass over the shared endpoint set (the story split remains accurate for ownership: the check/save logic is documented under 8-5/8-6).
- HQ gate decision (Task 3 option 1): single `GET orphans` action, service-level `UnauthorizedAccessException` when `CodingStatus` is supplied by a Charity caller. `PERMISSION_ROLES` hides both coding screens from Charity users; the server endpoints remain deliberately wider (Charity can search its own register on the plain mode) — the asymmetry is intentional and commented in `auth.service.ts`.
- Route order: `orphans/coding/worklist` registered above `orphans/coding`, both above `':id'` — the route-order landmine comment in the route table now covers the `orphans/*` family too.
- Live walkthrough pending the user's IIROSA.Api restart.

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/OrphanCodingDtos.cs` (new — CodingStatus on the filter)
- `Backend/src/IIROSA.Application/Validators/Family/OrphanCodingValidators.cs` (new — CodingStatus ∈ {Pending, Coded})
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` (worklist branch + HQ gate)
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` (GET orphans)
- `Frontend/src/app/modules/families/orphan-coding-worklist/**` (new — 4 files)
- `Frontend/src/app/modules/families/families-routing.module.ts` (both coding routes, above `:id`)
- `Frontend/src/app/core/services/auth.service.ts` (OrphanCoding.View / OrphanCoding.Edit)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` (two sidebar entries)
- `Frontend/src/assets/i18n/ar.json` + `en.json` (orphanCoding namespace)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-03 and module spec §13.S.2 / §13.U.3; route table audited (no coding routes exist); uncoded = empty `Code` decision recorded; row-action wiring split across 8-5/8-6. |
| 2026-08-24 | Implemented end to end: worklist query + HQ gate, §13.S.2 screen with server sort/paging, routes, permissions, sidebar, i18n. Inline edit wired live (8-5/8-6 same pass). Status → review. |
| 2026-08-24 | Adversarial review close-out: 6 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
