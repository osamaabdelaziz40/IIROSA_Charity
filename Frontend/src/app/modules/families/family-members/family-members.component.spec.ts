import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';

import { FamilyMembersComponent } from './family-members.component';
import { NotificationService } from '../../../core/services/notification.service';
import { FamilyService } from '../services/family.service';

describe('FamilyMembersComponent', () => {
  let component: FamilyMembersComponent;
  let fixture: ComponentFixture<FamilyMembersComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FamilyMembersComponent,
        HttpClientTestingModule,
        RouterTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [NotificationService]
    }).compileComponents();

    fixture = TestBed.createComponent(FamilyMembersComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('starts with the move modal closed', () => {
    expect(component.showMoveModal).toBeFalse();
  });

  it('resets modal fields when opening the move modal for an orphan', () => {
    component.moveTargetCode = 'FAM-1';
    component.moveJustification = 'old';
    component.orphans = [{
      id: 'orphan-1',
      code: 'ORP-1',
      fullName: 'Test Orphan'
    } as any];
    component.openMoveModal(component.orphans[0], 1);

    expect(component.showMoveModal).toBeTrue();
    expect(component.moveTargetMemberType).toBe(1);
    expect(component.moveTargetCode).toBe('');
    expect(component.moveJustification).toBe('');
  });

  it('opens the same modal for a guardian (memberType 2)', () => {
    component.openMoveModal({ id: 'provider-1', fullName: 'Test Guardian' }, 2);

    expect(component.showMoveModal).toBeTrue();
    expect(component.moveTargetMemberType).toBe(2);
    expect(component.moveTarget?.fullName).toBe('Test Guardian');
  });

  it('starts with the delete-link modal closed (UC-FAM-13)', () => {
    expect(component.showDeleteModal).toBeFalse();
    expect(component.removing).toBeFalse();
  });

  it('resets the comment when opening the delete-link modal', () => {
    component.deleteComment = 'old reason';
    component.openDeleteModal();

    expect(component.showDeleteModal).toBeTrue();
    expect(component.deleteComment).toBe('');
  });

  it('sends nothing when the delete-link confirm is declined (AC 3)', async () => {
    const notification = TestBed.inject(NotificationService);
    const familyService = TestBed.inject(FamilyService);
    spyOn(notification, 'confirm').and.resolveTo(false);
    const removeSpy = spyOn(familyService, 'removeProviderSponsorLink');

    component.openDeleteModal();
    await component.submitRemoveLink();

    expect(removeSpy).not.toHaveBeenCalled();
    expect(component.showDeleteModal).toBeTrue(); // modal stays open, nothing was sent
  });

  // ==================== load() — HTTP wiring (review 2026-08-24) ====================
  // The routing test bed carries no route params, so ngOnInit never auto-loads here — every
  // test drives load() explicitly and flushes the two GETs it issues (family file, orphans).

  it('loads the family file and its orphans through the member endpoints', () => {
    component.familyId = 'fam-1';
    component.load();

    httpMock
      .expectOne(req => req.method === 'GET' && req.url.endsWith('/api/Families/fam-1'))
      .flush({ id: 'fam-1', code: 'F-001', providerType: 'Other' });
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.endsWith('/api/Families/fam-1/orphans'))
      .flush([{ id: 'orphan-1', code: 'ORP-1', fullName: 'Test Orphan' }]);

    expect(component.family?.id).toBe('fam-1');
    expect(component.family?.providerType).toBe('Other');
    expect(component.orphans.length).toBe(1);
    expect(component.loadError).toBeFalse();
    httpMock.verify();
  });

  it('flags loadError when the family file cannot be read (a read failure is not an empty family)', () => {
    const notification = TestBed.inject(NotificationService);
    spyOn(notification, 'error');

    component.familyId = 'fam-2';
    component.load();

    httpMock
      .expectOne(req => req.method === 'GET' && req.url.endsWith('/api/Families/fam-2'))
      .flush({ message: 'boom' }, { status: 500, statusText: 'Server Error' });
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.endsWith('/api/Families/fam-2/orphans'))
      .flush([]);

    expect(component.loadError).toBeTrue();
    expect(notification.error).toHaveBeenCalled();
    httpMock.verify();
  });

  // ==================== BR-06 ruling 2026-08-24 — guardian move warning ====================

  it('warns about the stranded family when moving the ACTING guardian', async () => {
    const notification = TestBed.inject(NotificationService);
    const familyService = TestBed.inject(FamilyService);
    const confirmSpy = spyOn(notification, 'confirm').and.resolveTo(true);
    const controlSpy = spyOn(familyService, 'controlMember').and.returnValue(of(undefined as any));

    component.family = { id: 'fam-1', providerType: 'Other' } as any;
    component.openMoveModal({ id: 'provider-1', fullName: 'Acting Guardian' }, 2);
    component.moveTargetCode = 'FAM-9';
    await component.submitMove();

    // No translations are loaded in the test bed, so translate.instant echoes the key —
    // asserting on the key proves the warning branch fired instead of the plain confirm.
    expect(confirmSpy).toHaveBeenCalledWith('families.members.moveGuardianConfirm');
    // A target code means ATTACH (legacy action contract: code ⇒ 1, no code ⇒ 0 detach).
    expect(controlSpy).toHaveBeenCalledWith('fam-1', 'provider-1', {
      memberType: 2,
      action: 1,
      targetFamilyCode: 'FAM-9',
      justification: undefined
    });
  });

  it('keeps the plain move confirm for orphans and parent-designated families', async () => {
    const notification = TestBed.inject(NotificationService);
    const familyService = TestBed.inject(FamilyService);
    const confirmSpy = spyOn(notification, 'confirm').and.resolveTo(true);
    const controlSpy = spyOn(familyService, 'controlMember').and.returnValue(of(undefined as any));

    // Father-designated family: the provider row is auxiliary, the seat does not vacate.
    component.family = { id: 'fam-1', providerType: 'Father' } as any;
    component.openMoveModal({ id: 'provider-1', fullName: 'Auxiliary Guardian' }, 2);
    component.moveJustification = 'reason';
    await component.submitMove();

    expect(confirmSpy).toHaveBeenCalledWith('families.members.moveConfirm');
    // No code means DETACH — the justification travels as the reason (action 0).
    expect(controlSpy).toHaveBeenCalledWith('fam-1', 'provider-1', {
      memberType: 2,
      action: 0,
      targetFamilyCode: undefined,
      justification: 'reason'
    });
  });
});
