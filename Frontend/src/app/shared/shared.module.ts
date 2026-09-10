import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';

// Standalone Components - to be re-exported
import { PageHeaderComponent } from './components/page-header/page-header.component';
import { LoadingComponent } from './components/loading/loading.component';
import { PaginationComponent } from './components/pagination/pagination.component';
import { BreadcrumbComponent } from './components/breadcrumb/breadcrumb.component';
import { SessionExtensionDialogComponent } from './components/session-extension-dialog/session-extension-dialog.component';
import { SignalrToastComponent } from './components/signalr-toast/signalr-toast.component';
import { AttachmentInputComponent } from './components/attachment-input/attachment-input.component';
import { DropDownComponent } from './components/drop-down/drop-down.component';
import { CollapsibleCardComponent } from './components/collapsible-card/collapsible-card.component';

// Traditional Components - need to be declared
import { InputTextComponent } from './components/text-input/text-input.component';

// Impersonation Module
import { ImpersonationModule } from './impersonation.module';

// Re-export types
export * from './models';

@NgModule({
  declarations: [
    InputTextComponent
  ],
  imports: [
    CommonModule,
    TranslateModule.forChild(),
    ReactiveFormsModule,
    BsDatepickerModule.forRoot(),
    // Import standalone components for re-export
    PageHeaderComponent,
    LoadingComponent,
    PaginationComponent,
    BreadcrumbComponent,
    SessionExtensionDialogComponent,
    SignalrToastComponent,
    AttachmentInputComponent,
    DropDownComponent,
    CollapsibleCardComponent,
    // Impersonation Module
    ImpersonationModule
  ],
  exports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    ReactiveFormsModule,
    BsDatepickerModule,
    // Re-export standalone components
    PageHeaderComponent,
    LoadingComponent,
    PaginationComponent,
    BreadcrumbComponent,
    SessionExtensionDialogComponent,
    SignalrToastComponent,
    AttachmentInputComponent,
    DropDownComponent,
    CollapsibleCardComponent,
    // Export traditional components
    InputTextComponent,
    // Re-export impersonation components
    ImpersonationModule
  ]
})
export class SharedModule { }
