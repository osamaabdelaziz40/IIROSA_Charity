import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

// Import components (will be created)
import { CharityListComponent } from './charity-list/charity-list.component';
import { CharityDetailComponent } from './charity-detail/charity-detail.component';
import { CharityFormComponent } from './charity-form/charity-form.component';

const routes: Routes = [
  {
    path: '',
    component: CharityListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'charities.title',
      permission: 'Charities.View'
    }
  },
  {
    path: 'create',
    component: CharityFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'charities.addCharity',
      permission: 'Charities.Create'
    }
  },
  {
    path: ':id',
    component: CharityDetailComponent,
    canActivate: [AuthGuard],
    data: {
      title: 'charities.charityDetails'
    }
  },
  {
    path: ':id/edit',
    component: CharityFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'charities.editCharity',
      permission: 'Charities.Edit'
    }
  },
  {
    path: 'profile',
    component: CharityDetailComponent,
    canActivate: [AuthGuard],
    data: {
      title: 'charities.charityProfile'
    }
  },
  {
    path: 'my-profile',
    component: CharityDetailComponent,
    canActivate: [AuthGuard],
    data: {
      title: 'charities.myProfile'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CharitiesRoutingModule { }
