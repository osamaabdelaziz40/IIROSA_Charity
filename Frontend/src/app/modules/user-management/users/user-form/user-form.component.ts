import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserManagementService } from '../../services/user-management.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { User, CreateUserRequest, UpdateUserRequest } from '../../../../core/models/user.model';
import { Role } from '../../../../core/models/role.model';
import { ApiResponse } from '../../../../core/models/common.model';
import { Observable } from 'rxjs';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { LoadingComponent } from '../../../../shared/components/loading/loading.component';
import { TranslateModule } from '@ngx-translate/core';
import { BreadcrumbComponent, BreadcrumbItem } from '../../../../shared/components';
import { DropDownComponent } from '../../../../shared/components/drop-down/drop-down.component';
import { SharedModule } from '../../../../shared/shared.module';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PageHeaderComponent, LoadingComponent, TranslateModule, BreadcrumbComponent, DropDownComponent, SharedModule],
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss']
})
export class UserFormComponent implements OnInit {
  // Breadcrumb items
  breadcrumbs: BreadcrumbItem[] = [
    { label: 'common.home', url: '/dashboard' },
    { label: 'userManagement.title', url: '/user-management/users' }
  ];

  get breadcrumbsWithAction(): BreadcrumbItem[] {
    return [
      ...this.breadcrumbs,
      { label: this.isEdit ? 'userManagement.editUser' : 'userManagement.addUser' }
    ];
  }
  userForm: FormGroup;
  loading = false;
  submitting = false;
  loadingRoles = false;
  isEdit = false;
  userId?: string;
  allRoles: Role[] = [];
  submitted = false;

  // Dropdown data for roles
  rolesDropdownData: Array<{ id: string; name: string }> = [];

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
    private userManagementService: UserManagementService,
    private notification: NotificationService
  ) {
    this.userForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      roles: [[], Validators.required],
      isActive: [true],
      password: ['']
    });
  }

  ngOnInit() {
    // Page actions are now directly bound

    this.userId = this.route.snapshot.params['id'];

    if (this.userId) {
      this.isEdit = true;
      this.loadRolesAndUser();
    } else {
      this.loadRoles();
      // For new users, password is required
      this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
    }
  }

  // Getter for easy access to form fields
  get f() {
    return this.userForm.controls;
  }

  loadRoles() {
    this.loadingRoles = true;
    this.userManagementService.getAllRoles().subscribe({
      next: (roles: Role[]) => {
        this.allRoles = roles;
        this.rolesDropdownData = roles.map(role => ({ id: role.name, name: role.name }));
        this.loadingRoles = false;
      },
      error: (error: any) => {
        console.error('Error loading roles:', error);
        this.loadingRoles = false;
        // Fallback to static roles if backend fails
        this.allRoles = [
          { id: '1', name: 'SuperAdmin', description: 'Full system access', isSystemRole: true, userCount: 0, permissions: [] },
          { id: '2', name: 'Admin', description: 'Organization management', isSystemRole: true, userCount: 0, permissions: [] },
          { id: '3', name: 'Charity', description: 'Charity operations', isSystemRole: true, userCount: 0, permissions: [] },
          { id: '4', name: 'Accountant', description: 'Financial management', isSystemRole: true, userCount: 0, permissions: [] },
          { id: '5', name: 'FinancialOfficer', description: 'Financial oversight', isSystemRole: true, userCount: 0, permissions: [] }
        ];
        this.rolesDropdownData = this.allRoles.map(role => ({ id: role.name, name: role.name }));
      }
    });
  }

  loadRolesAndUser() {
    // Load roles first, then load user data
    this.loadingRoles = true;
    this.userManagementService.getAllRoles().subscribe({
      next: (roles: Role[]) => {
        this.allRoles = roles;
        this.rolesDropdownData = roles.map(role => ({ id: role.name, name: role.name }));
        this.loadingRoles = false;
        // After roles are loaded, load user data
        this.loadUser();
      },
      error: (error: any) => {
        console.error('Error loading roles:', error);
        this.loadingRoles = false;
        // Fallback to static roles if backend fails
        this.allRoles = [
          { id: '1', name: 'SuperAdmin', description: 'Full system access', isSystemRole: true, userCount: 0, permissions: [] },
          { id: '2', name: 'Admin', description: 'Organization management', isSystemRole: true, userCount: 0, permissions: [] },
          { id: '3', name: 'Charity', description: 'Charity operations', isSystemRole: true, userCount: 0, permissions: [] },
          { id: '4', name: 'Accountant', description: 'Financial management', isSystemRole: true, userCount: 0, permissions: [] },
          { id: '5', name: 'FinancialOfficer', description: 'Financial oversight', isSystemRole: true, userCount: 0, permissions: [] }
        ];
        this.rolesDropdownData = this.allRoles.map(role => ({ id: role.name, name: role.name }));
        this.loadUser();
      }
    });
  }

  loadUser() {
    this.loading = true;
    this.userManagementService.getUserById(this.userId!).subscribe({
      next: (user: User) => {
        // Filter user roles to only include roles that exist in available roles
        const userRoles = (user.roles || []).filter((role: string) =>
          this.allRoles.some(availableRole => availableRole.name === role)
        );

        this.userForm.patchValue({
          fullName: user.fullName,
          email: user.email,
          phoneNumber: user.phoneNumber || '',
          roles: userRoles,
          isActive: user.isActive
        });
        this.loading = false;
      },
      error: () => {
        this.notification.error('Failed to load user');
        this.loading = false;
      }
    });
  }

  onSubmit() {
    this.submitted = true;
    if (this.userForm.invalid) return;

    this.submitting = true;

    if (this.isEdit) {
      const updateRequest: UpdateUserRequest = {
        fullName: this.f['fullName'].value,
        email: this.f['email'].value,
        phoneNumber: this.f['phoneNumber'].value,
        roles: this.f['roles'].value,
        isActive: this.f['isActive'].value
      };

      this.userManagementService.updateUser(this.userId!, updateRequest).subscribe({
        next: () => {
          this.notification.success('User updated successfully');
          this.router.navigate(['/user-management/users']);
        },
        error: (error: any) => {
          this.notification.error(error.error?.message || 'Operation failed');
          this.submitting = false;
        }
      });
    } else {
      const createRequest: CreateUserRequest = {
        fullName: this.f['fullName'].value,
        email: this.f['email'].value,
        phoneNumber: this.f['phoneNumber'].value,
        roles: this.f['roles'].value,
        password: this.f['password'].value || 'P@ssw0rd@2022' // Default password as per use case
      };

      this.userManagementService.createUser(createRequest).subscribe({
        next: () => {
          this.notification.success('User created successfully');
          this.router.navigate(['/user-management/users']);
        },
        error: (error: any) => {
          this.notification.error(error.error?.message || 'Operation failed');
          this.submitting = false;
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['/user-management/users']);
  }
}
