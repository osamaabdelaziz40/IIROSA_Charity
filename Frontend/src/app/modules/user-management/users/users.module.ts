import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { UsersRoutingModule } from './users-routing.module';
import { SharedModule } from '../../../shared/shared.module';

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    UsersRoutingModule,
    SharedModule
  ]
})
export class UsersModule {}
