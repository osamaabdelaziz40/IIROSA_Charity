import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbModalModule } from '@ng-bootstrap/ng-bootstrap';
import { ImpersonationBannerComponent } from './impersonation-banner/impersonation-banner.component';
import { EndImpersonationConfirmComponent } from './impersonation-dialogs/end-impersonation-confirm.component';
import { QuickImpersonationDialogComponent } from './impersonation-dialogs/quick-impersonation-dialog.component';
import { ImpersonationConfirmDialogComponent } from './impersonation-dialogs/impersonation-confirm-dialog.component';
import { ActiveImpersonationSessionsComponent } from './impersonation-sessions/active-impersonation-sessions.component';
import { TerminateSessionConfirmComponent } from './impersonation-sessions/terminate-session-confirm.component';
import { ImpersonationHistoryComponent } from './impersonation-sessions/impersonation-history.component';

@NgModule({
  declarations: [
    ImpersonationBannerComponent,
    EndImpersonationConfirmComponent,
    QuickImpersonationDialogComponent,
    ImpersonationConfirmDialogComponent,
    ActiveImpersonationSessionsComponent,
    TerminateSessionConfirmComponent,
    ImpersonationHistoryComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgbModalModule
  ],
  exports: [
    ImpersonationBannerComponent,
    EndImpersonationConfirmComponent,
    QuickImpersonationDialogComponent,
    ImpersonationConfirmDialogComponent,
    ActiveImpersonationSessionsComponent,
    TerminateSessionConfirmComponent,
    ImpersonationHistoryComponent
  ]
})
export class ImpersonationModule { }
