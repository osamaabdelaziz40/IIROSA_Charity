import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

// Import components
import { FamilyListComponent } from './family-list/family-list.component';
import { FamilyDetailComponent } from './family-detail/family-detail.component';
import { FamilyFormComponent } from './family-form/family-form.component';
import { FamilyMembersComponent } from './family-members/family-members.component';
import { ProviderRequestListComponent } from './provider-request-list/provider-request-list.component';
import { RefugeeFamilyListComponent } from './refugee-family-list/refugee-family-list.component';
import { RefugeeFamilyFormComponent } from './refugee-family-form/refugee-family-form.component';
import { RefugeeFamilyDetailComponent } from './refugee-family-detail/refugee-family-detail.component';
import { OrphanCodingComponent } from './orphan-coding/orphan-coding.component';
import { OrphanCodingWorklistComponent } from './orphan-coding-worklist/orphan-coding-worklist.component';

const routes: Routes = [
  {
    path: '',
    component: FamilyListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'families.title',
      permission: 'Families.View'
    }
  },
  {
    path: 'create',
    component: FamilyFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'families.addFamily',
      permission: 'Families.Create'
    }
  },
  // Refugee register (UC-REF / epic 7) — child routes of `families`, same FamiliesController.
  // Route-order landmine: `refugees*` MUST stay ABOVE `':id'` or the router matches it as an id.
  {
    path: 'refugees/create',
    component: RefugeeFamilyFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'families.refugeeFormTitle',
      permission: 'Families.Create'
    }
  },
  // UC-REF-04 — the §12.S.2 view/edit screens. `:id/edit` MUST stay above `refugees/:id` and
  // both above `':id'` (same ordering landmine as above).
  {
    path: 'refugees/:id/edit',
    component: RefugeeFamilyFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'families.refugeeEditTitle',
      permission: 'Families.Edit'
    }
  },
  {
    path: 'refugees/:id',
    component: RefugeeFamilyDetailComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'families.refugeeDetailTitle',
      permission: 'Families.View'
    }
  },
  {
    path: 'refugees',
    component: RefugeeFamilyListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'families.refugeeTitle',
      permission: 'Families.View'
    }
  },
  // Orphan coding (UC-ORP / epic 8) — the literal segments `orphans/coding*` MUST also stay
  // ABOVE `':id'` for the same reason.
  {
    path: 'orphans/coding/worklist',
    component: OrphanCodingWorklistComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'orphanCoding.worklistTitle',
      permission: 'OrphanCoding.View'
    }
  },
  {
    path: 'orphans/coding',
    component: OrphanCodingComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'orphanCoding.title',
      permission: 'OrphanCoding.View'
    }
  },
  // Guardian-change review queue (UC-FAM-09) — literal segment, also above ':id'.
  {
    path: 'provider-requests',
    component: ProviderRequestListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'families.providerRequests.title',
      permission: 'Families.ProviderRequests'
    }
  },
  {
    path: ':id',
    component: FamilyDetailComponent,
    canActivate: [AuthGuard],
    data: {
      title: 'families.familyDetails'
    }
  },
  {
    path: ':id/edit',
    component: FamilyFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'families.editFamily',
      permission: 'Families.Edit'
    }
  },
  // Members screen (UC-FAM-07 نقل يتيم بين الأسر, §10.S.3) — orphan/guardian move commands.
  {
    path: ':id/members',
    component: FamilyMembersComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'families.members.title',
      permission: 'Families.Members'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FamiliesRoutingModule { }
