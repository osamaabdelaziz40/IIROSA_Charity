import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LookupManagementComponent } from './lookup-management.component';
import { CountriesListComponent } from './countries/countries-list.component';
import { RegionsListComponent } from './regions/regions-list.component';
import { CentersListComponent } from './centers/centers-list.component';
import { DepartmentsListComponent } from './departments/departments-list.component';

const routes: Routes = [
  {
    path: '',
    component: LookupManagementComponent
  },
  {
    path: 'countries',
    component: CountriesListComponent
  },
  {
    path: 'regions',
    component: RegionsListComponent
  },
  {
    path: 'centers',
    component: CentersListComponent
  },
  {
    path: 'departments',
    component: DepartmentsListComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LookupManagementRoutingModule { }