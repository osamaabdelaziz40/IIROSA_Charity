/**
 * Reports Module (founded by epic 5, UC-FAM-11)
 * Lazy-loaded reporting vertical under #/reports. Deliberately minimal: each report screen is a
 * standalone component referenced directly by the routing module. Epic 18 (Reports & Printing)
 * extends this module later — this is a host, not a framework.
 */

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsRoutingModule } from './reports-routing.module';

@NgModule({
  imports: [
    CommonModule,
    ReportsRoutingModule
  ]
})
export class ReportsModule { }
