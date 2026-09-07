# Story 4.5: Update an employee and change their role — تعديل بيانات الموظف

| Field | Value |
| --- | --- |
| Story key | `4-5-update-an-employee-and-change-their-role` |
| Epic | EP-04 — Employee & User Administration |
| Use case | UC-EMP-05 — تعديل بيانات الموظف |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/09-UC-EMP-Employee-and-User-Administration.md` (§9.S.2 screen, §9.U.5 scenario) |
| Route | `#/employees/:id/edit` (role change: `#/employees/:id/roles`) |
| Endpoint | `PUT /api/EmployeeManagement/{id}` (board records `PUT /api/EmployeeManagement`; the shipped action is route-templated `{id}` — `EmployeeManagementController.cs:115`) |

## Status

done — 2026-08-24 code review passed: PUT semantics decision applied, all patches in (see
Review Findings); the audit's DEVIATES verdict is cleared. Review-and-complete pass before
that. (History: an as-built record of the audit's `done` noting the shared broken wire
contract.)

## Story

As a general director, I want to be able to update an employee and change their role تعديل
بيانات الموظف, so that a record that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given a general director with an active session in the module, when the actor invokes the
   function with valid input, then the stored record carries the new values; no other record is
   affected.
2. Given the request is accepted, when it is served, then it is handled by
   `PUT /api/EmployeeManagement` and the response is rendered on the screen without a page
   reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor
   saves, then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §9.S are implemented with their mandatory flags and
lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## As built

- `EmployeeFormComponent` in edit mode → `EmployeeService.updateEmployee(id, …)` →
  `EmployeeManagementController.UpdateEmployee` → `EmployeeService.UpdateEmployeeAsync`
  (`EmployeeService.cs:213-248`): loads row, re-checks `IsEmailUniqueAsync(email, id)`
  (exclusion present — the pattern 4-2 mirrors), maps, saves.
- **Role change is a separate flow** from profile update: `RoleAssignmentComponent`
  (`#/employees/:id/roles`) → `POST {id}/roles` / `DELETE {id}/roles/{roleName}` →
  `AssignEmployeeRoleAsync` / `RemoveEmployeeRoleAsync`, which resolve `FK_UserId` and delegate to
  `IUserAppServiceExtended`. The spec's atomic old-role→new-role swap (§9.U.5 summary) is
  realised as two explicit membership operations instead.
- Identity profile fields (email, phone, active) are **not** synced to the identity user on
  employee update — only the Employee row changes.

## Known defects in the shipped implementation

1. **Same wire-contract break as 4-3, amplified by nullables.** `UpdateEmployeeRequest` sends
   `firstName`/`lastName`/`employeeCode`/`department`/`roleName`; `UpdateEmployeeDto` binds
   `FullName?`/`Code?`/`DepartmentId?`/`Roles?` — all bind to null, and `AutoMapper` then maps
   nulls over the row. Effectively only `email` (and the other matching names: position,
   phoneNumber, dateOfBirth, gender, address, salary) reach the database.
2. **`UpdateEmployeeAsync` re-checks email uniqueness but not the identity store**, and does not
   push a changed email to the linked identity user — employee row and login credential can
   diverge.
3. AC 3 is client-side only (`employeeForm.invalid`); the service layer has no validator
   (module-wide; see 4-2 decision D4).

**Recommendation recorded, not executed:** rework 4-3 + 4-5 together (contract reconciliation,
credential/role wiring, identity sync). `/bmad-correct-course` or a defect story.

## Systemic debt inherited (do NOT fix per-story)

See `4-1-list-employees.md`.

## Dev Notes

### References

- [Source: _bmad-output/planning-artifacts/epics.md §3.4 US-EMP-05]
- [Source: docs/Modules/09-UC-EMP-Employee-and-User-Administration.md §9.S.2, §9.U.5]
- [Source: Backend/src/IIROSA.Application/Services/EmployeeService.cs:213-248, 360-421]
- [Source: Frontend/src/app/modules/employees/employee-form/employee-form.component.ts:112-144, 155-182]

### Environment warnings

See `4-1-list-employees.md`.

## Dev Agent Record

### Agent Model Used

Claude (GLM-5) — 2026-08-24 review-and-complete pass over the pre-existing implementation.

### Debug Log References

- Same verification batch as 4-3 (backend compile clean; tsc clean for touched files).

### Completion Notes List

- Update map is null-conditioned (`ForAllMembers(srcMember != null)`): an omitted field no
  longer wipes the stored value.
- Email availability checked against BOTH stores with `excludeEmployeeId` — the employee's own
  account never blocks its own save.
- When `FK_UserId` exists, the PUT mirrors profile changes onto the identity user
  (`UpdateUserDetailAsync`) and syncs roles (diff via `GetUserRolesAsync` + assign/remove) —
  the identity UserName IS the email, so an unsynced change would leave the old login working.
- Soft-deleted employee ids are rejected like missing ones.
- Frontend edit form sends the fixed contract (see 4-3); password field is create-only.

### File List

- `Backend/src/IIROSA.Application/Services/EmployeeService.cs` — `UpdateEmployeeAsync` rework
- `Backend/src/IIROSA.Application/Profiles/EmployeeProfile.cs` — null-conditioned update map
- `Frontend/src/app/modules/employees/employee-form/employee-form.component.ts` + `.html`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Null-conditioned update, both-store availability with self-exclusion, identity profile + role sync on the PUT. |
| 2026-08-24 | Code review: PUT semantics (user decision), dirty-only role sync, 404/400 split, loud identity/role-sync failures, salary-0 fix, password clear on load error. |

## Review Findings (code review 2026-08-24)

The decision was put to the user on 2026-08-24 (recommended option chosen); all items
resolved in the same-day review-fix pass — this clears the audit's DEVIATES verdict.

- [x] [Review][Decision] **Clearing an optional field was a silent no-op** — **RESOLVED (user,
  2026-08-24): PUT semantics.** The update map writes explicit nulls (`IsActive` ignored — the
  dedicated activate/deactivate endpoints own it); the form sends explicit state; the service
  guards Email/FullName against blanking and keeps the original Code when blank
- [x] [Review][Patch] Multi-role employees lose extra roles on edit save — fixed: the form
  sends `roles` only when the role control is dirty; an untouched role set is left alone
  server-side (single-select cannot represent multi-role, so it never speaks for it)
- [x] [Review][Patch] Update conflicts return 404 [EmployeeManagementController.cs] — fixed:
  not-found throws `KeyNotFoundException` (→ 404); `InvalidOperationException` maps to
  `BadRequest` (→ 400), matching create
- [x] [Review][Patch] Identity mirror + role-sync results ignored [EmployeeService.cs] —
  fixed: `UpdateUserDetailAsync`'s `Guid.Empty` and failed assign/remove calls now throw with
  explicit messages — no more success-over-drift
- [x] [Review][Patch] `salary: formValue.salary || undefined` drops 0 — fixed: `!== ''` sends
  0, empty sends null (clears); create branch likewise; detail screen renders 0 via `!= null`
- [x] [Review][Patch] Password validators cleared only on the success path — fixed: cleared in
  the error callback too
