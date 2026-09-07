import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

// Import existing components
import { IncomingLettersListComponent } from './incoming-letters/incoming-letters-list.component';
import { OutgoingLettersListComponent } from './outgoing-letters/outgoing-letters-list.component';
import { IncomingLetterFormComponent } from './incoming-letters/incoming-letter-form.component';
import { IncomingLetterDetailComponent } from './incoming-letters/incoming-letter-detail.component';
import { IncomingLetterEmployeesComponent } from './incoming-employees/incoming-letter-employees.component';
import { OutgoingLetterFormComponent } from './outgoing-letters/outgoing-letter-form.component';
import { OutgoingLetterDetailComponent } from './outgoing-letters/outgoing-letter-detail.component';
import { OutgoingLetterOrphansComponent } from './outgoing-orphans/outgoing-letter-orphans.component';
import { OutgoingOrphansReportComponent } from './outgoing-orphans-report/outgoing-orphans-report.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'incoming',
    pathMatch: 'full',
    data: { title: 'incomingOutgoing.incomingLetters' }
  },
  // Incoming Letters Routes
  {
    path: 'incoming',
    component: IncomingLettersListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.incomingLetters',
      permission: 'IncomingOutgoing.View'
    }
  },
  {
    path: 'incoming/create',
    component: IncomingLetterFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.addIncomingLetter',
      permission: 'IncomingOutgoing.Create'
    }
  },
  {
    path: 'incoming/:id',
    component: IncomingLetterDetailComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.incomingLetterDetails',
      permission: 'IncomingOutgoing.View'
    }
  },
  {
    path: 'incoming/:id/edit',
    component: IncomingLetterFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.editIncomingLetter',
      permission: 'IncomingOutgoing.Edit'
    }
  },
  // Outgoing Letters Routes
  {
    path: 'outgoing',
    component: OutgoingLettersListComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.outgoingLetters',
      permission: 'IncomingOutgoing.View'
    }
  },
  {
    path: 'outgoing/create',
    component: OutgoingLetterFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.addOutgoingLetter',
      permission: 'IncomingOutgoing.Create'
    }
  },
  {
    path: 'outgoing/:id',
    component: OutgoingLetterDetailComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.outgoingLetterDetails',
      permission: 'IncomingOutgoing.View'
    }
  },
  {
    path: 'outgoing/:id/edit',
    component: OutgoingLetterFormComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.editOutgoingLetter',
      permission: 'IncomingOutgoing.Edit'
    }
  },
  // Employee attachment (UC-COR-09 / §21.S.3) — the legacy export-wizard route
  // re-pointed at the correspondence screen it was always meant to serve.
  {
    path: 'export/incoming',
    component: IncomingLetterEmployeesComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.attachEmployeesTitle',
      permission: 'IncomingOutgoing.Edit'
    }
  },
  // Orphan-report attachment (UC-COR-18 / §21.S.6) — the second wizard route re-pointed.
  {
    path: 'export/outgoing',
    component: OutgoingLetterOrphansComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.attachOrphansTitle',
      permission: 'IncomingOutgoing.Edit'
    }
  },
  // Orphans-by-letter report (UC-COR-19 / §21.S.7)
  {
    path: 'export/outgoing-orphans',
    component: OutgoingOrphansReportComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.orphansReportTitle',
      permission: 'IncomingOutgoing.View'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class IncomingOutgoingRoutingModule { }
