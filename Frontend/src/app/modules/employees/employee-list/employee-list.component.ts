import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Employee, EmployeeSearchRequest } from '../../../core/models/employee.model';
import { EmployeeService } from '../services/employee.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { TranslateModule } from '@ngx-translate/core';
import { AppDatePipe } from '../../../shared/pipes/date.pipe';
import { RouterModule } from '@angular/router';
import { PaginationComponent } from '../../../shared/components';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeaderComponent, BreadcrumbComponent, TranslateModule, AppDatePipe, RouterModule, PaginationComponent],
  templateUrl: './employee-list.component.html',
  styleUrls: ['./employee-list.component.scss']
})
export class EmployeeListComponent implements OnInit {
  employees: Employee[] = [];
  allDepartments: string[] = [];
  allRoles: string[] = [];
  loading = false;
  loadingDepartments = false;
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;

  // Filter properties
  searchValue = '';
  selectedDepartment: string | null = null;
  selectedRole: string | null = null;
  isActiveFilter: boolean | null = null;

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
    private employeeService: EmployeeService,
    private notification: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Page actions are now directly bound
    this.loadDepartments();
    this.loadRoles();
    this.loadEmployees();
  }

  loadDepartments(): void {
    this.loadingDepartments = true;
    this.employeeService.getDepartments().subscribe({
      next: (departments: string[]) => {
        this.allDepartments = departments;
        this.loadingDepartments = false;
      },
      error: () => {
        // Fallback to static departments if backend fails
        this.allDepartments = [
          'Management',
          'Human Resources',
          'Finance',
          'IT',
          'Operations',
          'Marketing',
          'Sales',
          'Customer Service'
        ];
        this.loadingDepartments = false;
      }
    });
  }

  loadRoles(): void {
    // Load roles dynamically from the user management service
    // For now, use static roles
    this.allRoles = ['SuperAdmin', 'Admin', 'Charity', 'Accountant', 'FinancialOfficer'];
  }

  loadEmployees(): void {
    this.loading = true;
    const searchRequest: EmployeeSearchRequest = {
      search: this.searchValue || undefined,
      department: this.selectedDepartment || undefined,
      role: this.selectedRole || undefined,
      isActive: this.isActiveFilter !== null ? this.isActiveFilter : undefined,
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
        this.notification.error(`Failed to load employees: ${error.message || 'Unknown error'}`);
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadEmployees();
  }

  clearFilters(): void {
    this.searchValue = '';
    this.selectedDepartment = null;
    this.selectedRole = null;
    this.isActiveFilter = null;
    this.currentPage = 1;
    this.loadEmployees();
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
    const action = employee.isActive ? 'deactivate' : 'activate';
    const confirmed = confirm(`Are you sure you want to ${action} this employee?`);

    if (confirmed) {
      const action$ = employee.isActive
        ? this.employeeService.deactivateEmployee(employee.id)
        : this.employeeService.activateEmployee(employee.id);

      action$.subscribe({
        next: () => {
          this.notification.success(`Employee ${action}d successfully`);
          this.loadEmployees();
        },
        error: (error: any) => {
          console.error(`Error ${action}ing employee:`, error);
          this.notification.error(`Failed to ${action} employee`);
        }
      });
    }
  }

  resetPassword(employee: Employee): void {
    const confirmed = confirm('Are you sure you want to reset this employee\'s password?');

    if (confirmed) {
      this.employeeService.resetPassword(employee.id).subscribe({
        next: (response: any) => {
          this.notification.success('Password reset successfully. New password sent to employee email.');
        },
        error: (error: any) => {
          console.error('Error resetting password:', error);
          this.notification.error('Failed to reset password');
        }
      });
    }
  }

  assignRole(employee: Employee): void {
    this.router.navigate(['/employees', employee.id, 'roles']);
  }

  deleteEmployee(employee: Employee): void {
    const confirmed = confirm(`Are you sure you want to delete employee "${employee.fullName}"?`);

    if (confirmed) {
      this.employeeService.deleteEmployee(employee.id).subscribe({
        next: () => {
          this.notification.success('Employee deleted successfully');
          this.loadEmployees();
        },
        error: (error: any) => {
          console.error('Error deleting employee:', error);
          this.notification.error('Failed to delete employee');
        }
      });
    }
  }

  exportEmployees(): void {
    this.loading = true;

    const exportRequest: EmployeeSearchRequest = {
      search: this.searchValue || undefined,
      department: this.selectedDepartment || undefined,
      role: this.selectedRole || undefined,
      isActive: this.isActiveFilter !== null ? this.isActiveFilter : undefined,
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
}
