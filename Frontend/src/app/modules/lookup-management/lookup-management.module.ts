import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LookupManagementRoutingModule } from './lookup-routing.module';
import { LookupManagementService } from './services/lookup-management.service';

@NgModule({
  imports: [
    CommonModule,
    LookupManagementRoutingModule
  ],
  providers: [
    LookupManagementService
  ]
})
export class LookupManagementModule { }
