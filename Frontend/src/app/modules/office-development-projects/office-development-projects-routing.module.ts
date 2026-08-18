import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';
import { ProjectListComponent } from './project-list/project-list.component';
import { ProjectDetailComponent } from './project-detail/project-detail.component';
import { ProjectFormComponent } from './project-form/project-form.component';

const routes: Routes = [
  {
    path: '',
    component: ProjectListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'officeDevelopmentProjects.title',
      permission: 'OfficeDevelopmentProjects.View'
    }
  },
  {
    path: 'create',
    component: ProjectFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'officeDevelopmentProjects.addProject',
      permission: 'OfficeDevelopmentProjects.Create'
    }
  },
  {
    path: 'progress',
    component: ProjectListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'officeDevelopmentProjects.projectProgress',
      permission: 'OfficeDevelopmentProjects.View'
    }
  },
  {
    path: ':id',
    component: ProjectDetailComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'officeDevelopmentProjects.projectDetails',
      permission: 'OfficeDevelopmentProjects.View'
    }
  },
  {
    path: ':id/edit',
    component: ProjectFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'officeDevelopmentProjects.editProject',
      permission: 'OfficeDevelopmentProjects.Edit'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OfficeDevelopmentProjectsRoutingModule {}
