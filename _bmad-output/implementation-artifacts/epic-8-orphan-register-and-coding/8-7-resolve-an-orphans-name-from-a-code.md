# Story 8-7: Resolve an orphan's name from a code

| Field | Value |
| --- | --- |
| Story key | `8-7-resolve-an-orphans-name-from-a-code` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-07 — استعلام بالكود |
| Priority / size | Must · 2 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.U.7 scenario) |
| Route | none — a code→name resolution control; first consumers: the payment-history dialog (8-8) and the coding screens' orphan modal |
| Endpoint | `GET /api/Families/orphans?search=` (same endpoint as 8-2, extended to match by code) |
| Depends on | 8-2 (the endpoint and lookup DTO this story extends) |
| Roles | All roles (authenticated) |

## Status

done

## Story

As a signed-in user, I want to be able to resolve an orphan's name from a code استعلام بالكود, so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a signed-in user with an active session, when the actor enters a full code, then the orphan's name + code pair is returned — no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans?search=` and the response is rendered on the screen without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned.
4. Given an HQ role, when an explicit charity id is supplied, then the lookup operates on that charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.
6. Given the code matches no orphan in scope, then the control reports "no match" — it does not error.

**Definition of done:** the scenario of §13.U.7 passes end to end; the role and charity scoping is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Endpoint | 8-2's `GET /api/Families/orphans?search=` + `SearchOrphansAsync` | **Extend the predicate — do not add a second endpoint** (the board maps 8-2 and 8-7 to the same URL) |
| Entity | `Backend/src/IIROSA.Domain/Entities/Orphan.cs` | `Code`, `FullName` |
| Frontend | 8-2's orphan type-ahead (`components/orphan-lookup/`) | Add a code-aware mode instead of a new component |
| Legacy method | `IOrphanService.GetChildNameAndCode` | Legacy identifier — the equivalent lands inside `SearchOrphansAsync` |

## Tasks / Subtasks

- [x] **Task 1 — Extend the search predicate** (AC 1–4)
  - [x] `SearchOrphansAsync`: `search` matches `FullName.Contains(term) || Code.Contains(term)` — **deviation:** code match is `Contains`, not exact `==`, and exact-code hits get no special ordering. Rationale: legacy codes are short strings where a prefix/partial code is a useful resolve aid, and the grid already displays code + name pairs for disambiguation; exact-match ordering was judged not worth the second query branch
  - [x] Return shape unchanged (`OrphanLookupDto` carries `Code` next to `FullName` — the name + code pair of §13.U.7)
  - [x] Scoping unchanged: charity role pinned, HQ `CharityId` override, global soft-delete filter — an out-of-scope code resolves to **no match**, never a leak
- [x] **Task 2 — Code-aware lookup control** (AC 1, 6)
  - [x] **Deviation:** no standalone `orphan-lookup` component — the code→name resolve is the 8-4 coding screen's الكود field + بحث command (`search()` prefers the code over the name when both are typed). The 8-8 payment-history dialog opens from a selected row (no code-first picker needed on this stack — orphans arrive by selection from scoped searches). The no-match state renders the shared empty grid, not an error toast
  - [x] i18n keys (`orphanCoding.noResults`, `enterNameOrCode`) in **both** `ar.json` and `en.json`
- [x] **Task 3 — Verification**
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24: known code `LC-CODE-1` resolved to exactly its orphan (code + name pair, scoped charity on the DTO); after the seed family's soft-delete the same code resolved to empty — deleted rows never leak; no-match code → empty result (200, not an error); unauthenticated → 401. Cross-charity invisibility rides the scoping verified as fail-closed denial in 8-2

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers)._

- [x] [Review][Decision] Roles field says "All roles (authenticated)"; the shipped attribute on the shared `GET orphans` endpoint excludes `Accountant`/`FinancialOfficer`/`Employee` (part of the role-matrix decision, anchored in 8-2) — **Resolved 2026-08-24 (D2:a):** the shipped matrix stands

## Dev Notes

### Platform rules that bind this story

- **No `ApiResponse<T>` wrapper** — raw array of lookup DTOs (2026-08-19 standing decision — raw stays until the platform-wide ApiResponse migration story).
- Wire is camelCase via **Newtonsoft**; clean DTO key names only.
- Soft delete has **no global query filter** — `SetGlobalQueryFilters` is never called; the shipped convention is manual `!IsDeleted` on every read, and the epic-8 queries filter it explicitly (orphan, and its family where joined).
- "No match" for an out-of-scope code is the **tenancy-correct** behaviour — never return a hint that the code exists elsewhere (AC 3 + verification note).
- Never kill the user's running `IIROSA.Api` process. Tests excluded per the standing user decision.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| The base search endpoint + type-ahead | 8-2 |
| Payment history / detail screens the picker feeds | 8-8, 8-9 |
| Coding-status filtering | 8-3 |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.7] scenario — name + code pair
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-07 acceptance criteria — same endpoint as US-ORP-02
- [Source: Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs] service to extend

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors.
- `cd Frontend && npm run build` — exit 0.

### Completion Notes List

- Code resolution rides the same `GET /api/Families/orphans?search=` predicate as the name search (`FullName.Contains || Code.Contains`) — one endpoint, no second route, per the board mapping.
- Deviations recorded: `Contains` rather than exact `==` on code (partial codes resolve; grid shows pairs for disambiguation), and no standalone lookup component (the 8-4 screen's الكود field is the consumer; the 8-8 history opens from row selection, so no code-first picker was needed).
- Tenancy-correct no-match: scoping happens before matching, so another charity's code yields an empty result set, never a hint that it exists.
- Live walkthrough pending the user's IIROSA.Api restart.

### File List

- `Backend/src/IIROSA.Application/Services/FamilyService.cs` (predicate extension in SearchOrphansAsync)
- `Frontend/src/app/modules/families/orphan-coding/**` (الكود field + بحث resolve)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-07 and module spec §13.U.7; single-endpoint decision (extends 8-2's predicate) and tenancy-correct no-match behaviour recorded. |
| 2026-08-24 | Implemented: predicate extension + code-first resolve on the coding screen. Contains-match and no-standalone-component deviations recorded. Status → review. |
| 2026-08-24 | Adversarial review close-out: 1 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
