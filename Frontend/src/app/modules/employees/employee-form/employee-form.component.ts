import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../services/employee.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Employee, CreateEmployeeRequest, UpdateEmployeeRequest, departments } from '../../../core/models/employee.model';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent, BreadcrumbComponent, LoadingComponent],
  templateUrl: './employee-form.component.html',
  styleUrls: ['./employee-form.component.scss']
})
export class EmployeeFormComponent implements OnInit {
  employeeForm: FormGroup;
  loading = false;
  isEditMode = false;
  employeeId?: string;
  allDepartments = departments;
  allRoles: string[] = [];

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
    private notification: NotificationService
  ) {
    this.employeeForm = this.createForm();
  }

  ngOnInit(): void {
    // Page actions are now directly bound

    this.loadRoles();
    this.loadDepartments();

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

  createForm(): FormGroup {
    return this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      employeeCode: [''],
      position: ['', Validators.required],
      department: [''],
      phoneNumber: [''],
      dateOfBirth: [''],
      gender: [''],
      address: [''],
      hireDate: [new Date().toISOString().split('T')[0], Validators.required],
      salary: ['', [Validators.min(0)]],
      roleName: ['', Validators.required]
    });
  }

  loadDepartments(): void {
    this.employeeService.getDepartments().subscribe({
      next: (departments: string[]) => {
        this.allDepartments = departments;
      },
      error: () => {
        // Fallback to static departments
        this.allDepartments = departments;
      }
    });
  }

  loadRoles(): void {
    // TODO: Load roles dynamically from user management service
    this.allRoles = ['SuperAdmin', 'Admin', 'Charity', 'Accountant', 'FinancialOfficer'];
  }

  loadEmployeeData(): void {
    this.loading = true;
    this.employeeService.getEmployeeById(this.employeeId!).subscribe({
      next: (employee: Employee) => {
        // Extract first and last name from full name
        const nameParts = employee.fullName.split(' ');
        const firstName = nameParts[0] || '';
        const lastName = nameParts.slice(1).join(' ') || '';

        this.employeeForm.patchValue({
          email: employee.email,
          firstName: firstName,
          lastName: lastName,
          employeeCode: employee.employeeCode || '',
          position: employee.position,
          department: employee.department || '',
          phoneNumber: employee.phoneNumber || '',
          dateOfBirth: employee.dateOfBirth || '',
          gender: employee.gender || '',
          address: employee.address || '',
          hireDate: employee.hireDate,
          salary: employee.salary || '',
          roleName: employee.roles && employee.roles.length > 0 ? employee.roles[0] : ''
        });
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading employee:', error);
        this.notification.error('Failed to load employee data');
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.employeeForm.invalid) {
      this.notification.error('Please fill in all required fields');
      return;
    }

    const formValue = this.employeeForm.value;
    this.loading = true;

    if (this.isEditMode) {
      const request: UpdateEmployeeRequest = {
        email: formValue.email,
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        employeeCode: formValue.employeeCode,
        position: formValue.position,
        department: formValue.department || undefined,
        phoneNumber: formValue.phoneNumber || undefined,
        dateOfBirth: formValue.dateOfBirth || undefined,
        gender: formValue.gender || undefined,
        address: formValue.address || undefined,
        salary: formValue.salary || undefined,
        roleName: formValue.roleName
      };

      this.employeeService.updateEmployee(this.employeeId!, request).subscribe({
        next: () => {
          this.notification.success('employees.employeeUpdated');
          this.router.navigate(['/employees']);
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error updating employee:', error);
          this.notification.error(error?.error?.message || 'Failed to update employee');
          this.loading = false;
        }
      });
    } else {
      const request: CreateEmployeeRequest = {
        email: formValue.email,
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        employeeCode: formValue.employeeCode,
        position: formValue.position,
        department: formValue.department || undefined,
        phoneNumber: formValue.phoneNumber || undefined,
        dateOfBirth: formValue.dateOfBirth || undefined,
        gender: formValue.gender || undefined,
        address: formValue.address || undefined,
        hireDate: formValue.hireDate,
        salary: formValue.salary || undefined,
        roleName: formValue.roleName
      };

      this.employeeService.createEmployee(request).subscribe({
        next: () => {
          this.notification.success('employees.employeeCreated');
          this.router.navigate(['/employees']);
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error creating employee:', error);
          this.notification.error(error?.error?.message || 'Failed to create employee');
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

  getErrorMessage(field: string): string {
    const formControl = this.employeeForm.get(field);
    if (!formControl) return '';

    if (formControl.hasError('required')) {
      return 'This field is required';
    }
    if (formControl.hasError('email')) {
      return 'Please enter a valid email address';
    }
    if (formControl.hasError('min')) {
      return 'Value must be positive';
    }

    return '';
  }
}
