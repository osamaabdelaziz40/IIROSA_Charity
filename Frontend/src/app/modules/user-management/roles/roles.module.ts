import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RolesRoutingModule } from './roles-routing.module';
import { SharedModule } from '../../../shared/shared.module';

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RolesRoutingModule,
    SharedModule
  ]
})
export class RolesModule {}
