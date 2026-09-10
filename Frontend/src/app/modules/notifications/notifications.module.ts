/**
 * Notifications Module (UC-NTF web notifications epic)
 * The list screen is every user's; composing/pushing is Admin/SuperAdmin only.
 */

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { NotificationsRoutingModule } from './notifications-routing.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TranslateModule,
    NotificationsRoutingModule
  ]
})
export class NotificationsModule { }
