import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RoleManagementService } from '../../../../core/services/role-management.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Role } from '../../../../core/models/role.model';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { LoadingComponent } from '../../../../shared/components/loading/loading.component';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, LoadingComponent, PaginationComponent, TranslateModule],
  templateUrl: './role-list.component.html',
  styleUrls: ['./role-list.component.scss']
})
export class RoleListComponent implements OnInit {
  roles: Role[] = [];
  loading = false;
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  pageActions = [
    {
      label: 'userManagement.addRole',
      type: 'primary',
      icon: 'fe-plus',
      click: () => this.createRole()
    }
  ];

  constructor(
    private roleManagementService: RoleManagementService,
    private notification: NotificationService,
    private router: Router
  ) {}

  ngOnInit() {
    // Page actions are now directly bound
    this.loadRoles();
  }

  loadRoles() {
    this.loading = true;
    // Note: The getRoles method doesn't have pagination in the current service
    // We'll need to adapt this based on actual API
    this.roleManagementService.getRoles().subscribe({
      next: (response: any) => {
        // Handle both array response and paginated response
        if (Array.isArray(response)) {
          this.roles = response;
          this.totalCount = response.length;
        } else {
          this.roles = response.items || [];
          this.totalCount = response.totalCount || 0;
        }
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        this.loading = false;
      },
      error: () => {
        this.notification.error('Failed to load roles');
        this.loading = false;
      }
    });
  }

  createRole() {
    this.router.navigate(['/user-management/roles/create']);
  }

  editRole(id: string) {
    this.router.navigate(['/user-management/roles', id, 'edit']);
  }

  viewRole(id: string) {
    this.router.navigate(['/user-management/roles', id]);
  }

  deleteRole(id: string, name: string) {
    if (confirm(`Are you sure you want to delete the role "${name}"?`)) {
      this.roleManagementService.deleteRole(id).subscribe({
        next: () => {
          this.notification.success('Role deleted successfully');
          this.loadRoles();
        },
        error: () => {
          this.notification.error('Failed to delete role');
        }
      });
    }
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadRoles();
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  isSystemRole(role: Role): boolean {
    return role.isSystemRole;
  }
}
