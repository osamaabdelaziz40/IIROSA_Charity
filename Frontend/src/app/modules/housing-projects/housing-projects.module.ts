/**
 * Housing Projects Module
 * Housing project management functionality for IIROSA application
 * Access: Admin and Super Admin only (Charity users CANNOT access)
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
