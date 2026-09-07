import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';

import { MaxTransferAmountComponent } from './max-transfer-amount.component';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { of } from 'rxjs';

describe('MaxTransferAmountComponent', () => {
  let component: MaxTransferAmountComponent;
  let fixture: ComponentFixture<MaxTransferAmountComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MaxTransferAmountComponent, TranslateModule.forRoot()],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: { hasPermission: () => true, isAuthenticated: () => true } },
        { provide: NotificationService, useValue: { success: () => {}, error: () => {} } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MaxTransferAmountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
