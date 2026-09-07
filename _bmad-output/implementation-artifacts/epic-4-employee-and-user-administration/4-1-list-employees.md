# Story 4.1: List employees — قائمة الموظفين

| Field | Value |
| --- | --- |
| Story key | `4-1-list-employees` |
| Epic | EP-04 — Employee & User Administration |
| Use case | UC-EMP-01 — قائمة الموظفين |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/09-UC-EMP-Employee-and-User-Administration.md` (§9.S.1 screen, §9.U.1 scenario) |
| Route | `#/employees` |
| Endpoint | `GET /api/EmployeeManagement` |

## Status

done — 2026-08-24 code review passed: every finding fixed or deferred (see Review Findings);
audit verdict COMPLIANT-WITH-NOTES. Review-and-complete pass before that. (History: the
2026-08-19 audit's `done` reflected endpoint presence, not a working flow.)

## Story

As a general director, I want to be able to list employees قائمة الموظفين, so that I can find the
record I need without leaving the system.

## Acceptance Criteria

1. Given a general director with an active session on the screen at `#/employees`, when the actor
   opens the screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/EmployeeManagement` and the response is rendered on the screen without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §9.S are implemented with their mandatory flags and
lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## As built

Wire path: `EmployeeListComponent` (`Frontend/src/app/modules/employees/employee-list/`) →
`EmployeeService.getEmployees` (`api/employeemanagement`) →
`EmployeeManagementController.GetEmployees` (`[FromQuery] search/department/position/isActive/
page/pageSize`, builds `EmployeeFilterDto`) → `EmployeeService.GetEmployeesFilteredAsync` →
`EmployeeRepository.GetAllAsync()`.

- Route registered: `app-routing.module.ts:31` lazy-loads `EmployeesModule`;
  `employees-routing.module.ts` maps `''` → `EmployeeListComponent`.
- Columns rendered follow §9.S.1's grid (employee, role, country, last login, stop, change
  password, edit) only loosely — see defects.
- AC 5: 401 handled by `auth.interceptor`; a 403 branch was added during story 3-2 review
  follow-up and applies app-wide.

## Known defects in the shipped implementation

Recorded for follow-up; **not** licensed to fix inside a later story without a board change:

1. **`Department` / `Position` filter parameters are accepted and silently ignored.**
   `GetEmployeesFilteredAsync` applies only `SearchText` and `IsActive`
   (`EmployeeService.cs:50-65`); the controller still binds `department`/`position`, so the
   UI filters are no-ops.
2. **N+1 role loading per row.** `GetEmployeesFilteredAsync` calls `GetByIdAsync` again per
   returned row and `GetEmployeeRolesAsync` fetches the **entire** filtered user list via
   `GetUsersFilteredAsync` then `GetUserDetailAsync` per employee (`EmployeeService.cs:78-85`,
   `449-472`). Correct at page size 20, punitive as the register grows.
3. **`getDepartments()` calls a route that does not exist.** The frontend service calls
   `GET /api/employeemanagement/departments` (`employee.service.ts:63`); no such action exists on
   the controller. The component silently falls back to a hard-coded English `departments` array
   (`core/models/employee.model.ts:73`).
4. **Roles are hard-coded.** `loadRoles()` TODO returns
   `['SuperAdmin','Admin','Charity','Accountant','FinancialOfficer']`
   (`employee-form.component.ts:109`).
5. **Dead duplicate component tree.** `modules/employees/employees-list/` is not routed, redefines
   its own local `Employee` interface and uses `HttpService` instead of `ApiService`. The routed
   component is `employee-list/`. Treat `employees-list/` as deletion candidate (same known-debt
   family as `EmployeeAppService.cs`).
6. **Country column has no server source.** §9.S.1 lists البلد; neither `Employee` nor
   `EmployeeListDto` carries a country field.
7. AC 3/4 (charity scoping) are structurally inapplicable today: the endpoint returns **all**
   employees with no charity/country scope at all. Employees are HQ staff (module purpose), so
   the audit accepted this; if employees ever become charity-scoped, scoping must be added
   server-side.

## Systemic debt this module inherits (do NOT "fix" per-story)

- Controller inherits `ControllerBase`, not the project `ApiController`
  (`IIROSA.Api/Controllers/ApiController.cs`); returns anonymous `{ message }` shapes, not
  `Framework.Core.ApiResponse<T>`. 20 of 21 controllers are identical — story 3-2's review ruled
  this **deferred systemic debt**, one migration across all controllers. See
  `architecture.md` §6.1/§6.2/§10.
- `[Authorize(Policy = "ManagementOnly")]` / `"CanManageUsers"` resolve correctly: the active
  registration lives in `AddAuth` (`IIROSA.Application/ServiceCollectionExtensions.cs:174-194`,
  called at `Program.cs:95`). The `AddAuthorization` block at `Program.cs:334-353` is a dead
  commented duplicate of it. (Correction 2026-08-24: an earlier version of this bullet — and of
  decision D3 in 4-2, the 4-3/4-4 debt lists, and the board note — claimed the policies were
  unregistered, a misread of the dead block.)

## Dev Notes

### References

- [Source: _bmad-output/planning-artifacts/epics.md §3.4 US-EMP-01]
- [Source: docs/Modules/09-UC-EMP-Employee-and-User-Administration.md §9.S.1, §9.U.1]
- [Source: Backend/src/IIROSA.Api/Controllers/EmployeeManagementController.cs:33-63]
- [Source: Backend/src/IIROSA.Application/Services/EmployeeService.cs:41-100]
- [Source: _bmad-output/implementation-artifacts/epic-3-charity-administration/3-2-verify-charity-name-availability.md — review rulings cited above]

### Environment warnings (apply to every epic-4 story)

- The user runs the live API from the repo. `dotnet build` may fail with **MSB3021/MSB3027** —
  that is the output-copy step locked by the running process, not a compile error. Never kill the
  process.
- `ng serve` can serve a stale bundle after the watcher dies — grep the served chunk for the new
  code before believing a change had no effect.
- `npm install` needs `--legacy-peer-deps` (`ngx-bootstrap@12` peer-conflicts with Angular 18);
  do not commit the rewritten `package-lock.json`.

## Dev Agent Record

### Agent Model Used

Claude (GLM-5) — 2026-08-24 review-and-complete pass over the pre-existing implementation.

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — compile clean; remaining MSB3021/3027 are the live-API
  output-copy lock (the running process predates these changes — restart it to load them).
- `npx tsc --noEmit -p tsconfig.app.json` — zero errors in every touched file; 21 pre-existing
  errors remain confined to `modules/incoming-outgoing/` (untouched by this story).

### Completion Notes List

- Dept/position/role filters now enforced server-side in `GetEmployeesFilteredAsync`
  (`DepartmentId`, `Position`, `Role` via `GetUsersInRoles`); controller/export params updated.
- Soft-deleted rows excluded from the list and the detail read.
- Roles loaded once per page row (re-fetch removed); `GetEmployeeRolesAsync` simplified to
  `GetUserRolesAsync` (was fetch-all-users + per-user detail).
- Dead `getDepartments()` call replaced with the live lookup endpoint; hard-coded roles list
  replaced with `GET /api/RoleManagement`.
- Dead `employees-list/` component tree deleted (unrouted duplicate).
- NOT done (deferred, as ruled): charity/country scoping — module is HQ-only, employees are not
  charity-owned in this schema; ApiResponse<T> migration — deferred systemic debt.

### File List

- `Backend/src/IIROSA.Application/Services/EmployeeService.cs` — filters, soft delete, roles
- `Backend/src/IIROSA.Application/DTOs/EmployeeManagement/Employees.cs` — filter DTO fields
- `Backend/src/IIROSA.Api/Controllers/EmployeeManagementController.cs` — query params, export
- `Frontend/src/app/modules/employees/services/employee.service.ts` — live lookups
- `Frontend/src/app/modules/employees/employee-list/employee-list.component.ts` + `.html`
- `Frontend/src/app/modules/employees/employee-detail/employee-detail.component.html` — dead bindings
- `Frontend/src/app/modules/employees/employees-list/` — deleted

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Review-and-complete pass: filters wired server-side, soft delete, N+1 fixed, live lookups, dead tree deleted. Corrected the misread "policies unregistered" debt entry. |
| 2026-08-24 | Code review fixes: page clamp, FK_UserId roles guard, departments paging, en.json keys, Department column fed via Include reads. |

## Review Findings (code review 2026-08-24)

From the epic-4 three-layer review (blind diff hunt, edge-case hunt, acceptance audit).
Cross-story items live in the story that owns them; deferred items are copied to
`deferred-work.md`.

All patch items were applied in the same-day review-fix pass.

- [x] [Review][Patch] `page`/`pageSize` unvalidated [EmployeeService.cs:95] — fixed: both
  clamped to ≥1 before Skip/Take
- [x] [Review][Patch] Roles guard keyed on Email instead of `FK_UserId` — fixed: guard is
  `FK_UserId.HasValue` and roles come straight from the identity user (also removes the per-row
  employee re-fetch on the list path); `GetEmployeeByIdAsync` keyed the same way
- [x] [Review][Patch] Departments lookup capped at `pageSize: 100` [employee.service.ts] —
  fixed: pages through the whole active catalogue (`forkJoin` over the remaining pages)
- [x] [Review][Patch] `employees.roles` missing from en.json (+ `employees.employees` in both
  languages) — fixed in both files
- [x] Cross-ref: the department **column**'s dead display — fixed via 4-4's Include-carrying
  reads (`GetAllWithDepartmentAsync` on the list path); list column and export column now
  populate
- [x] [Review][Defer] Whole-table materialization [EmployeeService.cs:45] — deferred,
  pre-existing: `GetAllAsync()` loads every Employee before filtering/paginating in
  LINQ-to-Objects; needs an IQueryable repository surface
- [x] [Review][Defer] Per-row sequential role fetch within the page — deferred (partially
  improved: the employee re-fetch is gone, sequential identity calls remain); batch by
  `FK_UserId` when reworking
- [x] [Review][Defer] Role filter materializes full role membership per request
  [EmployeeService.cs:81] — deferred: `GetUsersInRoles` loads every user in the role on each
  filtered list call
