import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../services/employee.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Role } from '../../../core/models/role.model';
import { TranslateModule } from '@ngx-translate/core';
import { LoadingComponent } from '../../../shared/components/loading/loading.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { DropDownComponent } from '../../../shared/components';

@Component({
  selector: 'app-role-assignment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, LoadingComponent, PageHeaderComponent, DropDownComponent],
  templateUrl: './role-assignment.component.html',
  styleUrls: ['./role-assignment.component.scss']
})
export class RoleAssignmentComponent implements OnInit {
  roleForm: FormGroup;
  loading = false;
  submitting = false;
  allRoles: Role[] = [];
  selectedRole: string = '';
  employeeName: string = '';
  employeeId: string = '';

  // Select2 option array ({id, name}) — id is the role NAME the API expects.
  roleOptions: Array<{ id: string; name: string }> = [];

  pageActions = [
    {
      label: 'common.back',
      type: 'secondary',
      icon: 'fe-arrow-left',
      click: () => this.cancel()
    },
    {
      label: 'common.save',
      type: 'primary',
      icon: 'fe-save',
      click: () => this.onSubmit()
    }
  ];

  constructor(
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private notification: NotificationService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.roleForm = this.fb.group({
      roleName: ['']
    });
  }

  ngOnInit(): void {
    // Page actions are now directly bound

    this.employeeId = this.route.snapshot.params['id'];
    this.loadRoles();
    this.loadEmployeeData();
  }

  loadRoles(): void {
    // TODO: Load roles from role management service
    // For now, use static roles
    this.allRoles = [
      { id: '1', name: 'SuperAdmin', description: 'Super Admin', isSystemRole: true, userCount: 0, permissions: [] },
      { id: '2', name: 'Admin', description: 'Admin', isSystemRole: true, userCount: 0, permissions: [] },
      { id: '3', name: 'Charity', description: 'Charity', isSystemRole: true, userCount: 0, permissions: [] },
      { id: '4', name: 'Accountant', description: 'Accountant', isSystemRole: true, userCount: 0, permissions: [] },
      { id: '5', name: 'FinancialOfficer', description: 'Financial Officer', isSystemRole: true, userCount: 0, permissions: [] }
    ];
    this.roleOptions = this.allRoles.map(role => ({ id: role.name, name: role.name }));
  }

  loadEmployeeData(): void {
    if (this.employeeId) {
      this.employeeService.getEmployeeById(this.employeeId).subscribe({
        next: (employee: any) => {
          this.employeeName = employee.fullName;
          if (employee.roles && employee.roles.length > 0) {
            this.selectedRole = employee.roles[0];
            this.roleForm.patchValue({
              roleName: this.selectedRole
            });
          }
        },
        error: (error: any) => {
          console.error('Failed to load employee:', error);
          this.notification.error('Failed to load employee information');
        }
      });
    }
  }

  onSubmit(): void {
    if (!this.roleForm.value.roleName) {
      this.notification.error('userManagement.atLeastOneRole');
      return;
    }

    this.submitting = true;
    this.employeeService.assignEmployeeRole(this.employeeId, this.roleForm.value.roleName).subscribe({
      next: () => {
        this.notification.success('employees.roleAssigned');
        this.router.navigate(['/employees']);
        this.submitting = false;
      },
      error: (error: any) => {
        console.error('Error assigning role:', error);
        this.notification.error(error?.error?.message || 'Failed to assign role');
        this.submitting = false;
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/employees']);
  }
}
