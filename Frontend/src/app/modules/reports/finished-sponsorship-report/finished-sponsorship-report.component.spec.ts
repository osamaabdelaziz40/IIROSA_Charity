import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { FinishedSponsorshipReportComponent } from './finished-sponsorship-report.component';

describe('FinishedSponsorshipReportComponent', () => {
  let component: FinishedSponsorshipReportComponent;
  let fixture: ComponentFixture<FinishedSponsorshipReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FinishedSponsorshipReportComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FinishedSponsorshipReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('defaults to the finished variant and resolves its i18n prefix', () => {
    expect(component.variant).toBe('finished');
    expect(component.keyPrefix).toBe('reports.finishedSponsorship');
  });
});
