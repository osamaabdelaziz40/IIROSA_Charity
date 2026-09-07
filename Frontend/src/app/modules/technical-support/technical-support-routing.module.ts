import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TicketListComponent } from './ticket-list/ticket-list.component';
import { TicketDetailComponent } from './ticket-detail/ticket-detail.component';
import { TicketFormComponent } from './ticket-form/ticket-form.component';
import { SupportReportComponent } from './support-report/support-report.component';

const routes: Routes = [
  {
    path: '',
    component: TicketListComponent,
    pathMatch: 'full'
  },
  {
    path: 'my-tickets',
    component: TicketListComponent,
    data: { viewMode: 'my-tickets' }
  },
  {
    path: 'all-tickets',
    component: TicketListComponent,
    data: { viewMode: 'all-tickets' }
  },
  {
    path: 'create',
    component: TicketFormComponent
  },
  {
    // Must be registered before ':id' or 'reports' matches the id param
    path: 'reports',
    component: SupportReportComponent
  },
  {
    path: ':id',
    component: TicketDetailComponent
  },
  {
    path: ':id/edit',
    component: TicketFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TechnicalSupportRoutingModule {}
