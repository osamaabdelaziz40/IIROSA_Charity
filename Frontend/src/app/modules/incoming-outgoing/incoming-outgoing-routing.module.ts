import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';
import { PermissionGuard } from '../../core/guards/permission.guard';

// Import existing components
import { IncomingLettersListComponent } from './incoming-letters/incoming-letters-list.component';
import { OutgoingLettersListComponent } from './outgoing-letters/outgoing-letters-list.component';
import { ImportWizardComponent } from './import-wizard/import-wizard.component';
import { ExportWizardComponent } from './export-wizard/export-wizard.component';
import { HistoryComponent } from './history/history.component';
import { IncomingLetterFormComponent } from './incoming-letters/incoming-letter-form.component';
import { IncomingLetterDetailComponent } from './incoming-letters/incoming-letter-detail.component';
import { OutgoingLetterFormComponent } from './outgoing-letters/outgoing-letter-form.component';
import { OutgoingLetterDetailComponent } from './outgoing-letters/outgoing-letter-detail.component';

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
  // Import/Export Routes
  {
    path: 'import/:type',
    component: ImportWizardComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.importLetters',
      permission: 'IncomingOutgoing.Import'
    }
  },
  {
    path: 'export/:type',
    component: ExportWizardComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.exportLetters',
      permission: 'IncomingOutgoing.Export'
    }
  },
  {
    path: 'history',
    component: HistoryComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: {
      title: 'incomingOutgoing.history',
      permission: 'IncomingOutgoing.ViewHistory'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class IncomingOutgoingRoutingModule { }
