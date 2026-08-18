import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

// Import components
import { FamilyListComponent } from './family-list/family-list.component';
import { FamilyDetailComponent } from './family-detail/family-detail.component';
import { FamilyFormComponent } from './family-form/family-form.component';

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
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FamiliesRoutingModule { }
