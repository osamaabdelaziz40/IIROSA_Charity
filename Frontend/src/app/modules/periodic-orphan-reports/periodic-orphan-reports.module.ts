/**
 * Periodic Orphan Reports Module
 * Implements UC-6.1 through UC-6.17 for Periodic Orphan Reports functionality
 *
 * This module provides:
 * - Orphan Reports generation with filters (UC-6.1 through UC-6.10)
 * - Periodic orphan report creation and management (UC-6.11 through UC-6.17)
 * - Report history, export, comparison, and scheduling
 */

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { PeriodicOrphanReportsRoutingModule } from './periodic-orphan-reports-routing.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule,
    PeriodicOrphanReportsRoutingModule
  ]
})
export class PeriodicOrphanReportsModule { }
