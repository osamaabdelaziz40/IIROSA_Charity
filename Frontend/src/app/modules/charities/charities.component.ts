import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-charities',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="page-header">
      <h3>{{ 'charities.title' | translate }}</h3>
    </div>
    <div class="alert alert-info">
      Charities module - Coming Soon
    </div>
  `
})
export class CharitiesComponent {}
