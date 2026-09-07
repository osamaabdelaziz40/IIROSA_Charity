import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Employee, EmployeeSearchRequest, DepartmentLookup, RoleListItem } from '../../../core/models/employee.model';
import { EmployeeService } from '../services/employee.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem, DropDownComponent } from '../../../shared/components';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AppDatePipe } from '../../../shared/pipes/date.pipe';
import { RouterModule } from '@angular/router';
import { PaginationComponent } from '../../../shared/components';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PageHeaderComponent, BreadcrumbComponent, TranslateModule, AppDatePipe, RouterModule, PaginationComponent, DropDownComponent],
  templateUrl: './employee-list.component.html',
  styleUrls: ['./employee-list.component.scss']
})
export class EmployeeListComponent implements OnInit, OnDestroy {
  employees: Employee[] = [];
  allDepartments: DepartmentLookup[] = [];
  allRoles: RoleListItem[] = [];
  loading = false;
  loadingDepartments = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // Filter form — the shared select2 drop-downs bind to this (charities-list pattern).
  filterForm: FormGroup;

  // Select2 option arrays ({id, name}) fed to app-drop-down.
  departmentOptions: Array<{ id: number; name: string }> = [];
  roleOptions: Array<{ id: string; name: string }> = [];
  statusOptions: Array<{ id: string; name: string }> = [];

  /** Rebuilds translated option labels on language switch; torn down in ngOnDestroy. */
  private langChangeSubscription?: Subscription;

  /** Sentinel ids meaning "no filter" — departmentId 0 is falsy, role/status use 'all'. */
  private static readonly ALL_DEPARTMENT = 0;
  private static readonly ALL = 'all';

  pageActions = [
    {
      label: 'employees.addEmployee',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createEmployee()
    }
  ];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'employees.title' }
  ];

  constructor(
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private notification: NotificationService,
    private translate: TranslateService,
    private router: Router
  ) {
    this.filterForm = this.fb.group({
      searchValue: [''],
      departmentId: [EmployeeListComponent.ALL_DEPARTMENT],
      role: [EmployeeListComponent.ALL],
      status: [EmployeeListComponent.ALL]
    });
  }

  ngOnInit(): void {
    // Page actions are now directly bound
    this.loadDepartments();
    this.loadRoles();
    this.initializeStatusOptions();
    this.loadEmployees();

    // The option labels are pre-translated (app-drop-down renders raw text), so a
    // language switch needs a rebuild — the translate pipe can't refresh them.
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.initializeStatusOptions();
    });
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  /** Status options carry string ids — toOptionId in app-drop-down keeps them verbatim. */
  initializeStatusOptions(): void {
    this.statusOptions = [
      { id: 'all', name: this.translate.instant('employees.allStatus') },
      { id: 'active', name: this.translate.instant('common.active') },
      { id: 'inactive', name: this.translate.instant('common.inactive') }
    ];
  }

  loadDepartments(): void {
    this.loadingDepartments = true;
    this.employeeService.getDepartments().subscribe({
      next: (departments: DepartmentLookup[]) => {
        this.allDepartments = departments;
        this.departmentOptions = [
          { id: EmployeeListComponent.ALL_DEPARTMENT, name: this.translate.instant('employees.allDepartments') },
          ...departments.map(dept => ({ id: dept.id, name: this.departmentLabel(dept) }))
        ];
        this.loadingDepartments = false;
      },
      error: () => {
        // No static fallback: an unreachable lookup must be visible as an empty select,
        // not silently replaced with a hard-coded list that drifts from the database.
        this.allDepartments = [];
        this.departmentOptions = [
          { id: EmployeeListComponent.ALL_DEPARTMENT, name: this.translate.instant('employees.allDepartments') }
        ];
        this.loadingDepartments = false;
      }
    });
  }

  loadRoles(): void {
    this.employeeService.getRoles().subscribe({
      next: (roles: RoleListItem[]) => {
        this.allRoles = roles;
        this.roleOptions = [
          { id: EmployeeListComponent.ALL, name: this.translate.instant('employees.allRoles') },
          ...roles.map(role => ({ id: role.name, name: this.roleLabel(role) }))
        ];
      },
      error: () => {
        this.allRoles = [];
        this.roleOptions = [{ id: EmployeeListComponent.ALL, name: this.translate.instant('employees.allRoles') }];
      }
    });
  }

  loadEmployees(): void {
    this.loading = true;
    const filters = this.filterForm.value;
    const searchRequest: EmployeeSearchRequest = {
      search: filters.searchValue || undefined,
      departmentId: filters.departmentId || undefined,
      role: filters.role && filters.role !== EmployeeListComponent.ALL ? filters.role : undefined,
      isActive: filters.status === 'active' ? true : filters.status === 'inactive' ? false : undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.employeeService.getEmployees(searchRequest).subscribe({
      next: (response: any) => {
        this.employees = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading employees:', error);
        this.notification.error(this.translate.instant('employees.employeeLoadFailed'));
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadEmployees();
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchValue: '',
      departmentId: EmployeeListComponent.ALL_DEPARTMENT,
      role: EmployeeListComponent.ALL,
      status: EmployeeListComponent.ALL
    });
    this.currentPage = 1;
    this.loadEmployees();
  }

  /** Mirrors the charities list: drives the conditional Clear Filters button. */
  hasActiveFilters(): boolean {
    const filters = this.filterForm.value;
    return !!(
      filters.searchValue ||
      (filters.departmentId !== undefined && filters.departmentId !== EmployeeListComponent.ALL_DEPARTMENT) ||
      (filters.role && filters.role !== EmployeeListComponent.ALL) ||
      (filters.status && filters.status !== EmployeeListComponent.ALL)
    );
  }

  createEmployee(): void {
    this.router.navigate(['/employees/create']);
  }

  viewEmployee(id: string): void {
    this.router.navigate(['/employees', id]);
  }

  editEmployee(id: string): void {
    this.router.navigate(['/employees', id, 'edit']);
  }

  toggleEmployeeStatus(employee: Employee): void {
    const deactivating = employee.isActive;
    const confirmed = confirm(this.translate.instant(
      deactivating ? 'employees.confirmEmployeeDeactivation' : 'employees.confirmEmployeeActivation'
    ));

    if (confirmed) {
      const action$ = deactivating
        ? this.employeeService.deactivateEmployee(employee.id)
        : this.employeeService.activateEmployee(employee.id);

      action$.subscribe({
        next: () => {
          this.notification.success(this.translate.instant(
            deactivating ? 'employees.employeeDeactivated' : 'employees.employeeActivated'
          ));
          this.loadEmployees();
        },
        error: (error: any) => {
          console.error('Error toggling employee status:', error);
          this.notification.error(this.translate.instant('employees.employeeUpdateFailed'));
        }
      });
    }
  }

  resetPassword(employee: Employee): void {
    const confirmed = confirm(this.translate.instant('employees.confirmPasswordReset'));

    if (confirmed) {
      this.employeeService.resetPassword(employee.id).subscribe({
        next: (response: any) => {
          this.notification.success(this.translate.instant('employees.passwordReset'));
        },
        error: (error: any) => {
          console.error('Error resetting password:', error);
          this.notification.error(this.translate.instant('employees.employeeUpdateFailed'));
        }
      });
    }
  }

  assignRole(employee: Employee): void {
    this.router.navigate(['/employees', employee.id, 'roles']);
  }

  deleteEmployee(employee: Employee): void {
    const confirmed = confirm(this.translate.instant('employees.confirmEmployeeDeletion'));

    if (confirmed) {
      this.employeeService.deleteEmployee(employee.id).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('employees.employeeDeleted'));
          this.loadEmployees();
        },
        error: (error: any) => {
          console.error('Error deleting employee:', error);
          this.notification.error(this.translate.instant('employees.employeeUpdateFailed'));
        }
      });
    }
  }

  exportEmployees(): void {
    this.loading = true;

    const filters = this.filterForm.value;
    const exportRequest: EmployeeSearchRequest = {
      search: filters.searchValue || undefined,
      departmentId: filters.departmentId || undefined,
      role: filters.role && filters.role !== EmployeeListComponent.ALL ? filters.role : undefined,
      isActive: filters.status === 'active' ? true : filters.status === 'inactive' ? false : undefined,
      page: 1,
      pageSize: 10000
    };

    this.employeeService.exportEmployees(exportRequest).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `employees_${new Date().toISOString().split('T')[0]}.csv`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);

        this.notification.success('Employees exported successfully');
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Export failed:', error);
        this.notification.error('Failed to export employees');
        this.loading = false;
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadEmployees();
  }

  getRolesDisplay(roles: string[]): string {
    if (!roles || roles.length === 0) {
      return 'No roles';
    }
    return roles.join(', ');
  }

  /** Bilingual display name for a lookup row — Arabic first (RTL-first platform). */
  departmentLabel(dept: DepartmentLookup): string {
    return dept.nameAr || dept.nameEn || dept.name;
  }

  roleLabel(role: RoleListItem): string {
    return role.displayNameAr || role.displayNameEn || role.name;
  }

  trackEmployeeById(_index: number, employee: Employee): string {
    return employee.id;
  }
}
