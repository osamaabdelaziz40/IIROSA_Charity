import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeListComponent } from './employee-list/employee-list.component';
import { EmployeeDetailComponent } from './employee-detail/employee-detail.component';
import { EmployeeFormComponent } from './employee-form/employee-form.component';
import { RoleAssignmentComponent } from './role-assignment/role-assignment.component';

const routes: Routes = [
  {
    path: '',
    component: EmployeeListComponent,
    pathMatch: 'full'
  },
  {
    path: 'create',
    component: EmployeeFormComponent
  },
  {
    path: ':id',
    component: EmployeeDetailComponent
  },
  {
    path: ':id/edit',
    component: EmployeeFormComponent
  },
  {
    path: ':id/roles',
    component: RoleAssignmentComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EmployeesRoutingModule {}
