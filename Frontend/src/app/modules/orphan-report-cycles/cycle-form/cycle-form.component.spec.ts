import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';

import { CycleFormComponent } from './cycle-form.component';
import { NotificationService } from '../../../core/services/notification.service';
import { OrphanReportCycleDto, OrphanReportCycleStatus } from '../models/orphan-report-cycle.model';

const storedCycle: OrphanReportCycleDto = {
  id: 'cycle-1',
  titleAr: 'دورة الربع الأول',
  titleEn: 'Q1 Cycle',
  descriptionAr: 'وصف',
  startDate: '2026-01-01T00:00:00Z',
  endDate: '2026-03-31T00:00:00Z',
  status: OrphanReportCycleStatus.InProgress,
  reportsCount: 10,
  codedOrphansCount: 40,
  progressPercent: 25,
  createdOn: '2025-12-01T00:00:00Z'
};

describe('CycleFormComponent (UC-ORC-02/03)', () => {
  let component: CycleFormComponent;
  let fixture: ComponentFixture<CycleFormComponent>;
  let httpMock: HttpTestingController;
  let router: Router;

  function create(id?: string): void {
    fixture = TestBed.createComponent(CycleFormComponent);
    // Snapshot stub — the real ActivatedRoute carries no :id in the routing test bed.
    TestBed.inject(ActivatedRoute).snapshot = { paramMap: { get: () => id ?? null } } as any;
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CycleFormComponent,
        HttpClientTestingModule,
        RouterTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        NotificationService,
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } } }
      ]
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => httpMock.verify());

  it('should create in create mode with an empty form', () => {
    create();
    expect(component.isEditMode).toBeFalse();
    expect(component.cycleForm.get('titleAr')?.value).toBe('');
  });

  it('loads the cycle and patches local date parts in edit mode', () => {
    create('cycle-1');

    httpMock
      .expectOne(req => req.method === 'GET' && req.url.endsWith('/api/OrphanReportCycles/cycle-1'))
      .flush(storedCycle);

    expect(component.isEditMode).toBeTrue();
    expect(component.cycleForm.get('startDate')?.value).toBe('2026-01-01');
    expect(component.cycleForm.get('endDate')?.value).toBe('2026-03-31');
    expect(component.readOnly).toBeFalse();
  });

  it('opens read-only for a completed cycle (BR-ORC-08)', () => {
    create('cycle-1');
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.endsWith('/api/OrphanReportCycles/cycle-1'))
      .flush({ ...storedCycle, status: OrphanReportCycleStatus.Completed });

    expect(component.readOnly).toBeTrue();
  });

  it('refuses to save when the end date precedes the start', () => {
    create();
    const warning = spyOn(TestBed.inject(NotificationService), 'warning');
    spyOn(router, 'navigate');

    component.cycleForm.patchValue({
      titleAr: 'دورة', startDate: '2026-03-01', endDate: '2026-01-01'
    });
    component.save();

    expect(warning).toHaveBeenCalledWith('orphanReportCycles.form.endAfterStart');
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('posts the payload on create and returns to the register', () => {
    create();
    spyOn(router, 'navigate');

    component.cycleForm.patchValue({
      titleAr: 'دورة جديدة', startDate: '2026-01-01', endDate: '2026-03-31'
    });
    component.save();

    const req = httpMock.expectOne(r => r.method === 'POST' && r.url.endsWith('/api/OrphanReportCycles'));
    expect(req.request.body.titleAr).toBe('دورة جديدة');
    req.flush(storedCycle);

    expect(router.navigate).toHaveBeenCalledWith(['/orphan-report-cycles']);
  });

  it('puts the payload with the route id on edit', () => {
    create('cycle-1');
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.endsWith('/api/OrphanReportCycles/cycle-1'))
      .flush(storedCycle);
    spyOn(router, 'navigate');

    component.cycleForm.patchValue({ titleAr: 'دورة معدلة' });
    component.save();

    const req = httpMock.expectOne(r => r.method === 'PUT' && r.url.endsWith('/api/OrphanReportCycles/cycle-1'));
    expect(req.request.body.id).toBe('cycle-1');
    expect(req.request.body.titleAr).toBe('دورة معدلة');
    req.flush(storedCycle);
  });

  it('maps PascalCase server errors onto the camelCase controls', () => {
    create();
    const error = spyOn(TestBed.inject(NotificationService), 'error');

    component.cycleForm.patchValue({ titleAr: 'x', startDate: '2026-01-01', endDate: '2026-03-31' });
    component.save();

    httpMock
      .expectOne(r => r.method === 'POST')
      .flush({ message: 'invalid', errors: { TitleAr: ['required'] } }, { status: 400, statusText: 'Bad' });

    expect(component.serverErrors['titleAr']).toBe('required');
    expect(error).toHaveBeenCalled();
  });
});
