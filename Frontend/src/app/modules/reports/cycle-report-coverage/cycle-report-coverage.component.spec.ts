import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { TranslateModule } from '@ngx-translate/core';

import { CycleReportCoverageComponent } from './cycle-report-coverage.component';
import { NotificationService } from '../../../core/services/notification.service';

describe('CycleReportCoverageComponent (UC-ORC-09)', () => {
  let component: CycleReportCoverageComponent;
  let fixture: ComponentFixture<CycleReportCoverageComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CycleReportCoverageComponent,
        HttpClientTestingModule,
        RouterTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [NotificationService]
    }).compileComponents();

    fixture = TestBed.createComponent(CycleReportCoverageComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  const flushSelector = (): void => {
    httpMock
      .expectOne(req => req.method === 'GET' && req.url.endsWith('/api/OrphanReportCycles/selector'))
      .flush([]);
  };

  it('should create', () => {
    flushSelector();
    expect(component).toBeTruthy();
  });

  it('loads the cycle selector options on init', () => {
    flushSelector();
    expect(component.cycleOptions).toEqual([]);
  });

  it('refuses to run before a cycle is picked (the cycle is mandatory)', () => {
    flushSelector();
    const notification = TestBed.inject(NotificationService);
    const warning = spyOn(notification, 'warning');

    component.run();

    expect(warning).toHaveBeenCalledWith('reports.cycleCoverage.pickCycleFirst');
    httpMock.expectNone(r => r.method === 'POST');
  });

  it('posts the filter with the chosen cycle and pages one category', () => {
    flushSelector();
    component.filterForm.get('cycleId')!.setValue('cycle-1');
    component.setCategory('Submitted');

    const req = httpMock.expectOne(r => r.method === 'POST' && r.url.endsWith('/api/Reports/cycle-reports'));
    expect(req.request.body.cycleId).toBe('cycle-1');
    expect(req.request.body.category).toBe('Submitted');
    req.flush({
      cycleId: 'cycle-1',
      cycleTitle: 'دورة الربع الأول',
      items: [
        { orphanId: 'o1', orphanCode: 'C-1', orphanName: 'يتيم', hasReport: true, reportDate: '2026-02-01T00:00:00Z', reviewState: 'Accepted' },
        { orphanId: 'o2', orphanCode: 'C-2', orphanName: 'يتيم آخر', hasReport: false }
      ],
      totalCount: 2,
      submittedCount: 5,
      notSubmittedCount: 3,
      page: 1,
      pageSize: 20
    });

    expect(component.rows.length).toBe(2);
    expect(component.submittedCount).toBe(5);
    expect(component.notSubmittedCount).toBe(3);
    expect(component.loading).toBeFalse();
  });

  it('switching tabs resets the page and re-runs with the new category', () => {
    flushSelector();
    component.filterForm.get('cycleId')!.setValue('cycle-1');
    component.currentPage = 3;

    component.setCategory('NotSubmitted');
    const req = httpMock.expectOne(r => r.method === 'POST');
    expect(req.request.body.category).toBe('NotSubmitted');
    expect(req.request.body.page).toBe(1);
    req.flush({ items: [], totalCount: 0, submittedCount: 0, notSubmittedCount: 0, page: 1, pageSize: 20, cycleId: 'cycle-1', cycleTitle: '' });
  });

  it('keeps the same tab when clicked again (no redundant reload)', () => {
    flushSelector();
    component.category = 'Submitted';
    component.setCategory('Submitted');
    httpMock.expectNone(r => r.method === 'POST');
  });
});
