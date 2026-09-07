import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ProviderRequestListComponent } from './provider-request-list.component';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { GuardianChangeRequestRow } from '../models/family.model';

function pendingRow(): GuardianChangeRequestRow {
  return {
    id: 'req-1',
    familyId: 'fam-1',
    newGuardianName: 'معيل جديد',
    newGuardianNationalId: '29901011234567',
    relationship: 'عم',
    reason: 'وفاة العائل',
    requestedByName: 'user',
    createdOn: '2026-08-24T00:00:00Z',
    status: 1
  } as GuardianChangeRequestRow;
}

describe('ProviderRequestListComponent', () => {
  let component: ProviderRequestListComponent;
  let fixture: ComponentFixture<ProviderRequestListComponent>;
  let httpMock: HttpTestingController;

  /** ngOnInit auto-fires the queue GET plus (HQ stub) the charities options GET — flush both
   *  before driving a scenario so the test's own requests are the only ones pending. */
  function flushInitialLoads(): void {
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.includes('/Families/provider-requests'))
      .flush({ items: [], totalCount: 0 });
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.includes('/api/Charities'))
      .flush({ items: [] });
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ProviderRequestListComponent,
        HttpClientTestingModule,
        RouterTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        NotificationService,
        { provide: AuthService, useValue: { hasAnyRole: () => true, hasPermission: () => true } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProviderRequestListComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('opens on the pending queue view', () => {
    expect(component.statusFilter).toBe(1);
    expect(component.currentPage).toBe(1);
  });

  it('grants the decision action to the General Director (SuperAdmin) only', () => {
    // The stub authorizes every role — the queue itself admits HQ, the decision admits SuperAdmin.
    expect(component.canDecide).toBe(true);
  });

  it('opens the decision modal on a pending request with approve pre-selected', () => {
    component.openDecision({
      id: 'req-1',
      familyId: 'fam-1',
      newGuardianName: 'معيل جديد',
      newGuardianNationalId: '29901011234567',
      relationship: 'عم',
      reason: 'وفاة العائل',
      requestedByName: 'user',
      createdOn: '2026-08-24T00:00:00Z',
      status: 1
    } as any);

    expect(component.decisionModalOpen).toBe(true);
    expect(component.decisionTarget?.id).toBe('req-1');
    expect(component.decisionIsApproved).toBe(true);
    expect(component.decisionReason).toBe('');
  });

  it('refuses to record a refusal without a reason (AC 3 client mirror)', async () => {
    component.openDecision({
      id: 'req-1',
      familyId: 'fam-1',
      newGuardianName: 'معيل جديد',
      newGuardianNationalId: '29901011234567',
      relationship: 'عم',
      reason: 'وفاة العائل',
      requestedByName: 'user',
      createdOn: '2026-08-24T00:00:00Z',
      status: 1
    } as any);
    component.decisionIsApproved = false;
    component.decisionReason = '   ';

    const notification = TestBed.inject(NotificationService);
    spyOn(notification, 'error');
    await component.submitDecision();

    expect(notification.error).toHaveBeenCalled();
    expect(component.decisionModalOpen).toBe(true);
  });

  // ==================== queue load + post-decision refresh — HTTP wiring (review 2026-08-24) ====================

  it('loads the pending queue through the provider-requests endpoint', () => {
    flushInitialLoads();
    component.load();

    httpMock
      .expectOne(req => req.method === 'GET' && req.url.includes('/Families/provider-requests'))
      .flush({ items: [pendingRow()], totalCount: 1 });

    expect(component.requests.length).toBe(1);
    expect(component.totalCount).toBe(1);
    expect(component.loading).toBeFalse();
    httpMock.verify();
  });

  it('refreshes the queue after a decision lands (5-10 post-decision re-fetch)', async () => {
    flushInitialLoads();
    const notification = TestBed.inject(NotificationService);
    spyOn(notification, 'confirm').and.resolveTo(true);
    spyOn(notification, 'success'); // keep the real toasts out of the test browser

    const row = pendingRow();
    component.requests = [row];
    component.openDecision(row);
    await component.submitDecision();

    const decide = httpMock.expectOne(
      req => req.method === 'POST' && req.url.includes('/Families/provider-requests/req-1/approve')
    );
    expect(decide.request.body).toEqual({ isApproved: true });
    decide.flush({});

    httpMock
      .expectOne(req => req.method === 'GET' && req.url.includes('/Families/provider-requests'))
      .flush({ items: [], totalCount: 0 });

    expect(component.decisionModalOpen).toBeFalse();
    expect(component.requests.length).toBe(0); // the decided row left the refreshed pending view
    httpMock.verify();
  });

  it('pulls the page back in range when the last row of a page is decided', async () => {
    flushInitialLoads();
    const notification = TestBed.inject(NotificationService);
    spyOn(notification, 'confirm').and.resolveTo(true);
    spyOn(notification, 'success');

    const row = pendingRow();
    component.currentPage = 2;
    component.requests = [row];
    component.openDecision(row);
    await component.submitDecision();

    httpMock
      .expectOne(req => req.method === 'POST' && req.url.includes('/Families/provider-requests/req-1/approve'))
      .flush({});
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.includes('/Families/provider-requests'))
      .flush({ items: [], totalCount: 0 });

    expect(component.currentPage).toBe(1);
    httpMock.verify();
  });
});
