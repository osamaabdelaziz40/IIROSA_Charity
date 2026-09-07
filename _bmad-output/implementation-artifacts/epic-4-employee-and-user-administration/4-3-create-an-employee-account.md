# Story 4.3: Create an employee account — اضافة موظف

| Field | Value |
| --- | --- |
| Story key | `4-3-create-an-employee-account` |
| Epic | EP-04 — Employee & User Administration |
| Use case | UC-EMP-03 — اضافة موظف |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/09-UC-EMP-Employee-and-User-Administration.md` (§9.S.2 screen, §9.U.3 scenario) |
| Route | `#/employees/create` (board records `#/employees/:id`; the routing module maps `create` explicitly — `employees-routing.module.ts:14-17`) |
| Endpoint | `POST /api/EmployeeManagement` |

## Status

done — 2026-08-24 code review passed: both decisions resolved by the user, all patches applied
(see Review Findings); audit verdict COMPLIANT-WITH-NOTES. Review-and-complete pass before
that. (History: the original audit's `done` judged endpoint-exists + screen-reachable while
the wire contract between them was broken.)

## Story

As a general director, I want to be able to create an employee account اضافة موظف, so that the
register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a general director with an active session on the screen at `#/employees/:id`, when the
   actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the
   creating user, and appears in the list screen of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/EmployeeManagement` and the response is rendered on the screen without a page
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

- Screen: `EmployeeFormComponent` (`modules/employees/employee-form/`), used for both create and
  edit. Fields: email, firstName, lastName, employeeCode, position, department, phoneNumber,
  dateOfBirth, gender, address, hireDate, salary, roleName.
- API: `EmployeeManagementController.CreateEmployee` (`[FromBody] CreateEmployeeDto`) →
  `EmployeeService.CreateEmployeeAsync`: validates `IsEmailUniqueAsync` / `IsCodeUniqueAsync`,
  inserts the Employee row, then — only when **both** Email and Password are non-empty — creates
  the identity user via `IUserAppServiceExtended.CreateUserAsync` and links `FK_UserId`
  (`EmployeeService.cs:133-208`).

## Known defects in the shipped implementation

These are why "done" means *audit-done*, not *works*:

1. **The wire contract is broken — the save cannot work as shipped.** The form POSTs
   `CreateEmployeeRequest { email, firstName, lastName, employeeCode, department, roleName, … }`
   (`core/models/employee.model.ts:25-39`) but `CreateEmployeeDto` binds `Code`, `FullName`,
   `DepartmentId (int?)`, `Roles` (`DTOs/EmployeeManagement/Employees.cs:49-67`). JSON binding is
   case-insensitive, not name-translating: `FullName` arrives as its default `""` (a required
   column), `Code` as `""`, `Roles` empty. First save inserts a nameless row; the **unique index
   on `Code`** (`EmployeeConfiguration.cs:51`) makes the second save a 500.
2. **No identity account is ever created from this screen.** The form has no password field, and
   `CreateEmployeeAsync` skips user creation when Password is empty — so UC-EMP-03's "login
   credentials" half (spec §9.S.2: إسم المستخدم + كلمة المرور, both mandatory) silently never
   happens. `roleName` from the form binds nothing (`Roles` is the DTO property).
3. **Charity stamping (AC 1) is absent.** `Employee` has no CharityId; nothing stamps "owned by
   the charity of the creating user". Consistent with the module being HQ-only, but AC 1's
   wording is then unsatisfiable as written.
4. **User-creation failure is swallowed.** If `CreateUserAsync` fails (e.g. duplicate identity
   email), the employee row is already committed; the code logs a warning and returns success
   (`EmployeeService.cs:180-189`) — an employee without a login, reported as created.
5. Repository calls `SaveChangesAsync` directly (`EmployeeRepository` inherits the pattern);
   CLAUDE.md's "only `IUnitOfWork` saves" is not followed anywhere in this service yet.

**Recommendation recorded, not executed:** the create/edit contract needs a deliberate rework
decision (`/bmad-correct-course` or a defect story covering 4-3 + 4-5 together: reconcile
`CreateEmployeeRequest`↔`CreateEmployeeDto`, add the credential/role fields the spec mandates,
decide Employee.CharityId). Story 4-2 deliberately works around it (attaches its validator to
`email`, the one field that binds correctly).

## Systemic debt inherited (do NOT fix per-story)

ControllerBase-not-ApiController, bare response shapes, hard-coded roles list, dead
`getDepartments()` endpoint. Full list in `4-1-list-employees.md`. (Corrections 2026-08-24: the
former "unregistered named policies" entry was a misread of the dead Program.cs block — the
policies are registered in `ServiceCollectionExtensions.AddAuth`, see 4-1; the dead
`employees-list/` tree was deleted in this pass.)

## Dev Notes

### References

- [Source: _bmad-output/planning-artifacts/epics.md §3.4 US-EMP-03]
- [Source: docs/Modules/09-UC-EMP-Employee-and-User-Administration.md §9.S.2, §9.U.3]
- [Source: Backend/src/IIROSA.Application/Services/EmployeeService.cs:133-208]
- [Source: Frontend/src/app/modules/employees/employee-form/employee-form.component.ts:77-93,184-212]

### Environment warnings

See `4-1-list-employees.md` — MSB3021/3027 live-API lock, stale `ng serve` bundles,
`--legacy-peer-deps`.

## Dev Agent Record

### Agent Model Used

Claude (GLM-5) — 2026-08-24 review-and-complete pass over the pre-existing implementation.

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — compile clean (MSB3021/3027 = live-API copy lock; restart
  the running process to load these changes). `npx tsc --noEmit` — clean for all touched files.

### Completion Notes List

- Wire contract fixed: the form now sends `fullName`, `code`, `departmentId`, `roles[]`,
  `password` — every field the backend `CreateEmployeeDto` actually binds.
- Identity user is created FIRST via `CreateUserAsync` and failure is loud (400 with the
  identity verification message) — an Employee row can no longer be silently account-less.
- `Code` is generated (`EMP-XXXXXXXX`) when the screen leaves it blank, so the unique index
  can no longer 500 the second screen-created employee.
- Email availability pre-checked through the same UC-EMP-02 probe the screen uses.
- Departments/roles in the form come from the live lookup endpoints (bilingual labels,
  `[ngValue]` ids); password field is create-only with `passwordStrengthValidator(3)`.
- NOT done: per-charity scoping (module is HQ-only, per 4-1 ruling).

### File List

- `Backend/src/IIROSA.Application/Services/EmployeeService.cs` — `CreateEmployeeAsync` rework
- `Backend/src/IIROSA.Application/Profiles/EmployeeProfile.cs` — create/update maps
- `Frontend/src/app/core/models/employee.model.ts` — request/response contract
- `Frontend/src/app/modules/employees/employee-form/employee-form.component.ts` + `.html`
- `Frontend/src/assets/i18n/ar.json`, `en.json` — form keys

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Wire contract reworked end to end; identity user created first with loud failure; Code auto-generated. Debt-list entry about unregistered policies corrected (misread). |
| 2026-08-24 | Code review: server-side mandatory fields (user decision), rollback of orphaned logins, unique generated codes, DepartmentId FK validation, CanManageUsers widened to Admin (user decision). |

## Review Findings (code review 2026-08-24)

Both decisions were put to the user on 2026-08-24 (recommended options chosen); all items
resolved in the same-day review-fix pass.

- [x] [Review][Decision] Mandatory account fields not enforced server-side — **RESOLVED
  (user, 2026-08-24): enforce server-side.** `CreateEmployeeAsync` now refuses blank
  Email/Password/Roles/Position; the identity account is always created, never optional
- [x] [Review][Decision] All writes SuperAdmin-only vs the spec's General Director actor —
  **RESOLVED (user, 2026-08-24): widen.** `CanManageUsers` now admits Admin alongside
  SuperAdmin. Deliberate platform-wide change: it also widens `UserManagementController`'s
  eight actions — that is the accepted blast radius
- [x] [Review][Patch] Orphaned identity user on insert failure [EmployeeService.cs] — fixed:
  compensating `_userAppService.DeleteAsync(userId)` rollback in the create catch (logs loudly
  if the rollback itself fails)
- [x] [Review][Patch] Generated `Code` never uniqueness-checked — fixed: `do/while` re-draws
  until `IsCodeUniqueAsync` passes
- [x] [Review][Patch] `DepartmentId` FK unvalidated — fixed: existence check via
  `IDepartmentService.GetLookupByIdAsync` on create and update
