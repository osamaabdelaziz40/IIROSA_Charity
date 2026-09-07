import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';

import { RefugeeFamilyDetailComponent } from './refugee-family-detail.component';
import { FamilyService } from '../services/family.service';
import { LookupManagementService } from '../../lookup-management/services/lookup-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

describe('RefugeeFamilyDetailComponent', () => {
  let component: RefugeeFamilyDetailComponent;
  let fixture: ComponentFixture<RefugeeFamilyDetailComponent>;
  let familyServiceSpy: jasmine.SpyObj<FamilyService>;
  let lookupServiceSpy: jasmine.SpyObj<LookupManagementService>;
  let authSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  const family: any = {
    id: 'f-1',
    code: 'REF-001',
    familyType: 'Refugee',
    address: 'خيطان',
    cityVillage: 'الخزندارية',
    regionName: 'دمشق',
    centerName: 'المزة',
    houseOwnershipName: 'إيجار',
    houseStatusName: 'جيدة',
    housingTypeName: 'شقة',
    incomeTypeName: 'عمل',
    rentAmount: 150
  };
  const provider: any = { id: 'p-1', fullName: 'سعاد محمد', isAlive: true };
  const orphans: any[] = [{ id: 'o-1', fullName: 'طفل لاجئ', gender: 'ذكر', dateOfBirth: '2016-04-01T00:00:00Z' }];
  const relatives: any[] = [{ id: 'r-1', fullName: 'مرافق لاجئ', gender: 'أنثى', dateOfBirth: '1990-02-02T00:00:00Z', notes: 'خالة' }];

  beforeEach(async () => {
    familyServiceSpy = jasmine.createSpyObj<FamilyService>('FamilyService',
      ['getFamily', 'getProvider', 'getFamilyOrphans', 'getRelatives']);
    lookupServiceSpy = jasmine.createSpyObj<LookupManagementService>('LookupManagementService',
      ['getCountries']);
    lookupServiceSpy.getCountries.and.returnValue(of({ items: [] } as any));
    authSpy = jasmine.createSpyObj<AuthService>('AuthService', ['hasPermission']);
    routerSpy = jasmine.createSpyObj<Router>('Router', ['navigate']);
    const notificationSpy = jasmine.createSpyObj<NotificationService>('NotificationService',
      ['error', 'success', 'warning']);

    await TestBed.configureTestingModule({
      imports: [RefugeeFamilyDetailComponent, TranslateModule.forRoot()],
      providers: [
        { provide: FamilyService, useValue: familyServiceSpy },
        { provide: LookupManagementService, useValue: lookupServiceSpy },
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: routerSpy },
        { provide: NotificationService, useValue: notificationSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: (key: string) => (key === 'id' ? 'f-1' : null) } } }
        }
      ]
    }).overrideComponent(RefugeeFamilyDetailComponent, { set: { changeDetection: 0 } })
      .compileComponents();

    fixture = TestBed.createComponent(RefugeeFamilyDetailComponent);
    component = fixture.componentInstance;
  });

  const emitSuccessfulLoad = (): void => {
    familyServiceSpy.getFamily.and.returnValue(of(family));
    familyServiceSpy.getProvider.and.returnValue(of(provider));
    familyServiceSpy.getFamilyOrphans.and.returnValue(of(orphans));
    familyServiceSpy.getRelatives.and.returnValue(of(relatives));
    authSpy.hasPermission.and.returnValue(true);
  }

  it('should create', () => {
    emitSuccessfulLoad();
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('loads household, provider, orphans and relatives together (UC-REF-04 read)', () => {
    emitSuccessfulLoad();
    fixture.detectChanges();
    expect(familyServiceSpy.getFamily).toHaveBeenCalledWith('f-1');
    expect(familyServiceSpy.getProvider).toHaveBeenCalledWith('f-1');
    expect(familyServiceSpy.getFamilyOrphans).toHaveBeenCalledWith('f-1');
    expect(familyServiceSpy.getRelatives).toHaveBeenCalledWith('f-1');
    expect(component.family?.code).toBe('REF-001');
    expect(component.provider?.fullName).toBe('سعاد محمد');
    expect(component.orphans.length).toBe(1);
    expect(component.relatives.length).toBe(1);
    expect(component.loading).toBeFalse();
  });

  it('renders the cheque-grid section wired-but-empty (epic-10 gap)', () => {
    emitSuccessfulLoad();
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.textContent).toContain('families.refugeeChequeEmpty');
    expect(el.querySelectorAll('table').length).toBeGreaterThanOrEqual(3);
  });

  it('shows تعديل اعضاء الاسرة only with Families.Edit', () => {
    emitSuccessfulLoad();
    fixture.detectChanges();
    expect(component.canEdit).toBeTrue();

    authSpy.hasPermission.and.returnValue(false);
    component.ngOnInit();
    expect(component.canEdit).toBeFalse();
  });

  it('edit() navigates to refugees/:id/edit', () => {
    emitSuccessfulLoad();
    fixture.detectChanges();
    component.edit();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/families/refugees', 'f-1', 'edit']);
  });

  it('a failed load surfaces an error and stops the spinner', () => {
    familyServiceSpy.getFamily.and.returnValue(throwError(() => new Error('403')));
    familyServiceSpy.getProvider.and.returnValue(of(provider));
    familyServiceSpy.getFamilyOrphans.and.returnValue(of([]));
    familyServiceSpy.getRelatives.and.returnValue(of([]));
    authSpy.hasPermission.and.returnValue(false);
    fixture.detectChanges();
    expect(component.loading).toBeFalse();
    expect(component.family).toBeUndefined();
  });
});
