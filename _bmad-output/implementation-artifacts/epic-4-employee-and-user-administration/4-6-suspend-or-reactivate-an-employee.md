# Story 4.6: Suspend or reactivate an employee — إيقاف / تفعيل الموظف

| Field | Value |
| --- | --- |
| Story key | `4-6-suspend-or-reactivate-an-employee` |
| Epic | EP-04 — Employee & User Administration |
| Use case | UC-EMP-06 — إيقاف / تفعيل الموظف |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/09-UC-EMP-Employee-and-User-Administration.md` (§9.S.1 screen commands, §9.U.6 scenario) |
| Route | action on `#/employees` (list row icon) |
| Endpoint | `PATCH /api/EmployeeManagement/{id}/deactivate` (reactivation: `PATCH /api/EmployeeManagement/{id}/activate`) |

## Status

done — 2026-08-24 code review passed (audit verdict COMPLIANT-WITH-NOTES): soft-delete guards
added to this story's paths; two items deferred as recorded. Review-and-complete pass before
that (core behaviour unchanged, list actions translated). History: an as-built record of the
audit's `done`.

## Story

As a general director, I want to be able to suspend or reactivate an employee إيقاف / تفعيل
الموظف, so that a suspended account cannot sign in while its audit history is kept.

## Acceptance Criteria

1. Given a general director with an active session in the module, when the actor opens the screen
   with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `PATCH /api/EmployeeManagement/{id}/deactivate` and the response is rendered on the screen
   without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §9.S are implemented with their mandatory flags and
lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## As built

- List row icon → `EmployeeService.deactivateEmployee` / `activateEmployee` →
  `EmployeeManagementController.DeactivateEmployee` / `ActivateEmployee` →
  `EmployeeService.DeactivateEmployeeAsync` / `ActivateEmployeeAsync`
  (`EmployeeService.cs:253-322`): flips `Employee.IsActive`, saves, and — when `FK_UserId`
  exists — mirrors the flag onto the identity user via `SetUserActiveStatusAsync`, so sign-in is
  actually blocked (identity-user failure is logged, not surfaced).
- No hard delete involved; the separate `DELETE {id}` action exists on the controller but is not
  part of this use case (and `DeleteEmployeeAsync` hard-deletes rather than soft-deleting —
  flagged below).

## Known defects in the shipped implementation

1. **AC 1 mislabels a write.** The generator stamped the read-only clause on a state-changing
   operation; the shipped behaviour (flag flip + identity sync) is the correct reading of
   §9.U.6's summary. Recorded rather than silently ignored.
2. **The identity flag is advisory-only.** `SetUserActiveStatusAsync` failures are swallowed with
   a warning — the employee row says suspended while the login still works. Worth a follow-up
   decision (fail the request vs. accept drift).
3. AC 3/4 (charity scoping) are inapplicable as shipped — the endpoints operate on any employee
   id with no charity/country scope (module is HQ-only; same ruling as 4-1).
4. Adjacent hazard for any follow-up: `DELETE {id}` (`DeleteEmployeeAsync`, `EmployeeService.cs:
   426-444`) bypasses soft delete entirely and leaves the linked identity user alive — orphaning
   the account UC-EMP-06 exists to protect.

## Systemic debt inherited (do NOT fix per-story)

See `4-1-list-employees.md`.

## Dev Notes

### References

- [Source: _bmad-output/planning-artifacts/epics.md §3.4 US-EMP-06]
- [Source: docs/Modules/09-UC-EMP-Employee-and-User-Administration.md §9.S.1 commands, §9.U.6]
- [Source: Backend/src/IIROSA.Application/Services/EmployeeService.cs:253-322, 426-444]
- [Source: Frontend/src/app/modules/employees/services/employee.service.ts:36-42]

### Environment warnings

See `4-1-list-employees.md`.

## Dev Agent Record

### Agent Model Used

Claude (GLM-5) — 2026-08-24 review-and-complete pass over the pre-existing implementation.

### Debug Log References

- Same verification batch as 4-1 (backend compile clean; tsc clean for touched files).

### Completion Notes List

- Core suspend/reactivate flow left as shipped — it was already correct (IsActive flip +
  `SetUserActiveStatusAsync` mirror).
- Hard-coded English confirm dialogs and notifications on the list actions replaced with
  i18n keys (existing `employees.confirmEmployee*` / `employees.employeeDeactivated` etc.;
  en.json backfilled where the keys were missing).
- Open follow-up unchanged (defect 2 below): identity-sync failure is advisory-only — worth a
  deliberate fail-vs-drift decision outside this story.

### File List

- `Frontend/src/app/modules/employees/employee-list/employee-list.component.ts` — translated actions
- `Frontend/src/assets/i18n/en.json` — backfilled confirm/status keys

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | List actions translated; no behavioural change to the endpoints. |
| 2026-08-24 | Code review: IsDeleted guards on deactivate/activate/reset-password/role ops/delete. |

## Review Findings (code review 2026-08-24)

Patch applied in the same-day review-fix pass; the two defers stand as recorded.

- [x] [Review][Patch] No `IsDeleted` guard on this story's paths [EmployeeService.cs] — fixed:
  deactivate/activate/reset-password, role assign/remove (and delete) now reject soft-deleted
  rows like missing ones
- [x] [Review][Defer] Identity sync on deactivate/activate is advisory-only — deferred: this
  story's recorded open defect 2; the deliberate fail-vs-drift decision is still pending
- [x] [Review][Defer] `DELETE {id}` hard-deletes and orphans the identity user — deferred:
  recorded adjacent hazard (defect 4 below); needs its own decision/story
