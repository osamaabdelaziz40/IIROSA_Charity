import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { IncomingOutgoingRoutingModule } from './incoming-outgoing-routing.module';

// Import components
import { IncomingLettersListComponent } from './incoming-letters/incoming-letters-list.component';
import { IncomingLetterFormComponent } from './incoming-letters/incoming-letter-form.component';
import { IncomingLetterDetailComponent } from './incoming-letters/incoming-letter-detail.component';
import { IncomingLetterEmployeesComponent } from './incoming-employees/incoming-letter-employees.component';
import { OutgoingLettersListComponent } from './outgoing-letters/outgoing-letters-list.component';
import { OutgoingLetterFormComponent } from './outgoing-letters/outgoing-letter-form.component';
import { OutgoingLetterDetailComponent } from './outgoing-letters/outgoing-letter-detail.component';
import { OutgoingLetterOrphansComponent } from './outgoing-orphans/outgoing-letter-orphans.component';
import { OutgoingOrphansReportComponent } from './outgoing-orphans-report/outgoing-orphans-report.component';

@NgModule({
  declarations: [
    // Standalone components cannot be declared here - they are now imported below
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    IncomingOutgoingRoutingModule,
    // Import standalone components
    IncomingLettersListComponent,
    IncomingLetterFormComponent,
    IncomingLetterDetailComponent,
    IncomingLetterEmployeesComponent,
    OutgoingLettersListComponent,
    OutgoingLetterFormComponent,
    OutgoingLetterDetailComponent,
    OutgoingLetterOrphansComponent,
    OutgoingOrphansReportComponent
  ]
})
export class IncomingOutgoingModule { }
