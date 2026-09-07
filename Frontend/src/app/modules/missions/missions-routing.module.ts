/**
 * Missions Routing Module (epic 15)
 * Route definitions for mission management — AuthGuard + PermissionGuard with
 * Missions.* permission entries (see auth.service.ts PERMISSION_ROLES).
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Mission Components
import { MissionListComponent } from './mission-list/mission-list.component';
import { MissionDetailComponent } from './mission-detail/mission-detail.component';
import { MissionFormComponent } from './mission-form/mission-form.component';
import { MissionRegisterComponent } from './mission-register/mission-register.component';

// Guards
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

const missionsRoutes: Routes = [
  {
    path: '',
    component: MissionListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'missions.title',
      permission: 'Missions.View'
    }
  },
  {
    path: 'create',
    component: MissionFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'missions.addMission',
      permission: 'Missions.Create'
    }
  },
  {
    path: ':id',
    component: MissionDetailComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'missions.missionDetails',
      permission: 'Missions.View'
    }
  },
  {
    path: ':id/edit',
    component: MissionFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'missions.editMission',
      permission: 'Missions.Edit'
    }
  },
  {
    path: ':id/register',
    component: MissionRegisterComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'missions.registerResult',
      permission: 'Missions.Edit'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(missionsRoutes)],
  exports: [RouterModule]
})
export class MissionsRoutingModule { }
