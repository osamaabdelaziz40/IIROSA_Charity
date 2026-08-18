import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { OrphanPaymentsRoutingModule } from './orphan-payments-routing.module';

// Import shared components
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { LoadingComponent } from '../../shared/components/loading/loading.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { BreadcrumbComponent } from '../../shared/components/breadcrumb/breadcrumb.component';
import { SharedModule } from '../../shared/shared.module';

// Import module components
import { OrphanPaymentListComponent } from './orphan-payment-list/orphan-payment-list.component';
import { OrphanPaymentDetailComponent } from './orphan-payment-detail/orphan-payment-detail.component';
import { OrphanPaymentFormComponent } from './orphan-payment-form/orphan-payment-form.component';
import { AddOrphansToGroupComponent } from './add-orphans-to-group/add-orphans-to-group.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    OrphanPaymentsRoutingModule,
    SharedModule,
    // Standalone components
    OrphanPaymentListComponent,
    OrphanPaymentDetailComponent,
    OrphanPaymentFormComponent,
    AddOrphansToGroupComponent,
    // Shared components
    PageHeaderComponent,
    LoadingComponent,
    PaginationComponent,
    BreadcrumbComponent
  ]
})
export class OrphanPaymentsModule { }
