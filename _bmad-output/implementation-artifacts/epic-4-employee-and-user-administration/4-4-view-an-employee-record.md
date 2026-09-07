# Story 4.4: View an employee record — بيانات الموظف

| Field | Value |
| --- | --- |
| Story key | `4-4-view-an-employee-record` |
| Epic | EP-04 — Employee & User Administration |
| Use case | UC-EMP-04 — بيانات الموظف |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/09-UC-EMP-Employee-and-User-Administration.md` (§9.U.4 scenario) |
| Route | `#/employees/:id` |
| Endpoint | `GET /api/EmployeeManagement/{id}` |

## Status

done — 2026-08-24 code review passed: the DEVIATES verdict is cleared (DepartmentName now fed
via Include reads, HasAccount rendered); see Review Findings. Review-and-complete pass before
that. (History: an as-built record of the 2026-08-19 audit's `done`.)

## Story

As a general director, I want to be able to view an employee record بيانات الموظف, so that I can
see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a general director with an active session in the module, when the actor opens the screen
   with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/EmployeeManagement/{id}` and the response is rendered on the screen without a page
   reload.
3. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §9.S are implemented with their mandatory flags and
lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## As built

- `EmployeeDetailComponent` (`modules/employees/employee-detail/`) → `EmployeeService
  .getEmployeeById` → `EmployeeManagementController.GetEmployee` → `EmployeeService
  .GetEmployeeByIdAsync` (`EmployeeService.cs:105-128`): maps `EmployeeDetailDto`, then loads
  identity roles **only when the row has an email** (via the fetch-all + per-user pattern — see
  4-1 defect 2).
- The component exposes Manage-Roles and Edit page actions; `NotFound` returns an anonymous
  `{ message }` shape the frontend surfaces through `error?.error?.message`.
- The detail screen shows what the row holds — which, per 4-3's broken create contract, can be an
  empty `FullName`/`Code` for rows created through the screen.

## Known defects in the shipped implementation

1. **Soft-deleted rows are returned.** `GetByIdAsync` has no `IsDeleted` filter; reads in this
   module do not honour the soft-delete rule (CLAUDE.md: always filter deleted rows out of
   reads).
2. **`EmployeeDetailDto` does not expose `FK_UserId`** — the screen cannot tell whether the
   employee has a login account at all (relevant to 4-2's edit-mode exclusion and to any future
   "link account" work).
3. Roles display depends on the fetch-all user list (`GetUsersFilteredAsync`) — the same N+1
   pattern as 4-1.

## Systemic debt inherited (do NOT fix per-story)

See `4-1-list-employees.md`: ControllerBase inheritance, bare response shapes. (Corrections
2026-08-24: the "unregistered `ManagementOnly` policy" entry was a misread — see 4-1's
correction; the dead component tree was deleted in this pass.)

## Dev Notes

### References

- [Source: _bmad-output/planning-artifacts/epics.md §3.4 US-EMP-04]
- [Source: docs/Modules/09-UC-EMP-Employee-and-User-Administration.md §9.U.4]
- [Source: Backend/src/IIROSA.Application/Services/EmployeeService.cs:105-128]
- [Source: Frontend/src/app/modules/employees/employee-detail/employee-detail.component.ts]

### Environment warnings

See `4-1-list-employees.md`.

## Dev Agent Record

### Agent Model Used

Claude (GLM-5) — 2026-08-24 review-and-complete pass over the pre-existing implementation.

### Debug Log References

- Same verification batch as 4-1/4-3 (backend compile clean; tsc clean for touched files).

### Completion Notes List

- `GetEmployeeByIdAsync` now returns null for soft-deleted rows → 404, not a ghost record.
- `EmployeeDetailDto` exposes `FK_UserId` + computed `HasAccount` so the screen can tell an
  account-less employee from a linked one.
- Detail template's dead bindings fixed (`employeeCode` → `code`, `department` →
  `departmentName` — the old names never matched the wire, so both cells always rendered "-").
- Edit-mode load splits ISO datetimes to date-only inputs and binds `departmentId` by id.

### File List

- `Backend/src/IIROSA.Application/Services/EmployeeService.cs` — soft-delete guard
- `Backend/src/IIROSA.Application/DTOs/EmployeeManagement/Employees.cs` — `FK_UserId`/`HasAccount`
- `Frontend/src/app/modules/employees/employee-detail/employee-detail.component.html`
- `Frontend/src/app/modules/employees/employee-form/employee-form.component.ts` — edit-mode load

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Soft-deleted rows 404; HasAccount exposed; dead template bindings fixed. Debt-list entry about unregistered policy corrected (misread). |
| 2026-08-24 | Code review: Include-carrying reads feed DepartmentName (detail/list/export); HasAccount badge rendered; salary 0 renders. |

## Review Findings (code review 2026-08-24)

Both patches applied in the same-day review-fix pass — this clears the audit's DEVIATES
verdict.

- [x] [Review][Patch] **DepartmentName is never populated — the headline binding fix was
  ineffective.** Fixed: new `GetByIdWithDepartmentAsync` / `GetAllWithDepartmentAsync`
  (Include-carrying, pattern of the repository's own `GetByEmailAsync`) are now used by the
  detail read and the list; the detail cell, 4-1's list column and the export column populate
- [x] [Review][Patch] `HasAccount` exposed but consumed by nothing — fixed: login-account
  badge rendered in the detail template (`employees.loginAccount`/`hasAccount`/`noAccount`
  keys in ar+en)
- [x] Roles loaded only when Email non-null — fixed via 4-1's `FK_UserId` guard (both call
  sites rekeyed)
