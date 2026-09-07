import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { first, Subscription } from 'rxjs';
import { EmployeeService } from '../services/employee.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  Employee,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
  DepartmentLookup,
  RoleListItem
} from '../../../core/models/employee.model';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, DropDownComponent } from '../../../shared/components';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { employeeUserNameUniqueValidator, passwordStrengthValidator } from '../../../shared/validators';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent, BreadcrumbComponent, LoadingComponent, DropDownComponent],
  templateUrl: './employee-form.component.html',
  styleUrls: ['./employee-form.component.scss']
})
export class EmployeeFormComponent implements OnInit, OnDestroy {
  employeeForm: FormGroup;
  loading = false;
  isEditMode = false;
  employeeId?: string;
  allDepartments: DepartmentLookup[] = [];
  allRoles: RoleListItem[] = [];

  // Select2 option arrays ({id, name}) fed to app-drop-down.
  departmentOptions: Array<{ id: number; name: string }> = [];
  roleOptions: Array<{ id: string; name: string }> = [];
  genderOptions: Array<{ id: string; name: string }> = [];

  /** Rebuilds translated gender labels on language switch; torn down in ngOnDestroy. */
  private langChangeSubscription?: Subscription;

  /** True while waiting for the email availability check to settle; see onSubmit(). */
  awaitingValidation = false;
  /** Held so it can be torn down; see the PENDING branch of onSubmit(). */
  private pendingValidationSubscription?: Subscription;

  pageActions = [
    {
      label: 'common.back',
      type: 'secondary',
      icon: 'fe-arrow-left',
      click: () => this.goBack()
    },
    {
      label: 'common.save',
      type: 'primary',
      icon: 'fe-save',
      click: () => this.onSubmit()
    }
  ];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'employees.title', url: '/employees' },
    { label: 'employees.addEmployee' }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private employeeService: EmployeeService,
    private notification: NotificationService,
    private translate: TranslateService
  ) {
    this.employeeForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadRoles();
    this.loadDepartments();
    this.initializeGenderOptions();

    // Gender labels are pre-translated (app-drop-down renders raw text), so a
    // language switch needs a rebuild — the translate pipe can't refresh them.
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.initializeGenderOptions();
    });

    this.employeeId = this.route.snapshot.params['id'];
    if (this.employeeId) {
      this.isEditMode = true;
      this.breadcrumbs = [
        { label: 'common.home', url: '/dashboard' },
        { label: 'employees.title', url: '/employees' },
        { label: 'employees.editEmployee' }
      ];
      this.loadEmployeeData();
    }
  }

  ngOnDestroy(): void {
    this.pendingValidationSubscription?.unsubscribe();
    this.langChangeSubscription?.unsubscribe();
  }

  createForm(): FormGroup {
    return this.fb.group({
      // The identity UserName IS the email in this stack, so the login-name availability
      // check (UC-EMP-02) probes exactly this field. The exclude getter is read at check
      // time, when the route param is already resolved.
      email: ['', {
        validators: [Validators.required, Validators.email],
        asyncValidators: [employeeUserNameUniqueValidator(
          this.employeeService,
          () => this.employeeId
        )]
      }],
      password: ['', [Validators.required, passwordStrengthValidator(3)]],
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      code: [''],
      position: ['', Validators.required],
      departmentId: [null],
      phoneNumber: [''],
      dateOfBirth: [''],
      gender: [''],
      address: [''],
      hireDate: [new Date().toISOString().split('T')[0], Validators.required],
      salary: ['', [Validators.min(0)]],
      notes: [''],
      role: ['', Validators.required]
    });
  }

  /** Gender ids are the API values ('Male'/'Female'); names are translated labels. */
  initializeGenderOptions(): void {
    this.genderOptions = [
      { id: 'Male', name: this.translate.instant('employees.male') },
      { id: 'Female', name: this.translate.instant('employees.female') }
    ];
  }

  loadDepartments(): void {
    this.employeeService.getDepartments().subscribe({
      next: (departments: DepartmentLookup[]) => {
        this.allDepartments = departments;
        this.departmentOptions = departments.map(dept => ({ id: dept.id, name: this.departmentLabel(dept) }));
      },
      error: () => {
        // No static fallback: an unreachable lookup must be visible as an empty select,
        // not silently replaced with a hard-coded list that drifts from the database.
        this.allDepartments = [];
        this.departmentOptions = [];
        this.notification.warning(this.translate.instant('employees.departmentsLoadFailed'));
      }
    });
  }

  loadRoles(): void {
    this.employeeService.getRoles().subscribe({
      next: (roles: RoleListItem[]) => {
        this.allRoles = roles;
        // Role control holds the role NAME (what the API filters/echo), not its id.
        this.roleOptions = roles.map(role => ({ id: role.name, name: this.roleLabel(role) }));
      },
      error: () => {
        this.allRoles = [];
        this.roleOptions = [];
        this.notification.warning(this.translate.instant('employees.rolesLoadFailed'));
      }
    });
  }

  loadEmployeeData(): void {
    this.loading = true;
    this.employeeService.getEmployeeById(this.employeeId!).subscribe({
      next: (employee: Employee) => {
        this.employeeForm.patchValue({
          email: employee.email || '',
          fullName: employee.fullName || '',
          code: employee.code || '',
          position: employee.position || '',
          departmentId: employee.departmentId ?? null,
          phoneNumber: employee.phoneNumber || '',
          dateOfBirth: employee.dateOfBirth ? employee.dateOfBirth.split('T')[0] : '',
          gender: employee.gender || '',
          address: employee.address || '',
          hireDate: employee.hireDate ? employee.hireDate.split('T')[0] : '',
          salary: employee.salary ?? '',
          notes: employee.notes || '',
          role: employee.roles && employee.roles.length > 0 ? employee.roles[0] : ''
        });
        // The password belongs to account creation only; password changes go through
        // the reset-password action, not the edit form.
        this.employeeForm.get('password')?.clearValidators();
        this.employeeForm.get('password')?.setValue('');
        this.employeeForm.get('password')?.updateValueAndValidity();
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading employee:', error);
        this.notification.error(this.translate.instant('employees.employeeLoadFailed'));
        // Do not leave the create-only required password wedging a form we failed to populate
        this.employeeForm.get('password')?.clearValidators();
        this.employeeForm.get('password')?.setValue('');
        this.employeeForm.get('password')?.updateValueAndValidity();
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    // A control whose async validator is still running is PENDING, not INVALID, so the guard
    // below would let the save through and bypass the availability check entirely. Wait for
    // the pending validators to settle, then re-enter.
    if (this.employeeForm.pending) {
      if (this.awaitingValidation) {
        return;
      }

      this.awaitingValidation = true;
      this.loading = true;

      this.pendingValidationSubscription = this.employeeForm.statusChanges
        .pipe(first(status => status !== 'PENDING'))
        .subscribe(() => {
          this.awaitingValidation = false;
          this.loading = false;
          this.onSubmit();
        });
      return;
    }

    if (this.employeeForm.invalid) {
      this.employeeForm.markAllAsTouched();
      this.notification.error(this.translate.instant('validation.fixErrors'));
      return;
    }

    const formValue = this.employeeForm.value;
    this.loading = true;

    if (this.isEditMode) {
      // PUT semantics (review decision 2026-08-24): the screen owns the whole record, so the
      // payload carries explicit state — an emptied field must clear in the database. Roles
      // are only sent when the operator actually touched the role control: a single-select
      // cannot represent a multi-role employee, and sending it untouched would strip the
      // extra roles server-side.
      const roleDirty = this.employeeForm.get('role')?.dirty ?? false;
      const request: UpdateEmployeeRequest = {
        email: formValue.email,
        fullName: formValue.fullName,
        code: formValue.code || undefined,
        position: formValue.position,
        departmentId: formValue.departmentId ?? null,
        phoneNumber: formValue.phoneNumber ?? null,
        dateOfBirth: formValue.dateOfBirth || null,
        gender: formValue.gender || null,
        address: formValue.address ?? null,
        hireDate: formValue.hireDate,
        salary: formValue.salary !== '' ? formValue.salary : null,
        notes: formValue.notes ?? null,
        roles: roleDirty ? (formValue.role ? [formValue.role] : []) : undefined
      };

      this.employeeService.updateEmployee(this.employeeId!, request).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('employees.employeeUpdated'));
          this.router.navigate(['/employees']);
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error updating employee:', error);
          this.notification.error(error?.error?.message || this.translate.instant('employees.employeeUpdateFailed'));
          this.loading = false;
        }
      });
    } else {
      const request: CreateEmployeeRequest = {
        email: formValue.email,
        password: formValue.password,
        fullName: formValue.fullName,
        code: formValue.code || undefined,
        position: formValue.position,
        departmentId: formValue.departmentId ?? null,
        phoneNumber: formValue.phoneNumber || undefined,
        dateOfBirth: formValue.dateOfBirth || undefined,
        gender: formValue.gender || undefined,
        address: formValue.address || undefined,
        hireDate: formValue.hireDate,
        salary: formValue.salary !== '' ? formValue.salary : null,
        notes: formValue.notes || undefined,
        roles: formValue.role ? [formValue.role] : []
      };

      this.employeeService.createEmployee(request).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('employees.employeeCreated'));
          this.router.navigate(['/employees']);
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error creating employee:', error);
          this.notification.error(error?.error?.message || this.translate.instant('employees.employeeCreateFailed'));
          this.loading = false;
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/employees']);
  }

  isFieldInvalid(field: string): boolean {
    const formControl = this.employeeForm.get(field);
    return formControl ? formControl.invalid && (formControl.dirty || formControl.touched) : false;
  }

  /** Bilingual display name for a lookup row — Arabic first (RTL-first platform). */
  departmentLabel(dept: DepartmentLookup): string {
    return dept.nameAr || dept.nameEn || dept.name;
  }

  roleLabel(role: RoleListItem): string {
    return role.displayNameAr || role.displayNameEn || role.name;
  }
}
