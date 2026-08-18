import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { GeneralChecksRoutingModule } from './general-checks-routing.module';

// Services
import { GeneralChecksService } from './services/general-checks.service';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    GeneralChecksRoutingModule
  ],
  providers: [
    GeneralChecksService
  ]
})
export class GeneralChecksModule {}
