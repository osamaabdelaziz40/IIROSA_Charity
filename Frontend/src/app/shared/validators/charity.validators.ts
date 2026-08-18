import { AbstractControl, ValidationErrors, ValidatorFn, AsyncValidatorFn } from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { CharityService } from '../../modules/charities/services/charity.service';

/**
 * IBAN Validator
 * Validates IBAN format according to international standards
 */
export function ibanValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    const iban = control.value.toString().toUpperCase().replace(/\s/g, '');

    // Basic IBAN format validation
    const ibanPattern = /^[A-Z]{2}[0-9]{2}[A-Z0-9]{11,30}$/;
    if (!ibanPattern.test(iban)) {
      return { invalidIban: true };
    }

    // IBAN checksum validation
    const rearranged = iban.substring(4) + iban.substring(0, 4);
    const numeric = rearranged.split('').map((c: string) => {
      const code = c.charCodeAt(0);
      return (code >= 48 && code <= 57) ? c : (code - 55).toString();
    }).join('');

    let remainder = numeric.substring(0, 15);
    for (let i = 15; i < numeric.length; i += 9) {
      remainder = (parseInt(remainder) % 97).toString() + numeric.substring(i, i + 9);
    }

    if (parseInt(remainder) % 97 !== 1) {
      return { invalidIbanChecksum: true };
    }

    return null;
  };
}

/**
 * Phone Number Validator
 * Validates international phone number formats
 */
export function phoneNumberValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    const phonePattern = /^[\+]?[(]?[0-9]{1,4}[)]?[-\s\.]?[(]?[0-9]{1,4}[)]?[-\s\.]?[0-9]{1,9}$/;

    if (!phonePattern.test(control.value)) {
      return { invalidPhoneNumber: true };
    }

    return null;
  };
}

/**
 * Charity Name Unique Validator (Async)
 * Checks if charity name already exists in the system
 */
export function charityNameUniqueValidator(
  charityService: CharityService,
  excludeId?: string
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value) {
      return of(null);
    }

    // Debounce to avoid excessive API calls
    return timer(500).pipe(
      switchMap(() => {
        return charityService.getCharities({
          searchTerm: control.value,
          pageNumber: 1,
          pageSize: 10
        }).pipe(
          map(response => {
            const existingCharity = response.items.find(charity =>
              charity.name.toLowerCase() === control.value.toLowerCase() &&
              charity.id !== excludeId
            );

            return existingCharity ? { charityNameExists: true } : null;
          })
        );
      })
    );
  };
}

/**
 * Charity Code Unique Validator (Async)
 * Checks if charity code already exists in the system
 */
export function charityCodeUniqueValidator(
  charityService: CharityService,
  excludeId?: string
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value) {
      return of(null);
    }

    return timer(500).pipe(
      switchMap(() => {
        return charityService.getCharities({
          pageNumber: 1,
          pageSize: 100
        }).pipe(
          map(response => {
            const existingCharity = response.items.find(charity =>
              charity.code?.toLowerCase() === control.value.toLowerCase() &&
              charity.id !== excludeId
            );

            return existingCharity ? { charityCodeExists: true } : null;
          })
        );
      })
    );
  };
}

/**
 * Email Unique Validator (Async)
 * Checks if email already exists in the system
 */
export function charityEmailUniqueValidator(
  charityService: CharityService,
  excludeId?: string
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || !control.value.includes('@')) {
      return of(null);
    }

    return timer(500).pipe(
      switchMap(() => {
        return charityService.getCharities({
          pageNumber: 1,
          pageSize: 100
        }).pipe(
          map(response => {
            const existingCharity = response.items.find(charity =>
              charity.email.toLowerCase() === control.value.toLowerCase() &&
              charity.id !== excludeId
            );

            return existingCharity ? { charityEmailExists: true } : null;
          })
        );
      })
    );
  };
}

/**
 * URL Validator
 * Validates URL format for icons and map locations
 */
export function urlValidator(allowRelative = false): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    let urlPattern: RegExp;

    if (allowRelative) {
      urlPattern = /^(https?:\/\/)|(\/)/i;
    } else {
      urlPattern = /^(https?:\/\/)/i;
    }

    if (!urlPattern.test(control.value)) {
      return { invalidUrl: true };
    }

    return null;
  };
}

/**
 * GPS Coordinates Validator
 * Validates latitude and longitude format
 */
export function gpsCoordinatesValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    // Accept multiple formats:
    // 1. Decimal degrees: "24.7136, 46.6753"
    // 2. DMS format: "24°42'49\"N, 46°40'31\"E"
    // 3. Google Maps URL

    const value = control.value.toString().trim();

    // Check for Google Maps URL
    if (value.includes('maps.google.com') || value.includes('goo.gl/maps')) {
      return null;
    }

    // Check for decimal degrees format
    const decimalPattern = /^[-+]?([1-8]?\d(\.\d+)?|90(\.0+)?),\s*[-+]?(180(\.0+)?|((1[0-7]\d)|([1-9]?\d))(\.\d+)?)$/;
    if (decimalPattern.test(value)) {
      return null;
    }

    // Check for DMS format (simplified)
    const dmsPattern = /^\d{1,3}°\d{1,2}'\d{1,2}"[NS],\s*\d{1,3}°\d{1,2}'\d{1,2}"[EW]$/;
    if (dmsPattern.test(value)) {
      return null;
    }

    return { invalidGpsFormat: true };
  };
}

/**
 * Password Strength Validator
 * Validates password strength requirements
 */
export function passwordStrengthValidator(minStrength = 3): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    const password = control.value.toString();
    let strength = 0;

    // Length check
    if (password.length >= 8) strength++;
    if (password.length >= 12) strength++;

    // Complexity checks
    if (/[a-z]/.test(password)) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^a-zA-Z0-9]/.test(password)) strength++;

    if (strength < minStrength) {
      return { weakPassword: true, requiredStrength: minStrength, currentStrength: strength };
    }

    return null;
  };
}

/**
 * Bank Account Validator
 * Validates bank account number format
 */
export function bankAccountValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    const accountNumber = control.value.toString().replace(/\s/g, '');

    // Basic validation: 8-20 digits
    if (!/^\d{8,20}$/.test(accountNumber)) {
      return { invalidBankAccount: true };
    }

    return null;
  };
}

/**
 * Custom error messages for charity validators
 */
export const CHARITY_VALIDATOR_ERROR_MESSAGES = {
  invalidIban: 'Invalid IBAN format. Please enter a valid IBAN.',
  invalidIbanChecksum: 'IBAN checksum validation failed.',
  invalidPhoneNumber: 'Invalid phone number format.',
  charityNameExists: 'A charity with this name already exists.',
  charityCodeExists: 'A charity with this code already exists.',
  charityEmailExists: 'A charity with this email already exists.',
  invalidUrl: 'Please enter a valid URL starting with http:// or https://',
  invalidGpsFormat: 'Invalid GPS coordinates format. Use decimal degrees (e.g., 24.7136, 46.6753) or a Google Maps link.',
  weakPassword: 'Password is too weak. Please use a stronger password.',
  invalidBankAccount: 'Invalid bank account number. Must be 8-20 digits.'
};
