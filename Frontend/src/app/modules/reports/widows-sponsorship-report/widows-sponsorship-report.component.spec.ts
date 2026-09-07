import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { WidowsSponsorshipReportComponent } from './widows-sponsorship-report.component';

describe('WidowsSponsorshipReportComponent', () => {
  let component: WidowsSponsorshipReportComponent;
  let fixture: ComponentFixture<WidowsSponsorshipReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        WidowsSponsorshipReportComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(WidowsSponsorshipReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
