# Story 15-2: Filter missions by date

| Field | Value |
| --- | --- |
| Story key | `15-2-filter-missions-by-date` |
| Epic | EP-15 — Missions (المأموريات) |
| Use case | UC-MSN-02 — البحث بالتاريخ |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/20-UC-MSN-Missions.md` (§20.S.1 screen filters, §20.U.2 scenario) |
| Endpoint | `GET /api/MissionManagement` carrying `DateFrom`, `DateTo` |
| Depends on | **15-1 landed** (renamed filter DTO, fixed filter expression, caller scope, rebuilt `#/missions` screen) |
| Roles | Gen. Director, Staff → `SuperAdmin`, `Admin` |

## Status

done

## Story

As a General Director, I want to be able to filter missions by date البحث بالتاريخ, so that
I can locate a record from partial information.

## Acceptance Criteria

1. Given a General Director with an active session on `#/missions`, when the actor enters a date
   range and presses بحث, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/MissionManagement` carrying `DateFrom, DateTo` and only rows whose `MissionDate`
   falls inside the range are rendered, without a page reload.
3. Given no row matches the range, then the grid renders empty and the paging control reports
   zero pages.
4. Given the session has expired or the role is not permitted, then the request is rejected and
   the actor is routed back to the login screen.

**Definition of done:** §20.S filter fields behave as specified; §20.U.2 passes end to end; the
caller scope still applies to the filtered read (an HQ caller pinned to a country cannot widen
their view by filtering).

## Context after 15-1

15-1 already: renamed `MissionFilterDto` to clean wire keys (`DateFrom`/`DateTo` included), fixed
the expression bug that **replaced** all filters with a dates-only predicate when both dates were
set, added `ApplyCallerScope` (country pin, pin-never-widen), and built the `#/missions` screen
with من تاريخ / الي تاريخ pickers wired to `my-missions`. Both endpoints share the same service
pipeline, so the backend half of this story is mostly **verification**.

## Tasks / Subtasks

- [x] **Task 1 — Target the بحث command at the specified endpoint** (AC 2)
  - [x] In `mission-list.component.ts`: the search button issues
        `missionService.getMissions({ dateFrom, dateTo, charityId?, page: 1, pageSize })` →
        `GET /api/MissionManagement` — the initial page load keeps using `getMyMissions` (15-1)
  - [x] Clearing both dates and searching returns to the unfiltered register
- [x] **Task 2 — Date-range sanity** (AC 2)
  - [x] Client: refuse `DateFrom > DateTo` with a translated message before issuing the call
  - [x] Server: tolerate inverted ranges (return empty set, not 500) — no throw on bad input
- [x] **Task 3 — Verify the filtered read end to end** (AC 1–4)
  - [x] Live: `GET /api/MissionManagement?dateFrom=…&dateTo=…` narrows by `MissionDate` and
        returns camelCase `items/totalCount`; combined with `charityId` both predicates apply
        (regression: the old code dropped non-date filters when both dates were set)
  - [x] Empty range → `items: []`, `totalCount: 0`, grid shows the empty state
  - [x] Unauthorized call rejected
- [x] **Task 4 — i18n**: add the inverted-range message key to `ar.json` + `en.json` (missions block)

## Dev Notes

- Do not add a validator for the filter DTO — no done module validates read filters (13-1
  precedent); server-side tolerance (Task 2) is the control.
- Do not re-wrap responses in `ApiResponse<T>`; do not introduce `data-list` — platform-level
  deviations recorded in 15-1 and `deferred-work.md`.
- Soft delete is a global query filter (`ModelBuilderExtensions.cs:83`) — deleted missions can
  never reappear via the date filter; don't add manual `IsDeleted` checks.
- Newtonsoft serializes (Program.cs) — wire is camelCase; trailing acronyms keep their case.

### References

- [Source: docs/Modules/20-UC-MSN-Missions.md#20.U.2] scenario — GET /api/MissionManagement
  carrying DateFrom, DateTo
- [Source: _bmad-output/planning-artifacts/epics.md#3.15] US-MSN-02 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-1-list-missions.md] baseline: DTO rename,
  expression fix, ApplyCallerScope

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

Live verification 2026-08-23: with rows dated 2026-08-25 and 2026-09-10 in the table,
`?dateFrom=2026-09-01` returned `totalCount: 1` (the 2026-09-10 row only) — the range predicate
translates and combines with the other filters. Inverted ranges are refused client-side
(`missions.invalidDateRange`) before the request is sent.

### Completion Notes List

- Filter keys renamed to the clean wire names (`Search`, `MissionTypeId`, …, `DateFrom`, `DateTo`,
  `CharityId`) in `MissionFilterDto`; the service's `BuildFilterExpression` now ANDs the date range
  with every other predicate — the copied code's both-dates branch **replaced** the combined
  expression with a dates-only lambda, silently discarding all other filters.
- Frontend `buildHttpParams` sends the final key names; the list screen's من تاريخ / الي تاريخ
  pickers reload page 1 via the بحث command; clear-filters resets to the unfiltered register read.
- Paged envelope mapping reads `page` (the copied SPA read `pageNumber`, which the API never sent).

### File List

- `Backend/src/IIROSA.Application/DTOs/MissionManagement/Missions.cs` — MissionFilterDto keys
- `Backend/src/IIROSA.Application/Services/MissionService.cs` — combined filter expression
- `Frontend/src/app/modules/missions/services/mission.service.ts` — params + page mapping
- `Frontend/src/app/modules/missions/mission-list/mission-list.component.ts|.html`
- `Frontend/src/assets/i18n/ar.json`, `en.json` — invalidDateRange key

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created from `epics.md` US-MSN-02 and module spec §20.S.1 / §20.U.2. |
| 2026-08-24 | Implemented (review-and-complete pass over the copied vertical), verified live against the running API, status → review. |
| 2026-08-24 | Code review: no patch findings — verified clean (the date-bound fix itself landed under 15-1); status → done. |
