import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LookupManagementComponent } from './lookup-management.component';
import { CountriesListComponent } from './countries/countries-list.component';
import { RegionsListComponent } from './regions/regions-list.component';
import { CentersListComponent } from './centers/centers-list.component';
import { DepartmentsListComponent } from './departments/departments-list.component';
import { OfficeProjectTypesListComponent } from './office-project-types/office-project-types-list.component';
import { HousingBuildingsListComponent } from './housing-buildings/housing-buildings-list.component';
import { HousingFlatsListComponent } from './housing-flats/housing-flats-list.component';
import { OutgoingCategoriesListComponent } from './outgoing-categories/outgoing-categories-list.component';
import { SimpleLookupListComponent } from './simple-lookups/simple-lookup-list.component';

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
  },
  {
    path: 'office-project-types',
    component: OfficeProjectTypesListComponent
  },
  {
    path: 'housing-buildings',
    component: HousingBuildingsListComponent
  },
  {
    path: 'housing-flats',
    component: HousingFlatsListComponent
  },
  {
    path: 'outgoing-categories',
    component: OutgoingCategoriesListComponent
  },
  {
    path: 'house-ownerships',
    component: SimpleLookupListComponent,
    data: { table: 'house-ownerships' }
  },
  {
    path: 'income-types',
    component: SimpleLookupListComponent,
    data: { table: 'income-types' }
  },
  {
    path: 'family-project-statuses',
    component: SimpleLookupListComponent,
    data: { table: 'family-project-statuses' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LookupManagementRoutingModule { }