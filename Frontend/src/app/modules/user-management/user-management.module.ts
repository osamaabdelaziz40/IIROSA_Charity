import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';

const routes: Routes = [
  {
    path: 'users',
    loadChildren: () => import('./users/users.module').then(m => m.UsersModule)
  },
  {
    path: 'roles',
    loadChildren: () => import('./roles/roles.module').then(m => m.RolesModule)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes), SharedModule],
  exports: []
})
export class UserManagementModule {}
