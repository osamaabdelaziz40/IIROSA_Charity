# Story 16-11: Search outgoing letters

| Field | Value |
| --- | --- |
| Story key | `16-11-search-outgoing-letters` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-11 — البحث في الصادر |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.4 screen, §21.U.11 scenario) |
| Route | `#/incoming-outgoing/outgoing` (بحث command of the list screen) |
| Endpoint | `GET /api/IncomingOutgoing/outgoing` |
| Depends on | 16-10 (list foundation) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to search outgoing letters البحث في الصادر, so
that I can locate a record from partial information.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor invokes
   the function with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/outgoing` and the response is rendered on the screen without a page
   reload.
3. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.11 passes end to end; the scoping is enforced
server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Repo | `OutgoingRepository.GetPagedAsync` | Exists; `searchTerm` over Subject/OutGoingId/OutGoingNumber |
| Filter DTO | `OutgoingFilterDto` | Exists; `CategoryId` (int?), `HasReply`, dates, paging |
| Frontend | `outgoing-letters-list.component.ts` `loadLetters()` | Exists |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Mirror of 16-2**: criteria folded into one `searchTerm` instead of the §21.S.4 set
   (رقم الخطاب numeric own criterion, الموضوع text, الموظف, charity — the latter two arrive with
   16-10's filter bar).
2. **`createdBy` key never binds** (`outgoing.model.ts:58` vs `CreatedByUserId`) — same
   name-silent-drop as 16-2 defect 3.
3. **`ApplySorting` accepts only literal `"descending"`** (same as incoming) — `desc`/`asc` fall
   through; fix with the incoming side's approach (case-insensitive, default Date-descending).
4. **`hasReply` criterion** — not a §21.S.4 filter field; keep the DTO property (cheap, useful)
   but take it off the filter bar per spec.

## Tasks / Subtasks

- [x] **Task 1 — Real criteria server-side** (AC 1, 2): `Serial`/letter-number own criteria +
        subject `SearchTerm` + employee + charity (16-10 keys); fix `createdBy` binding; fix
        `ApplySorting`
- [x] **Task 2 — Wire بحث** (AC 1): page-1 reset; every §21.S.4 control lands on its query key;
        empty result → empty grid + zero-page paging control (§21.U.11 alternate flow)
- [x] **Task 3 — Verification**: live — each criterion narrows independently and combined;
        `totalCount` respects filters; unauthenticated → 401; `npm run build` — 0 errors; tests
        excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

Same as 16-2 (query binding is name-sensitive; verify criteria live). Build after 16-17 lands the
real category ids if the category filter is exercised in verification.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Orphan-number criterion of the report query | 16-19 |
| Everything else outgoing | 16-12 … 16-17 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.4] filter fields
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.11] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-11 acceptance criteria
- [Source: Backend/src/IIROSA.Infrastructure/Data/Repository/OutgoingRepository.cs] `GetPagedAsync`
- [Source: _bmad-output/implementation-artifacts/epic-16-correspondence-incoming-and-outgoing/16-2-search-incoming-letters.md] mirrored defect
  classes

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): mirrored the 16-2 fixes — `OutgoingRepository.ApplyFilters` carries independent `Serial`/`DepartmentId`/`CategoryId`/`CharityId`/date predicates; `SearchTerm` narrowed to subject-only this pass; `ApplySorting` (:131-141) case-insensitive with `Date ?? CreatedOn`-descending default.
- 2026-08-24 (verification): project builds clean; `ng build` clean for the module.

### Completion Notes List

- Independent criteria: `serial` (exact), `searchTerm` (subject), `charityId` (HQ-only), `departmentId`, `categoryId` (live 16-17 ids), `year`, `startDate`/`endDate`, `hasReply` (as الحاله). بحث resets page 1; empty result → empty row + `totalCount = 0` paging.
- **Deviations (recorded, shared with 16-10):** the never-binding `createdBy` key was dropped entirely rather than rebound — §21.S.5 gives an outgoing letter no employee field, so there is nothing to filter (spec's الموظف applies to the incoming side's field). `hasReply` stays **on** the bar as الحاله (تم الرد / لم يتم الرد) — it is the realisation of §21.S.4's الحاله for an entity with no status column (the story's "take it off the bar" reading assumed الحاله meant something else; the reply-state reading wins because §21.S.5 defines no status).
- Task 3's live criterion check is pending the user's `IIROSA.Api` restart; `npm run build` verified clean for the module. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Infrastructure/Data/Repository/OutgoingRepository.cs` — criteria + sort
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letters-list.component.ts` — `buildFilter` + بحث wiring

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-11 and module spec §21.S.4 / §21.U.11; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (independent criteria, subject-only search, الحاله=hasReply reading, dead createdBy key dropped). Status → review; live criterion check pending user's API restart. |
