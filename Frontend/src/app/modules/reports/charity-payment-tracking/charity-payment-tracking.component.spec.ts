import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { CharityPaymentTrackingComponent } from './charity-payment-tracking.component';

describe('CharityPaymentTrackingComponent', () => {
  let component: CharityPaymentTrackingComponent;
  let fixture: ComponentFixture<CharityPaymentTrackingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CharityPaymentTrackingComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CharityPaymentTrackingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
