import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from '@angular/router';

import { CycleReportChooserComponent } from './cycle-report-chooser.component';
import { NotificationService } from '../../../core/services/notification.service';
import { OrphanCycleReportStatusDto } from '../models/orphan-report-cycle.model';

/** One chooser row against the mocked /orphan/{id}/report-status read. */
const row = (over: Partial<OrphanCycleReportStatusDto>): OrphanCycleReportStatusDto => ({
  cycleId: 'cycle-1',
  titleAr: 'دورة الربع الأول',
  startDate: '2026-01-01T00:00:00Z',
  endDate: '2026-03-31T00:00:00Z',
  submitted: false,
  ...over
});

describe('CycleReportChooserComponent (UC-ORC-06)', () => {
  let component: CycleReportChooserComponent;
  let fixture: ComponentFixture<CycleReportChooserComponent>;
  let httpMock: HttpTestingController;
  let router: Router;
  let notification: NotificationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CycleReportChooserComponent,
        HttpClientTestingModule,
        RouterTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [NotificationService]
    }).compileComponents();

    fixture = TestBed.createComponent(CycleReportChooserComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    notification = TestBed.inject(NotificationService);
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  const flushStatus = (rows: OrphanCycleReportStatusDto[]): void => {
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.includes('/api/OrphanReportCycles/orphan/orph-1/report-status'))
      .flush(rows);
  };

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('warns and keeps the modal closed when no cycle is running (button-race guard)', () => {
    spyOn(notification, 'warning');
    const navigateSpy = spyOn(router, 'navigate');

    component.openFor({ orphanId: 'orph-1', code: 'ORP-1' });
    flushStatus([]);

    expect(notification.warning).toHaveBeenCalled();
    expect(component.show).toBeFalse();
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('routes straight to the create form when one cycle runs and is unsubmitted', () => {
    const navigateSpy = spyOn(router, 'navigate');

    component.openFor({ orphanId: 'orph-1', code: 'ORP-1' });
    flushStatus([row({})]);

    expect(component.show).toBeFalse();
    expect(navigateSpy).toHaveBeenCalledWith(
      ['/periodic-orphan-reports/create'],
      { queryParams: { code: 'ORP-1', cycleId: 'cycle-1' } }
    );
  });

  it('routes to the existing report edit route when the single cycle is already submitted', () => {
    const navigateSpy = spyOn(router, 'navigate');

    component.openFor({ orphanId: 'orph-1', code: 'ORP-1' });
    flushStatus([row({ submitted: true, reportId: 'rep-9' })]);

    expect(navigateSpy).toHaveBeenCalledWith(['/periodic-orphan-reports', 'rep-9', 'edit']);
  });

  it('opens the modal listing every running cycle when more than one runs', () => {
    spyOn(router, 'navigate');

    component.openFor({ orphanId: 'orph-1', code: 'ORP-1' });
    flushStatus([row({}), row({ cycleId: 'cycle-2', titleAr: 'دورة الربع الثاني' })]);

    expect(component.show).toBeTrue();
    expect(component.rows.length).toBe(2);
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('navigates for the chosen cycle and closes the modal', () => {
    const navigateSpy = spyOn(router, 'navigate');

    component.openFor({ orphanId: 'orph-1', code: 'ORP-1' });
    flushStatus([row({}), row({ cycleId: 'cycle-2', submitted: true, reportId: 'rep-2' })]);

    component.choose(component.rows[1]);

    expect(navigateSpy).toHaveBeenCalledWith(['/periodic-orphan-reports', 'rep-2', 'edit']);
    expect(component.show).toBeFalse();
    expect(component.rows).toEqual([]);
  });

  it('toasts an error when the status read fails', () => {
    spyOn(notification, 'error');
    spyOn(router, 'navigate');

    component.openFor({ orphanId: 'orph-1', code: 'ORP-1' });
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.includes('/report-status'))
      .flush({ message: 'boom' }, { status: 500, statusText: 'Server Error' });

    expect(notification.error).toHaveBeenCalled();
    expect(component.loading).toBeFalse();
  });
});
