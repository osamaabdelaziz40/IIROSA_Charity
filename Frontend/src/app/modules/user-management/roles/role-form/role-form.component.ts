import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RoleManagementService } from '../../../../core/services/role-management.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Role, CreateRoleRequest, UpdateRoleRequest, RoleClaim } from '../../../../core/models/role.model';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { LoadingComponent } from '../../../../shared/components/loading/loading.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-role-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PageHeaderComponent, LoadingComponent, TranslateModule],
  templateUrl: './role-form.component.html',
  styleUrls: ['./role-form.component.scss']
})
export class RoleFormComponent implements OnInit {
  roleForm: FormGroup;
  loading = false;
  submitting = false;
  isEdit = false;
  roleId?: string;
  submitted = false;

  // Available permissions for the application
  availablePermissions = [
    // User Management
    { name: 'users.view', label: 'View Users', category: 'User Management' },
    { name: 'users.create', label: 'Create Users', category: 'User Management' },
    { name: 'users.edit', label: 'Edit Users', category: 'User Management' },
    { name: 'users.delete', label: 'Delete Users', category: 'User Management' },
    { name: 'users.activate', label: 'Activate/Deactivate Users', category: 'User Management' },
    { name: 'users.resetPassword', label: 'Reset User Passwords', category: 'User Management' },

    // Role Management
    { name: 'roles.view', label: 'View Roles', category: 'Role Management' },
    { name: 'roles.create', label: 'Create Roles', category: 'Role Management' },
    { name: 'roles.edit', label: 'Edit Roles', category: 'Role Management' },
    { name: 'roles.delete', label: 'Delete Roles', category: 'Role Management' },
    { name: 'roles.assign', label: 'Assign Roles', category: 'Role Management' },

    // Permissions & Claims
    { name: 'permissions.manage', label: 'Manage Permissions', category: 'Role Management' },
    { name: 'claims.manage', label: 'Manage Claims', category: 'Role Management' },

    // Audit & Activity
    { name: 'audit.view', label: 'View Audit Logs', category: 'Audit' },
    { name: 'activity.view', label: 'View User Activity', category: 'Audit' },

    // Orphans Management
    { name: 'orphans.view', label: 'View Orphans', category: 'Orphans Management' },
    { name: 'orphans.create', label: 'Create Orphans', category: 'Orphans Management' },
    { name: 'orphans.edit', label: 'Edit Orphans', category: 'Orphans Management' },
    { name: 'orphans.delete', label: 'Delete Orphans', category: 'Orphans Management' },

    // Families Management
    { name: 'families.view', label: 'View Families', category: 'Families Management' },
    { name: 'families.create', label: 'Create Families', category: 'Families Management' },
    { name: 'families.edit', label: 'Edit Families', category: 'Families Management' },
    { name: 'families.delete', label: 'Delete Families', category: 'Families Management' },

    // Financial Management
    { name: 'financial.view', label: 'View Financial Data', category: 'Financial Management' },
    { name: 'financial.create', label: 'Create Financial Records', category: 'Financial Management' },
    { name: 'financial.edit', label: 'Edit Financial Records', category: 'Financial Management' },
    { name: 'financial.delete', label: 'Delete Financial Records', category: 'Financial Management' },
    { name: 'financial.approve', label: 'Approve Financial Transactions', category: 'Financial Management' },
    { name: 'financial.reports', label: 'Generate Financial Reports', category: 'Financial Management' },

    // System Settings
    { name: 'settings.view', label: 'View Settings', category: 'System Settings' },
    { name: 'settings.edit', label: 'Edit Settings', category: 'System Settings' }
  ];

  // Group permissions by category
  permissionCategories: { [key: string]: typeof RoleFormComponent.prototype.availablePermissions } = {};

  pageActions = [
    {
      label: 'common.back',
      type: 'secondary',
      icon: 'fe-arrow-left',
      click: () => this.cancel()
    }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private roleManagementService: RoleManagementService,
    private notification: NotificationService
  ) {
    // Group permissions by category
    this.availablePermissions.forEach(perm => {
      if (!this.permissionCategories[perm.category]) {
        this.permissionCategories[perm.category] = [];
      }
      this.permissionCategories[perm.category].push(perm);
    });

    this.roleForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      permissions: [[]],
      claims: this.fb.array([])
    });
  }

  ngOnInit() {
    // Page actions are now directly bound

    this.roleId = this.route.snapshot.params['id'];

    if (this.roleId) {
      this.isEdit = true;
      this.loadRole();
    }
  }

  // Getter for easy access to form fields
  get f() {
    return this.roleForm.controls;
  }

  loadRole() {
    this.loading = true;
    this.roleManagementService.getRole(this.roleId!).subscribe({
      next: (role: any) => {
        this.roleForm.patchValue({
          name: role.name,
          description: role.description,
          permissions: role.permissions || []
        });
        this.loading = false;
      },
      error: () => {
        this.notification.error('Failed to load role');
        this.loading = false;
      }
    });
  }

  onSubmit() {
    this.submitted = true;
    if (this.roleForm.invalid) return;

    this.submitting = true;

    if (this.isEdit) {
      const updateRequest: UpdateRoleRequest = {
        name: this.f['name'].value,
        description: this.f['description'].value,
        permissions: this.f['permissions'].value
      };

      this.roleManagementService.updateRole(this.roleId!, updateRequest).subscribe({
        next: () => {
          this.notification.success('Role updated successfully');
          this.router.navigate(['/user-management/roles']);
        },
        error: (error: any) => {
          this.notification.error(error.error?.message || 'Operation failed');
          this.submitting = false;
        }
      });
    } else {
      const createRequest: CreateRoleRequest = {
        name: this.f['name'].value,
        description: this.f['description'].value,
        permissions: this.f['permissions'].value,
        claims: []
      };

      this.roleManagementService.createRole(createRequest).subscribe({
        next: () => {
          this.notification.success('Role created successfully');
          this.router.navigate(['/user-management/roles']);
        },
        error: (error: any) => {
          this.notification.error(error.error?.message || 'Operation failed');
          this.submitting = false;
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['/user-management/roles']);
  }

  ObjectKeys(obj: any): string[] {
    return Object.keys(obj);
  }

  togglePermission(permissionName: string) {
    const currentPermissions = this.f['permissions'].value as string[];
    const index = currentPermissions.indexOf(permissionName);

    if (index === -1) {
      currentPermissions.push(permissionName);
    } else {
      currentPermissions.splice(index, 1);
    }

    this.f['permissions'].setValue(currentPermissions);
  }

  hasPermission(permissionName: string): boolean {
    const currentPermissions = this.f['permissions'].value as string[];
    return currentPermissions.includes(permissionName);
  }

  getPermissionLabel(permissionName: string): string {
    const found = this.availablePermissions.find(p => p.name === permissionName);
    return found ? found.label : permissionName;
  }

  selectAllInCategory(category: string) {
    const categoryPermissions = this.permissionCategories[category];
    const currentPermissions = this.f['permissions'].value as string[];

    categoryPermissions.forEach(perm => {
      if (!currentPermissions.includes(perm.name)) {
        currentPermissions.push(perm.name);
      }
    });

    this.f['permissions'].setValue(currentPermissions);
  }

  deselectAllInCategory(category: string) {
    const categoryPermissions = this.permissionCategories[category];
    let currentPermissions = this.f['permissions'].value as string[];

    categoryPermissions.forEach(perm => {
      const index = currentPermissions.indexOf(perm.name);
      if (index !== -1) {
        currentPermissions.splice(index, 1);
      }
    });

    this.f['permissions'].setValue(currentPermissions);
  }

  isAllSelectedInCategory(category: string): boolean {
    const categoryPermissions = this.permissionCategories[category];
    const currentPermissions = this.f['permissions'].value as string[];

    return categoryPermissions.every(perm => currentPermissions.includes(perm.name));
  }

  isSomeSelectedInCategory(category: string): boolean {
    const categoryPermissions = this.permissionCategories[category];
    const currentPermissions = this.f['permissions'].value as string[];

    return categoryPermissions.some(perm => currentPermissions.includes(perm.name)) &&
           !this.isAllSelectedInCategory(category);
  }
}