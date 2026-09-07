import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, of, timer } from 'rxjs';
import { catchError, first, map, switchMap } from 'rxjs/operators';
import { EmployeeService } from '../../modules/employees/services/employee.service';

/**
 * UC-EMP-02: reports `userNameTaken` while the operator is still filling the form, instead of
 * letting a completed form die on a refused save.
 *
 * Follows the corrected charity-name-validator pattern:
 * - `timer(400)` debounces the check off individual keystrokes.
 * - `switchMap` cancels a superseded request, so a slow earlier reply cannot overwrite a newer one.
 * - The server echoes the name it checked; a reply about a different string (stale or mangled in
 *   transit) must not mark this control invalid.
 * - A failed request resolves to `null` (valid) — a network blip must not block the save. The
 *   server re-checks uniqueness on write regardless.
 * - `first()` completes the observable; an async validator that never completes leaves the control
 *   stuck in `pending` forever.
 */
export function employeeUserNameUniqueValidator(
  employeeService: EmployeeService,
  /** Getter rather than value: the excluded id is only known after ngOnInit resolves the route. */
  excludeEmployeeId?: () => string | undefined
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    const name = (control.value ?? '').trim();

    if (!name) {
      return of(null);
    }

    return timer(400).pipe(
      switchMap(() => employeeService.checkUserNameAvailability(name, excludeEmployeeId?.())),
      map(result => {
        if (result.userName !== name) {
          return null;
        }
        return result.isAvailable ? null : { userNameTaken: true };
      }),
      catchError((error: HttpErrorResponse) => {
        console.warn(
          `Employee username availability check failed (${error.status}); treating the name as unverified.`
        );
        return of(null);
      }),
      first()
    );
  };
}
