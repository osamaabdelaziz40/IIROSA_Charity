import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';

import { CycleListComponent } from './cycle-list.component';
import { NotificationService } from '../../../core/services/notification.service';
import { OrphanReportCycleDto, OrphanReportCycleStatus } from '../models/orphan-report-cycle.model';

const cycle = (over: Partial<OrphanReportCycleDto>): OrphanReportCycleDto => ({
  id: 'cycle-1',
  titleAr: 'دورة الربع الأول',
  startDate: '2026-01-01T00:00:00Z',
  endDate: '2026-03-31T00:00:00Z',
  status: OrphanReportCycleStatus.InProgress,
  reportsCount: 10,
  codedOrphansCount: 40,
  progressPercent: 25,
  createdOn: '2025-12-01T00:00:00Z',
  ...over
});

describe('CycleListComponent (UC-ORC-01)', () => {
  let component: CycleListComponent;
  let fixture: ComponentFixture<CycleListComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CycleListComponent,
        HttpClientTestingModule,
        RouterTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [NotificationService]
    }).compileComponents();

    fixture = TestBed.createComponent(CycleListComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  const flushCycles = (items: OrphanReportCycleDto[]): void => {
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.endsWith('/api/OrphanReportCycles'))
      .flush({ items, totalCount: items.length, page: 1, pageSize: 10 });
  };

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads the register with the default paging (no status sent for the all sentinel)', () => {
    flushCycles([cycle({})]);

    expect(component.cycles.length).toBe(1);
    expect(component.totalCount).toBe(1);
    expect(component.loading).toBeFalse();
  });

  it('maps the status dropdown onto the numeric enum filter', () => {
    component.filterForm.setValue({ searchValue: '', status: '2' });
    component.onFilterChange();

    const req = httpMock.expectOne(r => r.method === 'GET' && r.url.endsWith('/api/OrphanReportCycles'));
    expect(req.request.params.get('status')).toBe('2');
    req.flush({ items: [], totalCount: 0, page: 1, pageSize: 10 });
  });

  it('renders the derived status keys per cycle', () => {
    expect(component.statusKey(cycle({ status: OrphanReportCycleStatus.NotStarted })))
      .toBe('orphanReportCycles.status.notStarted');
    expect(component.statusKey(cycle({ status: OrphanReportCycleStatus.Completed })))
      .toBe('orphanReportCycles.status.completed');
  });

  it('deletes only after the confirm resolves true, then reloads (UC-ORC-04)', async () => {
    const notification = TestBed.inject(NotificationService);
    spyOn(notification, 'confirm').and.resolveTo(true);

    component.cycles = [cycle({ status: OrphanReportCycleStatus.NotStarted })];
    const del = component.deleteCycle(component.cycles[0]);
    httpMock.expectOne(r => r.method === 'DELETE' && r.url.endsWith('/api/OrphanReportCycles/cycle-1'))
      .flush({ message: 'deleted' });
    await del;

    // deleteCycle triggers the reload GET
    flushCycles([]);
    expect(component.deletingId).toBeUndefined();
  });

  it('never calls delete when the confirm is declined', async () => {
    const notification = TestBed.inject(NotificationService);
    spyOn(notification, 'confirm').and.resolveTo(false);

    component.cycles = [cycle({ status: OrphanReportCycleStatus.NotStarted })];
    await component.deleteCycle(component.cycles[0]);

    httpMock.expectNone(r => r.method === 'DELETE');
    expect(component.deletingId).toBeUndefined();
  });

  it('offers delete only for a not-started cycle', () => {
    expect(component.isNotStarted(cycle({ status: OrphanReportCycleStatus.NotStarted }))).toBeTrue();
    expect(component.isNotStarted(cycle({ status: OrphanReportCycleStatus.InProgress }))).toBeFalse();
    expect(component.isNotStarted(cycle({ status: OrphanReportCycleStatus.Completed }))).toBeFalse();
  });
});
