import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { MezaCardsReportComponent } from './meza-cards-report.component';

describe('MezaCardsReportComponent', () => {
  let component: MezaCardsReportComponent;
  let fixture: ComponentFixture<MezaCardsReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MezaCardsReportComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MezaCardsReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
