# Story 13-2: Select a project type

| Field | Value |
| --- | --- |
| Story key | `13-2-select-project-type` |
| Epic | EP-13 — Office Development Projects |
| Use case | UC-OFP-02 — اختيار نوع المشروع |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/18-UC-OFP-Office-Development-Projects.md` (§18.S.2 screen, §18.U.2 scenario) |
| Route | `#/office-development-projects/create` (form lookup feed) |
| Endpoint | `GET /api/LookupManagement/office-project-types` |
| Depends on | EP-01 (authentication and role resolution) |

## Status

done

## Story

As a General Director, I want to be able to select a project type نوع المشروع, so that I can see
the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a General Director with an active session in the module, when the actor opens the screen
   with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/LookupManagement/office-project-types` and the response is rendered on the screen
   without a page reload.
3. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §18.S are implemented with their mandatory flags and
lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Tasks / Subtasks

- [x] **Task 1 — Remove the dead lookup methods from the module's own service**
  - [x] Delete `getCountries()` / `getRegions()` / `getCenters()` / `getCharities()` from
        `office-project.service.ts` — they target `/api/Lookup/Countries`, an endpoint that does
        not exist; every component already loads lookups through `LookupManagementService`
  - [x] Keep `getProjectTypes()` only if it targets the real lookup endpoint; otherwise delete it
        too and standardise on `LookupManagementService`
- [x] **Task 2 — Verify the type feed reaches both consumers**
  - [x] List screen filter dropdown (نوع المشروع) populated from the lookup endpoint
  - [x] Create/edit form required-field dropdown populated from the same endpoint
- [~] **Task 3 — Tests** — EXCLUDED FROM SCOPE by user decision (same standing decision as epic 3;
      no test project exists under `Backend/tests`)

## Dev Notes

### Findings from the review that produced these tasks (2026-08-19)

- The live lookup read works: components call `LookupManagementService`, which reaches the real
  lookup controller, and the نوع المشروع dropdowns populate. The read path this story asks for is
  already realised.
- `office-project.service.ts` carries four dead lookup methods (`/api/Lookup/Countries` etc.) that
  no component calls and whose endpoints do not exist. They are noise that invites a future
  "fix the URL" commit wiring the form to a second, divergent lookup source — removing them keeps
  one source of truth for lookups.
- Roles: spec's Gen. Director ≈ seeded `SuperAdmin` (permission matrix "Office development
  projects" F/F/–/–/–). The lookup endpoint is reachable by authenticated users of the module;
  the module's screens are gated `Admin,SuperAdmin` in the menu and routes.

## Dev Agent Record

### Implementation Plan

1. Delete the dead lookup methods from `office-project.service.ts`.
2. Confirm both dropdowns (list filter, form) load through `LookupManagementService` against the
   live API.

### Debug Log

- 2026-08-19: `getProjectTypes()` already targeted the real endpoint
  (`/api/LookupManagement/office-project-types`), so it was kept and documented as UC-OFP-02's
  read; the four dead `/api/Lookup/*` methods were deleted along with the granular endpoint
  methods removed by 13-6.

### Completion Notes

- The lookup read this story specifies was already realised: both the list filter dropdown and
  the form's mandatory نوع المشروع dropdown load through `LookupManagementService` /
  `OfficeProjectService.getProjectTypes()` against the live API.
- `office-project.service.ts` now exposes exactly one lookup method — there is a single source of
  truth per lookup, so a future "fix the URL" commit cannot wire the form to a divergent feed.
- Verified live: `GET /api/LookupManagement/office-project-types` returns the seeded types
  (`خياطه`, …) that the detail/create responses carry.

## File List

| File | Change |
| --- | --- |
| `Frontend/src/app/modules/office-development-projects/services/office-project.service.ts` | Dead `/api/Lookup/*` methods and granular endpoint methods deleted; `getProjectTypes()` documented as the UC-OFP-02 read |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-OFP-02 and module spec §18.S.2 / §18.U.2; audit findings recorded. |
| 2026-08-19 | Dead lookup surface removed; type feed verified against both consumers and the live API. |
