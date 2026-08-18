/**
 * Missions Routing Module
 * Route definitions for mission management
 * All routes restricted to Admin and Super Admin roles only
 * Charity users CANNOT access any mission routes
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Mission Components
import { MissionListComponent } from './mission-list/mission-list.component';
import { MissionDetailComponent } from './mission-detail/mission-detail.component';
import { MissionFormComponent } from './mission-form/mission-form.component';
import { MyMissionsComponent } from './my-missions/my-missions.component';

// Guards
import { AuthGuard } from '../../core/guards/auth.guard';

const missionsRoutes: Routes = [
  {
    path: '',
    component: MissionListComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'missions.title',
      roles: ['Admin', 'SuperAdmin']
    }
  },
  {
    path: 'create',
    component: MissionFormComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'missions.addMission',
      roles: ['Admin', 'SuperAdmin']
    }
  },
  {
    path: 'my-missions',
    component: MyMissionsComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'missions.myMissions',
      roles: ['Admin', 'SuperAdmin']
    }
  },
  {
    path: ':id',
    component: MissionDetailComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'missions.missionDetails',
      roles: ['Admin', 'SuperAdmin']
    }
  },
  {
    path: ':id/edit',
    component: MissionFormComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'missions.editMission',
      roles: ['Admin', 'SuperAdmin']
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(missionsRoutes)],
  exports: [RouterModule]
})
export class MissionsRoutingModule { }
