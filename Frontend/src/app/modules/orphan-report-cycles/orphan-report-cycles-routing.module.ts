/**
 * Orphan Report Cycles routing (UC-ORC, docs/Modules/26-UC-ORC) — HQ-only management
 * screens; every route mirrors OrphanReportCyclesController's [Authorize(Roles =
 * "Admin,SuperAdmin")] split through the permission map. The charity-facing pieces of
 * the feature (chooser, coverage report) live in the periodic-reports and reports
 * modules and are deliberately NOT routed here.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

import { CycleListComponent } from './cycle-list/cycle-list.component';
import { CycleFormComponent } from './cycle-form/cycle-form.component';

const routes: Routes = [
  {
    // UC-ORC-01 — the register (§26.S.1)
    path: '',
    component: CycleListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'orphanReportCycles.title',
      permission: 'OrphanReportCycles.View'
    }
  },
  {
    // UC-ORC-02 — create (§26.S.2)
    path: 'create',
    component: CycleFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'orphanReportCycles.createCycle',
      permission: 'OrphanReportCycles.Create'
    }
  },
  {
    // UC-ORC-03 — edit/detail; the form renders read-only for a completed cycle (BR-ORC-08)
    path: ':id/edit',
    component: CycleFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'orphanReportCycles.editCycle',
      permission: 'OrphanReportCycles.Edit'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrphanReportCyclesRoutingModule {}
