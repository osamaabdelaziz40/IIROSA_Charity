import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { OrphanReportCyclesRoutingModule } from './orphan-report-cycles-routing.module';

/**
 * Orphan Report Cycles (دورات التقارير الدورية للأيتام) — UC-ORC-01…09,
 * docs/Modules/26-UC-ORC. This module wires the HQ register/form routes; its screens
 * are standalone components. The CycleReportChooserComponent is NOT declared here —
 * hosts import it directly (standalone), like the shared DropDownComponent.
 */
@NgModule({
  imports: [CommonModule, OrphanReportCyclesRoutingModule]
})
export class OrphanReportCyclesModule {}
