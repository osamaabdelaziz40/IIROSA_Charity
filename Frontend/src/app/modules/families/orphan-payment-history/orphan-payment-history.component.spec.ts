import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { OrphanPaymentHistoryComponent } from './orphan-payment-history.component';
import { OrphanLookupDto } from '../models/family.model';

describe('OrphanPaymentHistoryComponent', () => {
  let component: OrphanPaymentHistoryComponent;
  let fixture: ComponentFixture<OrphanPaymentHistoryComponent>;

  const orphan: OrphanLookupDto = {
    orphanId: '00000000-0000-0000-0000-000000000001',
    fullName: 'يتيم اختبار'
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        OrphanPaymentHistoryComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrphanPaymentHistoryComponent);
    component = fixture.componentInstance;
    component.orphan = orphan;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
