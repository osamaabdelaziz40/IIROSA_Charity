/**
 * Housing Projects Routing Module (epic 6, chapter 11)
 * The housing-FAMILY register routes. Roles: Charity + HQ (Admin/SuperAdmin) — the
 * §11.S.1/§11.S.2 register is a charity-facing function (the pre-re-cut comment that
 * barred Charity users described the deleted construction tracker, not this register).
 *
 * Route map across the epic:
 *   ''            — §11.S.1 register list (6-1/6-2)
 *   create        — §11.S.2 register form, add mode (6-3)
 *   :id/edit      — §11.S.2 form in edit mode, UC-HOU-04 (6-4; loads the family, PUT on save)
 *   :id/reports   — periodic reports of a housing family's beneficiaries, UC-HOU-06 §11.S.3 (6-6)
 *   :id/reports/:reportId — §11.S.4 report form, 'new' = add mode, UC-HOU-08 (6-8)
 *   :id           — UC-HOU-04 detail screen (6-4) — ordered after the two-segment routes
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Housing Project Components
import { HousingProjectListComponent } from './housing-project-list/housing-project-list.component';
import { HousingProjectFormComponent } from './housing-project-form/housing-project-form.component';
import { HousingProjectDetailComponent } from './housing-project-detail/housing-project-detail.component';
import { HousingReportListComponent } from './housing-report-list/housing-report-list.component';
import { HousingReportFormComponent } from './housing-report-form/housing-report-form.component';

// Guards
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

// Review 2026-08-24 (6-1): PERMISSION_ROLES in auth.service.ts has carried the
// HousingProjects.* → role mappings since the epic landed, but these routes only ran
// AuthGuard — the map entries were dead and route access was role-data only, weaker than
// every sibling module (charities/families/orphan-payments all pair AuthGuard with
// PermissionGuard). The `roles` arrays stay as the AuthGuard belt; `permission` is the
// suspenders that actually reads PERMISSION_ROLES.
const housingProjectsRoutes: Routes = [
  {
    path: '',
    component: HousingProjectListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'housingProjects.title',
      roles: ['Admin', 'SuperAdmin', 'Charity'],
      permission: 'HousingProjects.View'
    }
  },
  {
    path: 'create',
    component: HousingProjectFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'housingProjects.addFamily',
      roles: ['Admin', 'SuperAdmin', 'Charity'],
      permission: 'HousingProjects.Create'
    }
  },
  {
    path: ':id/edit',
    component: HousingProjectFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'housingProjects.editFamily',
      roles: ['Admin', 'SuperAdmin', 'Charity'],
      permission: 'HousingProjects.Edit'
    }
  },
  {
    path: ':id/reports',
    component: HousingReportListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'housingProjects.reports.title',
      roles: ['Admin', 'SuperAdmin', 'Charity'],
      permission: 'HousingProjects.View'
    }
  },
  {
    // §11.S.4 report form — 'new' in add mode (the 6-7 resolved beneficiary rides the
    // query params), a report id in edit mode. Declared beside :id/reports, before :id.
    // One route serves add + edit; the Create and Edit permission sets are identical
    // (SuperAdmin/Admin/Charity), so Edit is annotated for both modes.
    path: ':id/reports/:reportId',
    component: HousingReportFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'housingProjects.reports.form.title',
      roles: ['Admin', 'SuperAdmin', 'Charity'],
      permission: 'HousingProjects.Edit'
    }
  },
  {
    path: ':id',
    component: HousingProjectDetailComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      pageTitle: 'housingProjects.familyDetails',
      roles: ['Admin', 'SuperAdmin', 'Charity'],
      permission: 'HousingProjects.View'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(housingProjectsRoutes)],
  exports: [RouterModule]
})
export class HousingProjectsRoutingModule { }
