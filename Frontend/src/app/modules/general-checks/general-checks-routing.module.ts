import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./check-list/check-list.component').then(m => m.CheckListComponent),
        data: { title: 'generalChecks.title' }
      },
      {
        path: 'create',
        loadComponent: () => import('./check-form/check-form.component').then(m => m.CheckFormComponent),
        data: { title: 'generalChecks.createCheck' }
      },
      {
        path: 'edit/:id',
        loadComponent: () => import('./check-form/check-form.component').then(m => m.CheckFormComponent),
        data: { title: 'generalChecks.editCheck' }
      },
      {
        path: ':id',
        loadComponent: () => import('./check-detail/check-detail.component').then(m => m.CheckDetailComponent),
        data: { title: 'generalChecks.checkDetails' }
      },
      {
        path: 'reconcile',
        loadComponent: () => import('./check-reconcile/check-reconcile.component').then(m => m.CheckReconcileComponent),
        data: { title: 'generalChecks.reconciliation' }
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class GeneralChecksRoutingModule {}
