import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

// Import components
import { OrphanPaymentListComponent } from './orphan-payment-list/orphan-payment-list.component';
import { OrphanPaymentDetailComponent } from './orphan-payment-detail/orphan-payment-detail.component';
import { OrphanPaymentFormComponent } from './orphan-payment-form/orphan-payment-form.component';
import { AddOrphansToGroupComponent } from './add-orphans-to-group/add-orphans-to-group.component';

const routes: Routes = [
  {
    path: '',
    component: OrphanPaymentListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'orphanPayments.title',
      permission: 'OrphanPayments.View',
      breadcrumb: 'orphanPayments.title'
    }
  },
  {
    path: 'create',
    component: OrphanPaymentFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'orphanPayments.addNewGroup',
      permission: 'OrphanPayments.Create',
      breadcrumb: 'orphanPayments.addNewGroup'
    }
  },
  {
    path: ':id',
    component: OrphanPaymentDetailComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'orphanPayments.groupDetails',
      permission: 'OrphanPayments.View',
      breadcrumb: 'orphanPayments.groupDetails'
    }
  },
  {
    path: ':id/edit',
    component: OrphanPaymentFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'orphanPayments.editGroup',
      permission: 'OrphanPayments.Edit',
      breadcrumb: 'orphanPayments.editGroup'
    }
  },
  {
    path: ':id/add-orphans',
    component: AddOrphansToGroupComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'orphanPayments.addOrphans',
      permission: 'OrphanPayments.AddOrphans',
      breadcrumb: 'orphanPayments.addOrphans'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrphanPaymentsRoutingModule { }
