/**
 * Housing Projects Routing Module
 * Route definitions for housing project management
 * All routes restricted to Admin and Super Admin roles only
 * Charity users CANNOT access any housing project routes
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Housing Project Components
import { HousingProjectListComponent } from './housing-project-list/housing-project-list.component';
import { HousingProjectDetailComponent } from './housing-project-detail/housing-project-detail.component';
import { HousingProjectFormComponent } from './housing-project-form/housing-project-form.component';

// Guards
import { AuthGuard } from '../../core/guards/auth.guard';

const housingProjectsRoutes: Routes = [
  {
    path: '',
    component: HousingProjectListComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'housingProjects.title',
      roles: ['Admin', 'SuperAdmin']
    }
  },
  {
    path: 'create',
    component: HousingProjectFormComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'housingProjects.addProject',
      roles: ['Admin', 'SuperAdmin']
    }
  },
  {
    path: ':id',
    component: HousingProjectDetailComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'housingProjects.projectDetails',
      roles: ['Admin', 'SuperAdmin']
    }
  },
  {
    path: ':id/edit',
    component: HousingProjectFormComponent,
    canActivate: [AuthGuard],
    data: {
      pageTitle: 'housingProjects.editProject',
      roles: ['Admin', 'SuperAdmin']
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(housingProjectsRoutes)],
  exports: [RouterModule]
})
export class HousingProjectsRoutingModule { }
