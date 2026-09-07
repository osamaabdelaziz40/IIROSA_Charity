# Story 13-1: List development projects

| Field | Value |
| --- | --- |
| Story key | `13-1-list-development-projects` |
| Epic | EP-13 — Office Development Projects |
| Use case | UC-OFP-01 — المشاريع التنموية |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/18-UC-OFP-Office-Development-Projects.md` (§18.S.1 screen, §18.U.1 scenario) |
| Route | `#/office-development-projects` |
| Endpoint | `GET /api/OfficeProjectManagement` |
| Depends on | EP-01 (authentication and role resolution) |

## Status

done

## Story

As a General Director, I want to be able to list development projects المشاريع التنموية, so that
I can find the record I need without leaving the system.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at `#/office-development-projects`,
   when the actor opens the screen with valid input, then no stored data is changed — the operation
   is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/OfficeProjectManagement` and the response is rendered on the screen without a page
   reload.
3. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §18.S are implemented with their mandatory flags and
lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Tasks / Subtasks

- [x] **Task 1 — Apply the caller's country scope on the read** (DoD)
  - [x] Inject `ICurrentUserService` into `OfficeProjectService`
  - [x] Pin `filter.CountryId` to the caller's country claim when the token carries one (module is
        head-office only; a General Director without a country claim sees all countries)
- [x] **Task 2 — Add the serial-number column to the grid** (§18.S.1 — الرقم)
- [x] **Task 3 — Map the guard's permissions to roles on the client**
  - [x] Add `OfficeDevelopmentProjects.*` entries to `PERMISSION_ROLES` so `PermissionGuard`
        enforces instead of warning and failing open
- [~] **Task 4 — Tests** — EXCLUDED FROM SCOPE by user decision (same standing decision as epic 3;
      no test project exists under `Backend/tests`)

## Dev Notes

### Findings from the review that produced these tasks (2026-08-19)

- Verified live against the running API: `GET /api/OfficeProjectManagement` returns
  `{"items":[…],"totalCount":n}` camelCase with real seeded data; the grid renders it with paging
  and filters. The read path itself works.
- The service applies **no caller scoping at all** — any HQ user can enumerate every country's
  projects. Epic 3 established the platform rule: an HQ caller pinned to a country must not read
  another country's register by omitting the filter (`CharityService.ApplyCallerScope`).
- §18.S.1 lists الرقم (row number) as the grid's first column; the implemented grid starts with
  the project name.
- `PermissionGuard` falls back to *allow with a console warning* for permissions missing from
  `PERMISSION_ROLES` (`auth.service.ts:337-353`); all five `OfficeDevelopmentProjects.*`
  permissions used by this module's routes are missing. The endpoints authorise server-side, so
  this is defence-in-depth, not the control itself.
- Roles: the spec's actors are Gen. Director and Staff; the seeded role set realises them as
  `SuperAdmin` and `Admin` respectively (permission matrix row "Office development projects":
  F/F/–/–/–). The controller and menu already gate on `Admin,SuperAdmin` — consistent, kept.

### Wire contract (verified empirically, not assumed)

Responses are camelCase on the wire (`token`, `items`, `projectName` confirmed by curl). The
`AddJsonOptions` + `AddNewtonsoftJson` combination in `Program.cs` looked statically like it would
emit PascalCase; it does not. All frontend camelCase reads are correct.

## Dev Agent Record

### Implementation Plan

1. Country scope in the service (same shape as `CharityService.ApplyCallerScope`, simplified for
   an HQ-only module: pin, never widen).
2. Row-number column in `project-list.component.html`.
3. `PERMISSION_ROLES` entries for the five route permissions.

### Debug Log

- 2026-08-19: while fixing the serial column, the grid turned out to read alias fields the API
  never sends (`officeProjectType`, `regionName`, `assignedCharityName`, …) — the type/region/
  center/village/charity columns had been rendering `-` since the copy. Fixed together with this
  story (real `OfficeProjectListDto` fields), because a serial column on a grid that shows nothing
  else is meaningless.
- 2026-08-19: the component also sliced the already-paginated server page a second time
  (`paginatedProjects`), so every page after the first rendered empty. Removed the client-side
  slice; the server owns paging.

### Completion Notes

- **Country scope** (`OfficeProjectService.ApplyCallerScope`): the caller's `countryId` claim pins
  the filter — an HQ caller pinned to a country cannot enumerate another's projects by omitting
  the filter; a caller without the claim (this module's SuperAdmin seeds) sees all countries.
  Same shape as `CharityService.ApplyCallerScope`, simplified for an HQ-only module: pin, never
  widen.
- **Serial column**: الرقم added as the grid's first column, numbered continuously across pages
  (`(currentPage - 1) * pageSize + i + 1`); i18n key `officeDevelopmentProjects.serial` added to
  both locales.
- **Permission map**: four `OfficeDevelopmentProjects.*` entries added to `PERMISSION_ROLES`,
  mirroring the controller's `[Authorize]` sets — delete mapped to `SuperAdmin` only. The five
  module routes now enforce instead of warn-and-allow.
- **Verified live** (https://localhost:60960): `GET /api/OfficeProjectManagement` returns clean
  camelCase keys with navigation names populated (`projectType: خياطه`, `countryName: مصر`).
- Tests excluded by user decision.

## File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Application/Services/OfficeProjectService.cs` | Injected `ICurrentUserService` + `IUnitOfWork`; added `ApplyCallerScope` pinning list reads to the caller's country |
| `Backend/src/IIROSA.Application/DTOs/OfficeProjectManagement/OfficeProjects.cs` | Filter DTO keys renamed `FK_*` → clean (shared with 13-3) |
| `Frontend/src/app/modules/office-development-projects/project-list/project-list.component.ts` | Filter keys renamed; double pagination removed; `canDelete` getter |
| `Frontend/src/app/modules/office-development-projects/project-list/project-list.component.html` | Serial column added; grid reads real wire fields; actions unlocked; delete gated |
| `Frontend/src/app/modules/office-development-projects/models/office-project.model.ts` | `OfficeProjectFilter`/`OfficeProjectListItem` cleaned of `fk_*` keys and aliases |
| `Frontend/src/app/core/services/auth.service.ts` | `PERMISSION_ROLES` entries for `OfficeDevelopmentProjects.View/Create/Edit/Delete` |
| `Frontend/src/assets/i18n/ar.json` · `en.json` | `officeDevelopmentProjects.serial` (الرقم / #) |
| `_bmad-output/implementation-artifacts/sprint-status.yaml` | Story 13-1 review note; epic-13 → done |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-OFP-01 and module spec §18.S.1 / §18.U.1; audit findings recorded. |
| 2026-08-19 | Implemented Tasks 1–3; grid alias-field and double-pagination defects found and fixed alongside. Backend builds 0 errors; frontend builds; list endpoint verified live. |
