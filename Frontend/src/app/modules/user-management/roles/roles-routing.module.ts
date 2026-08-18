import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RoleListComponent } from './role-list/role-list.component';
import { RoleFormComponent } from './role-form/role-form.component';
import { RoleDetailComponent } from './role-detail/role-detail.component';

const routes: Routes = [
  {
    path: '',
    component: RoleListComponent,
    pathMatch: 'full'
  },
  {
    path: 'create',
    component: RoleFormComponent
  },
  {
    path: ':id',
    component: RoleDetailComponent
  },
  {
    path: ':id/edit',
    component: RoleFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class RolesRoutingModule {}
