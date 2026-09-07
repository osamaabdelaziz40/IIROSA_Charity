import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';

import { RefugeeFamilyFormComponent } from './refugee-family-form.component';
import { FamilyService } from '../services/family.service';
import { NotificationService } from '../../../core/services/notification.service';

describe('RefugeeFamilyFormComponent', () => {
  let component: RefugeeFamilyFormComponent;
  let fixture: ComponentFixture<RefugeeFamilyFormComponent>;
  let familyService: FamilyService;
  let notification: NotificationService;
  // ActivatedRoute stub — set to a family id to mount the component in edit mode (UC-REF-04)
  let routeId: string | null = null;

  beforeEach(async () => {
    routeId = null;
    await TestBed.configureTestingModule({
      imports: [
        RefugeeFamilyFormComponent,
        HttpClientTestingModule,
        RouterTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        NotificationService,
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: (_key: string) => routeId } } }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RefugeeFamilyFormComponent);
    component = fixture.componentInstance;
    familyService = TestBed.inject(FamilyService);
    notification = TestBed.inject(NotificationService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('starts with empty اضافة ابن / اضافة مرافق staging arrays on the family form', () => {
    expect(component.childrenForm.length).toBe(0);
    expect(component.companionsForm.length).toBe(0);
    // The template binds formArrayName="orphans"/"relatives" — the arrays must live on familyForm
    expect(component.familyForm.get('orphans')).toBe(component.childrenForm);
    expect(component.familyForm.get('relatives')).toBe(component.companionsForm);
  });

  it('adds and removes child rows', () => {
    component.addChildRow();
    component.addChildRow();
    expect(component.childrenForm.length).toBe(2);

    component.removeChildRow(0);
    expect(component.childrenForm.length).toBe(1);
    expect(component.childGroup(0).get('gender')!.value).toBe('ذكر');
  });

  it('collects the server-mandatory gender and dateOfBirth on companion rows', () => {
    // CreateRelativeDto is [Required] on both — the row must stage them, not the payload fake them
    component.addCompanionRow();
    const row = component.companionGroup(0);
    expect(row.get('gender')!.value).toBe('ذكر');
    expect(row.get('gender')!.validator).toBeTruthy();
    expect(row.get('dateOfBirth')!.validator).toBeTruthy();

    component.removeCompanionRow(0);
    expect(component.companionsForm.length).toBe(0);
  });

  it('clears the centre and reloads its options when the region changes', () => {
    component.familyForm.get('centerId')!.setValue(7);
    spyOn(component, 'loadCenters');

    component.familyForm.get('regionId')!.setValue(3);

    expect(component.familyForm.get('centerId')!.value).toBeNull();
    expect(component.loadCenters).toHaveBeenCalledWith(3);
  });

  it('refuses to submit while mandatory §12.S.2 fields are missing', () => {
    const warningSpy = spyOn(notification, 'warning');
    const createSpy = spyOn(familyService, 'createFamily');

    component.save();

    expect(warningSpy).toHaveBeenCalled();
    expect(createSpy).not.toHaveBeenCalled();
    expect(component.saving).toBeFalse();
  });

  it('does not schedule member deletes in create mode (rows carry no id)', () => {
    component.addChildRow();
    component.addCompanionRow();
    component.removeChildRow(0);
    component.removeCompanionRow(0);
    expect(component.deletedOrphanIds.length).toBe(0);
    expect(component.deletedRelativeIds.length).toBe(0);
  });

  it('mounts in edit mode on :id, loads the members, and tracks staged removals (UC-REF-04)', () => {
    routeId = 'f-1';
    spyOn(familyService, 'getFamily').and.returnValue(of({
      id: 'f-1', familyType: 'Refugee', regionId: 3, centerId: 9, address: 'خيطان', cityVillage: 'الخزندارية'
    } as any));
    spyOn(familyService, 'getProvider').and.returnValue(of({
      id: 'p-1', fullName: 'سعاد محمد', relationshipToFamily: 'أم', isAlive: true
    } as any));
    spyOn(familyService, 'getFamilyOrphans').and.returnValue(of([
      { id: 'o-1', fullName: 'طفل لاجئ', gender: 'ذكر', dateOfBirth: '2016-04-01T00:00:00Z', socialStatusId: 2 } as any
    ]));
    spyOn(familyService, 'getRelatives').and.returnValue(of([
      { id: 'r-1', fullName: 'مرافق لاجئ', gender: 'أنثى', dateOfBirth: '1990-02-02T00:00:00Z', notes: 'خالة' } as any
    ]));

    const editFixture = TestBed.createComponent(RefugeeFamilyFormComponent);
    const editComponent = editFixture.componentInstance;
    editFixture.detectChanges();

    expect(editComponent.editMode).toBeTrue();
    expect(editComponent.familyId).toBe('f-1');
    expect(editComponent.formTitleKey).toBe('families.refugeeEditTitle');

    // Household + members loaded from the per-member GET endpoints
    expect(familyService.getFamily).toHaveBeenCalledWith('f-1');
    expect(familyService.getProvider).toHaveBeenCalledWith('f-1');
    expect(familyService.getFamilyOrphans).toHaveBeenCalledWith('f-1');
    expect(familyService.getRelatives).toHaveBeenCalledWith('f-1');
    expect(editComponent.familyForm.get('address')!.value).toBe('خيطان');
    expect(editComponent.childrenForm.length).toBe(1);
    expect(editComponent.childGroup(0).get('id')!.value).toBe('o-1');
    expect(editComponent.companionsForm.length).toBe(1);
    expect(editComponent.companionGroup(0).get('relationship')!.value).toBe('خالة');

    // The region patch is silent — the cascade must not have wiped the loaded centre
    expect(editComponent.familyForm.get('centerId')!.value).toBe(9);

    // Staged removals in edit mode queue server-side deletes, not just row removals
    editComponent.removeChildRow(0);
    editComponent.removeCompanionRow(0);
    expect(editComponent.deletedOrphanIds).toEqual(['o-1']);
    expect(editComponent.deletedRelativeIds).toEqual(['r-1']);
  });
});
