import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';

import { HqTransferDetailsComponent } from './hq-transfer-details.component';
import { HqTransferService } from '../services/hq-transfer.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

describe('HqTransferDetailsComponent', () => {
  let component: HqTransferDetailsComponent;
  let fixture: ComponentFixture<HqTransferDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HqTransferDetailsComponent, TranslateModule.forRoot()],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: HqTransferService,
          useValue: {
            getTransferDetails: () => of({
              transferId: '00000000-0000-0000-0000-000000000001',
              operationNumber: 'OP-1',
              amountOfPayment: 1000,
              countryName: null,
              lines: []
            })
          }
        },
        { provide: AuthService, useValue: { hasPermission: () => true, isAuthenticated: () => true } },
        { provide: NotificationService, useValue: { success: () => {}, error: () => {} } },
        { provide: ActivatedRoute, useValue: { snapshot: { params: { id: '00000000-0000-0000-0000-000000000001' } } } },
        { provide: Router, useValue: { navigate: () => Promise.resolve(true) } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HqTransferDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
