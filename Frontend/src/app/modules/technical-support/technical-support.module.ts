import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { TechnicalSupportRoutingModule } from './technical-support-routing.module';
import { SharedPipesModule } from '../../shared/pipes/shared-pipes.module';
import { SharedModule } from '../../shared/shared.module';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TechnicalSupportRoutingModule,
    SharedPipesModule,
    SharedModule,
    TranslateModule
  ]
})
export class TechnicalSupportModule {}
