import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PermissionGuard } from '../../core/guards/permission.guard';

// Permissions mirror CheckManagementController's [Authorize] sets:
// reads SuperAdmin,Admin,Accountant,FinancialOfficer; writes SuperAdmin,Accountant,FinancialOfficer.
const routes: Routes = [
  {
    path: '',
    canActivate: [PermissionGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./check-list/check-list.component').then(m => m.CheckListComponent),
        data: { title: 'generalChecks.title', permission: 'GeneralChecks.View' }
      },
      {
        path: 'create',
        loadComponent: () => import('./check-form/check-form.component').then(m => m.CheckFormComponent),
        data: { title: 'generalChecks.addCheck', permission: 'GeneralChecks.Create' }
      },
      {
        path: 'edit/:id',
        loadComponent: () => import('./check-form/check-form.component').then(m => m.CheckFormComponent),
        data: { title: 'generalChecks.editCheck', permission: 'GeneralChecks.Edit' }
      },
      {
        // §16.S.3 بيان الشيكات — a literal segment, so it must be declared before ':id'
        // or the router matches 'statement' as a Guid id and the detail screen 404s.
        path: 'statement',
        loadComponent: () => import('./check-statement/check-statement.component').then(m => m.CheckStatementComponent),
        data: { title: 'generalChecks.statement', permission: 'GeneralChecks.View' }
      },
      {
        path: ':id',
        loadComponent: () => import('./check-detail/check-detail.component').then(m => m.CheckDetailComponent),
        data: { title: 'generalChecks.checkDetails', permission: 'GeneralChecks.View' }
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class GeneralChecksRoutingModule {}
