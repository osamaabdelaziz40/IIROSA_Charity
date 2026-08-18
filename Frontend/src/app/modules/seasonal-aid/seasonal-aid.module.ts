import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { SeasonalAidRoutingModule } from './seasonal-aid-routing.module';
import { SeasonalAidService } from './services/seasonal-aid.service';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    SeasonalAidRoutingModule
  ],
  providers: [
    SeasonalAidService
  ]
})
export class SeasonalAidModule {}
