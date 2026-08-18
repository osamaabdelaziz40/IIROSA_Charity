import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { User, Role, UserListResponse } from '../models/index';

@Injectable({
  providedIn: 'root'
})
export class MockDataService {
  private mockUsers: User[] = [
    {
      id: '1',
      userName: 'admin',
      fullName: 'Admin User',
      email: 'admin@iirosa.org',
      phoneNumber: '+966501234567',
      isActive: true,
      roles: ['Super Admin', 'Admin'],
      createdOn: new Date('2024-01-15'),
      updatedOn: new Date('2024-04-20')
    },
    {
      id: '2',
      userName: 'ahmed',
      fullName: 'Ahmed Al-Rashid',
      email: 'ahmed@iirosa.org',
      phoneNumber: '+966502345678',
      isActive: true,
      roles: ['Admin'],
      createdOn: new Date('2024-02-10'),
      updatedOn: new Date('2024-04-15')
    },
    {
      id: '3',
      userName: 'fatima',
      fullName: 'Fatima Hassan',
      email: 'fatima@iirosa.org',
      phoneNumber: '+966503456789',
      isActive: true,
      roles: ['Charity'],
      createdOn: new Date('2024-03-05'),
      updatedOn: new Date('2024-04-10')
    },
    {
      id: '4',
      userName: 'mohammed',
      fullName: 'Mohammed Al-Saud',
      email: 'mohammed@iirosa.org',
      phoneNumber: '+966504567890',
      isActive: false,
      roles: ['Accountant'],
      createdOn: new Date('2024-01-20'),
      updatedOn: new Date('2024-04-01')
    },
    {
      id: '5',
      userName: 'aisha',
      fullName: 'Aisha Abdullah',
      email: 'aisha@iirosa.org',
      phoneNumber: '+966505678901',
      isActive: true,
      roles: ['FinancialOfficer'],
      createdOn: new Date('2024-02-15'),
      updatedOn: new Date('2024-04-12')
    }
  ];

  private mockRoles: Role[] = [
    {
      id: '1',
      name: 'Super Admin',
      description: 'Full system access with all permissions',
      isSystemRole: true,
      userCount: 1,
      permissions: ['users.create', 'users.read', 'users.update', 'users.delete', 'roles.create', 'roles.read', 'roles.update', 'roles.delete']
    },
    {
      id: '2',
      name: 'Admin',
      description: 'Administrative access for organization management',
      isSystemRole: true,
      userCount: 1,
      permissions: ['users.create', 'users.read', 'users.update', 'roles.read']
    },
    {
      id: '3',
      name: 'Charity',
      description: 'Charity operations management',
      isSystemRole: true,
      userCount: 1,
      permissions: ['users.read', 'orphans.read', 'families.read']
    },
    {
      id: '4',
      name: 'Accountant',
      description: 'Financial management and reporting',
      isSystemRole: true,
      userCount: 1,
      permissions: ['users.read', 'financial.read', 'financial.create']
    },
    {
      id: '5',
      name: 'FinancialOfficer',
      description: 'Financial oversight and approval',
      isSystemRole: true,
      userCount: 1,
      permissions: ['users.read', 'financial.read', 'financial.approve']
    }
  ];

  // Mock user methods
  getUsers(pageNumber: number = 1, pageSize: number = 10): Observable<UserListResponse> {
    const startIndex = (pageNumber - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedUsers = this.mockUsers.slice(startIndex, endIndex);

    const totalPages = Math.ceil(this.mockUsers.length / pageSize);

    const response: UserListResponse = {
      items: paginatedUsers,
      totalCount: this.mockUsers.length,
      pageNumber: pageNumber,
      pageSize: pageSize,
      totalPages: totalPages,
      hasPrevious: pageNumber > 1,
      hasNext: pageNumber < totalPages
    };

    return of(response).pipe(delay(500)); // Simulate network delay
  }

  getUser(userId: string): Observable<User> {
    const user = this.mockUsers.find(u => u.id === userId);
    if (!user) {
      throw new Error('User not found');
    }
    return of(user).pipe(delay(300));
  }

  createUser(user: any): Observable<User> {
    const newUser: User = {
      id: (this.mockUsers.length + 1).toString(),
      userName: user.fullName.toLowerCase().replace(/\s+/g, '.'),
      fullName: user.fullName,
      email: user.email,
      phoneNumber: user.phoneNumber || '',
      isActive: user.isActive ?? true,
      roles: user.roles || [],
      createdOn: new Date(),
      updatedOn: new Date()
    };
    this.mockUsers.push(newUser);
    return of(newUser).pipe(delay(500));
  }

  updateUser(userId: string, user: any): Observable<User> {
    const index = this.mockUsers.findIndex(u => u.id === userId);
    if (index === -1) {
      throw new Error('User not found');
    }
    this.mockUsers[index] = {
      ...this.mockUsers[index],
      ...user,
      updatedOn: new Date()
    };
    return of(this.mockUsers[index]).pipe(delay(500));
  }

  deleteUser(userId: string): Observable<void> {
    const index = this.mockUsers.findIndex(u => u.id === userId);
    if (index === -1) {
      throw new Error('User not found');
    }
    this.mockUsers.splice(index, 1);
    return of(void 0).pipe(delay(300));
  }

  activateUser(userId: string): Observable<void> {
    const user = this.mockUsers.find(u => u.id === userId);
    if (user) {
      user.isActive = true;
      user.updatedOn = new Date();
    }
    return of(void 0).pipe(delay(300));
  }

  deactivateUser(userId: string): Observable<void> {
    const user = this.mockUsers.find(u => u.id === userId);
    if (user) {
      user.isActive = false;
      user.updatedOn = new Date();
    }
    return of(void 0).pipe(delay(300));
  }

  resetUserPassword(userId: string, newPassword: string, forceChange: boolean): Observable<any> {
    return of({ success: true, message: 'Password reset successfully' }).pipe(delay(500));
  }

  // Mock role methods
  getRoles(): Observable<Role[]> {
    return of(this.mockRoles).pipe(delay(300));
  }

  getRole(roleId: string): Observable<Role> {
    const role = this.mockRoles.find(r => r.id === roleId);
    if (!role) {
      throw new Error('Role not found');
    }
    return of(role).pipe(delay(300));
  }

  createRole(role: any): Observable<Role> {
    const newRole: Role = {
      id: (this.mockRoles.length + 1).toString(),
      name: role.name,
      description: role.description || '',
      isSystemRole: false,
      userCount: 0,
      permissions: role.permissions || []
    };
    this.mockRoles.push(newRole);
    return of(newRole).pipe(delay(500));
  }

  updateRole(roleId: string, role: any): Observable<Role> {
    const index = this.mockRoles.findIndex(r => r.id === roleId);
    if (index === -1) {
      throw new Error('Role not found');
    }
    this.mockRoles[index] = {
      ...this.mockRoles[index],
      ...role
    };
    return of(this.mockRoles[index]).pipe(delay(500));
  }

  deleteRole(roleId: string): Observable<void> {
    const index = this.mockRoles.findIndex(r => r.id === roleId);
    if (index === -1) {
      throw new Error('Role not found');
    }
    if (this.mockRoles[index].isSystemRole) {
      throw new Error('Cannot delete system role');
    }
    this.mockRoles.splice(index, 1);
    return of(void 0).pipe(delay(300));
  }
}
