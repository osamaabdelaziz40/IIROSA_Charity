import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { EmployeesRoutingModule } from './employees-routing.module';
import { SharedPipesModule } from '../../shared/pipes/shared-pipes.module';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    EmployeesRoutingModule,
    SharedPipesModule,
    TranslateModule
  ]
})
export class EmployeesModule {}
