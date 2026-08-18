import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../services/employee.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Employee } from '../../../core/models/employee.model';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../shared/components';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { AppDatePipe } from '../../../shared/pipes/date.pipe';

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, PageHeaderComponent, BreadcrumbComponent, LoadingComponent, AppDatePipe],
  templateUrl: './employee-detail.component.html',
  styleUrls: ['./employee-detail.component.scss']
})
export class EmployeeDetailComponent implements OnInit {
  employee?: Employee;
  loading = false;
  employeeId?: string;

  pageActions = [
    {
      label: 'common.back',
      type: 'secondary',
      icon: 'fe-arrow-left',
      click: () => this.goBack()
    },
    {
      label: 'userManagement.manageRoles',
      type: 'primary',
      icon: 'fe-shield',
      click: () => this.manageRoles()
    },
    {
      label: 'employees.editEmployee',
      type: 'warning',
      icon: 'fe-edit',
      click: () => this.editEmployee()
    }
  ];

  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'employees.title', url: '/employees' },
    { label: 'employees.employeeDetails' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private employeeService: EmployeeService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    // Page actions are now directly bound

    this.employeeId = this.route.snapshot.params['id'];
    this.loadEmployee();
  }

  loadEmployee(): void {
    this.loading = true;
    this.employeeService.getEmployeeById(this.employeeId!).subscribe({
      next: (employee: Employee) => {
        this.employee = employee;
        this.loading = false;
      },
      error: (err: any) => {
        console.log('Full error:', err);
        console.log('Status:', err.status);
        console.log('Message:', err.message);
        console.log('Error body:', err.error);

        this.notification.error(err?.error?.message || 'Failed to load employee');
        this.loading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/employees']);
  }

  editEmployee(): void {
    if (this.employeeId) {
      this.router.navigate(['/employees', this.employeeId, 'edit']);
    }
  }

  manageRoles(): void {
    if (this.employeeId) {
      this.router.navigate(['/employees', this.employeeId, 'roles']);
    }
  }

  toggleEmployeeStatus(): void {
    if (!this.employee) return;

    const action = this.employee.isActive ? 'deactivate' : 'activate';
    if (confirm(`Are you sure you want to ${action} this employee?`)) {
      const action$ = this.employee.isActive
        ? this.employeeService.deactivateEmployee(this.employeeId!)
        : this.employeeService.activateEmployee(this.employeeId!);

      action$.subscribe({
        next: () => {
          this.notification.success(`Employee ${action}d successfully`);
          this.loadEmployee();
        },
        error: () => {
          this.notification.error(`Failed to ${action} employee`);
        }
      });
    }
  }

  resetPassword(): void {
    if (confirm('Are you sure you want to reset this employee\'s password?')) {
      this.employeeService.resetPassword(this.employeeId!, 'P@ssw0rd@2022').subscribe({
        next: (response: any) => {
          this.notification.success(`Password reset successfully. New password sent to employee email.`);
        },
        error: () => {
          this.notification.error('Failed to reset password');
        }
      });
    }
  }

  getStatusClass(): string {
    return this.employee?.isActive ? 'bg-success' : 'bg-danger';
  }

  getStatusText(): string {
    return this.employee?.isActive ? 'Active' : 'Inactive';
  }
}
