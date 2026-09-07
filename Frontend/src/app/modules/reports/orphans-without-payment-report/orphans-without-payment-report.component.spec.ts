import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { OrphansWithoutPaymentReportComponent } from './orphans-without-payment-report.component';

describe('OrphansWithoutPaymentReportComponent', () => {
  let component: OrphansWithoutPaymentReportComponent;
  let fixture: ComponentFixture<OrphansWithoutPaymentReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        OrphansWithoutPaymentReportComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrphansWithoutPaymentReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
