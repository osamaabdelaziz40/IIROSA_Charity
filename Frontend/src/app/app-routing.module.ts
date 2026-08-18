import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: 'auth',
    component: AuthLayoutComponent,
    loadChildren: () => import('./modules/auth/auth.module').then(m => m.AuthModule),
    data: { title: 'common.authentication' }
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        redirectTo: '/dashboard',
        pathMatch: 'full',
        data: { title: 'common.home' }
      },
      {
        path: 'dashboard',
        loadChildren: () => import('./modules/dashboard/dashboard.module').then(m => m.DashboardModule),
        data: { title: 'dashboard.title' }
      },
      {
        path: 'employees',
        loadChildren: () => import('./modules/employees/employees.module').then(m => m.EmployeesModule),
        data: { title: 'employees.title' }
      },
      {
        path: 'charities',
        loadChildren: () => import('./modules/charities/charities.module').then(m => m.CharitiesModule),
        data: { title: 'charities.title' }
      },
      {
        path: 'families',
        loadChildren: () => import('./modules/families/families.module').then(m => m.FamiliesModule),
        data: { title: 'families.title' }
      },
      {
        path: 'orphan-payments',
        loadChildren: () => import('./modules/orphan-payments/orphan-payments.module').then(m => m.OrphanPaymentsModule),
        data: { title: 'orphanPayments.title' }
      },
      {
        path: 'user-management',
        loadChildren: () => import('./modules/user-management/user-management.module').then(m => m.UserManagementModule),
        data: { title: 'userManagement.title' }
      },
      {
        path: 'lookup-management',
        loadChildren: () => import('./modules/lookup-management/lookup-management.module').then(m => m.LookupManagementModule),
        data: { title: 'lookupManagement.title' }
      },
      {
        path: 'missions',
        loadChildren: () => import('./modules/missions/missions.module').then(m => m.MissionsModule),
        data: { title: 'missions.title' }
      },
      {
        path: 'housing-projects',
        loadChildren: () => import('./modules/housing-projects/housing-projects.module').then(m => m.HousingProjectsModule),
        data: { title: 'housingProjects.title' }
      },
      {
        path: 'seasonal-aid',
        loadChildren: () => import('./modules/seasonal-aid/seasonal-aid.module').then(m => m.SeasonalAidModule),
        data: { title: 'seasonalAid.title' }
      },
      {
        path: 'office-development-projects',
        loadChildren: () => import('./modules/office-development-projects/office-development-projects.module').then(m => m.OfficeDevelopmentProjectsModule),
        data: { title: 'officeDevelopmentProjects.title' }
      },
      {
        path: 'general-checks',
        loadChildren: () => import('./modules/general-checks/general-checks.module').then(m => m.GeneralChecksModule),
        data: { title: 'generalChecks.title' }
      },
      {
        path: 'incoming-outgoing',
        loadChildren: () => import('./modules/incoming-outgoing/incoming-outgoing.module').then(m => m.IncomingOutgoingModule),
        data: { title: 'incomingOutgoing.title' }
      },
      {
        path: 'technical-support',
        loadChildren: () => import('./modules/technical-support/technical-support.module').then(m => m.TechnicalSupportModule),
        data: { title: 'technicalSupport.title' }
      }
    ]
  },
  {
    path: '**',
    redirectTo: '/dashboard',
    data: { title: 'common.home' }
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
