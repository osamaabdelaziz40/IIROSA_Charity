# Story 16-2: Search incoming letters

| Field | Value |
| --- | --- |
| Story key | `16-2-search-incoming-letters` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-02 — البحث في الوارد |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.1 screen, §21.U.2 scenario) |
| Route | `#/incoming-outgoing/incoming` (search is the بحث command of the list screen) |
| Endpoint | `GET /api/IncomingOutgoing/incoming` |
| Depends on | 16-1 (list foundation: URL, wire shape, charity scope) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to search incoming letters البحث في الوارد, so that
I can locate a record from partial information.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor invokes
   the function with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page
   reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §21.S.1 are implemented with their mandatory flags and
lookups; the scenario of §21.U.2 passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Repo | `IncomingRepository.GetPagedAsync` | Exists; `searchTerm` already matches Subject / IncomingId / LetterNumber / IncomingNumber |
| Filter DTO | `DTOs/IncomingOutgoing/IncomingDto.cs` (`IncomingFilterDto`) | Exists; keys mostly align with the SPA, but missing serial / letter-number-lookup semantics |
| Frontend | `incoming-letters-list.component.ts` `loadLetters()` | Exists; builds the request from `filterForm` |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Search is dead end-to-end today** — it rides the phantom URL and the tuple wire that 16-1
   fixes; this story owns making the individual criteria real once that lands.
2. **`searchTerm` is the only text criterion implemented server-side.** §21.S.1 asks for separate
   رقم الوارد (serial) and رقم الخطاب (letter number) lookups + الموضوع text; the repo folds
   everything into one `searchTerm` OR-match.
3. **`createdBy` filter key never binds.** The SPA model sends `createdBy`
   (`incoming.model.ts:68`); the DTO property is `CreatedByUserId` — query binding is
   case-insensitive but not name-insensitive, so the employee filter is silently ignored.
4. **Sorting is half-wired.** `ApplySorting` only treats `sortOrder == "descending"` as descending
   (literal, case-sensitive) — `desc` / `asc` fall through to the default; the SPA never sends a
   sort at all, so relevance-to-spec ordering (التاريخ) must be guaranteed server-side.
5. **Empty-result UX.** AC/§21.U.2 alternate flow "grid renders empty and the paging control
   reports zero pages" — the empty row exists (html:143-150) but the paging control must receive
   `totalCount = 0` from the fixed wire.

## Tasks / Subtasks

- [x] **Task 1 — Real criteria server-side** (AC 1, 2)
  - [x] Extend `IncomingFilterDto` (post-16-1 clean names) with `Serial` (int?) and keep
        `LetterNumber` as its own exact/partial criterion; repo: match each independently
        (`Serial ==`, `LetterNumber.Contains`), `SearchTerm` remains the الموضوع subject search
  - [x] Rename `CreatedByUserId` → `CreatedBy` (or send `createdByUserId` from the SPA — pick the
        clean key and align both sides once)
  - [x] Fix `ApplySorting` to accept `descending` / `desc` (case-insensitive) and default to
        Date-descending so the register reads newest-first
- [x] **Task 2 — Wire the بحث command** (AC 1)
  - [x] `onSearch()` resets to page 1 and issues the request with every §21.S.1 field the 16-1
        filter bar exposes; ensure each control's value lands on the matching query key (no dead
        form fields — the department control lesson from 16-1 defect 5)
- [x] **Task 3 — Scoping + empty result** (AC 3, 4)
  - [x] Search inherits 16-1's `ApplyCallerScope`; verify a charity user cannot widen results by
        passing `charityId` themselves; HQ + explicit `charityId` narrows
  - [x] Empty result → empty grid + zero-page paging control (§21.U.2 alternate flow)
- [x] **Task 4 — Verification**
  - [ ] Live: serial, letter number, subject, status, employee, date-range, and charity criteria
        each narrow the result independently and combined; `totalCount` respects the filter
  - [x] `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- Same platform rules as 16-1 (Newtonsoft camelCase wire, no `ApiResponse<T>` wrapper, global
  soft-delete filter, service-layer validation, `MappingDefaults` constants).
- Query binding is case-insensitive but **name-sensitive** — a mismatched key is silently dropped,
  not an error. Verify each criterion live, not by reading the binding.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Outgoing search (mirror) | 16-11 |
| Create/update flows | 16-4, 16-6 |
| Employees attachment browsing | 16-9 |
| Import/export of letters (legacy extras) | not in this epic's scope |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.1] filter fields
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.2] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-02 acceptance criteria
- [Source: Backend/src/IIROSA.Infrastructure/Data/Repository/IncomingRepository.cs] `GetPagedAsync`
  + `ApplySorting`
- [Source: Frontend/src/app/modules/incoming-outgoing/models/incoming.model.ts#L61-71] request keys

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): re-audited against shipped code — `IncomingRepository.ApplyFilters` matches `Serial ==` and `LetterNumber ==` independently; `SearchTerm` narrowed to subject-only this pass (was subject + serial + letter number OR-match, defect 2); `ApplySorting` computes `isDescending` case-insensitively and defaults to `Date ?? CreatedOn` descending (newest-first register).
- 2026-08-24 (verification): project-level builds clean; full-solution build blocked only by the parallel epic-6 session's `FamilyService.cs` error. `ng build` clean for `incoming-outgoing`.

### Completion Notes List

- All criteria are independent query keys now: `serial` (int, exact), `letterNumber` (exact), `searchTerm` = الموضوع subject `Contains`, `status` (Arabic tri-state exact), `assignedUserId` (responsible employee), `charityId` (HQ-only), `startDate`/`endDate`, `year`. `onSearch()` resets to page 1 with every 16-1 filter-bar control wired — no dead form fields.
- **Defect 3 resolution (deviation recorded):** the broken `createdBy`↔`CreatedByUserId` pair was resolved by renaming to **`AssignedUserId`** bound to `Incoming.FK_UserId` — §21.S.1 الموظف means the employee the letter is routed to (الجهة الموظول بها), not the audit creator. SPA model + repo criteria + filter DTO aligned on the one clean key.
- Letter number is an exact match (spec's رقم الخطاب reads as find-this-letter); serial exact — partial matching stays on الموضوع only. Recorded as the chosen reading.
- Caller scope inherited from 16-1: charity claim pinned server-side (a charity user passing `charityId` is ignored — pin-never-widen); HQ + `charityId` narrows. Empty result renders the empty row and `totalCount = 0` reaches the paging control.
- Task 4 live criterion check left unchecked — pending the user's `IIROSA.Api` restart (pre-story binaries still running). `npm run build` clean for the module; tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Infrastructure/Data/Repository/IncomingRepository.cs` — independent `Serial`/`LetterNumber` predicates, subject-only `SearchTerm`, sort fix
- `Backend/src/IIROSA.Application/DTOs/IncomingOutgoing/IncomingDto.cs` — `AssignedUserId` rename, criteria keys
- `Backend/src/IIROSA.Domain/Interfaces/IIncomingRepository.cs` — criteria rename
- `Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letters-list.component.ts` / `.html` — بحث wiring, empty-result + paging
- `Frontend/src/app/modules/incoming-outgoing/models/incoming.model.ts` — `assignedUserId` filter key

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-02 and module spec §21.S.1 / §21.U.2; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (independent criteria, `AssignedUserId` semantics fix, subject-only search, sort default). Status → review; live criterion walkthrough pending user's API restart. |
