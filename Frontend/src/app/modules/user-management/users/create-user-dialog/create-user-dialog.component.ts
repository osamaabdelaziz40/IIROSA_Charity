import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UserManagementService } from '../../../../modules/user-management/services/user-management.service';
import { CreateUserRequest } from '../../../../core/models/user.model';
import { Role } from '../../../../core/models/role.model';

@Component({
  selector: 'app-create-user-dialog',
  templateUrl: './create-user-dialog.component.html',
  styleUrls: ['./create-user-dialog.component.scss']
})
export class CreateUserDialogComponent implements OnInit {
  userForm: FormGroup;
  loading = false;
  hidePassword = true;
  loadingRoles = false;
  availableRoles: Role[] = [];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CreateUserDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private userManagementService: UserManagementService,
    private snackBar: MatSnackBar
  ) {
    this.userForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadAvailableRoles();
  }

  /**
   * Load available roles from backend
   */
  private loadAvailableRoles(): void {
    this.loadingRoles = true;
    this.userManagementService.getAllRoles().subscribe({
      next: (roles) => {
        this.availableRoles = roles;
        this.loadingRoles = false;
      },
      error: (error) => {
        console.error('Error loading roles:', error);
        this.loadingRoles = false;
        // Fallback to static roles if backend fails
        this.availableRoles = [
          { id: '1', name: 'SuperAdmin', description: 'Super Admin', isSystemRole: true },
          { id: '2', name: 'Admin', description: 'Admin', isSystemRole: true },
          { id: '3', name: 'Charity', description: 'Charity', isSystemRole: false },
          { id: '4', name: 'Accountant', description: 'Accountant', isSystemRole: false },
          { id: '5', name: 'FinancialOfficer', description: 'Financial Officer', isSystemRole: false }
        ];
      }
    });
  }

  /**
   * Create reactive form
   * UC-1.2: Create User
   */
  private createForm(): FormGroup {
    return this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      phoneNumber: [''],
      password: [''],
      roles: [[], [Validators.required]]
    });
  }

  /**
   * Submit form to create user
   */
  onSubmit(): void {
    if (this.userForm.invalid) {
      this.markFormGroupTouched(this.userForm);
      return;
    }

    this.loading = true;

    const request: CreateUserRequest = {
      email: this.userForm.value.email,
      fullName: this.userForm.value.fullName,
      phoneNumber: this.userForm.value.phoneNumber || undefined,
      password: this.userForm.value.password || undefined,
      roles: this.userForm.value.roles
    };

    this.userManagementService.createUser(request).subscribe({
      next: (response) => {
        this.loading = false;
        const user = response.value;
        this.snackBar.open(`User ${user?.email} created successfully`, 'Close', { duration: 3000 });
        this.dialogRef.close(user);
      },
      error: (error) => {
        this.loading = false;
        console.error('Error creating user:', error);
        const errorMessage = error.error?.message || 'Failed to create user';
        this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
      }
    });
  }

  /**
   * Cancel dialog
   */
  onCancel(): void {
    this.dialogRef.close();
  }

  /**
   * Toggle password visibility
   */
  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }

  /**
   * Mark all form fields as touched
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  /**
   * Get error message for form control
   */
  getErrorMessage(controlName: string): string {
    const control = this.userForm.get(controlName);
    if (!control || !control.errors || !control.touched) {
      return '';
    }

    const errors = control.errors;
    if (errors['required']) {
      return 'This field is required';
    }
    if (errors['email']) {
      return 'Please enter a valid email address';
    }
    if (errors['minlength']) {
      return `Minimum length is ${errors['minlength'].requiredLength} characters`;
    }

    return 'Invalid input';
  }
}