/**
 * HQ Financial Transfers Module (epic 17)
 * HQ financial transfer management — Admin and Super Admin only; reads are scoped
 * server-side to the caller's country claim.
 */

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

// HQ Transfers Routing Module
import { HqTransfersRoutingModule } from './hq-transfers-routing.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TranslateModule,
    HqTransfersRoutingModule
  ]
})
export class HqTransfersModule { }
