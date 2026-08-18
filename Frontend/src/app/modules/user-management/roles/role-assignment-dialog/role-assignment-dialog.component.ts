import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserManagementService } from '../../services/user-management.service';
import { RoleManagementService } from '../../../../core/services/role-management.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Role } from '../../../../core/models/role.model';
import { TranslateModule } from '@ngx-translate/core';
import { LoadingComponent } from '../../../../shared/components/loading/loading.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-role-assignment-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, LoadingComponent, PageHeaderComponent],
  templateUrl: './role-assignment-dialog.component.html',
  styleUrls: ['./role-assignment-dialog.component.scss']
})
export class RoleAssignmentDialogComponent implements OnInit {
  roleForm: FormGroup;
  loading = false;
  submitting = false;
  allRoles: Role[] = [];
  selectedRoles: string[] = [];
  userName: string = '';
  userId: string = '';

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
    private userManagementService: UserManagementService,
    private roleManagementService: RoleManagementService,
    private notification: NotificationService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.roleForm = this.fb.group({
      roles: [[]]
    });
  }

  ngOnInit() {
    // Page actions are now directly bound

    // Get user ID from route
    this.userId = this.route.snapshot.params['id'];
    this.loadRoles();
  }

  loadRoles() {
    this.loading = true;
    this.roleManagementService.getRoles().subscribe({
      next: (roles: any[]) => {
        // Transform the response to match our Role interface
        this.allRoles = roles.map(role => ({
          id: role.id,
          name: role.name,
          description: role.description,
          isSystemRole: role.isSystemRole || false,
          userCount: role.userCount || 0,
          permissions: role.permissions || []
        }));
        this.loadUserRoles();
        this.loading = false;
      },
      error: () => {
        this.notification.error('Failed to load roles');
        this.loading = false;
      }
    });
  }

  loadUserRoles() {
    if (this.userId) {
      // First load the user to get their info and current roles
      this.userManagementService.getUserById(this.userId).subscribe({
        next: (user: any) => {
          this.userName = user.fullName || user.email;
          // User roles are typically returned as an array of strings (role names)
          if (user.roles && Array.isArray(user.roles)) {
            this.selectedRoles = user.roles;
            this.roleForm.patchValue({
              roles: this.selectedRoles
            });
          }
        },
        error: (error: any) => {
          console.error('Failed to load user:', error);
          this.notification.error('Failed to load user information');
        }
      });
    }
  }

  toggleRole(roleName: string) {
    const index = this.selectedRoles.indexOf(roleName);

    if (index === -1) {
      this.selectedRoles.push(roleName);
    } else {
      this.selectedRoles.splice(index, 1);
    }

    this.roleForm.patchValue({
      roles: this.selectedRoles
    });
  }

  isRoleSelected(roleName: string): boolean {
    return this.selectedRoles.includes(roleName);
  }

  onSubmit() {
    if (this.selectedRoles.length === 0) {
      this.notification.error('userManagement.atLeastOneRole');
      return;
    }

    this.submitting = true;
    this.userManagementService.assignUserRoles(this.userId, this.selectedRoles).subscribe({
      next: () => {
        this.notification.success('userManagement.rolesAssigned');
        this.router.navigate(['/user-management/users']);
        this.submitting = false;
      },
      error: () => {
        this.notification.error('userManagement.failedToAssignRoles');
        this.submitting = false;
      }
    });
  }

  cancel() {
    this.router.navigate(['/user-management/users']);
  }

  getRoleDescription(role: Role): string {
    return role.description || 'No description available';
  }

  getRoleUserCount(role: Role): number {
    return role.userCount || 0;
  }

  isSystemRole(role: Role): boolean {
    return role.isSystemRole;
  }

  getSystemRoles(): Role[] {
    return this.allRoles.filter(role => role.isSystemRole);
  }

  getCustomRoles(): Role[] {
    return this.allRoles.filter(role => !role.isSystemRole);
  }
}