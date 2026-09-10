/**
 * Notifications Routing Module (UC-NTF web notifications epic)
 * The list is every authenticated user's; the create/edit routes are HQ-only
 * (Notifications.* permission entries, see auth.service.ts PERMISSION_ROLES).
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { NotificationListComponent } from './notification-list/notification-list.component';
import { NotificationFormComponent } from './notification-form/notification-form.component';

import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

const notificationsRoutes: Routes = [
  {
    path: '',
    component: NotificationListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'notifications.title',
      permission: 'Notifications.View'
    }
  },
  {
    path: 'create',
    component: NotificationFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'notifications.addNotification',
      permission: 'Notifications.Create'
    }
  },
  {
    path: ':id/edit',
    component: NotificationFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'notifications.editNotification',
      permission: 'Notifications.Edit'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(notificationsRoutes)],
  exports: [RouterModule]
})
export class NotificationsRoutingModule { }
