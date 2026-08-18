import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { RoleManagementService } from '../../../../core/services/role-management.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Role } from '../../../../core/models/role.model';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { LoadingComponent } from '../../../../shared/components/loading/loading.component';

@Component({
  selector: 'app-role-detail',
  standalone: true,
  imports: [CommonModule, TranslateModule, PageHeaderComponent, LoadingComponent],
  templateUrl: './role-detail.component.html',
  styleUrls: ['./role-detail.component.scss']
})
export class RoleDetailComponent implements OnInit {
  role?: Role;
  loading = false;
  roleId?: string;

  pageActions = [
    {
      label: 'common.back',
      type: 'secondary',
      icon: 'fe-arrow-left',
      click: () => this.goBack()
    },
    {
      label: 'userManagement.editRole',
      type: 'primary',
      icon: 'fe-edit',
      click: () => this.editRole()
    }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private roleManagementService: RoleManagementService,
    private notification: NotificationService
  ) {}

  ngOnInit() {
    // Page actions are now directly bound

    this.roleId = this.route.snapshot.params['id'];
    this.loadRole();
  }

  loadRole() {
    if (!this.roleId) return;

    this.loading = true;
    this.roleManagementService.getRole(this.roleId).subscribe({
      next: (role: any) => {
        // Transform the response to match our Role interface
        this.role = {
          id: role.id || this.roleId,
          name: role.name,
          description: role.description,
          isSystemRole: role.isSystemRole || false,
          userCount: role.userCount || 0,
          permissions: role.permissions || []
        };
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Error loading role:', err);
        this.notification.error(err?.error?.message || 'Failed to load role');
        this.loading = false;
      }
    });
  }

  goBack() {
    this.router.navigate(['/user-management/roles']);
  }

  editRole() {
    if (this.roleId) {
      this.router.navigate(['/user-management/roles', this.roleId, 'edit']);
    }
  }

  deleteRole() {
    if (!this.role) return;

    if (confirm(`Are you sure you want to delete the role "${this.role.name}"?`)) {
      this.roleManagementService.deleteRole(this.role.id).subscribe({
        next: () => {
          this.notification.success('Role deleted successfully');
          this.router.navigate(['/user-management/roles']);
        },
        error: () => {
          this.notification.error('Failed to delete role');
        }
      });
    }
  }

  isSystemRole(): boolean {
    return this.role?.isSystemRole || false;
  }
}
