import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function CustomEmailValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    const valid = emailRegex.test(control.value);
    return valid ? null : { invalidEmail: true };
  };
}

export function PasswordStrengthValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value || '';
    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumber = /\d/.test(value);
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(value);
    const isValid = hasUpperCase && hasLowerCase && hasNumber && hasSpecialChar && value.length >= 8;

    return isValid ? null : { weakPassword: true };
  };
}
