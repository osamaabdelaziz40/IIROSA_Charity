import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserListComponent } from './user-list/user-list.component';
import { UserDetailComponent } from './user-detail/user-detail.component';
import { UserFormComponent } from './user-form/user-form.component';
import { RoleAssignmentDialogComponent } from '../roles/role-assignment-dialog/role-assignment-dialog.component';

const routes: Routes = [
  {
    path: '',
    component: UserListComponent,
    pathMatch: 'full'
  },
  {
    path: 'create',
    component: UserFormComponent
  },
  {
    path: ':id',
    component: UserDetailComponent
  },
  {
    path: ':id/edit',
    component: UserFormComponent
  },
  {
    path: ':id/roles',
    component: RoleAssignmentDialogComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UsersRoutingModule {}
