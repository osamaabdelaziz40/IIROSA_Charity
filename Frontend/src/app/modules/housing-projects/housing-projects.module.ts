/**
 * Housing Projects Module (epic 6, chapter 11)
 * The housing-FAMILY register — families housed in organisation-owned buildings.
 * Access: Charity + HQ (Admin/SuperAdmin). The pre-re-cut header barred Charity
 * users; that described the deleted construction tracker, not this register.
 */

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

// Housing Projects Routing Module
import { HousingProjectsRoutingModule } from './housing-projects-routing.module';

// Shared Module
import { SharedModule } from '../../shared/shared.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule,
    SharedModule,
    HousingProjectsRoutingModule
  ]
})
export class HousingProjectsModule { }
