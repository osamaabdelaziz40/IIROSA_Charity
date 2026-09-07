/**
 * HQ Financial Transfers Routing Module (epic 17)
 * Route definitions for HQ transfer management — AuthGuard + PermissionGuard with
 * HqTransfers.* permission entries (see auth.service.ts PERMISSION_ROLES).
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// HQ Transfer Components
import { HqTransferListComponent } from './hq-transfer-list/hq-transfer-list.component';
import { HqTransferFormComponent } from './hq-transfer-form/hq-transfer-form.component';
import { MaxTransferAmountComponent } from './max-transfer-amount/max-transfer-amount.component';
import { HqTransferDetailsComponent } from './hq-transfer-details/hq-transfer-details.component';

// Guards
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

const hqTransfersRoutes: Routes = [
  {
    path: '',
    component: HqTransferListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'hqTransfers.title',
      permission: 'HqTransfers.View'
    }
  },
  {
    // UC-TRF-02: إضافة حوالة — create is its own permission; the endpoint re-authorises
    path: 'create',
    component: HqTransferFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'hqTransfers.addTransfer',
      permission: 'HqTransfers.Create'
    }
  },
  {
    // UC-TRF-07: ضبط الحدود — static segment BEFORE the :id routes. Route permission is the
    // module read (Admin opens read-only per the story); the save UI gates on ManageLimits and
    // the PUT endpoint authorises SuperAdmin-only — hiding is UX, the 403 is the control.
    path: 'max-amounts',
    component: MaxTransferAmountComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'hqTransfers.maxAmounts.title',
      permission: 'HqTransfers.View'
    }
  },
  {
    // UC-TRF-08: تفاصيل الحوالة — §22.S.3 lines screen. Route permission is the module read
    // (Admin opens read-only); write controls gate on HqTransfers.Edit and the PUT endpoint
    // authorises — hiding is UX, the server is the control.
    path: ':id/details',
    component: HqTransferDetailsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'hqTransfers.details.title',
      permission: 'HqTransfers.View'
    }
  },
  {
    // UC-TRF-03: عرض الحوالة — the same form component read-only (route decision in the
    // story: no new component for a disabled render; 17-8 owns :id/details)
    path: ':id/view',
    component: HqTransferFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'hqTransfers.viewTransfer',
      permission: 'HqTransfers.View',
      mode: 'view'
    }
  },
  {
    // UC-TRF-04: تعديل الحوالة — the same form component, populated and editable;
    // PUT carries the id in the body (board contract)
    path: ':id/edit',
    component: HqTransferFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'hqTransfers.editTransfer',
      permission: 'HqTransfers.Edit',
      mode: 'edit'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(hqTransfersRoutes)],
  exports: [RouterModule]
})
export class HqTransfersRoutingModule { }
