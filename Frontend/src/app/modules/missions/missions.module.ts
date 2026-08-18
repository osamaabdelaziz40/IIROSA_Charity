/**
 * Missions Module
 * Mission management functionality for IIROSA application
 * Access: Admin and Super Admin only (Charity users CANNOT access)
 */

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

// Missions Routing Module
import { MissionsRoutingModule } from './missions-routing.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TranslateModule,
    MissionsRoutingModule
  ]
})
export class MissionsModule { }
