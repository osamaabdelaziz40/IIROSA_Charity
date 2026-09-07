import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { BatchNonRenewedReportsComponent } from './batch-non-renewed-reports.component';

describe('BatchNonRenewedReportsComponent', () => {
  let component: BatchNonRenewedReportsComponent;
  let fixture: ComponentFixture<BatchNonRenewedReportsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        BatchNonRenewedReportsComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BatchNonRenewedReportsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
